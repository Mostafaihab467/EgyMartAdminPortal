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
    editors: {},

    init: (editorId, dotNetRef) => {
        ClassicEditor
            .create(document.getElementById(editorId))
            .then(editor => {
                window.CKEditorInterop.editors[editorId] = editor;
                editor.model.document.on('change:data', () => {
                    dotNetRef.invokeMethodAsync('EditorDataChanged', editor.getData());
                });
            })
            .catch(error => console.error('CKEditor initialization error:', error));
    },

    destroy: (editorId) => {
        if (window.CKEditorInterop.editors[editorId]) {
            window.CKEditorInterop.editors[editorId].destroy()
                .then(() => delete window.CKEditorInterop.editors[editorId])
                .catch(error => console.error('CKEditor destroy error:', error));
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
