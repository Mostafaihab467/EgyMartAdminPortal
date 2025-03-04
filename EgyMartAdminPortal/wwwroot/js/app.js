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
