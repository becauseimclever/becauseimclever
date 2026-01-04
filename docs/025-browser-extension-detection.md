# 025 - Browser Extension Detection

## Feature Description

Implement a system to detect specific browser extensions installed by visitors and display warnings for potentially harmful or controversial extensions. The initial focus is on warning users about the Honey browser extension.

## Goals

1. Detect if visitors have the Honey browser extension installed
2. Display a non-intrusive warning banner to users with Honey installed
3. Provide information about why uninstalling Honey is recommended
4. Allow users to dismiss the warning (with optional persistence)
5. Generate browser fingerprints to identify unique visitors
6. Track extension detection statistics over time per unique browser
7. Provide admin dashboard for viewing extension statistics
8. **Master switch** to enable/disable the entire feature from admin dashboard

## Background

The Honey browser extension has been criticized for:
- Replacing affiliate links, taking commission from content creators
- Collecting extensive browsing data
- Not always providing the best available coupons
- Potentially deceptive marketing practices

## Technical Approach

### Extension Detection Methods

Browser extensions can be detected through several methods:

1. **Web Accessible Resources**: Check for extension-specific resources that are exposed to web pages
2. **DOM Modifications**: Detect elements or attributes injected by extensions
3. **Behavior Detection**: Observe changes to page behavior (e.g., affiliate link modifications)

### Honey Extension Detection

Honey can be detected by checking for:
- Injected DOM elements with specific class names or IDs
- Web accessible resources from the extension
- Modifications to affiliate links on the page

### Implementation Components

#### 1. Browser Fingerprinting Service
- `IBrowserFingerprintService` interface in Application layer
- `BrowserFingerprintService` implementation using JavaScript interop
- Fingerprint components:
  - Canvas fingerprint
  - WebGL renderer info
  - Screen resolution and color depth
  - Timezone and language settings
  - Installed fonts (subset)
  - User agent string
  - Platform and hardware concurrency
- Generate a stable hash from collected attributes

#### 2. Client-Side Detection Service
- `IBrowserExtensionDetector` interface in Application layer
- `BrowserExtensionDetector` implementation using JavaScript interop
- Detection methods for specific extensions

#### 3. Server-Side Tracking
- `BrowserVisit` entity to store fingerprint and extension data
- `IBrowserVisitRepository` for data access
- API endpoint to receive fingerprint + extension detection results
- Track: fingerprint hash, detected extensions, timestamp, IP (hashed)

#### 4. Warning Banner Component
- Blazor component to display extension warnings
- Dismissible with localStorage persistence
- Link to more information about why the extension is problematic

#### 5. JavaScript Interop Module
- Fingerprinting scripts for browser attribute collection
- Detection scripts for various extension fingerprints
- Honey-specific detection logic

#### 6. Admin Statistics Dashboard
- View unique visitors over time
- Extension detection rates and trends
- Filter by date range

#### 7. Consent Management System
- `IConsentService` interface for managing user consent
- Consent banner Blazor component
- Privacy settings page
- Consent verification middleware

#### 8. Data Subject Rights API
- GET `/api/privacy/data/{fingerprintHash}` - Export user data
- DELETE `/api/privacy/data/{fingerprintHash}` - Delete user data
- POST `/api/privacy/consent` - Record consent
- DELETE `/api/privacy/consent` - Withdraw consent

#### 9. Feature Master Switch
- `IFeatureToggleService` interface for managing feature state
- Admin dashboard toggle component with confirmation dialog
- Server-side feature flag checked before any tracking/detection
- API endpoint: `PUT /api/admin/features/extension-detection` - Toggle feature on/off
- API endpoint: `GET /api/admin/features/extension-detection` - Get current state
- Client checks feature status on page load before initializing detection

## Affected Components/Layers

- **Domain**: `BrowserVisit` entity, `BrowserFingerprint` value object
- **Application**: Interface definitions for fingerprinting and extension detection
- **Infrastructure**: Repository implementation, database context updates
- **Server**: API endpoints for tracking data submission
- **Client**: Detection services, fingerprinting service, warning banner component, JS interop

## Design Decisions

1. **Consent-First Approach**: No fingerprinting or tracking until user explicitly consents
2. **GDPR Compliant**: Full compliance with EU General Data Protection Regulation
3. **Privacy-Conscious Fingerprinting**: Hash fingerprint data; don't store raw values
4. **IP Anonymization**: Truncate/hash IP addresses to prevent individual identification
5. **Non-Blocking**: Detection and fingerprinting should not impact page load performance
6. **User-Friendly**: Warning should be informative, not aggressive
7. **Dismissible**: Users can hide the warning; respect user choice
8. **Extensible**: Design allows adding detection for other extensions in the future
9. **Data Retention**: 90-day retention policy with automatic deletion
10. **Data Portability**: Users can request their data in machine-readable format
11. **Master Switch**: Single toggle to enable/disable entire feature instantly

## User Experience

### Visitor Experience
1. Page loads normally
2. **Feature check**: Client queries if extension detection feature is enabled
3. If feature is **disabled**: No consent banner, no detection, no warnings shown
4. If feature is **enabled**:
   - **Consent banner appears** asking user permission to collect analytics data
   - If user **declines**: Extension warning still shown (legitimate interest), but no fingerprinting/tracking occurs
   - If user **accepts**: Fingerprinting and extension detection run asynchronously
5. Tracking data is sent to server only after consent
6. If Honey is detected, a warning banner appears (shown regardless of consent - legitimate interest for user protection)
7. Banner includes:
   - Brief explanation of the issue
   - Link to detailed information
   - Dismiss button
8. Dismissed state persists in localStorage
9. User can withdraw consent at any time via privacy settings link

### Admin Experience
1. Navigate to Admin Dashboard > Settings > Feature Toggles
2. View current status of Extension Detection feature (Enabled/Disabled)
3. Toggle switch with confirmation dialog:
   - **Disabling**: "This will stop all extension detection and tracking. Existing data will be preserved. Are you sure?"
   - **Enabling**: "This will enable extension detection and show consent banners to visitors. Are you sure?"
4. Change is logged with timestamp and admin user
5. Change takes effect immediately (no restart required)

## Data Model

### BrowserVisit Entity
```csharp
public class BrowserVisit
{
    public Guid Id { get; set; }
    public string FingerprintHash { get; set; }
    public string IpAddressHash { get; set; }
    public List<string> DetectedExtensions { get; set; }
    public DateTime VisitedAt { get; set; }
    public string UserAgent { get; set; }
    public bool HasConsent { get; set; }
    public DateTime ConsentGivenAt { get; set; }
}
```

### ConsentRecord Entity
```csharp
public class ConsentRecord
{
    public Guid Id { get; set; }
    public string FingerprintHash { get; set; }
    public bool AnalyticsConsent { get; set; }
    public DateTime ConsentGivenAt { get; set; }
    public DateTime? ConsentWithdrawnAt { get; set; }
    public string ConsentVersion { get; set; } // Track privacy policy version
    public string IpAddressHash { get; set; } // For audit purposes
}
```

### FeatureSettings Entity
```csharp
public class FeatureSettings
{
    public Guid Id { get; set; }
    public string FeatureName { get; set; } // e.g., "ExtensionDetection"
    public bool IsEnabled { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public string LastModifiedBy { get; set; } // Admin user who toggled
    public string DisabledReason { get; set; } // Optional reason for audit
}
```

### BrowserFingerprint Value Object
```csharp
public record BrowserFingerprint(
    string CanvasHash,
    string WebGLRenderer,
    string ScreenResolution,
    string Timezone,
    string Language,
    string Platform,
    int HardwareConcurrency
);
```

## Future Considerations

- Add detection for other problematic extensions
- Geographic analysis of extension usage
- Trend analysis and reporting
- Export functionality for statistics
- A/B testing different warning messages

## Privacy Considerations

- Fingerprinting is used for aggregate statistics, not individual tracking
- No personally identifiable information (PII) is collected
- All identifiers are hashed before storage
- Data retention policy limits how long data is kept
- Consider adding privacy policy disclosure about fingerprinting

## Legal Compliance

### GDPR (EU General Data Protection Regulation)

#### Lawful Basis
- **Analytics/Fingerprinting**: Requires explicit **consent** (Article 6(1)(a))
- **Extension Warning**: Can use **legitimate interest** (Article 6(1)(f)) - protecting users from harmful software

#### Required Implementation

1. **Consent Management**
   - Cookie/consent banner before any tracking
   - Clear explanation of what data is collected and why
   - Granular consent options (accept all, reject all, customize)
   - Easy withdrawal of consent at any time
   - Consent must be freely given, specific, informed, and unambiguous

2. **Privacy Notice Requirements**
   - Identity of data controller
   - Purpose of processing
   - Categories of data collected
   - Data retention period (90 days)
   - Rights of data subjects
   - Contact information for privacy inquiries

3. **Data Subject Rights Implementation**
   - **Right to Access** (Article 15): API endpoint to retrieve user's data by fingerprint
   - **Right to Erasure** (Article 17): API endpoint to delete all data for a fingerprint
   - **Right to Data Portability** (Article 20): Export data in JSON format
   - **Right to Object** (Article 21): Ability to opt-out of processing

4. **Technical Measures**
   - Encrypt data at rest and in transit
   - Implement access controls for admin dashboard
   - Audit logging for data access
   - Automatic data deletion after retention period

### CCPA (California Consumer Privacy Act)

- "Do Not Sell My Personal Information" link (if applicable)
- Privacy policy disclosures about data collection
- Opt-out mechanism for sale of personal information
- Respond to consumer requests within 45 days

### ePrivacy Directive (Cookie Law)

- Consent required before setting non-essential cookies
- Clear information about cookie purposes
- localStorage usage for consent preferences is permitted

### LGPD (Brazil)

- Similar to GDPR requirements
- Consent or legitimate interest basis
- Data subject rights must be honored

### PIPEDA (Canada)

- Meaningful consent required
- Limited collection principle
- Individual access rights

### Global Best Practices

1. **Collect minimum necessary data** - Only fingerprint attributes needed for uniqueness
2. **Anonymize where possible** - Hash all identifiers
3. **Respect Do Not Track** - Honor browser DNT signals as consent withdrawal
4. **Age restrictions** - Do not knowingly collect data from children under 16
5. **Transparency** - Clear, plain-language privacy policy
6. **Security** - Industry-standard encryption and access controls

## Implementation Components - Consent

### Consent Banner Component
- Appears on first visit before any tracking
- Options: "Accept Analytics", "Reject Analytics", "Learn More"
- Stores consent choice in localStorage
- Re-prompts if consent is withdrawn or expires

### Privacy Settings Page
- View current consent status
- Withdraw consent
- Request data export
- Request data deletion
- Link to full privacy policy

### Server-Side Consent Verification
- API checks for valid consent token before storing data
- Reject tracking requests without consent
- Log consent status with each data submission

## References

- [Honey Extension Controversy](https://www.youtube.com/watch?v=vc4yL3YTwWk) - MegaLag's investigation
- Browser extension detection techniques
- [FingerprintJS](https://fingerprint.com/) - Reference for fingerprinting techniques
- [GDPR Official Text](https://gdpr-info.eu/) - EU regulation reference
- [CCPA Official Text](https://oag.ca.gov/privacy/ccpa) - California privacy law
- [ICO Guidance on Consent](https://ico.org.uk/for-organisations/guide-to-data-protection/guide-to-the-general-data-protection-regulation-gdpr/consent/) - UK regulator guidance
- [CNIL Cookie Guidelines](https://www.cnil.fr/en/cookies-and-other-tracking-devices-cnil-publishes-new-guidelines) - French regulator guidance
