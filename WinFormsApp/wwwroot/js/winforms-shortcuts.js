// Shortcut handler for WinForms-hosted Blazor hybrid page
// Registers Ctrl+Shift+<key> -> clicks the corresponding top-menu button
(function () {
    'use strict';

    function isEditable(el) {
        if (!el) return false;
        const tag = (el.tagName || '').toUpperCase();
        if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return true;
        if (el.isContentEditable) return true;
        return false;
    }

    // Diagnostic: log when script loads
    try { console.debug && console.debug('winforms-shortcuts.js loaded'); } catch (e) { }

    const keyMap = {
        c: 'btnClipboard',
        a: 'btnAIChat',
        s: 'btnSnippets',
        l: 'btnLauncher',
        t: 'btnTalonSearch'
    };

    function handler(e) {
        // Require Ctrl+Shift, ignore Alt and Meta
        if (!e.ctrlKey || !e.shiftKey || e.altKey || e.metaKey) return;
        // Ignore when typing in editable controls
        if (isEditable(e.target)) return;

        const key = (e.key || '').toLowerCase();
        const id = keyMap[key];
        if (!id) return;

        const el = document.getElementById(id);
        if (!el) return;

        try {
            try { console.debug && console.debug('winforms-shortcuts key=', key, 'id=', id); } catch (e) { }
            el.click();
            e.preventDefault();
            e.stopPropagation();
        } catch (err) {
            // swallow errors silently
            console.error('Shortcut handler error', err);
        }
    }

    document.addEventListener('keydown', handler, false);
    window.addEventListener('beforeunload', function () {
        document.removeEventListener('keydown', handler, false);
    });
})();
