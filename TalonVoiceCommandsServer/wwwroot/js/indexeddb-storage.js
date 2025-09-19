// IndexedDB storage wrapper for Talon Voice Commands
// Provides high-capacity storage for large datasets that exceed localStorage limits

const TalonStorageDB = {
    dbName: 'TalonVoiceCommandsV2', // Changed database name to force fresh creation
    version: 1, // Reset to version 1 for new database
    db: null,
    _initializing: false,
    _initialized: false,

    // Initialize IndexedDB (idempotent)
    async init() {
        // Prevent double-initialization from multiple parallel calls
        if (this._initialized) {
            console.log('TalonStorageDB: Already initialized');
            return this.db;
        }
        if (this._initializing) {
            // Wait until current initialization finishes
            return new Promise((resolve, reject) => {
                const waitForInit = () => {
                    if (this._initialized) return resolve(this.db);
                    if (!this._initializing) return reject(new Error('TalonStorageDB: initialization failed'));
                    setTimeout(waitForInit, 50);
                };
                waitForInit();
            });
        }

        this._initializing = true;

        return new Promise((resolve, reject) => {
            console.log('TalonStorageDB: Initializing IndexedDB...');

            const request = indexedDB.open(this.dbName, this.version);
            
            request.onerror = () => {
                console.error('TalonStorageDB: Failed to open IndexedDB', request.error);
                reject(request.error);
            };
            
            request.onsuccess = () => {
                this.db = request.result;
                console.log('TalonStorageDB: IndexedDB initialized successfully');
                resolve(this.db);
            };
            
            request.onupgradeneeded = (event) => {
                console.log('TalonStorageDB: Setting up IndexedDB schema...');
                const db = event.target.result;
                
                // Create object stores for commands and lists with auto-incrementing keys
                if (!db.objectStoreNames.contains('commands')) {
                    const commandStore = db.createObjectStore('commands', { keyPath: 'id', autoIncrement: true });
                    commandStore.createIndex('application', 'application', { unique: false });
                    commandStore.createIndex('mode', 'mode', { unique: false });
                    commandStore.createIndex('command', 'command', { unique: false });
                    console.log('TalonStorageDB: Commands store created with auto-increment');
                }
                
                if (!db.objectStoreNames.contains('lists')) {
                    const listStore = db.createObjectStore('lists', { keyPath: 'id', autoIncrement: true });
                    listStore.createIndex('listName', 'listName', { unique: false });
                    console.log('TalonStorageDB: Lists store created with auto-increment');
                }
                
                // Create metadata store for storage info
                if (!db.objectStoreNames.contains('metadata')) {
                    db.createObjectStore('metadata', { keyPath: 'key' });
                    console.log('TalonStorageDB: Metadata store created');
                }
            };
        });
    },

    // Ensure database is initialized
    async ensureDB() {
        if (!this.db) {
            await this.init();
        }
        return this.db;
    },

    // Save commands to IndexedDB
    async saveCommands(commands) {
        try {
            // Validate input
            if (!commands || !Array.isArray(commands)) {
                console.warn('TalonStorageDB: saveCommands called with invalid data:', commands);
                commands = []; // Default to empty array
            }
            
            const db = await this.ensureDB();
            const transaction = db.transaction(['commands', 'metadata'], 'readwrite');
            const commandStore = transaction.objectStore('commands');
            const metadataStore = transaction.objectStore('metadata');
            
            console.log(`TalonStorageDB: Saving ${commands.length} commands...`);
            
            // Clear existing commands
            await commandStore.clear();
            
            // Add new commands (remove existing ID to let IndexedDB auto-generate)
            for (const command of commands) {
                const commandToSave = { ...command };
                // Remove the original ID to let IndexedDB auto-generate a new one
                delete commandToSave.id;
                delete commandToSave.Id; // Handle both cases
                await commandStore.add(commandToSave);
            }
            
            // Update metadata
            await metadataStore.put({
                key: 'commands-info',
                count: commands.length,
                lastUpdated: new Date().toISOString(),
                sizeEstimate: JSON.stringify(commands).length
            });
            
            await transaction.complete;
            console.log(`TalonStorageDB: Successfully saved ${commands.length} commands`);
            return true;
            
        } catch (error) {
            console.error('TalonStorageDB: Error saving commands:', error);
            throw error;
        }
    },

    // Load commands from IndexedDB
    async loadCommands() {
        try {
            const db = await this.ensureDB();
            const transaction = db.transaction(['commands'], 'readonly');
            const store = transaction.objectStore('commands');
            
            return new Promise((resolve, reject) => {
                const request = store.getAll();
                
                request.onsuccess = () => {
                    const commands = request.result;
                    console.log(`TalonStorageDB: Loaded ${commands.length} commands from IndexedDB`);
                    resolve(commands);
                };
                
                request.onerror = () => {
                    console.error('TalonStorageDB: Error loading commands:', request.error);
                    reject(request.error);
                };
            });
            
        } catch (error) {
            console.error('TalonStorageDB: Error in loadCommands:', error);
            throw error;
        }
    },

    // Load commands in smaller batches to avoid blocking the browser
    async loadCommandsChunked(chunkSize = 1000) {
        try {
            const db = await this.ensureDB();
            const allCommands = [];
            let lastKey = null;
            let totalProcessed = 0;
            
            // Process in smaller batches with yields between transactions
            while (true) {
                const batch = await this.loadCommandBatch(db, lastKey, chunkSize);
                
                if (batch.length === 0) {
                    break; // No more data
                }
                
                allCommands.push(...batch);
                totalProcessed += batch.length;
                
                console.log(`TalonStorageDB: Processed ${totalProcessed} commands...`);
                
                // Get the last key for the next batch
                if (batch.length > 0) {
                    lastKey = batch[batch.length - 1].id;
                }
                
                // Yield control to prevent blocking if we have more data to load
                if (batch.length === chunkSize) {
                    await new Promise(resolve => setTimeout(resolve, 10));
                } else {
                    break; // Last batch (incomplete batch means we're done)
                }
            }
            
            console.log(`TalonStorageDB: Loaded ${allCommands.length} commands from IndexedDB (chunked)`);
            return allCommands;
            
        } catch (error) {
            console.error('TalonStorageDB: Error in loadCommandsChunked:', error);
            throw error;
        }
    },

    // Helper method to load a single batch of commands
    loadCommandBatch: function(db, lastKey, limit) {
        return new Promise((resolve, reject) => {
            const transaction = db.transaction(['commands'], 'readonly');
            const store = transaction.objectStore('commands');
            const commands = [];
            
            let request;
            if (lastKey) {
                // Continue from where we left off
                const range = IDBKeyRange.lowerBound(lastKey, true); // exclusive of lastKey
                request = store.openCursor(range);
            } else {
                // Start from the beginning
                request = store.openCursor();
            }
            
            request.onsuccess = (event) => {
                const cursor = event.target.result;
                if (cursor && commands.length < limit) {
                    commands.push(cursor.value);
                    cursor.continue();
                } else {
                    resolve(commands);
                }
            };
            
            request.onerror = () => {
                reject(request.error);
            };
        });
    },

    // Save lists to IndexedDB
    async saveLists(lists) {
        try {
            // Validate input
            if (!lists || !Array.isArray(lists)) {
                console.warn('TalonStorageDB: saveLists called with invalid data:', lists);
                lists = []; // Default to empty array
            }
            
            const db = await this.ensureDB();
            const transaction = db.transaction(['lists', 'metadata'], 'readwrite');
            const listStore = transaction.objectStore('lists');
            const metadataStore = transaction.objectStore('metadata');
            
            console.log(`TalonStorageDB: Saving ${lists.length} lists...`);
            
            // Clear existing lists
            await listStore.clear();
            
            // Add new lists (remove existing ID to let IndexedDB auto-generate)
            for (const list of lists) {
                const listToSave = { ...list };
                // Remove the original ID to let IndexedDB auto-generate a new one
                delete listToSave.id;
                delete listToSave.Id; // Handle both cases
                await listStore.add(listToSave);
            }
            
            // Update metadata
            await metadataStore.put({
                key: 'lists-info',
                count: lists.length,
                lastUpdated: new Date().toISOString(),
                sizeEstimate: JSON.stringify(lists).length
            });
            
            await transaction.complete;
            console.log(`TalonStorageDB: Successfully saved ${lists.length} lists`);
            return true;
            
        } catch (error) {
            console.error('TalonStorageDB: Error saving lists:', error);
            throw error;
        }
    },

    // Save commands from JSON string (for Blazor JSInterop)
    async saveCommandsFromJson(commandsJson) {
        try {
            if (!commandsJson || typeof commandsJson !== 'string') {
                console.warn('TalonStorageDB: saveCommandsFromJson called with invalid JSON:', commandsJson);
                return await this.saveCommands([]);
            }
            
            const commands = JSON.parse(commandsJson);
            return await this.saveCommands(commands);
        } catch (error) {
            console.error('TalonStorageDB: Error parsing commands JSON:', error);
            return await this.saveCommands([]);
        }
    },

    // Save lists from JSON string (for Blazor JSInterop)
    async saveListsFromJson(listsJson) {
        try {
            if (!listsJson || typeof listsJson !== 'string') {
                console.warn('TalonStorageDB: saveListsFromJson called with invalid JSON:', listsJson);
                return await this.saveLists([]);
            }
            
            const lists = JSON.parse(listsJson);
            return await this.saveLists(lists);
        } catch (error) {
            console.error('TalonStorageDB: Error parsing lists JSON:', error);
            return await this.saveLists([]);
        }
    },

    // Load lists from IndexedDB
    async loadLists() {
        try {
            const db = await this.ensureDB();
            const transaction = db.transaction(['lists'], 'readonly');
            const store = transaction.objectStore('lists');
            
            return new Promise((resolve, reject) => {
                const request = store.getAll();
                
                request.onsuccess = () => {
                    const lists = request.result;
                    console.log(`TalonStorageDB: Loaded ${lists.length} lists from IndexedDB`);
                    resolve(lists);
                };
                
                request.onerror = () => {
                    console.error('TalonStorageDB: Error loading lists:', request.error);
                    reject(request.error);
                };
            });
            
        } catch (error) {
            console.error('TalonStorageDB: Error in loadLists:', error);
            throw error;
        }
    },

    // Get storage statistics
    async getStorageInfo() {
        try {
            const db = await this.ensureDB();
            const transaction = db.transaction(['metadata'], 'readonly');
            const store = transaction.objectStore('metadata');
            
            const commandsInfo = await this.getMetadata(store, 'commands-info');
            const listsInfo = await this.getMetadata(store, 'lists-info');
            
            return {
                commands: commandsInfo || { count: 0, sizeEstimate: 0 },
                lists: listsInfo || { count: 0, sizeEstimate: 0 },
                totalSizeEstimate: (commandsInfo?.sizeEstimate || 0) + (listsInfo?.sizeEstimate || 0)
            };
            
        } catch (error) {
            console.error('TalonStorageDB: Error getting storage info:', error);
            return { commands: { count: 0 }, lists: { count: 0 }, totalSizeEstimate: 0 };
        }
    },

    // Get lists breakdown: number of lists and total number of list items
    async getListsBreakdown() {
        try {
            // The lists object store stores one record per list entry (each TalonList is a single item)
            const db = await this.ensureDB();
            const transaction = db.transaction(['lists'], 'readonly');
            const store = transaction.objectStore('lists');

            return new Promise((resolve, reject) => {
                const request = store.openCursor();
                const perListMap = new Map();
                let totalItems = 0;

                request.onsuccess = (event) => {
                    const cursor = event.target.result;
                    if (cursor) {
                        const record = cursor.value;
                        // Each record represents one list item
                        totalItems += 1;

                        // Support multiple possible property names for the list name
                        const listName = record.ListName || record.listName || record.List || record.list || '';
                        const key = listName || 'Unnamed';

                        perListMap.set(key, (perListMap.get(key) || 0) + 1);

                        cursor.continue();
                    } else {
                        const perList = Array.from(perListMap.entries()).map(([listName, itemCount]) => ({ listName, itemCount }));
                        perList.sort((a, b) => b.itemCount - a.itemCount || a.listName.localeCompare(b.listName));
                        const listCount = perListMap.size;
                        console.log('TalonStorageDB: Lists breakdown computed', { listCount, totalItems });
                        resolve({ listCount, totalItems, perList });
                    }
                };

                request.onerror = () => {
                    console.error('TalonStorageDB: Error computing lists breakdown', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('TalonStorageDB: Error in getListsBreakdown:', error);
            return { listCount: 0, totalItems: 0, perList: [] };
        }
    },

    // Get all items for a specific list name using the listName index for speed
    async getListItems(listName) {
        try {
            if (!listName) return [];
            const db = await this.ensureDB();
            const tx = db.transaction(['lists'], 'readonly');
            const store = tx.objectStore('lists');

            // Prefer using the index for fast lookup
            if (store.indexNames && store.indexNames.contains('listName')) {
                return new Promise((resolve, reject) => {
                    try {
                        const idx = store.index('listName');
                        const req = idx.getAll(listName);
                        req.onsuccess = () => resolve(req.result || []);
                        req.onerror = () => reject(req.error);
                    } catch (err) {
                        reject(err);
                    }
                });
            }

            // Fallback: scan cursor and filter
            return new Promise((resolve, reject) => {
                const results = [];
                const req = store.openCursor();
                req.onsuccess = (event) => {
                    const cursor = event.target.result;
                    if (cursor) {
                        const rec = cursor.value;
                        const rn = rec.ListName || rec.listName || rec.List || rec.list || '';
                        if (rn === listName) results.push(rec);
                        cursor.continue();
                    } else {
                        resolve(results);
                    }
                };
                req.onerror = () => reject(req.error);
            });
        } catch (error) {
            console.error('TalonStorageDB: getListItems error:', error);
            return [];
        }
    },
    // Fast helper: return JSON string of list items to avoid double marshalling
    async getListItemsJson(listName) {
        try {
            const items = await this.getListItems(listName);
            return JSON.stringify(items || []);
        } catch (err) {
            console.error('TalonStorageDB: getListItemsJson error:', err);
            return '[]';
        }
    },

    // Fast helper that applies client-side fixes/normalization to list items
    // before returning JSON. This mirrors the search direct-JS approach where
    // the client prepares corrected data for immediate rendering on the server.
    async getListItemsJsonWithFixes(listName) {
        try {
            const rawItems = await this.getListItems(listName);
            if (!rawItems || !Array.isArray(rawItems)) return '[]';

            const fixed = rawItems.map((rec) => {
                // Normalize property names and types
                const listNameValue = rec.ListName || rec.listName || rec.List || rec.list || listName || '';

                // Prefer ListValue -> listValue
                let value = rec.ListValue || rec.listValue || rec.Value || rec.value || '';
                if (typeof value !== 'string') value = String(value || '');
                value = value.trim();

                // SpokenForm normalization: fall back to value if missing
                let spoken = rec.SpokenForm || rec.spokenForm || rec.Spoken || rec.spoken || '';
                if (typeof spoken !== 'string') spoken = String(spoken || '');
                spoken = spoken.trim();
                if (!spoken && value) spoken = value;

                // Keep source metadata when available
                const meta = {};
                if (rec.FilePath) meta.file = rec.FilePath;
                if (rec.LineNumber) meta.line = rec.LineNumber;

                return {
                    ListName: listNameValue,
                    ListValue: value,
                    SpokenForm: spoken,
                    // Preserve any other fields the UI/code might expect
                    // but avoid deep-cloning everything to keep payload small
                    Original: rec,
                    _meta: meta
                };
            })
            // Filter out empty values which are not useful to display
            .filter(i => i.ListValue && i.ListValue.length > 0);

            return JSON.stringify(fixed || []);
        } catch (err) {
            console.error('TalonStorageDB: getListItemsJsonWithFixes error:', err);
            // Fallback to the simple JSON helper
            return await this.getListItemsJson(listName);
        }
    },

    // Helper to get metadata
    async getMetadata(store, key) {
        return new Promise((resolve) => {
            const request = store.get(key);
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => resolve(null);
        });
    },

    // Clear all data (for testing/reset)
    async clearAll() {
        try {
            const db = await this.ensureDB();
            const transaction = db.transaction(['commands', 'lists', 'metadata'], 'readwrite');
            
            await transaction.objectStore('commands').clear();
            await transaction.objectStore('lists').clear();
            await transaction.objectStore('metadata').clear();
            
            await transaction.complete;
            console.log('TalonStorageDB: All data cleared');
            return true;
            
        } catch (error) {
            console.error('TalonStorageDB: Error clearing data:', error);
            throw error;
        }
    },

    // Migration helper - check if localStorage data exists
    async checkLocalStorageData() {
        try {
            const commandsData = localStorage.getItem('talon-voice-commands');
            const listsData = localStorage.getItem('talon-lists');
            
            return {
                hasCommands: !!commandsData,
                hasLists: !!listsData,
                commandsSize: commandsData ? commandsData.length : 0,
                listsSize: listsData ? listsData.length : 0
            };
        } catch (error) {
            console.error('Error checking localStorage:', error);
            return { hasCommands: false, hasLists: false, commandsSize: 0, listsSize: 0 };
        }
    },

    // Migration helper - move data from localStorage to IndexedDB
    async migrateFromLocalStorage() {
        try {
            console.log('TalonStorageDB: Starting migration from localStorage...');
            const localData = await this.checkLocalStorageData();
            
            if (!localData.hasCommands && !localData.hasLists) {
                console.log('TalonStorageDB: No localStorage data to migrate');
                return { success: true, migrated: { commands: 0, lists: 0 } };
            }
            
            let migratedCommands = 0, migratedLists = 0;
            
            // Migrate commands
            if (localData.hasCommands) {
                const commandsJson = localStorage.getItem('talon-voice-commands');
                const commands = JSON.parse(commandsJson);
                await this.saveCommands(commands);
                migratedCommands = commands.length;
                console.log(`TalonStorageDB: Migrated ${migratedCommands} commands`);
            }
            
            // Migrate lists
            if (localData.hasLists) {
                const listsJson = localStorage.getItem('talon-lists');
                const lists = JSON.parse(listsJson);
                await this.saveLists(lists);
                migratedLists = lists.length;
                console.log(`TalonStorageDB: Migrated ${migratedLists} lists`);
            }
            
            // Clear localStorage after successful migration
            localStorage.removeItem('talon-voice-commands');
            localStorage.removeItem('talon-lists');
            
            console.log('TalonStorageDB: Migration completed successfully');
            return { 
                success: true, 
                migrated: { commands: migratedCommands, lists: migratedLists } 
            };
            
        } catch (error) {
            console.error('TalonStorageDB: Migration failed:', error);
            return { success: false, error: error.message };
        }
    },

    // Delete and recreate the entire database (for troubleshooting)
    async deleteAndRecreateDatabase() {
        try {
            console.log('TalonStorageDB: Deleting entire database for fresh start...');
            
            // Close current connection if open
            if (this.db) {
                this.db.close();
                this.db = null;
            }
            
            // Delete the database
            return new Promise((resolve, reject) => {
                const deleteRequest = indexedDB.deleteDatabase(this.dbName);
                
                deleteRequest.onsuccess = () => {
                    console.log('TalonStorageDB: Database deleted successfully');
                    resolve(true);
                };
                
                deleteRequest.onerror = () => {
                    console.error('TalonStorageDB: Failed to delete database', deleteRequest.error);
                    reject(deleteRequest.error);
                };
                
                deleteRequest.onblocked = () => {
                    console.warn('TalonStorageDB: Database deletion blocked - close all tabs using this database');
                    reject(new Error('Database deletion blocked'));
                };
            });
            
        } catch (error) {
            console.error('TalonStorageDB: Error during database deletion:', error);
            throw error;
        }
    },

    // Search with filters applied directly in IndexedDB, returning only IDs for performance
    async searchFilteredCommandIds(searchParams) {
        console.log('TalonStorageDB: searchFilteredCommandIds called with params:', searchParams);
        try {
            const db = await this.ensureDB();
            const transaction = db.transaction(['commands'], 'readonly');
            const store = transaction.objectStore('commands');
            
            const filteredCommandIds = [];
            
            return new Promise((resolve, reject) => {
                const request = store.openCursor();
                
                request.onsuccess = (event) => {
                    const cursor = event.target.result;
                    if (cursor) {
                        const command = cursor.value;
                        
                        // Apply filters
                        if (this.matchesFilters(command, searchParams)) {
                            filteredCommandIds.push(command.Id);
                        }
                        
                        cursor.continue();
                    } else {
                        console.log(`TalonStorageDB: Filtered search found ${filteredCommandIds.length} command IDs`);
                        resolve(filteredCommandIds);
                    }
                };
                
                request.onerror = () => {
                    console.error('TalonStorageDB: Error in filtered ID search:', request.error);
                    reject(request.error);
                };
            });
            
        } catch (error) {
            console.error('TalonStorageDB: Error in searchFilteredCommandIds:', error);
            throw error;
        }
    },

    // Search with filters applied directly in IndexedDB to avoid loading all data
    async searchFilteredCommands(searchParams) {
        console.log('TalonStorageDB: searchFilteredCommands called with params:', searchParams);
        try {
            const db = await this.ensureDB();
            const transaction = db.transaction(['commands'], 'readonly');
            const store = transaction.objectStore('commands');
            
            const filteredCommands = [];
            
            return new Promise((resolve, reject) => {
                const request = store.openCursor();
                
                request.onsuccess = (event) => {
                    const cursor = event.target.result;
                    if (cursor) {
                        const command = cursor.value;
                        
                        // Apply filters
                        if (this.matchesFilters(command, searchParams)) {
                            filteredCommands.push(command);
                        }
                        
                        cursor.continue();
                    } else {
                        console.log(`TalonStorageDB: Filtered search found ${filteredCommands.length} results`);
                        resolve(filteredCommands);
                    }
                };
                
                request.onerror = () => {
                    console.error('TalonStorageDB: Error in filtered search:', request.error);
                    reject(request.error);
                };
            });
            
        } catch (error) {
            console.error('TalonStorageDB: Error in searchFilteredCommands:', error);
            throw error;
        }
    },

    // Normalize common app name variants
    normalizeAppName(name) {
        if (!name) return '';
        const n = String(name).trim().toLowerCase();
        if (n === 'code' || n === 'vscode' || n === 'visual studio code' || n === 'vs code') return 'visual studio code';
        if (n === 'devenv' || n === 'visual studio' || n === 'vs' || n === 'msvs') return 'visual studio';
        if (n === 'msedge' || n === 'edge' || n === 'microsoft edge') return 'edge';
        if (n === 'google chrome') return 'chrome';
        return name;
    },

    // Helper function to check if a command matches the search filters
    matchesFilters(command, searchParams) {
        // Apply field filters first
        if (searchParams.application) {
            // Use normalized, case-insensitive equality for application filtering
            const appCommand = String(this.normalizeAppName(command.Application || '') || '').trim().toLowerCase();
            const appSearch = String(this.normalizeAppName(searchParams.application || '') || '').trim().toLowerCase();
            // Diagnostic: log application comparisons when debugging
            try {
                if (window && window.console && window.console.debug) {
                    window.console.debug('AppFilterDebug', { commandApp: command.Application, normalizedCommandApp: appCommand, searchApp: searchParams.application, normalizedSearchApp: appSearch, match: appCommand === appSearch });
                }
            } catch (e) {
                // ignore
            }
            if (!appCommand || !appSearch) return false;
            if (appCommand !== appSearch) return false;
        }
        if (searchParams.mode && command.Mode !== searchParams.mode) return false;
        if (searchParams.operatingSystem && command.OperatingSystem !== searchParams.operatingSystem) return false;
        if (searchParams.repository && command.Repository !== searchParams.repository) return false;
        if (searchParams.title && command.Title !== searchParams.title) return false;
        if (searchParams.codeLanguage && command.CodeLanguage !== searchParams.codeLanguage) return false;
        
        // Tags filter (check if any tag matches)
        if (searchParams.tags) {
            if (!command.Tags) return false;
            const commandTags = command.Tags.split(',').map(tag => tag.trim().toLowerCase());
            const searchTag = searchParams.tags.toLowerCase();
            if (!commandTags.includes(searchTag)) return false;
        }
        
        // Apply text search if provided
        if (searchParams.searchTerm) {
            // Normalize search term: trim whitespace and remove surrounding straight/smart quotes
            const rawTerm = String(searchParams.searchTerm || '').trim();
            const normalized = rawTerm.replace(/^(["'“”])+|(["'“”])+$/g, '').toLowerCase();
            const searchTerm = normalized;

            if (searchParams.useSemanticMatching) {
                // Simple semantic search - check both command and script
                return (command.Command && command.Command.toLowerCase().includes(searchTerm)) ||
                       (command.Script && command.Script.toLowerCase().includes(searchTerm));
            } else {
                // Apply scope-based search
                switch (searchParams.searchScope) {
                    case 0: // Names Only: use contains matching (case-insensitive)
                        if (!command.Command) return false;
                        const cmdName = String(command.Command).trim().replace(/^(["'“”])+|(["'“”])+$/g, '').toLowerCase();
                        // Diagnostic logging to help trace unexpected matches in the UI
                        try {
                            if (window && window.console && window.console.debug) {
                                window.console.debug('NamesOnlyMatchDebug', {
                                    rawSearchTerm: rawTerm,
                                    normalizedSearchTerm: searchTerm,
                                    commandId: command.Id || command.id || null,
                                    rawCommand: command.Command,
                                    normalizedCommand: cmdName,
                                    match: cmdName.includes(searchTerm)
                                });
                            }
                        } catch (e) {
                            // swallow any logging errors
                        }
                        return cmdName.includes(searchTerm);
                    case 1: // Scripts Only
                        return command.Script && command.Script.toLowerCase().includes(searchTerm);
                    case 2: // All: keep contains behavior across fields
                        return (command.Command && command.Command.toLowerCase().includes(searchTerm)) ||
                               (command.Script && command.Script.toLowerCase().includes(searchTerm));
                    default:
                        return command.Command && command.Command.toLowerCase().includes(searchTerm);
                }
            }
        }
        
        return true; // No text search, just filter-based
    },

    // Enhanced search function that includes list item matching for "Names Only" and "Search All" scope
    async searchWithEnhancedListMatching(searchParams, maxResults, db) {
        console.log('TalonStorageDB: Using enhanced search with list matching for Names Only and Search All scope');
        
        const filteredCommands = [];
        const searchTerm = String(searchParams.searchTerm || '').trim().replace(/^(["'""])+|(["'""])+$/g, '').toLowerCase();
        const searchWords = searchTerm.split(/\s+/).filter(word => word.length > 0);
        
        // Load all lists into memory for faster matching
        const allLists = {};
        const listTransaction = db.transaction(['lists'], 'readonly');
        const listStore = listTransaction.objectStore('lists');
        
        await new Promise((resolve, reject) => {
            const listRequest = listStore.openCursor();
            listRequest.onsuccess = (event) => {
                const cursor = event.target.result;
                if (cursor) {
                    const list = cursor.value;
                    // Handle multiple items with the same list name by creating arrays
                    const listName = list.ListName || list.listName;
                    if (!allLists[listName]) {
                        allLists[listName] = [];
                    }
                    allLists[listName].push(list);
                    cursor.continue();
                } else {
                    resolve();
                }
            };
            listRequest.onerror = () => reject(listRequest.error);
        });
        
        console.log(`TalonStorageDB: Loaded ${Object.keys(allLists).length} lists for enhanced search`);
        
        // Now search commands with enhanced list matching
        const commandTransaction = db.transaction(['commands'], 'readonly');
        const commandStore = commandTransaction.objectStore('commands');
        
        return new Promise((resolve, reject) => {
            const request = commandStore.openCursor();
            
            request.onsuccess = (event) => {
                const cursor = event.target.result;
                if (cursor) {
                    const command = cursor.value;
                    
                    // Debug: log first command to see property structure
                    if (filteredCommands.length === 0) {
                        console.log('TalonStorageDB: Sample command structure:', Object.keys(command));
                        console.log('TalonStorageDB: Sample command command field:', command.command);
                        console.log('TalonStorageDB: Sample command Command field:', command.Command);
                    }
                    
                    // Apply non-search filters first
                    if (this.matchesNonSearchFilters(command, searchParams)) {
                        
                        // Check direct text matches first
                        let matches = (command.Command && command.Command.toLowerCase().includes(searchTerm)) ||
                                     (command.Script && command.Script.toLowerCase().includes(searchTerm));
                        
                        // If no direct match, check list items
                        if (!matches && command.Command) {
                            matches = this.commandMatchesListItems(command, searchWords, allLists);
                        }
                        
                        if (matches && filteredCommands.length < maxResults) {
                            filteredCommands.push(command);
                        }
                    }
                    
                    cursor.continue();
                } else {
                    console.log(`TalonStorageDB: Enhanced search found ${filteredCommands.length} results (limited to ${maxResults})`);
                    resolve(filteredCommands);
                }
            };
            
            request.onerror = () => {
                console.error('TalonStorageDB: Error in enhanced search:', request.error);
                reject(request.error);
            };
        });
    },

    // Helper to check non-search filters
    matchesNonSearchFilters(command, searchParams) {
        if (searchParams.application) {
            const appCommand = String(this.normalizeAppName(command.Application || '') || '').trim().toLowerCase();
            const appSearch = String(this.normalizeAppName(searchParams.application || '') || '').trim().toLowerCase();
            if (!appCommand || !appSearch || appCommand !== appSearch) return false;
        }
        if (searchParams.mode && command.Mode !== searchParams.mode) return false;
        if (searchParams.operatingSystem && command.OperatingSystem !== searchParams.operatingSystem) return false;
        if (searchParams.repository && command.Repository !== searchParams.repository) return false;
        if (searchParams.title && command.Title !== searchParams.title) return false;
        if (searchParams.codeLanguage && command.CodeLanguage !== searchParams.codeLanguage) return false;
        
        if (searchParams.tags) {
            if (!command.Tags) return false;
            const commandTags = command.Tags.split(',').map(tag => tag.trim().toLowerCase());
            const searchTag = searchParams.tags.toLowerCase();
            if (!commandTags.includes(searchTag)) return false;
        }
        
        return true;
    },

    // Helper to check if command matches list items
    commandMatchesListItems(command, searchWords, allLists) {
        // Find list references in the command using both curly braces and angle brackets (Talon syntax)
        const curlyMatches = command.Command.match(/\{([^}]+)\}/g) || [];
        const angleMatches = command.Command.match(/<([^>]+)>/g) || [];
        
        if (curlyMatches.length === 0 && angleMatches.length === 0) return false;
        
        // Extract list names (remove braces/brackets)
        const listNames = [
            ...curlyMatches.map(match => match.slice(1, -1)),
            ...angleMatches.map(match => match.slice(1, -1))
        ];
        
        console.debug(`Checking command "${command.Command}" for list references: ${listNames.join(', ')}`);
        console.debug(`Available lists count: ${Object.keys(allLists).length}`);
        console.debug(`First few available lists: ${Object.keys(allLists).slice(0, 5).join(', ')}`);
        
        // Check each referenced list
        for (const listName of listNames) {
            const listItems = allLists[listName];
            console.debug(`Looking for list "${listName}": found ${listItems ? (Array.isArray(listItems) ? listItems.length + ' items' : 'not an array') : 'not found'}`);
            
            if (listItems && Array.isArray(listItems)) {
                
                // Check each list item for the search words
                for (const listItem of listItems) {
                    const listValue = (listItem.ListValue || listItem.listValue || '').toLowerCase();
                    const spokenForm = (listItem.SpokenForm || listItem.spokenForm || '').toLowerCase();
                    
                    console.debug(`Checking list item: value="${listValue}", spoken="${spokenForm}"`);
                    
                    // Check if any search word matches this list item
                    for (const word of searchWords) {
                        if (listValue.includes(word) || spokenForm.includes(word)) {
                            console.debug(`Found match: word "${word}" in list item value "${listValue}" or spoken "${spokenForm}"`);
                            return true;
                        }
                    }
                }
            } else {
                console.debug(`List ${listName} not found or not an array:`, typeof listItems);
            }
        }
        
        return false;
    },

    // Quick count helper used by the UI to decide if any data exists
    async getCommandsCount() {
        try {
            const db = await this.ensureDB();
            const transaction = db.transaction(['commands'], 'readonly');
            const store = transaction.objectStore('commands');
            return await new Promise((resolve, reject) => {
                const req = store.count();
                req.onsuccess = () => resolve(req.result || 0);
                req.onerror = () => reject(req.error);
            });
        } catch {
            return 0;
        }
    },

    // Search and directly display results in the UI (no C# transfer needed)
    async searchAndDisplayResults(searchParams, maxResults = 500) {
        console.log(`TalonStorageDB: searchAndDisplayResults called with maxResults: ${maxResults}`);
        try {
            // Store last search params for custom message
            this.lastSearchParams = searchParams;
            // Get the results
            const results = await this.searchFilteredCommandsSimple(searchParams, maxResults);
            
            // Display results directly in the UI
            this.displaySearchResults(results);
            
            return results.length;
        } catch (error) {
            console.error('TalonStorageDB: Error in searchAndDisplayResults:', error);
            throw error;
        }
    },

    // Display search results directly in the UI
    displaySearchResults(commands) {
        console.log(`TalonStorageDB: Displaying ${commands.length} search results`);
        
        // Store results for potential re-rendering when view mode changes
        this.lastSearchResults = commands;
        
        // Find the results container in the UI
        const resultsContainer = document.querySelector('.search-results-container');
        if (!resultsContainer) {
            console.warn('TalonStorageDB: Results container not found');
            return;
        }

        // Clear existing results
        resultsContainer.innerHTML = '';

            if (commands.length === 0) {
                resultsContainer.innerHTML = `<div class='no-results'>No results found.</div>`;
                return;
            }

        // Check if "Show Full Cards" is enabled
        const showFullCardsCheckbox = document.getElementById('showFullCardsToggle');
        // If the result count is small, force full cards. When JS forces the view, keep the
        // checkbox in sync with this decision so the Blazor-bound property also updates.
        const showFullCards = commands.length <= 6 ? true : (showFullCardsCheckbox ? showFullCardsCheckbox.checked : false);
        if (commands.length <= 6 && showFullCardsCheckbox && !showFullCardsCheckbox.checked) {
            // Update the checkbox and dispatch a change event so Blazor @bind picks up the new value.
            try {
                showFullCardsCheckbox.checked = true;
                const changeEvent = new Event('change', { bubbles: true });
                showFullCardsCheckbox.dispatchEvent(changeEvent);
                console.log('TalonStorageDB: Auto-enabled Show Full Cards due to small result set and synced checkbox');
            } catch (e) {
                console.debug('TalonStorageDB: Failed to sync Show Full Cards checkbox', e);
            }
        }

        // Create results HTML using a responsive two-column grid (Bootstrap)
        // Header spans full width, each result becomes a column (col-12 on xs, col-md-6 on md+)
        let html = `<div class="search-results">`;
        html += `<div class="results-header mb-3"><strong>Found ${commands.length} results</strong></div>`;
        html += `<div class="row">`;

        // Helper: render command text but replace <listName> tokens with buttons
        const renderCommandWithListButtons = (text) => {
            if (!text) return '';
            // Split by <list> pattern, preserve list tokens
            // Match <list.name> where list name allows letters, numbers, underscores, dots
            return text.replace(/<([a-zA-Z_][a-zA-Z0-9_.]*)>/g, (match, p1) => {
                // Escape single quotes/backslashes for inline JS call
                const safeName = (p1 || '').replace(/\\/g, '\\\\').replace(/'/g, "\\'");
                // Return a semantic button element that calls the JS bridge
                return `<button type="button" class="btn btn-link p-0 talon-list-inline" aria-label="Open list ${this.escapeHtml(p1)}" onclick="window.TalonStorageDB.onListButtonClick('${safeName}')">&lt;${this.escapeHtml(p1)}&gt;</button>`;
            });
        };

        commands.forEach((command, index) => {
            // Generate script name for compact view
            const scriptName = command.Title || this.extractFilename(command.FilePath || '') || 'Untitled';
            
            html += `
                <div class="col-12 col-md-6 mb-3">
                    <div class="result-item card h-100" data-command-id="${command.Id || index}">`;
            
            if (showFullCards) {
                // Full card view - command first, then application
                    html += `
                            <div class="card-body d-flex flex-column">
                                <h6 class="card-title mb-2">
                                    <button type="button" class="btn btn-sm btn-outline-primary talon-command-btn" 
                                            aria-label="Open lists referenced by this command" 
                                            onclick="window.TalonStorageDB.onCommandTitleClick('${(command.Command || 'No command').replace(/\\/g, '\\\\').replace(/'/g, "\\'")}')">
                                        ${renderCommandWithListButtons(command.Command || 'No command')}
                                        <span class="ms-2 visually-hidden">(click to view lists)</span>
                                    </button>
                                </h6>
                            <div class="mb-2">
                                <span class="badge filter-btn-application px-2 py-1 mb-1" style="font-size:1rem; border-radius:0.5rem;">${this.escapeHtml(command.Application || 'N/A')}</span>
                            </div>
                            <div class="mb-2">
                                <div class="d-flex flex-wrap gap-1 mt-1">
                                    ${command.Title ? `<span class="badge bg-success" title="Title">${this.escapeHtml(command.Title)}</span>` : ''}
                                    ${command.Mode ? `<span class="badge bg-secondary" title="Mode">${this.escapeHtml(command.Mode)}</span>` : ''}
                                    ${command.Repository ? `<span class="badge bg-primary" title="Repository">${this.escapeHtml(command.Repository)}</span>` : ''}
                                    ${command.Tags ? command.Tags.split(',').map(tag => `<span class="badge bg-warning text-dark" title="Tag">${this.escapeHtml(tag.trim())}</span>`).join('') : ''}
                                    ${command.CodeLanguage ? `<span class="badge bg-danger" title="Code Language">${this.escapeHtml(command.CodeLanguage)}</span>` : ''}
                                    ${command.OperatingSystem ? `<span class="badge bg-info" title="Operating System">${this.escapeHtml(command.OperatingSystem)}</span>` : ''}
                                </div>
                            </div>
                            <div class="script-card-container mb-2" id="script-card-${index}"></div>
                            <div class="mt-auto command-details">
                                ${command.FilePath ? `
                                    <span class="badge bg-secondary clickable-filename" 
                                          style="cursor: pointer; font-size: 0.75em;" 
                                          onclick="window.TalonStorageDB.openFileInVSCode('${command.FilePath.replace(/\\/g, '\\\\').replace(/'/g, "\\'")}')"
                                          title="Click to open in VS Code: ${this.escapeHtml(command.FilePath)}">
                                        ${this.escapeHtml(this.extractFilename(command.FilePath))}
                                    </span>
                                ` : ''}
                            </div>
                        </div>`;
            } else {
                // Compact single-line view
                html += `
                        <div class="card-body py-2">
                            <div class="d-flex align-items-center justify-content-between flex-wrap">
                                <div class="d-flex align-items-center flex-grow-1 me-2" style="min-width: 0;">
                                        <button type="button" class="btn btn-link p-0 talon-command-btn text-truncate me-2" 
                                                style="max-width:200px; text-align:left;" 
                                                title="${this.escapeHtml(command.Command || 'No command')}"
                                                onclick="window.TalonStorageDB.onCommandTitleClick('${(command.Command || 'No command').replace(/\\/g, '\\\\').replace(/'/g, "\\'")}')">
                                            ${renderCommandWithListButtons(command.Command || 'No command')}
                                        </button>
                                </div>
                                <div class="d-flex align-items-center gap-2 flex-wrap">
                                    ${command.Application && command.Application.trim() && command.Application.length <= 50 ? `
                                        <span class="badge bg-dark text-truncate" style="max-width: 120px;" title="Application: ${this.escapeHtml(command.Application)}">
                                            <i class="oi oi-monitor me-1"></i>${this.escapeHtml(command.Application)}
                                        </span>
                                    ` : command.Application && command.Application.trim() ? `
                                        <span class="badge bg-dark" title="Application: ${this.escapeHtml(command.Application)}">
                                            <i class="oi oi-monitor"></i>
                                        </span>
                                    ` : ''}
                                    ${command.Repository && command.Repository.trim() && command.Repository.length <= 50 ? `
                                        <span class="badge bg-primary text-truncate" style="max-width: 120px;" title="Repository: ${this.escapeHtml(command.Repository)}">
                                            <i class="oi oi-code me-1"></i>${this.escapeHtml(command.Repository)}
                                        </span>
                                    ` : command.Repository && command.Repository.trim() ? `
                                        <span class="badge bg-primary" title="Repository: ${this.escapeHtml(command.Repository)}">
                                            <i class="oi oi-code"></i>
                                        </span>
                                    ` : ''}
                                    ${command.OperatingSystem && command.OperatingSystem.trim() && command.OperatingSystem.length <= 50 ? `
                                        <span class="badge bg-info text-truncate" style="max-width: 100px;" title="Operating System: ${this.escapeHtml(command.OperatingSystem)}">
                                            <i class="oi oi-laptop me-1"></i>${this.escapeHtml(command.OperatingSystem)}
                                        </span>
                                    ` : command.OperatingSystem && command.OperatingSystem.trim() ? `
                                        <span class="badge bg-info" title="Operating System: ${this.escapeHtml(command.OperatingSystem)}">
                                            <i class="oi oi-laptop"></i>
                                        </span>
                                    ` : ''}
                                    ${command.FilePath ? `
                                        <span class="badge bg-secondary clickable-filename" 
                                              style="cursor: pointer; font-size: 0.75em;" 
                                              onclick="window.TalonStorageDB.openFileInVSCode('${command.FilePath.replace(/\\/g, '\\\\').replace(/'/g, "\\'")}')"
                                              title="Click to open in VS Code: ${this.escapeHtml(command.FilePath)}">
                                            <i class="oi oi-external-link me-1"></i>${this.escapeHtml(this.extractFilename(command.FilePath))}
                                        </span>
                                    ` : ''}
                                </div>
                            </div>
                        </div>`;
            }
            
            html += `
                    </div>
                </div>
            `;
        });

        html += `</div>`; // .row
        html += `</div>`; // .search-results
        resultsContainer.innerHTML = html;

        // After rendering, invoke ScriptCard for each result (only for full cards)
        if (showFullCards) {
            commands.forEach((command, index) => {
                const container = document.getElementById(`script-card-${index}`);
                if (container && window.ScriptCard && (command.Script || command.Code)) {
                    const script = command.Script || command.Code;
                    const lines = script.split('\n');
                    window.ScriptCard.render({
                        title: command.CodeLanguage || 'Script',
                        lines: lines
                    }, container);
                }
            });
        }

        // Initialize event listeners if not already done
        this.initializeEventListeners();

        // Attach click handlers to any list buttons rendered into the results.
        // These buttons are created by the Blazor/JS hybrid and have the
        // class 'btn-outline-secondary' and contain the list name as text.
        try {
            const listButtons = resultsContainer.querySelectorAll('button');
            listButtons.forEach(btn => {
                // If the button contains an SVG and text, assume it's a list button
                const text = btn.innerText && btn.innerText.trim();
                if (text && !btn.__talon_list_handler_attached) {
                    // Attach click handler that calls into Blazor if available
                    btn.addEventListener('click', (e) => {
                        // Determine the list name from button textContent
                        const name = (e.currentTarget.innerText || '').trim();
                        if (name) {
                            // Avoid blocking UI; call the async handler
                            this.onListButtonClick(name);
                        }
                    });
                    btn.__talon_list_handler_attached = true;
                }
            });
        } catch (e) {
            console.debug('TalonStorageDB: Failed to attach list button handlers', e);
        }
    },

    // Utility function to escape HTML
    escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    // Find items matching a list name from the lists store, normalizing property names and prefixes
    findItemsForList(allItems, listName) {
        if (!Array.isArray(allItems)) return [];
        if (!listName) return [];

        // Strip braces/angle-brackets if present (e.g. '<user.foo>' or '{user.foo}')
        const stripDelimiters = (s) => (s || '').toString().trim().replace(/^\s*[<{\[]+|[>}\]]+\s*$/g, '');

        const normalize = (n) => stripDelimiters(n).toString().trim();
        const candidates = [];

        // Prepare alternate forms: with and without 'user.' prefix
        const base = normalize(listName);
        const alt = base.startsWith('user.') ? base.substring(5) : `user.${base}`;
        const forms = new Set([base, alt]);

        for (const it of allItems) {
            const ln = normalize(it.ListName || it.listName || it.List || it.list || it.listname);
            if (!ln) continue;
            // Compare case-insensitively against any form
            for (const f of forms) {
                if (!f) continue;
                if (ln.toLowerCase() === f.toLowerCase()) {
                    candidates.push(it);
                    break;
                }
            }
        }
        return candidates;
    },

    // Render the list side-panel directly on the client when server-side panel is empty
    renderPanelClientSide(listName, items) {
        try {
            if (!listName) return false;
            const panel = document.querySelector('[role="dialog"][aria-label="List values panel"]');
            if (!panel) return false;

            // Ensure panel is open (matches Blazor CSS expectations)
            panel.classList.add('open');

            // Set the title
            const titleEl = panel.querySelector('.list-panel-title');
            if (titleEl) titleEl.textContent = listName;

            // Find tbody and populate rows. If table structure differs, create the table.
            let tbody = panel.querySelector('table tbody');
            const panelBody = panel.querySelector('.list-panel-body') || panel;
            if (!tbody) {
                try {
                    // Create table with header and tbody
                    const table = document.createElement('table');
                    table.className = 'table table-sm table-borderless mb-0';
                    table.setAttribute('role', 'presentation');
                    const thead = document.createElement('thead');
                    thead.innerHTML = '<tr><th>Spoken Form</th><th>Value</th></tr>';
                    tbody = document.createElement('tbody');
                    table.appendChild(thead);
                    table.appendChild(tbody);
                    // Insert at top of panel body so filter input remains visible below header
                    panelBody.appendChild(table);
                } catch (e) {
                    console.error('TalonStorageDB: Failed to create table for panel rendering', e);
                    return false;
                }
            }

            // Build inner HTML rows
            const esc = (t) => this.escapeHtml(t);
            let html = '';
            for (const it of items) {
                const spoken = esc(it.SpokenForm || it.spokenForm || '');
                const value = esc(it.ListValue || it.listValue || it.value || '');
                html += `<tr><td class="font-monospace">${spoken}</td><td class="font-monospace">${value}</td></tr>`;
            }
            tbody.innerHTML = html;

            // Ensure the filter input is shown and cleared
            const filter = panel.querySelector('input[placeholder="Filter items..."]');
            if (filter) filter.value = '';

            // Ensure the panel body scroll is reset to top and hide spinner
            if (panelBody) panelBody.scrollTop = 0;
            try {
                const spinner = panel.querySelector('.list-panel-spinner');
                if (spinner) spinner.style.display = 'none';
            } catch (e) { /* ignore */ }

            console.log(`TalonStorageDB: renderPanelClientSide populated ${items.length} rows for '${listName}'`);
            return true;
        } catch (e) {
            console.error('TalonStorageDB: renderPanelClientSide error', e);
            return false;
        }
    },

    // Extract filename from full path
    extractFilename(filePath) {
        if (!filePath) return '';
        const parts = filePath.split(/[\\\/]/);
        return parts[parts.length - 1];
    },

    // Open file in VS Code
    openFileInVSCode(filePath) {
        if (!filePath) return;
        
        // Use VS Code's URL scheme to open the file
        const vscodeUrl = `vscode://file/${filePath.replace(/\\/g, '/')}`;
        
        // Try to open in VS Code
        try {
            window.open(vscodeUrl, '_blank');
        } catch (error) {
            // Fallback: copy path to clipboard and show notification
            navigator.clipboard.writeText(filePath).then(() => {
                alert(`File path copied to clipboard: ${filePath}\n\nTo open in VS Code manually, use: code "${filePath}"`);
            }).catch(() => {
                alert(`Unable to open VS Code automatically. File path: ${filePath}\n\nTo open manually, use: code "${filePath}"`);
            });
        }
    },

    // Simple limited search that returns just an array of commands (no metadata)
    async searchFilteredCommandsSimple(searchParams, maxResults = 500) {
        console.log(`TalonStorageDB: searchFilteredCommandsSimple called with maxResults: ${maxResults}`);
        try {
            const db = await this.ensureDB();
            
            // For "Names Only" and "Search All" scope with search term, use enhanced search with list matching
            if (searchParams.searchTerm && (searchParams.searchScope === 0 || searchParams.searchScope === 2)) {
                return await this.searchWithEnhancedListMatching(searchParams, maxResults, db);
            }
            
            // Otherwise use regular search
            const transaction = db.transaction(['commands'], 'readonly');
            const store = transaction.objectStore('commands');
            
            const filteredCommands = [];
            
            return new Promise((resolve, reject) => {
                const request = store.openCursor();
                
                request.onsuccess = (event) => {
                    const cursor = event.target.result;
                    if (cursor) {
                        const command = cursor.value;
                        
                        // Debug: Log first command structure to understand data format
                        if (filteredCommands.length === 0 && searchParams.searchTerm) {
                            console.log('TalonStorageDB: First command structure:', command);
                            console.log('TalonStorageDB: Command keys:', Object.keys(command || {}));
                        }
                        
                        // Apply filters
                        if (this.matchesFilters(command, searchParams)) {
                            if (filteredCommands.length < maxResults) {
                                filteredCommands.push(command);
                            }
                        }
                        
                        cursor.continue();
                    } else {
                        console.log(`TalonStorageDB: Simple search found ${filteredCommands.length} results (limited to ${maxResults})`);
                        resolve(filteredCommands);
                    }
                };
                
                request.onerror = () => {
                    console.error('TalonStorageDB: Error in simple search:', request.error);
                    reject(request.error);
                };
            });
            
        } catch (error) {
            console.error('TalonStorageDB: Error in searchFilteredCommandsSimple:', error);
            throw error;
        }
    },

    // Store last search results for re-rendering when view mode changes
    lastSearchResults: [],

    // Re-render the current search results when view mode changes
    refreshDisplayMode() {
        if (this.lastSearchResults.length > 0) {
            this.displaySearchResults(this.lastSearchResults);
        }
    },

    // Placeholder for initializeEventListeners - implementation consolidated later in this file.
    initializeEventListeners() {
        // No-op here. The full implementation that attaches handlers and mutation observers
        // lives later in the file to ensure DOM is available when attaching.
    },

    // Allow Blazor to register a DotNetObjectReference so JS-rendered buttons
    // can call instance methods on the Blazor component.
    registerDotNetRef(dotNetRef) {
        try {
            this._dotNetRef = dotNetRef;
            console.log('TalonStorageDB: DotNet reference registered');
        }
        catch (e) {
            console.error('TalonStorageDB: Error registering DotNet reference', e);
        }
    },

    // Called when a JS-rendered list button is clicked. Tries to invoke the
    // Blazor component instance method OpenListInSidePanel(listName).
    async onListButtonClick(listName) {
        try {
            // Quick client-side immediate render: try to find items locally and render
            // them instantly so the user doesn't see a spinner while server operations
            // or IndexedDB lookups complete. We still call the server afterward.
            try {
                const allItemsImmediate = await this.loadLists();
                const immediateItems = this.findItemsForList(allItemsImmediate, listName);
                if (immediateItems && immediateItems.length > 0) {
                    try {
                        const rendered = this.renderPanelClientSide(listName, immediateItems);
                        if (rendered) {
                            console.log(`TalonStorageDB: Immediately rendered ${immediateItems.length} items client-side for '${listName}'`);
                        }
                        // Inform the server about the client-provided items so Blazor can update
                        // its loading state and cache. Do this in background so UI is instant.
                        try {
                            // Ensure server knows a request was made so it sets _lastRequestedPanelListName
                            if (this._dotNetRef && this._dotNetRef.invokeMethodAsync) {
                                this._dotNetRef.invokeMethodAsync('OpenListInSidePanel', listName)
                                    .then(() => this._dotNetRef.invokeMethodAsync('OpenListInSidePanelWithClientData', listName, JSON.stringify(immediateItems)))
                                    .then(() => console.log('TalonStorageDB: OpenListInSidePanel + WithClientData invoked (background)'))
                                    .catch(e => console.debug('TalonStorageDB: OpenListInSidePanel sequence failed', e));
                            } else if (window.DotNet && window.DotNet.invokeMethodAsync) {
                                window.DotNet.invokeMethodAsync('TalonVoiceCommandsServer', 'OpenListInSidePanel', listName)
                                    .then(() => window.DotNet.invokeMethodAsync('TalonVoiceCommandsServer', 'OpenListInSidePanelWithClientData', listName, JSON.stringify(immediateItems)))
                                    .then(() => console.log('TalonStorageDB: static OpenListInSidePanel sequence invoked'))
                                    .catch(e => console.debug('TalonStorageDB: static OpenListInSidePanel sequence failed', e));
                            }
                        } catch (e) {
                            console.debug('TalonStorageDB: background server notify failed', e);
                        }
                    } catch (e) { console.debug('TalonStorageDB: immediate client render failed', e); }
                }
            } catch (e) {
                console.debug('TalonStorageDB: immediate client-side lookup failed', e);
            }

            // Prefer invoking the instance method on the Blazor page if available
            if (this._dotNetRef && this._dotNetRef.invokeMethodAsync) {
                try {
                    await this._dotNetRef.invokeMethodAsync('OpenListInSidePanel', listName);
                    // Give the UI a moment; if the panel remains empty, fall back to sending client data
                    await new Promise(f => setTimeout(f, 200));
                    const panelRows = document.querySelectorAll('[role="dialog"][aria-label="List values panel"] table tbody tr');
                    if (panelRows.length === 0) {
                        // Find client-side items for this list and send to server for population
                        try {
                            const allItems = await this.loadLists();
                            const items = this.findItemsForList(allItems, listName);
                            console.log(`TalonStorageDB: Fallback found ${items.length} client-side items for list '${listName}'`);
                            if (items && items.length > 0) {
                                // Immediately render on the client so the user sees values even if server-side fails.
                                try {
                                    // Try to render immediately on the client so the user sees values even if server-side fails.
                                    try {
                                        const rendered = this.renderPanelClientSide(listName, items);
                                        if (rendered) {
                                            console.log('TalonStorageDB: Rendered panel client-side as immediate fallback');
                                        }
                                    } catch (e) {
                                        console.error('TalonStorageDB: renderPanelClientSide threw', e);
                                    }
                                } catch (e) {
                                    console.error('TalonStorageDB: client-side render fallback failed', e);
                                }

                                // Still attempt to inform the server so it can cache/populate the Blazor panel later.
                                try {
                                    if (this._dotNetRef && this._dotNetRef.invokeMethodAsync) {
                                        console.log('TalonStorageDB: Invoking OpenListInSidePanelWithClientData on DotNet ref (background)');
                                        // Send as JSON string to avoid marshalling issues
                                        this._dotNetRef.invokeMethodAsync('OpenListInSidePanelWithClientData', listName, JSON.stringify(items))
                                            .then(() => console.log('TalonStorageDB: OpenListInSidePanelWithClientData completed'))
                                            .catch(e => console.error('TalonStorageDB: OpenListInSidePanelWithClientData failed', e));
                                    }
                                } catch (e) {
                                    console.error('TalonStorageDB: instance fallback to OpenListInSidePanelWithClientData failed', e);
                                }
                                return;
                            } else {
                                console.debug(`TalonStorageDB: No client-side items matched list '${listName}'`);
                            }
                        } catch (e) {
                            console.error('TalonStorageDB: Error while loading client-side lists for fallback', e);
                        }
                    }
                    return;
                } catch (e) {
                    console.debug('TalonStorageDB: instance invoke failed, will try static fallback', e);
                }
            }

            // Fallback: try static invocation if available (less preferred)
            if (window.DotNet && window.DotNet.invokeMethodAsync) {
                try {
                    await window.DotNet.invokeMethodAsync('TalonVoiceCommandsServer', 'OpenListInSidePanel', listName);
                    return;
                } catch (e) {
                    // ignore and fallback to client-side rendering
                }
            }
            console.warn('TalonStorageDB: No DotNet reference available to open list:', listName);
            // Last resort: try client-side rendering from IndexedDB
            try {
                const allItems = await this.loadLists();
                const items = this.findItemsForList(allItems, listName);
                if (items && items.length > 0) {
                    try { this.renderPanelClientSide(listName, items); } catch(e) { console.error('renderPanelClientSide error', e); }
                    return;
                }
            } catch (e) {
                console.debug('TalonStorageDB: client-side render fallback failed', e);
            }
        }
        catch (e) {
            console.error('TalonStorageDB: Error invoking DotNet to open list:', e);
        }
    },

    // Called when the command title button is clicked. Extracts any list tokens
    // from the command text and opens the first referenced list (or all) with
    // UI feedback (spinner + debug notification).
    async onCommandTitleClick(commandText) {
        try {
            if (!commandText) return;
            // Provide quick debug notification
            this._showTemporaryNotification(`Loading lists for command...`);

            // Show spinner in the side panel while loading
            this._showPanelSpinner(true);

            // Extract list tokens (both <list> and {list})
            const curly = (commandText.match(/\{([^}]+)\}/g) || []).map(s => s.slice(1, -1));
            const angle = (commandText.match(/<([^>]+)>/g) || []).map(s => s.slice(1, -1));
            const listNames = [...curly, ...angle].filter(Boolean);

            if (listNames.length === 0) {
                this._showTemporaryNotification('No list references found in this command.');
                this._showPanelSpinner(false);
                return;
            }

            // Prefer opening the first list to mimic previous behaviour
            const primary = listNames[0];
            console.log('TalonStorageDB: Command title clicked, opening list(s):', listNames);

            // Attempt to open via existing handler
            await this.onListButtonClick(primary);

            // If the panel didn't populate after a short wait, try to fallback to client render
            const ok = await this.waitForPanelRows(3000);
            if (!ok) {
                // Try client-side lookup directly
                try {
                    const all = await this.loadLists();
                    const items = this.findItemsForList(all, primary);
                    if (items && items.length > 0) {
                        // this.renderPanelClientSide(primary, items);
                        this._showTemporaryNotification(`Rendered ${items.length} items client-side for ${primary}`);
                    } else {
                        this._showTemporaryNotification('No items found for ' + primary);
                    }
                } catch (e) {
                    console.error('TalonStorageDB: onCommandTitleClick fallback failed', e);
                    this._showTemporaryNotification('Error loading list');
                }
            }

            this._showPanelSpinner(false);
        } catch (e) {
            console.error('TalonStorageDB: onCommandTitleClick error', e);
            this._showTemporaryNotification('Error loading lists');
            this._showPanelSpinner(false);
        }
    },

    // Small helper to show/hide a spinner inside the list side panel
    _showPanelSpinner(show) {
        try {
            const panel = document.querySelector('[role="dialog"][aria-label="List values panel"]');
            if (!panel) return;
            let spinner = panel.querySelector('.list-panel-spinner');
            if (show) {
                if (!spinner) {
                    spinner = document.createElement('div');
                    spinner.className = 'list-panel-spinner my-2';
                    spinner.innerHTML = '<div class="spinner-border spinner-border-sm" role="status"><span class="visually-hidden">Loading...</span></div>';
                    const body = panel.querySelector('.list-panel-body') || panel;
                    body.insertBefore(spinner, body.firstChild);
                }
                spinner.style.display = '';
            } else {
                if (spinner) spinner.style.display = 'none';
            }
        } catch (e) {
            // ignore
        }
    },

    // Ephemeral notification - uses a small toast at the bottom-right of the page
    _showTemporaryNotification(message, timeout = 3000) {
        try {
            if (!message) return;
            let container = document.getElementById('talon-notification-container');
            if (!container) {
                container = document.createElement('div');
                container.id = 'talon-notification-container';
                container.style.position = 'fixed';
                container.style.right = '12px';
                container.style.bottom = '12px';
                container.style.zIndex = 9999;
                document.body.appendChild(container);
            }
            const el = document.createElement('div');
            el.className = 'toast align-items-center text-white bg-dark border-0 mb-2 p-2';
            el.style.minWidth = '200px';
            el.style.opacity = '0.95';
            el.textContent = message;
            container.appendChild(el);
            setTimeout(() => { try { el.remove(); } catch(e){} }, timeout);
        } catch (e) {
            console.debug('TalonStorageDB: notification failed', e);
        }
    },

    // Utility for tests: wait until the Blazor side panel table has rows for the opened list
    // Resolves to true if rows found within timeout, false otherwise
    async waitForPanelRows(timeoutMs = 5000) {
        const start = Date.now();
    // The side panel is rendered as a div with role="dialog"; use role selector to match
    const selector = '[role="dialog"][aria-label="List values panel"] table tbody tr';
        while (Date.now() - start < timeoutMs) {
            try {
                const el = document.querySelector(selector);
                if (el) return true;
            } catch (e) {
                // ignore
            }
            await new Promise(f => setTimeout(f, 100));
        }
        return false;
    },

    // Debug hook - populated by server via JSRuntime to indicate what list and how many items were loaded
    lastPanelLoad: null,
    _onPanelDataLoaded(info) {
        try {
            this.lastPanelLoad = info;
            console.log('TalonStorageDB: Panel data loaded from server', info);
        } catch (e) {
            console.debug('TalonStorageDB: _onPanelDataLoaded error', e);
        }
    },

    // Limited search that returns at most maxResults commands to prevent SignalR timeouts
    async searchFilteredCommandsLimited(searchParams, maxResults = 500) {
        console.log(`TalonStorageDB: searchFilteredCommandsLimited called with params:`, searchParams, `maxResults: ${maxResults}`);
        try {
            const db = await this.ensureDB();
            const transaction = db.transaction(['commands'], 'readonly');
            const store = transaction.objectStore('commands');
            
            const filteredCommands = [];
            let totalMatches = 0;
            
            return new Promise((resolve, reject) => {
                const request = store.openCursor();
                
                request.onsuccess = (event) => {
                    const cursor = event.target.result;
                    if (cursor) {
                        const command = cursor.value;
                        
                        // Apply filters
                        if (this.matchesFilters(command, searchParams)) {
                            totalMatches++;
                            if (filteredCommands.length < maxResults) {
                                filteredCommands.push(command);
                            }
                        }
                        
                        cursor.continue();
                    } else {
                        console.log(`TalonStorageDB: Limited search found ${totalMatches} total matches, returning ${filteredCommands.length} results`);
                        resolve({
                            commands: filteredCommands,
                            totalMatches: totalMatches,
                            truncated: totalMatches > maxResults
                        });
                    }
                };
                
                request.onerror = () => {
                    console.error('TalonStorageDB: Error in limited search:', request.error);
                    reject(request.error);
                };
            });
            
        } catch (error) {
            console.error('TalonStorageDB: Error in searchFilteredCommandsLimited:', error);
            throw error;
        }
    },

    // Re-render search results when display mode changes
    refreshDisplayMode() {
        if (this.lastSearchResults && this.lastSearchResults.length > 0) {
            console.log('TalonStorageDB: Refreshing display mode for existing results');
            this.displaySearchResults(this.lastSearchResults);
        }
    },

    // Initialize event listeners for checkbox changes
    initializeEventListeners() {
        const showFullCardsCheckbox = document.getElementById('showFullCardsToggle');
        if (showFullCardsCheckbox && !showFullCardsCheckbox.hasAttribute('data-listener-attached')) {
            // Use a named handler so we can avoid re-entrancy if JS updates the checked state programmatically.
            const handler = (e) => {
                try {
                    console.log('TalonStorageDB: Show full cards changed to:', showFullCardsCheckbox.checked);
                    this.refreshDisplayMode();
                } catch (ex) {
                    console.debug('TalonStorageDB: showFullCards change handler error', ex);
                }
            };
            showFullCardsCheckbox.addEventListener('change', handler);
            // Mark as having listener attached to avoid duplicates
            showFullCardsCheckbox.setAttribute('data-listener-attached', 'true');
        }
        // Ensure we attach list button handlers whenever results are re-rendered
        try {
            const resultsContainer = document.querySelector('.search-results-container');
            if (resultsContainer && !resultsContainer.__talon_observer_attached) {
                const observer = new MutationObserver((mutations) => {
                    // When children change, attempt to attach handlers
                    mutations.forEach(() => {
                        try {
                            const listButtons = resultsContainer.querySelectorAll('button');
                            listButtons.forEach(btn => {
                                const text = btn.innerText && btn.innerText.trim();
                                if (text && !btn.__talon_list_handler_attached) {
                                    btn.addEventListener('click', (e) => {
                                        const name = (e.currentTarget.innerText || '').trim();
                                        if (name) {
                                            this.onListButtonClick(name);
                                        }
                                    });
                                    btn.__talon_list_handler_attached = true;
                                }
                            });
                        } catch (e) {
                            console.debug('TalonStorageDB: Mutation observer attach failed', e);
                        }
                    });
                });
                observer.observe(resultsContainer, { childList: true, subtree: true });
                resultsContainer.__talon_observer_attached = true;
                this._resultsMutationObserver = observer;
                console.log('TalonStorageDB: MutationObserver attached to results container');
            }
        } catch (e) {
            console.debug('TalonStorageDB: Failed to attach MutationObserver', e);
        }
    },

    // Get unique filter values directly from IndexedDB without loading all data to Blazor
    async getFilterValues() {
        console.log('TalonStorageDB: Getting filter values directly from IndexedDB...');
        try {
            await this.init();
            
            return new Promise((resolve, reject) => {
                const transaction = this.db.transaction(['commands'], 'readonly');
                const store = transaction.objectStore('commands');
                const request = store.openCursor();
                
                // Use Sets to collect unique values
                const applications = new Set();
                const modes = new Set();
                const operatingSystems = new Set();
                const repositories = new Set();
                const tags = new Set();
                const titles = new Set();
                const codeLanguages = new Set();
                
                request.onsuccess = (event) => {
                    const cursor = event.target.result;
                    if (cursor) {
                        const command = cursor.value;
                        
                        // Collect applications
                        if (command.Application) {
                            applications.add(command.Application);
                        }
                        
                        // Collect modes (split by comma)
                        if (command.Mode) {
                            command.Mode.split(',').forEach(mode => {
                                const trimmedMode = mode.trim();
                                if (trimmedMode && !trimmedMode.startsWith('user.')) {
                                    modes.add(trimmedMode);
                                }
                            });
                        }
                        
                        // Collect operating systems
                        if (command.OperatingSystem) {
                            operatingSystems.add(command.OperatingSystem);
                        }
                        
                        // Collect repositories
                        if (command.Repository) {
                            repositories.add(command.Repository);
                        }
                        
                        // Collect tags (split by comma)
                        if (command.Tags) {
                            command.Tags.split(',').forEach(tag => {
                                const trimmedTag = tag.trim();
                                if (trimmedTag) {
                                    tags.add(trimmedTag);
                                }
                            });
                        }
                        
                        // Collect titles
                        if (command.Title) {
                            titles.add(command.Title);
                        }
                        
                        // Collect code languages (split by comma)
                        if (command.CodeLanguage) {
                            command.CodeLanguage.split(',').forEach(language => {
                                const trimmedLanguage = language.trim();
                                if (trimmedLanguage) {
                                    codeLanguages.add(trimmedLanguage);
                                }
                            });
                        }
                        
                        cursor.continue();
                    } else {
                        // Convert Sets to sorted arrays
                        const filterValues = {
                            applications: Array.from(applications).sort((a, b) => a.localeCompare(b, undefined, { sensitivity: 'base' })),
                            modes: Array.from(modes).sort((a, b) => a.localeCompare(b, undefined, { sensitivity: 'base' })),
                            operatingSystems: Array.from(operatingSystems).sort((a, b) => a.localeCompare(b, undefined, { sensitivity: 'base' })),
                            repositories: Array.from(repositories).sort((a, b) => a.localeCompare(b, undefined, { sensitivity: 'base' })),
                            tags: Array.from(tags).sort((a, b) => a.localeCompare(b, undefined, { sensitivity: 'base' })),
                            titles: Array.from(titles).sort((a, b) => a.localeCompare(b, undefined, { sensitivity: 'base' })),
                            codeLanguages: Array.from(codeLanguages).sort((a, b) => a.localeCompare(b, undefined, { sensitivity: 'base' }))
                        };
                        
                        console.log('TalonStorageDB: Filter values extracted:', {
                            applications: filterValues.applications.length,
                            modes: filterValues.modes.length,
                            operatingSystems: filterValues.operatingSystems.length,
                            repositories: filterValues.repositories.length,
                            tags: filterValues.tags.length,
                            titles: filterValues.titles.length,
                            codeLanguages: filterValues.codeLanguages.length
                        });
                        
                        resolve(filterValues);
                    }
                };
                
                request.onerror = () => {
                    console.error('TalonStorageDB: Error getting filter values:', request.error);
                    reject(request.error);
                };
            });
            
        } catch (error) {
            console.error('TalonStorageDB: Error in getFilterValues:', error);
            throw error;
        }
    },

    // Compute repository and application breakdowns (counts) from the commands store
    async getRepositoryAndApplicationBreakdown(topN = 0) {
        console.log('TalonStorageDB: Computing repository and application breakdowns...');
        try {
            await this.init();

            return new Promise((resolve, reject) => {
                const transaction = this.db.transaction(['commands'], 'readonly');
                const store = transaction.objectStore('commands');
                const request = store.openCursor();

                const repoCounts = new Map();
                const appCounts = new Map();

                request.onsuccess = (event) => {
                    const cursor = event.target.result;
                    if (cursor) {
                        const command = cursor.value;

                        const repo = command.Repository || command.repository || 'None';
                        const app = command.Application || command.application || 'Unknown';

                        repoCounts.set(repo, (repoCounts.get(repo) || 0) + 1);
                        appCounts.set(app, (appCounts.get(app) || 0) + 1);

                        cursor.continue();
                    } else {
                        // Convert maps to sorted arrays
                        const repoArray = Array.from(repoCounts.entries()).map(([k, v]) => ({ repository: k, count: v }));
                        repoArray.sort((a, b) => b.count - a.count || a.repository.localeCompare(b.repository));

                        const appArray = Array.from(appCounts.entries()).map(([k, v]) => ({ application: k, count: v }));
                        appArray.sort((a, b) => b.count - a.count || a.application.localeCompare(b.application));

                        const result = {
                            repositories: repoArray,
                            applications: topN > 0 ? appArray.slice(0, topN) : appArray
                        };

                        console.log('TalonStorageDB: Breakdown computed', { repositories: repoArray.length, applications: appArray.length });
                        resolve(result);
                    }
                };

                request.onerror = () => {
                    console.error('TalonStorageDB: Error computing breakdowns:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('TalonStorageDB: Error in getRepositoryAndApplicationBreakdown:', error);
            throw error;
        }
    },

    // Get a count of total commands and other statistics without loading all data
    async getDataStatistics() {
        console.log('TalonStorageDB: Getting data statistics...');
        try {
            await this.init();
            
            return new Promise((resolve, reject) => {
                const transaction = this.db.transaction(['commands'], 'readonly');
                const store = transaction.objectStore('commands');
                const countRequest = store.count();
                
                countRequest.onsuccess = () => {
                    const totalCommands = countRequest.result;
                    console.log(`TalonStorageDB: Found ${totalCommands} total commands`);
                    
                    resolve({
                        totalCommands: totalCommands,
                        hasData: totalCommands > 0
                    });
                };
                
                countRequest.onerror = () => {
                    console.error('TalonStorageDB: Error getting command count:', countRequest.error);
                    reject(countRequest.error);
                };
            });
            
        } catch (error) {
            console.error('TalonStorageDB: Error in getDataStatistics:', error);
            throw error;
        }
    },

    // Get random commands for idle display
    async getRandomCommands(count = 6) {
        console.log(`TalonStorageDB: Getting ${count} random commands...`);
        try {
            await this.init();
            
            return new Promise((resolve, reject) => {
                const transaction = this.db.transaction(['commands'], 'readonly');
                const store = transaction.objectStore('commands');
                
                // First, get the total count
                const countRequest = store.count();
                
                countRequest.onsuccess = () => {
                    const totalCommands = countRequest.result;
                    
                    if (totalCommands === 0) {
                        console.log('TalonStorageDB: No commands available for random selection');
                        resolve([]);
                        return;
                    }
                    
                    // Generate random indices to select
                    const randomIndices = new Set();
                    const maxToSelect = Math.min(count, totalCommands);
                    
                    while (randomIndices.size < maxToSelect) {
                        randomIndices.add(Math.floor(Math.random() * totalCommands));
                    }
                    
                    const selectedCommands = [];
                    const sortedIndices = Array.from(randomIndices).sort((a, b) => a - b);
                    let currentIndex = 0;
                    let targetIndex = 0;
                    
                    const cursorRequest = store.openCursor();
                    cursorRequest.onsuccess = (event) => {
                        const cursor = event.target.result;
                        
                        if (!cursor || targetIndex >= sortedIndices.length) {
                            // Finished or no more commands
                            console.log(`TalonStorageDB: Selected ${selectedCommands.length} random commands`);
                            resolve(selectedCommands);
                            return;
                        }
                        
                        if (currentIndex === sortedIndices[targetIndex]) {
                            // This is one of our random selections
                            selectedCommands.push(cursor.value);
                            targetIndex++;
                        }
                        
                        currentIndex++;
                        cursor.continue();
                    };
                    
                    cursorRequest.onerror = () => {
                        console.error('TalonStorageDB: Error getting random commands:', cursorRequest.error);
                        reject(cursorRequest.error);
                    };
                };
                
                countRequest.onerror = () => {
                    console.error('TalonStorageDB: Error getting command count for random selection:', countRequest.error);
                    reject(countRequest.error);
                };
            });
            
        } catch (error) {
            console.error('TalonStorageDB: Error in getRandomCommands:', error);
            throw error;
        }
    }
};

// Make available globally for Blazor JSInterop
window.TalonStorageDB = TalonStorageDB;

// Helper: open first list referenced by the last JS search results
window.TalonStorageDB.openFirstReferencedList = async function () {
    try {
        const results = window.TalonStorageDB.lastSearchResults || [];
        for (const cmd of results) {
            const matches = (cmd.Command || '').match(/\{([^}]+)\}|<([^>]+)>/g) || [];
            if (matches.length > 0) {
                // pick the first match and strip braces
                const raw = matches[0];
                const name = raw.replace(/[{}<>]/g, '').trim();
                if (name) {
                    await window.TalonStorageDB.onListButtonClick(name);
                    return { opened: true, listName: name };
                }
            }
        }
        return { opened: false, reason: 'no referenced lists in lastSearchResults' };
    } catch (e) {
        console.error('openFirstReferencedList error', e);
        return { opened: false, error: e?.toString() };
    }
};