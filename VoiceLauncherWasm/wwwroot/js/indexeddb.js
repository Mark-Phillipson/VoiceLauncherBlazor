window.voiceIndexedDB = {
    db: null,
    dbName: '',
    version: 1,

    initialize: function (databaseName, version) {
        return new Promise((resolve, reject) => {
            this.dbName = databaseName;
            this.version = version;

            const request = indexedDB.open(databaseName, version);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => {
                this.db = request.result;
                resolve();
            };

            request.onupgradeneeded = (event) => {
                const db = event.target.result;

                // Create TalonVoiceCommands store
                if (!db.objectStoreNames.contains('TalonVoiceCommands')) {
                    const talonCommandsStore = db.createObjectStore('TalonVoiceCommands', { keyPath: 'id', autoIncrement: true });
                    talonCommandsStore.createIndex('command', 'command', { unique: false });
                    talonCommandsStore.createIndex('application', 'application', { unique: false });
                    talonCommandsStore.createIndex('repository', 'repository', { unique: false });
                    talonCommandsStore.createIndex('createdAt', 'createdAt', { unique: false });
                }

                // Create TalonLists store
                if (!db.objectStoreNames.contains('TalonLists')) {
                    const talonListsStore = db.createObjectStore('TalonLists', { keyPath: 'id', autoIncrement: true });
                    talonListsStore.createIndex('listName', 'listName', { unique: false });
                    talonListsStore.createIndex('spokenForm', 'spokenForm', { unique: false });
                    talonListsStore.createIndex('createdAt', 'createdAt', { unique: false });
                }
            };
        });
    },

    getAll: function (storeName) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized'));
                return;
            }

            const transaction = this.db.transaction([storeName], 'readonly');
            const store = transaction.objectStore(storeName);
            const request = store.getAll();

            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve(JSON.stringify(request.result));
        });
    },

    getById: function (storeName, id) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized'));
                return;
            }

            const transaction = this.db.transaction([storeName], 'readonly');
            const store = transaction.objectStore(storeName);
            const request = store.get(id);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve(request.result ? JSON.stringify(request.result) : null);
        });
    },

    add: function (storeName, itemJson) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized'));
                return;
            }

            const item = JSON.parse(itemJson);
            const transaction = this.db.transaction([storeName], 'readwrite');
            const store = transaction.objectStore(storeName);
            const request = store.add(item);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve(request.result);
        });
    },

    update: function (storeName, itemJson) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized'));
                return;
            }

            const item = JSON.parse(itemJson);
            const transaction = this.db.transaction([storeName], 'readwrite');
            const store = transaction.objectStore(storeName);
            const request = store.put(item);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve(request.result);
        });
    },

    delete: function (storeName, id) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized'));
                return;
            }

            const transaction = this.db.transaction([storeName], 'readwrite');
            const store = transaction.objectStore(storeName);
            const request = store.delete(id);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve();
        });
    },

    clear: function (storeName) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized'));
                return;
            }

            const transaction = this.db.transaction([storeName], 'readwrite');
            const store = transaction.objectStore(storeName);
            const request = store.clear();

            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve();
        });
    },

    query: function (storeName, indexName, keyRange) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized'));
                return;
            }

            const transaction = this.db.transaction([storeName], 'readonly');
            const store = transaction.objectStore(storeName);
            const index = store.index(indexName);
            const request = index.getAll(keyRange);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve(JSON.stringify(request.result));
        });
    },

    count: function (storeName) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized'));
                return;
            }

            const transaction = this.db.transaction([storeName], 'readonly');
            const store = transaction.objectStore(storeName);
            const request = store.count();

            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve(request.result);
        });
    }
};