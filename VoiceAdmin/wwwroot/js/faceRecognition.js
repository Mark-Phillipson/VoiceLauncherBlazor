// Face Recognition JavaScript Helper Functions

window.faceRecognitionHelpers = {
    // Get the bounding rectangle of an image element by its alt text
    getImageBoundingRect: function (altText) {
        const img = document.querySelector(`img[alt="${CSS.escape(altText)}"]`);
        if (!img) {
            return null;
        }
        const rect = img.getBoundingClientRect();
        return {
            left: rect.left,
            top: rect.top,
            width: rect.width,
            height: rect.height,
            right: rect.right,
            bottom: rect.bottom
        };
    },

    // Focus an input element by its ID
    focusInputById: function (inputId) {
        const input = document.getElementById(inputId);
        if (input) {
            input.focus();
            return true;
        }
        return false;
    },

    // Focus the name input after clicking on an image
    focusNameInput: function () {
        // Find the input with placeholder 'Enter first name'
        const input = document.querySelector('input[placeholder="Enter first name"]');
        if (input) {
            input.focus();
            return true;
        }
        return false;
    }
};
