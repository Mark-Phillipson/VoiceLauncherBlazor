// Automatically refresh page if offline when tab regains focus
(function() {
    function isOffline() {
        return !navigator.onLine;
    }
    document.addEventListener('visibilitychange', function() {
        if (document.visibilityState === 'visible' && isOffline()) {
            location.reload();
        }
    });
})();
