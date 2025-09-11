window.talonImportClient = {
    getDefaultPath: function (key) {
        try {
            return localStorage.getItem(key) || '';
        } catch (e) {
            return '';
        }
    },
    setDefaultPath: function (key, value) {
        try {
            localStorage.setItem(key, value || '');
            return true;
        } catch (e) {
            return false;
        }
    }
};
