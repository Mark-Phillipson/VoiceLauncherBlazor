// wwwroot/js/site.js
window.applyBoldStyling = function () {
    const selectElement = document.getElementById('mySelect');
    const options = selectElement.options;

    for (let i = 0; i < options.length; i++) {
        if (options[i].classList.contains('bold-option')) {
            options[i].style.fontWeight = 'bold';
        }
    }
};

// Ensure reconnect-bridge is loaded even if the explicit script tag is missing
(function injectReconnectBridge() {
    try {
        if (document.querySelector('script[src="/js/reconnect-bridge.js"]')) return;
        const s = document.createElement('script');
        s.src = '/js/reconnect-bridge.js';
        s.async = true;
        document.head.appendChild(s);
        console.debug && console.debug('[site.js] injected /js/reconnect-bridge.js');
    } catch (e) { }
})();
