# 039 - Browser Console Detection

## Status: 📋 Planning

## Feature Description

Detect when a visitor opens the browser's developer tools (console) and display a playful, tongue-in-cheek message warning them not to cheat — and that "we're watching." This is a lighthearted Easter egg, not a security mechanism. The message should be fun and memorable, not threatening.

## Goals

1. Detect when the browser DevTools / console is opened (best-effort, not foolproof)
2. Display a styled console message using `console.log` with CSS formatting (large text, color, emoji)
3. Optionally show an in-page visual indicator (e.g., a brief toast or overlay) when DevTools is first detected
4. Keep the detection lightweight and non-intrusive — it must not degrade site performance or accessibility
5. Respect user consent preferences — only activate if analytics/tracking consent has been granted (ties into existing consent system)

## Console Message Design

The message displayed in the browser console should be visually striking using `console.log` CSS formatting:

```
    ____                                ____          ________
   / __ ) ___  _____ ____ _ __  __ ___/ __/         /  _/ __ \__
  / __  |/ _ \/ ___// __ `// / / // __  /          / / / /_/ __ \
 / /_/ //  __/ /__ / /_/ // /_/ /(__  ) ___       / / / __  / / /
/______/ \___/\___/ \__,_/ \__,_/ ____/ /__/   /___//_/ /_/ /_/
  / ____// /___  _   __ ___  _____
 / /    / // _ \| | / // _ \/ ___/
/ /___ / //  __/| |/ //  __/ /
\____//_/ \___/ |___/ \___/_/

🚨 Hey there, curious one!

We see you poking around in the console.
No cheating allowed — we're watching. 👀

(Just kidding. Mostly. Welcome to the source code!)

If you're a developer, check out the repo:
https://github.com/becauseimclever
```

Styled with:
- ASCII art banner rendered in monospace via `%c` with a neon/terminal-green color (`#00ff41`) on a dark background
- Large, bold header text (via `%c` console formatting) for the warning message below the art
- Color accents matching the site's theme
- Friendly, non-hostile tone

---

## Technical Design

### Detection Approach

DevTools detection is inherently imprecise and browser-dependent. We use multiple complementary heuristics rather than relying on a single method:

#### Method 1 — `console.log` Object Trick
Create an object with a custom getter that fires when DevTools reads/formats it. When the console is closed, the getter is never invoked. When open, it fires and we detect the state change.

```javascript
const detector = {
    get isOpen() {
        this._opened = true;
        return false;
    },
    _opened: false
};

setInterval(() => {
    detector._opened = false;
    console.debug(detector);
    if (detector._opened) {
        onDevToolsOpened();
    }
}, 2000);
```

#### Method 2 — Window Size Differential
Compare `window.outerWidth - window.innerWidth` and `window.outerHeight - window.innerHeight`. Large differentials suggest a docked DevTools panel. This only works for docked (not undocked/detached) DevTools.

#### Method 3 — `debugger` Statement Timing
Execute a `debugger` statement and measure how long it takes. If DevTools is open, the debugger pauses execution briefly even when "pause on debugger statements" is off in some browsers. This is the least reliable and most intrusive method — used sparingly.

### Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                        Client (Blazor WASM)                          │
│                                                                      │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │                    MainLayout.razor                              │ │
│  │                                                                  │ │
│  │  ┌───────────────────────────────────────────────────────────┐  │ │
│  │  │            ConsoleWatcher Component                       │  │ │
│  │  │  - Initializes JS module on AfterRender                   │  │ │
│  │  │  - Receives callback when DevTools detected               │  │ │
│  │  │  - Shows optional in-page toast                           │  │ │
│  │  └───────────────────────────────────────────────────────────┘  │ │
│  └─────────────────────────────────────────────────────────────────┘ │
│                                                                      │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │                  console-watcher.js (ES module)                  │ │
│  │  - DevTools detection logic (Methods 1 & 2)                     │ │
│  │  - Styled console.log message output                            │ │
│  │  - Fires only once per session (sessionStorage flag)            │ │
│  │  - Exports: initialize(), dispose()                             │ │
│  └─────────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────────┘
```

### Key Constraints

- **One-time per session**: The message fires once per browser session (tracked via `sessionStorage`). Reopening DevTools doesn't repeat it.
- **No polling in background tabs**: Detection pauses when the tab is not visible (`document.visibilityState`).
- **Graceful degradation**: If detection fails or is blocked, nothing happens — no errors, no broken functionality.
- **No production `debugger` statements**: Method 3 is excluded from the default implementation to avoid interfering with developers actually debugging the site.

---

## Vertical Slices

### Slice 1: Console Message on Page Load

**What changes:**

- **Client — JS**: Create `wwwroot/js/console-watcher.js` ES module that:
  - Prints the styled warning message to the console immediately on initialization
  - Uses `console.log('%c ...', 'font-size: ...; color: ...; ...')` for styled output
  - Guards against repeat messages with a `sessionStorage` flag
- **Client — Components**: Create `ConsoleWatcher.razor` component that initializes the JS module via `IJSRuntime` in `OnAfterRenderAsync`
- **Client — Layout**: Add `<ConsoleWatcher />` to `MainLayout.razor`

**Affected layers:** Client

**Acceptance criteria:**
- Opening the browser console shows the styled warning message
- Message only appears once per session
- No errors if console is never opened
- No performance impact on page load

---

### Slice 2: DevTools Open Detection

**What changes:**

- **Client — JS**: Extend `console-watcher.js` with DevTools detection (Method 1: console.log object trick, Method 2: window size differential)
- **Client — JS**: When DevTools is first detected as open, invoke a .NET callback via `DotNetObjectReference`
- **Client — Components**: Update `ConsoleWatcher.razor` to handle the detection callback

**Affected layers:** Client

**Acceptance criteria:**
- DevTools detection runs on a 2-second polling interval
- Detection pauses when tab is hidden
- Detection stops after first successful detection (doesn't keep polling)
- .NET callback fires when DevTools is detected

---

### Slice 3: In-Page Toast Notification

**What changes:**

- **Client — Components**: Update `ConsoleWatcher.razor` to show a brief, styled toast notification when DevTools is detected
- **Client — CSS**: Style the toast with a playful design (e.g., "👀 We see you!" sliding in from the bottom-right)
- **Client — Behavior**: Toast auto-dismisses after 5 seconds, can also be manually dismissed

**Affected layers:** Client

**Acceptance criteria:**
- When DevTools is first opened, a toast appears on screen
- Toast is styled consistently with the site theme
- Toast disappears after 5 seconds or on click
- Toast only appears once per session
- Toast is not shown if consent has not been granted

---

### Slice 4: Consent Integration

**What changes:**

- **Client — Components**: Update `ConsoleWatcher.razor` to check consent status via `IConsentService` before activating detection
- **Client — JS**: Accept a configuration flag from Blazor to enable/disable detection

**Affected layers:** Client

**Acceptance criteria:**
- Console watcher is inactive until the user has granted consent
- If consent is later revoked, detection stops
- The static console message (Slice 1) still appears regardless of consent (it's just a `console.log`, not tracking)

---

## Design Decisions

1. **Fun, not hostile**: The tone is playful and welcoming to developers. This is an Easter egg, not DRM. We explicitly invite them to check out the GitHub repo.
2. **Console message always, detection opt-in**: The styled `console.log` message prints regardless of consent — it's not tracking. The DevTools *detection* (polling, callbacks, toast) only activates with consent, since it involves active monitoring.
3. **No `debugger` statement**: Using `debugger` to detect DevTools is too intrusive and would annoy real developers debugging the site. Excluded by design.
4. **Session-scoped**: Everything is session-scoped — no persistent tracking of console usage. This is ephemeral fun, not analytics.
5. **Lightweight JS module**: The detection logic lives in a small, dedicated ES module (`console-watcher.js`) rather than being inlined in Blazor components, keeping the JS interop clean and the module testable.

---

## Dependencies

- No external dependencies — uses browser APIs only

## Success Criteria

- [ ] Styled ASCII art message appears in the browser console on first visit
- [ ] DevTools detection fires callback when developer tools are opened
- [ ] In-page toast notification appears on first DevTools detection
- [ ] All detection respects consent preferences
- [ ] Console message displays only once per session
- [ ] No performance degradation from detection polling
- [ ] All tests passing with 90%+ coverage
