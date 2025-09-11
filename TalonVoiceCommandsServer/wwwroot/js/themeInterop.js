// tvcTheme: small helper to manage theme for TalonVoiceCommandsServer
// stores theme in localStorage under key 'tvc-theme' and sets documentElement's data-bs-theme.
window.tvcTheme = (function () {
    const key = 'tvc-theme';

    function applyTheme(theme) {
        try {
            if (theme === 'dark') {
                document.documentElement.setAttribute('data-bs-theme', 'dark');
            } else {
                document.documentElement.setAttribute('data-bs-theme', 'light');
            }
        } catch (e) {
            // ignore for non-browser contexts
        }
    }

    function getTheme() {
        try {
            const stored = localStorage.getItem(key);
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
            localStorage.setItem(key, theme);
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
