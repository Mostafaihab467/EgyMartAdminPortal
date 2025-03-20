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
    init: (editorId, dotNetRef) => {
        ClassicEditor
            .create(document.querySelector(`#${editorId}`), {
                width: '50rem',
                extraPlugins: [MyCustomUploadAdapterPlugin], // Custom Upload Adapter
                toolbar: [
                    'undo', 'redo', '|',
                    'bold', 'italic', 'underline', 'strikethrough', '|',
                    'heading', '|',
                    'link', 'blockQuote', '|',
                    'bulletedList', 'numberedList', '|',
                    'alignment', 'indent', 'outdent', '|',
                    'code', 'codeBlock', '|',
                    'insertTable'
                ],
            })
            .then(editor => {
                window[editorId] = editor;
                editor.model.document.on('change:data', () => {
                    dotNetRef.invokeMethodAsync('EditorDataChanged', editor.getData());
                });
                editor.ui.view.editable.element.style.width = '50rem';
                editor.ui.view.editable.element.style.maxWidth = '100%'
            })
            .catch(error => {
                console.error('CKEditor error:', error);
            });
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


function MyCustomUploadAdapterPlugin(editor) {
    editor.plugins.get('FileRepository').createUploadAdapter = (loader) => {
        return new MyUploadAdapter(loader);
    };
}

class MyUploadAdapter {
    constructor(loader) {
        this.loader = loader;
    }

    upload() {
        return this.loader.file
            .then(file => new Promise((resolve, reject) => {
                const formData = new FormData();
                formData.append('file', file);

                fetch('/api/upload', { // Update this to your API endpoint
                    method: 'POST',
                    body: formData
                })
                    .then(response => response.json())
                    .then(result => resolve({ default: result.url }))
                    .catch(error => reject(error));
            }));
    }

    abort() {
        // Handle aborting file uploads
    }
}

window.enableIframeClick = (iframeId, dotNetRef) => {
    let iframe = document.getElementById(iframeId);
    if (!iframe) return;

    iframe.onload = function () {
        let doc = iframe.contentDocument || iframe.contentWindow.document;
        if (!doc) return;

        doc.addEventListener("click", function () {
            dotNetRef.invokeMethodAsync("OnIframeClick");
        });
    };
};
