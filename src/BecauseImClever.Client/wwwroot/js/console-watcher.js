/**
 * Console Watcher Module
 * Displays a styled ASCII art message in the browser console
 * and detects when DevTools is opened.
 * This is a lighthearted Easter egg for curious developers.
 */

/**
 * @namespace consoleWatcher
 */
window.consoleWatcher = {
    /** @type {number|null} */
    _intervalId: null,

    /** @type {object|null} */
    _dotNetRef: null,

    /** @type {boolean} */
    _detected: false,

    /** @type {function|null} */
    _visibilityHandler: null,

    /**
     * Prints a styled ASCII art message to the browser console.
     * Uses %c formatting for colored, styled output.
     */
    showMessage: function () {
        const asciiArt = [
            '    ____                                ____          ________',
            '   / __ ) ___  _____ ____ _ __  __ ___/ __/         /  _/ __ \\__',
            '  / __  |/ _ \\/ ___// __ `// / / // __  /          / / / /_/ __ \\',
            ' / /_/ //  __/ /__ / /_/ // /_/ /(__  ) ___       / / / __  / / /',
            '/______/ \\___/\\___/ \\__,_/ \\__,_/ ____/ /__/   /___//_/ /_/ /_/',
            '  / ____// /___  _   __ ___  _____',
            ' / /    / // _ \\| | / // _ \\/ ___/',
            '/ /___ / //  __/| |/ //  __/ /',
            '\\____//_/ \\___/ |___/ \\___/_/'
        ].join('\n');

        const bannerStyle = [
            'font-family: monospace',
            'font-size: 12px',
            'color: #00ff41',
            'background: #0a0a0a',
            'padding: 16px',
            'border-radius: 4px',
            'line-height: 1.4'
        ].join(';');

        const headerStyle = [
            'font-size: 18px',
            'font-weight: bold',
            'color: #ff6b6b',
            'padding: 8px 0'
        ].join(';');

        const bodyStyle = [
            'font-size: 14px',
            'color: #e0e0e0',
            'line-height: 1.6'
        ].join(';');

        const linkStyle = [
            'font-size: 13px',
            'color: #4fc3f7',
            'text-decoration: underline'
        ].join(';');

        console.log('%c' + asciiArt, bannerStyle);
        console.log('%c\ud83d\udea8 Hey there, curious one!', headerStyle);
        console.log(
            '%cWe see you poking around in the console.\n' +
            'No cheating allowed \u2014 we\'re watching. \ud83d\udc40\n\n' +
            '(Just kidding. Mostly. Welcome to the source code!)',
            bodyStyle
        );
        console.log(
            '%cIf you\'re a developer, check out the repo:\nhttps://github.com/becauseimclever',
            linkStyle
        );
    },

    /**
     * Starts DevTools detection polling.
     * Uses two complementary heuristics:
     * 1. console.log object trick (getter fires when DevTools formats the object)
     * 2. Window size differential (detects docked DevTools panels)
     * Polling pauses when the tab is hidden and stops after first detection.
     * @param {object} dotNetRef - A DotNetObjectReference for calling back into Blazor.
     */
    startDetection: function (dotNetRef) {
        var self = this;
        self._dotNetRef = dotNetRef;
        self._detected = false;

        // Pause/resume on visibility change
        self._visibilityHandler = function () {
            if (document.visibilityState === 'hidden') {
                self._pausePolling();
            } else if (!self._detected) {
                self._resumePolling();
            }
        };
        document.addEventListener('visibilitychange', self._visibilityHandler);

        self._resumePolling();
    },

    /**
     * Stops DevTools detection and cleans up resources.
     */
    stopDetection: function () {
        this._pausePolling();
        if (this._visibilityHandler) {
            document.removeEventListener('visibilitychange', this._visibilityHandler);
            this._visibilityHandler = null;
        }
        this._dotNetRef = null;
        this._detected = false;
    },

    /**
     * Starts the polling interval.
     * @private
     */
    _resumePolling: function () {
        if (this._intervalId) {
            return;
        }
        var self = this;
        self._intervalId = setInterval(function () {
            self._checkDevTools();
        }, 2000);
    },

    /**
     * Clears the polling interval.
     * @private
     */
    _pausePolling: function () {
        if (this._intervalId) {
            clearInterval(this._intervalId);
            this._intervalId = null;
        }
    },

    /**
     * Runs a single DevTools detection check using multiple heuristics.
     * @private
     */
    _checkDevTools: function () {
        if (this._detected) {
            return;
        }

        var detected = false;

        // Method 1: console.log object trick
        var detector = { _opened: false };
        Object.defineProperty(detector, 'id', {
            get: function () {
                detected = true;
                return '';
            }
        });
        console.debug(detector);

        // Method 2: Window size differential (docked DevTools)
        if (!detected) {
            var widthDiff = window.outerWidth - window.innerWidth;
            var heightDiff = window.outerHeight - window.innerHeight;
            var threshold = 160;
            if (widthDiff > threshold || heightDiff > threshold) {
                detected = true;
            }
        }

        if (detected) {
            this._detected = true;
            this._pausePolling();
            this._notifyBlazor();
        }
    },

    /**
     * Notifies the Blazor component that DevTools was detected.
     * @private
     */
    _notifyBlazor: function () {
        if (this._dotNetRef) {
            try {
                this._dotNetRef.invokeMethodAsync('OnDevToolsOpened');
            } catch (e) {
                // Silently fail - notification is non-critical
            }
        }
    }
};
