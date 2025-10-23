// Automatically set Bootstrap dark mode if user prefers dark scheme
(function() {
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        document.body.setAttribute('data-bs-theme', 'dark');
    } else {
        document.body.setAttribute('data-bs-theme', 'light');
    }
    // Optional: Listen for changes in system theme
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function(e) {
        document.body.setAttribute('data-bs-theme', e.matches ? 'dark' : 'light');
    });
})();
