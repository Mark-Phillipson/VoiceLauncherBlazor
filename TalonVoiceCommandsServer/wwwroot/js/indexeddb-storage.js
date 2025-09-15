// IndexedDB storage wrapper for Talon Voice Commands
// Provides high-capacity storage for large datasets that exceed localStorage limits

const TalonStorageDB = {
    dbName: 'TalonVoiceCommandsV2', // Changed database name to force fresh creation
    version: 1, // Reset to version 1 for new database
    db: null,

    // Initialize IndexedDB
    async init() {
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
        const showFullCards = commands.length <= 6 ? true : (showFullCardsCheckbox ? showFullCardsCheckbox.checked : false);

        // Create results HTML using a responsive two-column grid (Bootstrap)
        // Header spans full width, each result becomes a column (col-12 on xs, col-md-6 on md+)
        let html = `<div class="search-results">`;
        html += `<div class="results-header mb-3"><strong>Found ${commands.length} results</strong></div>`;
        html += `<div class="row">`;

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
                            <h6 class="card-title mb-2">“${this.escapeHtml(command.Command || 'No command')}”</h6>
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
                                    <strong class="me-2 text-truncate" style="max-width: 200px;" title=“${this.escapeHtml(command.Command || 'No command')}”>
                                        ${this.escapeHtml(command.Command || 'No command')}
                                    </strong>
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
    },

    // Utility function to escape HTML
    escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
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

    // Initialize event listeners for the show full cards checkbox
    initializeEventListeners() {
        const showFullCardsCheckbox = document.getElementById('showFullCardsToggle');
        if (showFullCardsCheckbox) {
            showFullCardsCheckbox.addEventListener('change', () => {
                this.refreshDisplayMode();
            });
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
            showFullCardsCheckbox.addEventListener('change', () => {
                console.log('TalonStorageDB: Show full cards changed to:', showFullCardsCheckbox.checked);
                this.refreshDisplayMode();
            });
            // Mark as having listener attached to avoid duplicates
            showFullCardsCheckbox.setAttribute('data-listener-attached', 'true');
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
    }
};

// Make available globally for Blazor JSInterop
window.TalonStorageDB = TalonStorageDB;