When clicking the refresh filters button we are encountering an error in the console log which is causing the application to hang and not respond:

blazor.web.js:1 [2025-09-15T13:28:34.753Z] Information: Normalizing '_blazor' to 'http://localhost:5269/_blazor'.
blazor.web.js:1 [2025-09-15T13:28:34.770Z] Information: WebSocket connected to ws://localhost:5269/_blazor?id=md7usGXJHCptMSVmnjZF8g.
indexeddb-storage.js:12 TalonStorageDB: Initializing IndexedDB...
indexeddb-storage.js:23 TalonStorageDB: IndexedDB initialized successfully
blazor.web.js:1 Selection modal JS module loaded (client)
selectionModalInterop.js:95 selectionModalInterop.showModal called for #selectionModal
selectionModalInterop.js:116 selectionModalInterop: modal shown #selectionModal
blazor.web.js:1  [2025-09-15T13:47:01.874Z] Error: Connection disconnected with error 'Error: Server returned an error on close: Connection closed with an error.'.
log @ blazor.web.js:1
_stopConnection @ blazor.web.js:1
features.reconnect.transport.onclose @ blazor.web.js:1
_close @ blazor.web.js:1
stop @ blazor.web.js:1
_stopInternal @ blazor.web.js:1
await in _stopInternal
stop @ blazor.web.js:1
_processIncomingData @ blazor.web.js:1
connection.onreceive @ blazor.web.js:1
i.onmessage @ blazor.web.js:1
blazor.web.js:1 [2025-09-15T13:47:01.875Z] Information: Normalizing '_blazor' to 'http://localhost:5269/_blazor'.
blazor.web.js:1 [2025-09-15T13:47:01.882Z] Information: WebSocket connected to ws://localhost:5269/_blazor?id=yYgHrFLJ-0CDKypKejq-Cg.

 it may be that the application is trying to use Blazor to pull a lot of information directly from the index db database whereas it should be using javascript for this not sure if that is applicable in this case though what do you think?
