// Post Editor JavaScript utilities
window.postEditor = {
    _beforeUnloadHandler: null,
    _hasUnsavedChanges: false,
    
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
    }
};
