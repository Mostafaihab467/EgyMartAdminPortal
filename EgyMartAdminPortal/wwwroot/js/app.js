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

window.localStorageHelper = {
    getItem: function (key) {
        return localStorage.getItem(key);
    },
    setItem: function (key, value) {
        localStorage.setItem(key, value);
    },
    removeItem: function (key) {
        localStorage.removeItem(key);
    }
};


window.focusElement = (element) => {
    if (element) {
        element.focus();
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

document.addEventListener('click', function (event) {
    var dropdowns = document.querySelectorAll('.dropdown-menu');
    dropdowns.forEach(function (dropdown) {
        if (!dropdown.parentElement.contains(event.target)) {
            dropdown.classList.remove('show'); // Hide the dropdown
        }
    });
});

function handleImageError(imageElement) {
    // Check if the image exists by making a request
    fetch(imageElement.src)
        .then(response => {
            if (response.status === 404) {
                imageElement.src = "images/avatars/user.jpg"; // Fallback image
            }
        })
        .catch(() => {
            imageElement.src = "images/avatars/user.jpg"; // Fallback image on error
        });
}

window.cryptoHelper = {
    async signRequest(userEmail, endpoint, plainText, ivBase64) {
        function getMyKey(userEmail, endpoint) {
            const originalKey = "NlOd2ZNXdgDdz0k?vQP@sFWOXBOGp4)1";
            const normalize = str => str.replace(/[^a-zA-Z0-9]/g, '');
            const getDayIndex = () => {
                const today = new Date();
                const day = today.getDate();
                const month = today.getMonth() + 1;
                return month === 1 ? day : ((month - 1) * 30) + day;
            };

            //let key = normalize(userEmail) + normalize(endpoint) + getDayIndex().toString(); // no toLowerCase
            let key = normalize(userEmail) + normalize(endpoint.split('/').pop()) + getDayIndex().toString(); // no toLowerCase
            if (key.length < 32) {
                key += originalKey.substring(0, 32 - key.length);
            } else {
                key = key.substring(0, 32);
            }
            return key;
        }

        const keyText = getMyKey(userEmail, endpoint);
        const enc = new TextEncoder();
        const keyBytes = enc.encode(keyText);
        const iv = Uint8Array.from(atob(ivBase64), c => c.charCodeAt(0));
        const messageBytes = enc.encode(plainText);
        const cryptoKey = await crypto.subtle.importKey(
            "raw",
            keyBytes,
            { name: "AES-CBC" },
            false,
            ["encrypt"]
        );

        const encrypted = await crypto.subtle.encrypt(
            { name: "AES-CBC", iv: iv },
            cryptoKey,
            messageBytes
        );
        return btoa(String.fromCharCode(...new Uint8Array(encrypted)));
    }
};

window.sessionTimeout = {
    registerActivity: function (dotNetHelper) {
        const events = ['mousemove', 'keydown', 'click', 'touchstart'];

        const resetInactivity = () => {
            dotNetHelper.invokeMethodAsync('ResetInactivityTimer');
        };

        const onVisibilityChange = () => {
            dotNetHelper.invokeMethodAsync('OnVisibilityChange', document.visibilityState);
        };

        events.forEach(event =>
            document.addEventListener(event, resetInactivity)
        );

        document.addEventListener('visibilitychange', onVisibilityChange);
    },

    getCurrentUrl: function () {
        return window.location.href;
    }
};


