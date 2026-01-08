/**
 * Browser Extension Detection Module
 * Detects known browser extensions using various non-invasive techniques.
 */

/**
 * Known harmful extensions with their detection signatures.
 * @type {Array<{id: string, name: string, warningMessage: string, resourceUrls: string[]}>}
 */
const KNOWN_EXTENSIONS = [
    {
        id: 'honey',
        name: 'Honey (PayPal)',
        warningMessage: 'The Honey extension has been found to replace affiliate cookies and may track your browsing. Consider removing it.',
        resourceUrls: [
            'chrome-extension://bmnlcjabgnpnenekpadlanbbkooimhnj/manifest.json',
            'moz-extension://honey/manifest.json'
        ],
        domSignatures: [
            '[data-honey]',
            '#honey-button',
            '.honey-gold-button',
            '[class*="honey-"]'
        ]
    },
    {
        id: 'rakuten',
        name: 'Rakuten (Ebates)',
        warningMessage: 'The Rakuten extension may track your shopping behavior.',
        resourceUrls: [
            'chrome-extension://chhjbpecpncaggjpdakmflnfcopglcmi/manifest.json'
        ],
        domSignatures: [
            '[data-rakuten]',
            '#rakuten-button',
            '[class*="rakuten-"]'
        ]
    },
    {
        id: 'capital-one-shopping',
        name: 'Capital One Shopping',
        warningMessage: 'Capital One Shopping extension monitors your browsing for shopping deals.',
        resourceUrls: [
            'chrome-extension://nenlahapcbofgnanklpelkaejcehkggg/manifest.json'
        ],
        domSignatures: [
            '[data-wikibuy]',
            '[class*="wikibuy-"]'
        ]
    }
];

/**
 * Checks if a Chrome extension resource URL is accessible.
 * @param {string} url - The extension resource URL to check.
 * @returns {Promise<boolean>} True if the resource exists.
 */
async function checkExtensionResource(url) {
    try {
        const response = await fetch(url, { 
            method: 'HEAD',
            mode: 'no-cors'
        });
        // With no-cors, we can't read the response, but the request not throwing is a signal
        return true;
    } catch (e) {
        return false;
    }
}

/**
 * Checks for DOM elements that extensions typically inject.
 * @param {string[]} selectors - CSS selectors to look for.
 * @returns {boolean} True if any signature is found.
 */
function checkDomSignatures(selectors) {
    if (!selectors || selectors.length === 0) return false;
    
    for (const selector of selectors) {
        try {
            if (document.querySelector(selector)) {
                return true;
            }
        } catch (e) {
            // Invalid selector, skip
        }
    }
    return false;
}

/**
 * Detects a specific extension using multiple techniques.
 * @param {Object} extension - Extension definition.
 * @returns {Promise<boolean>} True if extension is detected.
 */
async function detectExtension(extension) {
    // Check DOM signatures first (fast, synchronous)
    if (extension.domSignatures && checkDomSignatures(extension.domSignatures)) {
        return true;
    }
    
    // Check resource URLs (requires network, async)
    if (extension.resourceUrls && extension.resourceUrls.length > 0) {
        for (const url of extension.resourceUrls) {
            // Only try chrome-extension URLs in Chrome-based browsers
            if (url.startsWith('chrome-extension://') && !window.chrome) {
                continue;
            }
            // Only try moz-extension URLs in Firefox
            if (url.startsWith('moz-extension://') && typeof InstallTrigger === 'undefined') {
                continue;
            }
            
            try {
                const detected = await checkExtensionResource(url);
                if (detected) {
                    return true;
                }
            } catch (e) {
                // Continue checking other URLs
            }
        }
    }
    
    return false;
}

/**
 * Detects all known extensions.
 * @returns {Promise<Array<{id: string, name: string, isHarmful: boolean, warningMessage: string}>>}
 */
async function detectAllExtensions() {
    const detected = [];
    
    for (const extension of KNOWN_EXTENSIONS) {
        try {
            const isDetected = await detectExtension(extension);
            if (isDetected) {
                detected.push({
                    id: extension.id,
                    name: extension.name,
                    isHarmful: true,
                    warningMessage: extension.warningMessage
                });
            }
        } catch (e) {
            console.warn(`Failed to detect extension ${extension.id}:`, e);
        }
    }
    
    return detected;
}

/**
 * Gets the list of known harmful extensions (without detection).
 * @returns {Array<{id: string, name: string, isHarmful: boolean, warningMessage: string}>}
 */
function getKnownHarmfulExtensions() {
    return KNOWN_EXTENSIONS.map(ext => ({
        id: ext.id,
        name: ext.name,
        isHarmful: true,
        warningMessage: ext.warningMessage
    }));
}

// Export functions for Blazor interop
window.detectBrowserExtensions = detectAllExtensions;
window.getKnownHarmfulExtensions = getKnownHarmfulExtensions;
