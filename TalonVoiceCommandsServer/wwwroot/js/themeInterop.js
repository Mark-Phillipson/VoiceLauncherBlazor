// tvcTheme: small helper to manage theme for TalonVoiceCommandsServer
// stores theme in localStorage under key 'tvc-theme' and sets documentElement's data-bs-theme.
window.tvcTheme = (function () {
    const key = 'tvc-theme';

    function applyTheme(theme) {
        try {
            // Keep backward compatibility: set both Bootstrap's data-bs-theme and our data-theme
            const t = theme === 'dark' ? 'dark' : 'light';
            document.documentElement.setAttribute('data-bs-theme', t);
            document.documentElement.setAttribute('data-theme', t);
        } catch (e) {
            // ignore for non-browser contexts
        }
    }

    function getTheme() {
        try {
            // prefer the tvc key but fall back to the older app-theme key for compatibility
            const stored = localStorage.getItem(key) || localStorage.getItem('app-theme');
            if (stored) return stored;
            // fallback to prefers-color-scheme
            const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
            return prefersDark ? 'dark' : 'light';
        } catch (e) {
            return 'light';
        }
    }

    function setTheme(theme) {
        try {
            // write both keys for compatibility with other scripts
            localStorage.setItem(key, theme);
            try { localStorage.setItem('app-theme', theme); } catch (e) { /* ignore */ }
            applyTheme(theme);
        } catch (e) {
            // ignore
        }
    }

    function toggleTheme() {
        const current = getTheme();
        const next = current === 'dark' ? 'light' : 'dark';
        setTheme(next);
        return next;
    }

    // apply on load
    try { applyTheme(getTheme()); } catch (e) { }

    return {
        getTheme: getTheme,
        setTheme: setTheme,
        toggleTheme: toggleTheme
    };
})();
