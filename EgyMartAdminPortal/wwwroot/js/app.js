window.triggerClick = (element) => {
    if (element) {
        element.click();
    } else {
        console.error("Invalid element reference passed to triggerClick:", element);
    }
};

window.sessionStorageHelper = {
    setItem: function (key, value) {
        sessionStorage.setItem(key, value);
    },
    getItem: function (key) {
        return sessionStorage.getItem(key);
    },
    removeItem: function (key) {
        sessionStorage.removeItem(key);
    }
};

window.focusElement = (element) => {
    if (element) {
        element.focus();
    }
};

window.CKEditorInterop = {
    init: function (id, dotNetRef) {
        ClassicEditor
            .create(document.getElementById(id), {
                height: '75vh'
            })
            .then(editor => {
                window[id] = editor;
                editor.model.document.on('change:data', () => {
                    dotNetRef.invokeMethodAsync('EditorDataChanged', editor.getData());
                });
            })
            .catch(error => console.error(error));
    },
    focus: function (id) {
        if (window[id]) {
            window[id].editing.view.focus();
        }
    },
    getData: function (id) {
        return window[id] ? window[id].getData() : '';
    },
    destroy: function (id) {
        if (window[id]) {
            window[id].destroy().then(() => delete window[id]);
        }
    }
};



window.getActiveElementTag = () => {
    return document.activeElement.tagName.toLowerCase();
};

window.isInsideCKEditor = () => {
    let activeElement = document.activeElement;

    // Check if inside a CKEditor instance
    while (activeElement) {
        if (activeElement.classList?.contains("ck-editor__editable")) {
            return true;
        }
        activeElement = activeElement.parentElement;
    }
    return false;
};

window.disableFutureDates = (element) => {
    let today = new Date().toISOString().split("T")[0]; // Get today's date in YYYY-MM-DD format
    let minDate = new Date();
    minDate.setMonth(minDate.getMonth() - 2); // Subtract 2 months
    let minFormatted = minDate.toISOString().split("T")[0];

    element.setAttribute("max", today);
    element.setAttribute("min", minFormatted);
};

window.preventZeroInput = function (event) {
    if (event.key === "0" && event.target.value.length === 0) {
        event.preventDefault();
    }
};
