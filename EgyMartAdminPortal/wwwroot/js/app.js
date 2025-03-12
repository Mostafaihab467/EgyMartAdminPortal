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
    instances: {},

    init: function (id, dotNetReference) {
        ClassicEditor
            .create(document.getElementById(id))
            .then(editor => {
                window.CKEditorInterop.instances[id] = editor;
                editor.model.document.on('change:data', () => {
                    dotNetReference.invokeMethodAsync('EditorDataChanged', editor.getData());
                });
            })
            .catch(error => console.error('CKEditor Init Error:', error));
    },

    getData: function (id) {
        return window.CKEditorInterop.instances[id]?.getData() || "";
    },

    destroy: function (id) {
        if (window.CKEditorInterop.instances[id]) {
            window.CKEditorInterop.instances[id].destroy()
                .then(() => { delete window.CKEditorInterop.instances[id]; });
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
