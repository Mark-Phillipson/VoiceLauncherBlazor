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
