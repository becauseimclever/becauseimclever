// Post Editor JavaScript utilities
window.postEditor = {
    _beforeUnloadHandler: null,
    _hasUnsavedChanges: false,
    _fullscreenKeyHandler: null,
    _dotNetRef: null,
    
    registerBeforeUnload: function() {
        if (this._beforeUnloadHandler) {
            return;
        }
        
        const self = this;
        this._beforeUnloadHandler = function(e) {
            if (self._hasUnsavedChanges) {
                e.preventDefault();
                e.returnValue = '';
                return '';
            }
        };
        
        window.addEventListener('beforeunload', this._beforeUnloadHandler);
    },
    
    unregisterBeforeUnload: function() {
        if (this._beforeUnloadHandler) {
            window.removeEventListener('beforeunload', this._beforeUnloadHandler);
            this._beforeUnloadHandler = null;
        }
        this._hasUnsavedChanges = false;
    },
    
    setUnsavedChanges: function(hasChanges) {
        this._hasUnsavedChanges = hasChanges;
    },

    registerFullscreenKeys: function(dotNetRef) {
        this._dotNetRef = dotNetRef;
        
        if (this._fullscreenKeyHandler) {
            return;
        }

        const self = this;
        this._fullscreenKeyHandler = function(e) {
            if (e.key === 'F11') {
                e.preventDefault();
                if (self._dotNetRef) {
                    self._dotNetRef.invokeMethodAsync('HandleFullscreenKey', 'F11');
                }
            } else if (e.key === 'Escape') {
                if (self._dotNetRef) {
                    self._dotNetRef.invokeMethodAsync('HandleFullscreenKey', 'Escape');
                }
            }
        };

        window.addEventListener('keydown', this._fullscreenKeyHandler);
    },

    unregisterFullscreenKeys: function() {
        if (this._fullscreenKeyHandler) {
            window.removeEventListener('keydown', this._fullscreenKeyHandler);
            this._fullscreenKeyHandler = null;
        }
        this._dotNetRef = null;
    }
};
