/**
 * Browser Fingerprinting Module
 * Collects various browser and device attributes for fingerprinting.
 */

/**
 * Generates a canvas fingerprint by drawing text and shapes.
 * @returns {string} A hash of the canvas data.
 */
function getCanvasFingerprint() {
    try {
        const canvas = document.createElement('canvas');
        canvas.width = 200;
        canvas.height = 50;
        const ctx = canvas.getContext('2d');
        
        if (!ctx) return 'no-canvas';
        
        // Draw text with various styles
        ctx.textBaseline = 'top';
        ctx.font = '14px Arial';
        ctx.fillStyle = '#f60';
        ctx.fillRect(125, 1, 62, 20);
        ctx.fillStyle = '#069';
        ctx.fillText('BecauseImClever', 2, 15);
        ctx.fillStyle = 'rgba(102, 204, 0, 0.7)';
        ctx.fillText('fingerprint', 4, 17);
        
        // Get data URL and hash it
        const dataUrl = canvas.toDataURL();
        return simpleHash(dataUrl);
    } catch (e) {
        return 'canvas-error';
    }
}

/**
 * Gets the WebGL renderer string (GPU information).
 * @returns {string} The WebGL renderer or 'unknown'.
 */
function getWebGLRenderer() {
    try {
        const canvas = document.createElement('canvas');
        const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
        
        if (!gl) return 'no-webgl';
        
        const debugInfo = gl.getExtension('WEBGL_debug_renderer_info');
        if (debugInfo) {
            return gl.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL) || 'unknown';
        }
        return 'unknown';
    } catch (e) {
        return 'webgl-error';
    }
}

/**
 * Gets screen resolution.
 * @returns {string} Resolution in format "WIDTHxHEIGHT".
 */
function getScreenResolution() {
    return `${window.screen.width}x${window.screen.height}`;
}

/**
 * Gets screen color depth.
 * @returns {number} Color depth in bits.
 */
function getColorDepth() {
    return window.screen.colorDepth || 24;
}

/**
 * Gets the timezone.
 * @returns {string} IANA timezone identifier.
 */
function getTimezone() {
    try {
        return Intl.DateTimeFormat().resolvedOptions().timeZone || 'unknown';
    } catch (e) {
        return 'unknown';
    }
}

/**
 * Gets the browser language.
 * @returns {string} Browser language.
 */
function getLanguage() {
    return navigator.language || navigator.userLanguage || 'unknown';
}

/**
 * Gets the platform.
 * @returns {string} Platform identifier.
 */
function getPlatform() {
    return navigator.platform || 'unknown';
}

/**
 * Gets hardware concurrency (logical processors).
 * @returns {number} Number of logical processors.
 */
function getHardwareConcurrency() {
    return navigator.hardwareConcurrency || 1;
}

/**
 * Simple hash function for strings.
 * @param {string} str - String to hash.
 * @returns {string} Hash string.
 */
function simpleHash(str) {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
        const char = str.charCodeAt(i);
        hash = ((hash << 5) - hash) + char;
        hash = hash & hash; // Convert to 32bit integer
    }
    return Math.abs(hash).toString(16);
}

/**
 * Collects all fingerprint data.
 * @returns {Object} Object containing all fingerprint attributes.
 */
window.collectBrowserFingerprint = function() {
    return {
        canvasHash: getCanvasFingerprint(),
        webGLRenderer: getWebGLRenderer(),
        screenResolution: getScreenResolution(),
        colorDepth: getColorDepth(),
        timezone: getTimezone(),
        language: getLanguage(),
        platform: getPlatform(),
        hardwareConcurrency: getHardwareConcurrency()
    };
};
