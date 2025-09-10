let db;
const DB_NAME = 'VoiceLauncherDB';
const DB_VERSION = 1;

export async function initializeDatabase() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);
        
        request.onerror = () => reject(request.error);
        request.onsuccess = () => {
            db = request.result;
            resolve(db);
        };
        
        request.onupgradeneeded = (event) => {
            db = event.target.result;
            
            // Create commands store
            if (!db.objectStoreNames.contains('commands')) {
                const commandStore = db.createObjectStore('commands', { keyPath: 'id', autoIncrement: true });
                commandStore.createIndex('voiceCommand', 'voiceCommand', { unique: false });
                commandStore.createIndex('application', 'application', { unique: false });
                commandStore.createIndex('repository', 'repository', { unique: false });
            }
            
            // Create lists store
            if (!db.objectStoreNames.contains('lists')) {
                const listStore = db.createObjectStore('lists', { keyPath: 'id', autoIncrement: true });
                listStore.createIndex('listName', 'listName', { unique: false });
                listStore.createIndex('repository', 'repository', { unique: false });
            }
        };
    });
}

export async function getAllCommands() {
    if (!db) await initializeDatabase();
    
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(['commands'], 'readonly');
        const store = transaction.objectStore('commands');
        const request = store.getAll();
        
        request.onerror = () => reject(request.error);
        request.onsuccess = () => resolve(JSON.stringify(request.result));
    });
}

export async function getAllLists() {
    if (!db) await initializeDatabase();
    
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(['lists'], 'readonly');
        const store = transaction.objectStore('lists');
        const request = store.getAll();
        
        request.onerror = () => reject(request.error);
        request.onsuccess = () => resolve(JSON.stringify(request.result));
    });
}

export async function addCommand(commandJson) {
    if (!db) await initializeDatabase();
    
    const command = JSON.parse(commandJson);
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(['commands'], 'readwrite');
        const store = transaction.objectStore('commands');
        const request = store.add(command);
        
        request.onerror = () => reject(request.error);
        request.onsuccess = () => resolve(request.result);
    });
}

export async function addList(listJson) {
    if (!db) await initializeDatabase();
    
    const list = JSON.parse(listJson);
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(['lists'], 'readwrite');
        const store = transaction.objectStore('lists');
        const request = store.add(list);
        
        request.onerror = () => reject(request.error);
        request.onsuccess = () => resolve(request.result);
    });
}

export async function clearAllData() {
    if (!db) await initializeDatabase();
    
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(['commands', 'lists'], 'readwrite');
        
        const commandStore = transaction.objectStore('commands');
        const listStore = transaction.objectStore('lists');
        
        commandStore.clear();
        listStore.clear();
        
        transaction.oncomplete = () => resolve();
        transaction.onerror = () => reject(transaction.error);
    });
}