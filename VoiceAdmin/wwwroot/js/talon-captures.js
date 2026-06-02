window.talonCapturesInterop = (function () {
    function registerHandler(dotNetRef) {
        if (!dotNetRef) return;
        function handler(e) {
            try {
                var target = e.target || e.srcElement;
                if (!target) return;
                var a = target.closest ? target.closest('a[href]') : null;
                if (!a) return;
                var href = a.getAttribute('href') || '';
                try {
                    var url = new URL(href, window.location.href);
                    // If the link points to a hash on the current origin, handle it in-app
                    if (url.hash && url.origin === window.location.origin) {
                        e.preventDefault();
                        var anchor = url.hash.charAt(0) === '#' ? url.hash.substring(1) : url.hash;
                        dotNetRef.invokeMethodAsync('OnAnchorClick', anchor).catch(function (err) { console.warn(err); });
                            try { console.debug && console.debug('talonCapturesInterop: intercepted hash', anchor); } catch (e) {}
                        return;
                    }
                    if (url.protocol === 'file:') {
                        e.preventDefault();
                        try {
                            // Convert file URI to vscode://file/ URI so it opens in VS Code
                            var path = url.pathname || '';
                            try { path = decodeURIComponent(path); } catch (de) { }
                            // On Windows the pathname may start with a leading '/C:'. Remove leading '/'
                            if (path.length > 0 && path.charAt(0) === '/' && /^[A-Za-z]:/.test(path.substring(1, 3))) {
                                path = path.substring(1);
                            }
                            // Build vscode file URI and preserve line number if present (e.g. #L123)
                            var vscodeUri = 'vscode://file/' + encodeURI(path);
                            if (url.hash) {
                                var m = url.hash.match(/#L(\d+)/i);
                                if (m && m.length > 1) {
                                    vscodeUri += ':' + m[1];
                                }
                            }
                            try {
                                // Use location.href to invoke the protocol handler
                                window.location.href = vscodeUri;
                                try { console.debug && console.debug('talonCapturesInterop: opening in vscode', vscodeUri); } catch (e) {}
                            }
                            catch (inner) {
                                // Fallback to opening the file URL in a new tab if the protocol isn't handled
                                window.open(href);
                            }
                        } catch (ex) {
                            console.warn('Unable to open file link', ex);
                        }
                        return;
                    }
                } catch (parseEx) {
                    // Fallback: handle simple fragment links like '#anchor'
                    if (href && href.charAt(0) === '#') {
                        e.preventDefault();
                        var anchor2 = href.substring(1);
                        dotNetRef.invokeMethodAsync('OnAnchorClick', anchor2).catch(function (err) { console.warn(err); });
                            try { console.debug && console.debug('talonCapturesInterop: intercepted fallback hash', anchor2); } catch (e) {}
                        return;
                    }
                }
            } catch (ex) {
                console.warn('talonCaptures handler error', ex);
            }
        }
        // Remove previous if present (capture listener)
        if (document._talonCapturesHandler) {
            document.removeEventListener('click', document._talonCapturesHandler, true);
        }
            document._talonCapturesHandler = handler;
            document.addEventListener('click', handler, true);
            try { console.debug && console.debug('talonCapturesInterop: handler registered'); } catch(e) {}
    }

    function unregisterHandler() {
        if (document._talonCapturesHandler) {
            try { document.removeEventListener('click', document._talonCapturesHandler, true); } catch (e) { }
            delete document._talonCapturesHandler;
                try { console.debug && console.debug('talonCapturesInterop: handler unregistered'); } catch (e) {}
        }
    }

    return {
        registerHandler: registerHandler,
        unregisterHandler: unregisterHandler
    };
})();

// Utility to scroll to an element and briefly flash it for visibility
window.talonCapturesInterop = window.talonCapturesInterop || {};
window.talonCapturesInterop.scrollTo = function (id) {
    try {
        var el = document.getElementById(id);
        if (!el) return;
        el.scrollIntoView({ behavior: 'smooth', block: 'center' });
        el.classList.add('talon-captures-highlight');
        setTimeout(function () { el.classList.remove('talon-captures-highlight'); }, 1200);
    } catch (e) { console.warn('scrollTo error', e); }
}

