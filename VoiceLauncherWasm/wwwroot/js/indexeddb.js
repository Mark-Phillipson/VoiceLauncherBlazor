let db;
let lastError = null;
const DB_NAME = 'VoiceLauncherDB';
// DB_VERSION bumped to 3 after successful migration - keep in sync with latest on-disk version
const DB_VERSION = 3;
// Broadcast channel used to notify other windows/tabs to close their DB connections
let bc = null;
try {
    if (typeof BroadcastChannel !== 'undefined') {
        bc = new BroadcastChannel('VoiceLauncherDB');
        bc.onmessage = (ev) => {
            try {
                if (ev && ev.data === 'close') {
                    console.info('[indexeddb] BroadcastChannel: received close request, closing local DB if open');
                    try { if (db) { db.close(); db = null; } } catch (e) { console.warn('[indexeddb] error closing db from BC', e); }
                } else if (ev && ev.data && typeof ev.data === 'object') {
                    // structured message from other context
                    try { console.info('[indexeddb] BroadcastChannel: structured message', ev.data); } catch (e) { }
                }
            } catch (e) { console.warn('[indexeddb] BroadcastChannel message handler error', e); }
        };
    }

} catch (e) {
    console.warn('[indexeddb] BroadcastChannel not available or failed to initialize', e);
}

// Client identifier for this window/context (used for diagnostics)
const CLIENT_ID = (typeof crypto !== 'undefined' && crypto.randomUUID) ? crypto.randomUUID() : ('c-' + Date.now() + '-' + Math.floor(Math.random() * 10000));

function broadcastEvent(obj) {
    try {
        const payload = Object.assign({ type: 'unknown', clientId: CLIENT_ID, ua: (navigator && navigator.userAgent) ? navigator.userAgent : '', href: (typeof location !== 'undefined' ? location.href : ''), ts: new Date().toISOString() }, obj || {});
        try { console.info('[indexeddb] bc-out', payload); } catch (e) { }
        if (bc) {
            try { bc.postMessage(payload); } catch (e) { console.warn('[indexeddb] bc.postMessage failed', e); }
        }
    } catch (e) { console.warn('[indexeddb] broadcastEvent failed', e); }
}

// Normalization helpers usable across the module so reads always return the expected PascalCase model
function normalizeCommandItem(item) {
    if (!item) return item;
    // Copy to avoid mutating original result object from IndexedDB
    const copy = Object.assign({}, item);
    if (copy.voiceCommand && !copy.Command) copy.Command = copy.voiceCommand;
    if (copy.VoiceCommand && !copy.Command) copy.Command = copy.VoiceCommand;
    if (copy.talonScript && !copy.Script) copy.Script = copy.talonScript;
    if (copy.TalonScript && !copy.Script) copy.Script = copy.TalonScript;
    if (copy.filePath && !copy.FileName) copy.FileName = copy.filePath;
    if (copy.FilePath && !copy.FileName) copy.FileName = copy.FilePath;
    if (!copy.DateCreated) copy.DateCreated = new Date().toISOString();
    // Normalize id -> Id
    if (copy.id && !copy.Id) { copy.Id = copy.id; delete copy.id; }
    return copy;
}

function normalizeListItem(item) {
    if (!item) return item;
    const copy = Object.assign({}, item);
    if (copy.listName && !copy.ListName) copy.ListName = copy.listName;
    if (copy.repository && !copy.Repository) copy.Repository = copy.repository;
    if (!copy.DateCreated) copy.DateCreated = new Date().toISOString();
    if (copy.id && !copy.Id) { copy.Id = copy.id; delete copy.id; }
    return copy;
}

// Top-level upgrade handler so it can be invoked from initialize or a forced upgrade
function handleUpgrade(event) {
    db = event.target.result;
    const tx = event.target.transaction;
    console.info('[indexeddb] onupgradeneeded: upgrading DB to v', db.version);
    // reuse module-level normalization helpers when migrating

    if (db.objectStoreNames.contains('commands')) {
        try {
            const oldStore = tx.objectStore('commands');
            const needsRecreate = (oldStore.keyPath !== 'Id');
            if (needsRecreate) {
                console.info('[indexeddb] onupgradeneeded: detected legacy commands store, migrating...');
                const temp = [];
                const cursorReq = oldStore.openCursor();
                cursorReq.onsuccess = function (e) {
                    const cursor = e.target.result;
                    if (cursor) { temp.push(cursor.value); cursor.continue(); }
                    else {
                        try { db.deleteObjectStore('commands'); } catch (e) { }
                        const newStore = db.createObjectStore('commands', { keyPath: 'Id', autoIncrement: true });
                        newStore.createIndex('Command', 'Command', { unique: false });
                        newStore.createIndex('Application', 'Application', { unique: false });
                        newStore.createIndex('Repository', 'Repository', { unique: false });

                        const writeStore = tx.objectStore('commands');
                        temp.forEach(item => {
                            try { writeStore.add(normalizeCommandItem(item)); } catch (err) { console.error('[indexeddb] migration add error', err); lastError = err ? err.toString() : lastError; }
                        });
                        console.info('[indexeddb] onupgradeneeded: commands migration complete, migrated=', temp.length);
                    }
                };
            } else {
                try {
                    const store = tx.objectStore('commands');
                    try { store.createIndex('Command', 'Command', { unique: false }); } catch (e) { }
                    try { store.createIndex('Application', 'Application', { unique: false }); } catch (e) { }
                    try { store.createIndex('Repository', 'Repository', { unique: false }); } catch (e) { }
                } catch (e) { }
            }
        } catch (e) {
            console.warn('[indexeddb] onupgradeneeded: error inspecting commands store', e);
        }
    } else {
        const commandStore = db.createObjectStore('commands', { keyPath: 'Id', autoIncrement: true });
        commandStore.createIndex('Command', 'Command', { unique: false });
        commandStore.createIndex('Application', 'Application', { unique: false });
        commandStore.createIndex('Repository', 'Repository', { unique: false });
        console.info('[indexeddb] onupgradeneeded: created new commands store');
    }

    if (db.objectStoreNames.contains('lists')) {
        try {
            const oldListStore = tx.objectStore('lists');
            const needsRecreate = (oldListStore.keyPath !== 'Id');
            if (needsRecreate) {
                console.info('[indexeddb] onupgradeneeded: detected legacy lists store, migrating...');
                const temp = [];
                const cursorReq = oldListStore.openCursor();
                cursorReq.onsuccess = function (e) {
                    const cursor = e.target.result;
                    if (cursor) { temp.push(cursor.value); cursor.continue(); }
                    else {
                        try { db.deleteObjectStore('lists'); } catch (e) { }
                        const newListStore = db.createObjectStore('lists', { keyPath: 'Id', autoIncrement: true });
                        newListStore.createIndex('ListName', 'ListName', { unique: false });
                        newListStore.createIndex('Repository', 'Repository', { unique: false });

                        const writeStore = tx.objectStore('lists');
                        temp.forEach(item => {
                            try { writeStore.add(normalizeListItem(item)); } catch (err) { console.error('[indexeddb] lists migration add error', err); lastError = err ? err.toString() : lastError; }
                        });
                        console.info('[indexeddb] onupgradeneeded: lists migration complete, migrated=', temp.length);
                    }
                };
            } else {
                try {
                    const store = tx.objectStore('lists');
                    try { store.createIndex('ListName', 'ListName', { unique: false }); } catch (e) { }
                    try { store.createIndex('Repository', 'Repository', { unique: false }); } catch (e) { }
                } catch (e) { }
            }
        } catch (e) {
            console.warn('[indexeddb] onupgradeneeded: error inspecting lists store', e);
        }
    } else {
        const listStore = db.createObjectStore('lists', { keyPath: 'Id', autoIncrement: true });
        listStore.createIndex('ListName', 'ListName', { unique: false });
        listStore.createIndex('Repository', 'Repository', { unique: false });
        console.info('[indexeddb] onupgradeneeded: created new lists store');
    }
}

export async function initializeDatabase() {
    return new Promise((resolve, reject) => {
        console.log('[indexeddb] initializeDatabase: opening', DB_NAME, 'v', DB_VERSION);
        try { broadcastEvent({ type: 'initialize-start', requestedVersion: DB_VERSION }); } catch (e) { }

    // Open without a requested version to avoid VersionError if the on-disk DB has a higher version.
    const request = indexedDB.open(DB_NAME);

        request.onerror = () => {
            console.error('[indexeddb] initializeDatabase error', request.error);
            lastError = request.error ? request.error.toString() : 'initializeDatabase error';
            reject(request.error);
        };

        request.onupgradeneeded = handleUpgrade;

        request.onsuccess = () => {
            db = request.result;
            console.log('[indexeddb] initializeDatabase: opened', db.name, 'v', db.version, 'stores=', Array.from(db.objectStoreNames));
            try { broadcastEvent({ type: 'initialize-success', version: db.version, stores: Array.from(db.objectStoreNames) }); } catch (e) { }

            // Detect legacy schema even when DB version didn't change and trigger a version bump upgrade if needed
            try {
                let needsMigration = false;
                if (db.objectStoreNames.contains('commands')) {
                    const tx = db.transaction(['commands'], 'readonly');
                    const store = tx.objectStore('commands');
                    if (store.keyPath !== 'Id') needsMigration = true;
                }
                if (!needsMigration && db.objectStoreNames.contains('lists')) {
                    const tx2 = db.transaction(['lists'], 'readonly');
                    const store2 = tx2.objectStore('lists');
                    if (store2.keyPath !== 'Id') needsMigration = true;
                }

                if (needsMigration) {
                    console.info('[indexeddb] initializeDatabase: legacy schema detected, closing DB and reopening to trigger migration');
                    db.close();
                    const desiredVersion = Math.max(db.version + 1, DB_VERSION);
                    console.info('[indexeddb] initializeDatabase: reopening for upgrade to v', desiredVersion);
                    const upgradeReq = indexedDB.open(DB_NAME, desiredVersion);
                    upgradeReq.onerror = () => { console.error('[indexeddb] upgrade reopen error', upgradeReq.error); lastError = upgradeReq.error ? upgradeReq.error.toString() : lastError; reject(upgradeReq.error); };
                    upgradeReq.onupgradeneeded = handleUpgrade;
                    upgradeReq.onsuccess = () => { db = upgradeReq.result; console.info('[indexeddb] initializeDatabase: reopened after upgrade, v', db.version); resolve(db); };
                    return;
                }
            } catch (e) {
                console.warn('[indexeddb] initializeDatabase: error detecting legacy schema', e);
            }

            resolve(db);
        };
    });
}

export async function getAllCommands() {
    if (!db) await initializeDatabase();
    console.log('[indexeddb] getAllCommands');
    return new Promise((resolve, reject) => {
        try {
            const transaction = db.transaction(['commands'], 'readonly');
            const store = transaction.objectStore('commands');
            const request = store.getAll();
            request.onerror = () => {
                console.error('[indexeddb] getAllCommands error', request.error);
                lastError = request.error ? request.error.toString() : lastError;
                reject(request.error);
            };
            request.onsuccess = () => {
                console.log('[indexeddb] getAllCommands success count=', request.result.length);
                try {
                    const normalized = request.result.map(r => normalizeCommandItem(r));
                    resolve(JSON.stringify(normalized));
                } catch (e) {
                    console.warn('[indexeddb] getAllCommands normalization failed', e);
                    resolve(JSON.stringify(request.result));
                }
            };
        } catch (ex) {
            console.error('[indexeddb] getAllCommands exception', ex);
            lastError = ex ? ex.toString() : lastError;
            reject(ex);
        }
    });
}

export async function getAllLists() {
    if (!db) await initializeDatabase();
    console.log('[indexeddb] getAllLists');
    return new Promise((resolve, reject) => {
        try {
            const transaction = db.transaction(['lists'], 'readonly');
            const store = transaction.objectStore('lists');
            const request = store.getAll();
            request.onerror = () => {
                console.error('[indexeddb] getAllLists error', request.error);
                reject(request.error);
            };
            request.onsuccess = () => {
                console.log('[indexeddb] getAllLists success count=', request.result.length);
                try {
                    const normalized = request.result.map(r => normalizeListItem(r));
                    resolve(JSON.stringify(normalized));
                } catch (e) {
                    console.warn('[indexeddb] getAllLists normalization failed', e);
                    resolve(JSON.stringify(request.result));
                }
            };
        } catch (ex) {
            console.error('[indexeddb] getAllLists exception', ex);
            reject(ex);
        }
    });
}

export async function addCommand(commandJson) {
    if (!db) await initializeDatabase();
    console.log('[indexeddb] addCommand');
    const command = JSON.parse(commandJson);
    // Normalize known legacy property names to current PascalCase model
    if (command.voiceCommand && !command.Command) command.Command = command.voiceCommand;
    if (command.VoiceCommand && !command.Command) command.Command = command.VoiceCommand;
    if (command.talonScript && !command.Script) command.Script = command.talonScript;
    if (command.TalonScript && !command.Script) command.Script = command.TalonScript;
    if (command.filePath && !command.FileName) command.FileName = command.filePath;
    if (command.FilePath && !command.FileName) command.FileName = command.FilePath;
    if (!command.DateCreated) command.DateCreated = new Date().toISOString();
    return new Promise((resolve, reject) => {
        try {
            const transaction = db.transaction(['commands'], 'readwrite');
            const store = transaction.objectStore('commands');
            // Ensure no 'id' is present (use PascalCase Id for autoIncrement)
            if (command.id && !command.Id) {
                command.Id = command.id;
                delete command.id;
            }
            const request = store.add(command);
            request.onerror = () => { console.error('[indexeddb] addCommand error', request.error, command); reject(request.error); };
            request.onsuccess = () => { console.log('[indexeddb] addCommand success id=', request.result); resolve(request.result); };
        } catch (ex) {
            console.error('[indexeddb] addCommand exception', ex, command);
            reject(ex);
        }
    });
}

export async function addList(listJson) {
    if (!db) await initializeDatabase();
    console.log('[indexeddb] addList');
    const list = JSON.parse(listJson);
    return new Promise((resolve, reject) => {
        try {
            const transaction = db.transaction(['lists'], 'readwrite');
            const store = transaction.objectStore('lists');
            if (list.id && !list.Id) { list.Id = list.id; delete list.id; }
            const request = store.add(list);
            request.onerror = () => { console.error('[indexeddb] addList error', request.error, list); reject(request.error); };
            request.onsuccess = () => { console.log('[indexeddb] addList success id=', request.result); resolve(request.result); };
        } catch (ex) {
            console.error('[indexeddb] addList exception', ex, list);
            reject(ex);
        }
    });
}

export async function clearAllData() {
    if (!db) await initializeDatabase();
    console.log('[indexeddb] clearAllData');
    return new Promise((resolve, reject) => {
        try {
            const transaction = db.transaction(['commands', 'lists'], 'readwrite');
            const commandStore = transaction.objectStore('commands');
            const listStore = transaction.objectStore('lists');
            commandStore.clear();
            listStore.clear();
            transaction.oncomplete = () => { console.log('[indexeddb] clearAllData complete'); resolve(); };
            transaction.onerror = () => { console.error('[indexeddb] clearAllData error', transaction.error); reject(transaction.error); };
        } catch (ex) {
            console.error('[indexeddb] clearAllData exception', ex);
            reject(ex);
        }
    });
}

export function getLastError() {
    return lastError;
}

export async function deleteDatabase() {
    return new Promise((resolve, reject) => {
        try {
            console.info('[indexeddb] deleteDatabase:', DB_NAME);
            try { broadcastEvent({ type: 'delete-start' }); } catch (e) { }

            // Close our open connection first to avoid blocking the delete request
            try {
                if (db) {
                    console.info('[indexeddb] deleteDatabase: closing local DB connection before delete');
                    try { db.close(); } catch (e) { console.warn('[indexeddb] error closing db before delete', e); }
                    db = null;
                }
            } catch (e) {
                console.warn('[indexeddb] deleteDatabase: error while closing local db', e);
            }

            // Notify other tabs/windows to close their DB connections as well
            try { if (bc) { bc.postMessage('close'); console.info('[indexeddb] deleteDatabase: broadcasted close to other contexts'); } } catch (e) { console.warn('[indexeddb] deleteDatabase: broadcast failed', e); }

            const delReq = indexedDB.deleteDatabase(DB_NAME);
            let settled = false;
            delReq.onsuccess = function () { if (!settled) { settled = true; db = null; resolve(true); } };
            delReq.onerror = function (e) { if (!settled) { settled = true; lastError = e && e.target && e.target.error ? e.target.error.toString() : 'deleteDatabase error'; reject(e); } };
            delReq.onblocked = function () {
                console.warn('[indexeddb] deleteDatabase blocked');
                lastError = 'deleteDatabase blocked';
                try { broadcastEvent({ type: 'delete-blocked' }); } catch (e) { }

                // resolve false so callers can proceed and avoid hanging awaiting delete
                if (!settled) { settled = true; resolve(false); }
            };

            // Safety fallback: if nothing settles the promise within a short window, resolve false
            setTimeout(() => { if (!settled) { settled = true; lastError = 'deleteDatabase timed out'; console.warn('[indexeddb] deleteDatabase timed out'); resolve(false); } }, 8000);
        } catch (ex) { lastError = ex ? ex.toString() : lastError; reject(ex); }
    });
}

// Force an in-place upgrade by opening the DB at a higher version to trigger onupgradeneeded.
export async function forceUpgrade() {
    return new Promise((resolve, reject) => {
        try {
            console.info('[indexeddb] forceUpgrade: attempting to force DB upgrade');
            try { if (db) { db.close(); db = null; } } catch (e) { console.warn('[indexeddb] forceUpgrade: error closing db', e); }
            // Ask other contexts to close DB connections
            try { if (bc) { bc.postMessage('close'); console.info('[indexeddb] forceUpgrade: broadcasted close to other contexts'); } } catch (e) { console.warn('[indexeddb] forceUpgrade: broadcast failed', e); }
            // Find the current version by opening without a version, then request current+1 to trigger upgrade.
            const peek = indexedDB.open(DB_NAME);
            let peekSettled = false;
            const peekTimeout = setTimeout(() => { if (!peekSettled) { peekSettled = true; lastError = 'forceUpgrade peek timed out'; console.warn('[indexeddb] forceUpgrade peek timed out'); reject(new Error('forceUpgrade peek timed out')); } }, 5000);
            peek.onerror = () => { if (!peekSettled) { peekSettled = true; clearTimeout(peekTimeout); lastError = peek.error ? peek.error.toString() : 'forceUpgrade peek error'; console.error('[indexeddb] forceUpgrade peek error', peek.error); reject(peek.error); } };
            peek.onsuccess = () => {
                if (peekSettled) return;
                peekSettled = true; clearTimeout(peekTimeout);
                try {
                    const currentVersion = peek.result ? peek.result.version : DB_VERSION;
                    try { peek.result.close(); } catch (e) { }
                    const targetVersion = Math.max((currentVersion || DB_VERSION) + 1, DB_VERSION);
                    console.info('[indexeddb] forceUpgrade: requesting open at v', targetVersion);
                    try { broadcastEvent({ type: 'forceUpgrade-requested', targetVersion: targetVersion }); } catch (e) { }
                    const req = indexedDB.open(DB_NAME, targetVersion);
                    // If the upgrade is blocked by other open connections, handle onblocked and timeout to avoid hanging
                    let reqSettled = false;
                    const reqTimeout = setTimeout(() => { if (!reqSettled) { reqSettled = true; lastError = 'forceUpgrade timed out'; console.warn('[indexeddb] forceUpgrade timed out'); try { if (req.result) { req.result.close(); } } catch (e) {} resolve(false); } }, 8000);
                    req.onerror = () => { if (!reqSettled) { reqSettled = true; clearTimeout(reqTimeout); lastError = req.error ? req.error.toString() : 'forceUpgrade error'; console.error('[indexeddb] forceUpgrade error', req.error); reject(req.error); } };
                    req.onblocked = () => { if (!reqSettled) { reqSettled = true; clearTimeout(reqTimeout); lastError = 'forceUpgrade blocked'; console.warn('[indexeddb] forceUpgrade blocked'); try { broadcastEvent({ type: 'forceUpgrade-blocked' }); } catch (e) { } resolve(false); } };
                    req.onupgradeneeded = handleUpgrade;
                    req.onsuccess = () => { if (!reqSettled) { reqSettled = true; clearTimeout(reqTimeout); db = req.result; console.info('[indexeddb] forceUpgrade: success, v', db.version); try { broadcastEvent({ type: 'forceUpgrade-success', version: db.version }); } catch (e) { } resolve(true); } };
                } catch (ex) { lastError = ex ? ex.toString() : lastError; reject(ex); }
            };
        } catch (ex) { lastError = ex ? ex.toString() : lastError; reject(ex); }
    });
}

export async function forceCleanup() {
    return new Promise(async (resolve) => {
        try {
            console.info('[indexeddb] forceCleanup: unregistering service workers and clearing caches');
            // Unregister service workers
            try {
                if (navigator && navigator.serviceWorker) {
                    const regs = await navigator.serviceWorker.getRegistrations();
                    for (const r of regs) {
                        try { await r.unregister(); console.info('[indexeddb] service worker unregistered'); } catch (e) { console.warn('[indexeddb] sw unregister failed', e); }
                    }
                }
            } catch (e) { console.warn('[indexeddb] forceCleanup: sw unregister error', e); }

            // Clear caches
            try {
                if (window.caches) {
                    const keys = await caches.keys();
                    for (const k of keys) {
                        try { await caches.delete(k); console.info('[indexeddb] cache deleted', k); } catch (e) { console.warn('[indexeddb] cache delete failed', e); }
                    }
                }
            } catch (e) { console.warn('[indexeddb] forceCleanup: cache clear error', e); }

            // Clear localStorage
            try { localStorage.clear(); console.info('[indexeddb] localStorage cleared'); } catch (e) { console.warn('[indexeddb] localStorage clear failed', e); }

            // Try deleting DB first
            const deleted = await deleteDatabase().catch(err => { console.warn('[indexeddb] deleteDatabase threw', err); return false; });
            if (deleted) { resolve({ success: true, action: 'deleted' }); return; }

            // If delete was blocked, attempt in-place upgrade/migration
            console.info('[indexeddb] forceCleanup: delete blocked, attempting forceUpgrade');
            const upgraded = await forceUpgrade().catch(err => { console.warn('[indexeddb] forceUpgrade threw', err); return false; });
            if (upgraded) { resolve({ success: true, action: 'upgraded' }); return; }

            resolve({ success: false, action: 'failed' });
        } catch (e) { console.error('[indexeddb] forceCleanup exception', e); resolve({ success: false, action: 'exception', error: e ? e.toString() : '' }); }
    });
}

// Rewrite existing stores in-place: read all records, normalize them, clear stores, and re-insert normalized records.
export async function rewriteDatabase() {
    try {
    console.info('[indexeddb] rewriteDatabase: starting');
    try { broadcastEvent({ type: 'rewrite-start' }); } catch (e) { }
    await initializeDatabase();

        const cmdsJson = await getAllCommands();
        const listsJson = await getAllLists();
        let cmds = [];
        let lists = [];
        try { cmds = cmdsJson ? JSON.parse(cmdsJson) : []; } catch (e) { console.warn('[indexeddb] rewriteDatabase: parsing commands failed', e); }
        try { lists = listsJson ? JSON.parse(listsJson) : []; } catch (e) { console.warn('[indexeddb] rewriteDatabase: parsing lists failed', e); }

        // Open a readwrite transaction to clear and repopulate both stores
        return new Promise((resolve, reject) => {
            try {
                const tx = db.transaction(['commands', 'lists'], 'readwrite');
                const cmdStore = tx.objectStore('commands');
                const listStore = tx.objectStore('lists');

                // Clear stores first
                cmdStore.clear();
                listStore.clear();

                // Re-insert normalized items using put (will respect Id keyPath if present)
                cmds.forEach(it => {
                    try { cmdStore.put(normalizeCommandItem(it)); } catch (e) { console.warn('[indexeddb] rewriteDatabase: add command failed', e); }
                });
                lists.forEach(it => {
                    try { listStore.put(normalizeListItem(it)); } catch (e) { console.warn('[indexeddb] rewriteDatabase: add list failed', e); }
                });

                tx.oncomplete = () => { console.info('[indexeddb] rewriteDatabase: completed', { commands: cmds.length, lists: lists.length }); try { broadcastEvent({ type: 'rewrite-complete', commands: cmds.length, lists: lists.length }); } catch (e) { } resolve({ success: true, commands: cmds.length, lists: lists.length }); };
                tx.onerror = (e) => { console.error('[indexeddb] rewriteDatabase: transaction error', e); lastError = e && e.target && e.target.error ? e.target.error.toString() : lastError; reject(e); };
            } catch (ex) {
                console.error('[indexeddb] rewriteDatabase exception', ex);
                lastError = ex ? ex.toString() : lastError;
                reject(ex);
            }
        });
    } catch (e) {
        console.error('[indexeddb] rewriteDatabase top-level error', e);
        lastError = e ? e.toString() : lastError;
        return { success: false, error: e ? e.toString() : '' };
    }
}