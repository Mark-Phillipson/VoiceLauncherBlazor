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

    // Helper function to check if a command matches the search filters
    matchesFilters(command, searchParams) {
        // Apply field filters first
        if (searchParams.application && command.Application !== searchParams.application) return false;
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
            const searchTerm = searchParams.searchTerm.toLowerCase();
            
            if (searchParams.useSemanticMatching) {
                // Simple semantic search - check both command and script
                return (command.Command && command.Command.toLowerCase().includes(searchTerm)) ||
                       (command.Script && command.Script.toLowerCase().includes(searchTerm));
            } else {
                // Apply scope-based search
                switch (searchParams.searchScope) {
                    case 0: // CommandNamesOnly
                        return command.Command && command.Command.toLowerCase().includes(searchTerm);
                    case 1: // ScriptOnly
                        return command.Script && command.Script.toLowerCase().includes(searchTerm);
                    case 2: // All
                        return (command.Command && command.Command.toLowerCase().includes(searchTerm)) ||
                               (command.Script && command.Script.toLowerCase().includes(searchTerm));
                    default:
                        return command.Command && command.Command.toLowerCase().includes(searchTerm);
                }
            }
        }
        
        return true; // No text search, just filter-based
    },

    // Search and directly display results in the UI (no C# transfer needed)
    async searchAndDisplayResults(searchParams, maxResults = 500) {
        console.log(`TalonStorageDB: searchAndDisplayResults called with maxResults: ${maxResults}`);
        try {
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
        
        // Find the results container in the UI
        const resultsContainer = document.querySelector('.search-results-container');
        if (!resultsContainer) {
            console.warn('TalonStorageDB: Results container not found');
            return;
        }

        // Clear existing results
        resultsContainer.innerHTML = '';

        if (commands.length === 0) {
            resultsContainer.innerHTML = '<div class="alert alert-info">No results found.</div>';
            return;
        }

        // Create results HTML using a responsive two-column grid (Bootstrap)
        // Header spans full width, each result becomes a column (col-12 on xs, col-md-6 on md+)
        let html = `<div class="search-results">`;
        html += `<div class="results-header mb-3"><strong>Found ${commands.length} results</strong></div>`;
        html += `<div class="row">`;

        commands.forEach((command, index) => {
            html += `
                <div class="col-12 col-md-6 mb-3">
                    <div class="result-item card h-100" data-command-id="${command.Id || index}">
                        <div class="card-body d-flex flex-column">
                            <h6 class="card-title">${this.escapeHtml(command.Command || 'No command')}</h6>
                            <p class="card-text"><small class="text-muted">App: ${this.escapeHtml(command.Application || 'N/A')}</small></p>
                            <pre class="script-content mb-2">${this.escapeHtml(command.Script || 'No script')}</pre>
                            <div class="mt-auto command-details">
                                <small class="text-muted d-block">
                                    ${command.Mode ? `Mode: ${this.escapeHtml(command.Mode)} | ` : ''}
                                    ${command.Repository ? `Repo: ${this.escapeHtml(command.Repository)} | ` : ''}
                                    ${command.FilePath ? `File: ${this.escapeHtml(command.FilePath)}` : ''}
                                </small>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        });

        html += `</div>`; // .row
        html += `</div>`; // .search-results
        resultsContainer.innerHTML = html;
    },

    // Utility function to escape HTML
    escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
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
    }
};

// Make available globally for Blazor JSInterop
window.TalonStorageDB = TalonStorageDB;