# 040 - Escape Room Console Integration & Easter Eggs

## Status: 📋 Planning

## Feature Description

Integrate the browser console detection system (039) with the labyrinth escape room (038) to create a cohesive, playful experience when players try to peek behind the curtain. When a player opens the browser DevTools while inside the escape room, Clippy reacts with in-game dialogue rather than the generic site-wide console message. Additionally, embed classic gaming Easter eggs — including a Blizzard-inspired "cow level" secret — into the escape room and console experience.

## Goals

1. When DevTools is opened during the escape room, Clippy delivers a unique in-game admonishment instead of the standard site-wide console toast
2. The escape room contains a visible "there is no cow level" reference somewhere in the environment
3. Typing `ThereIsNoCowLevel` into the browser console while on the escape room page triggers a hidden Diablo-themed Easter egg message
4. All interactions are lighthearted, fun, and rewarding for players who explore

## Dependencies

- [038 — Labyrinth Escape Room](038-labyrinth-escape-room.md)
- [039 — Browser Console Detection](039-browser-console-detection.md)

---

## Technical Design

### Clippy DevTools Reaction

When the console detection system (039) detects DevTools opening, it invokes a .NET callback. On the escape room page, the `ConsoleWatcher` component delegates to `EscapeRoomStateService` instead of showing the generic toast. Clippy then plays a unique animation and speech-bubble sequence:

**Clippy dialogue pool (randomly selected from seed):**

- "It looks like you're trying to cheat! Would you like help with that? ...Just kidding. I can't help you there. 📎"
- "Hey! Those developer tools won't help you escape. Trust me, I've tried. 📎"
- "I see you opened the console. Bold move. The puzzles are solved with brains, not breakpoints! 📎"
- "Oh no, not the dev tools! I'm telling the other rooms you're snooping. 📎"
- "Fun fact: the escape code is definitely not stored in a global variable. Definitely not. 👀📎"

Clippy adopts a **suspicious/scolding pose** (arms crossed, raised eyebrow) during this dialogue, distinct from the normal hint poses.

If DevTools is opened multiple times in the same session, Clippy's responses escalate:

1. First open: Playful warning (from pool above)
2. Second open: "You again? I'm starting to think you're not even trying the puzzles. 📎"
3. Third+ open: "At this point I'm just impressed by your persistence. Fine, I'll allow it. 📎" (Clippy shrugs)

### "There Is No Cow Level" — In-Game Reference

In the **Library room**, one of the book spines on the bookshelf reads: *"There Is No Cow Level: A Comprehensive History of Bovine Denial"* by E. Brevik. Clicking the book causes Clippy to say: "That book? It's just an old legend. There is no cow level. Move along. 📎"

The text also appears in one other subtle location — scrawled as faint graffiti on the wall of the **Garden** room hedge maze: "there is no cow level" in a barely visible, mossy-green font that blends with the hedge texture.

### Secret Console Command — `ThereIsNoCowLevel`

When the user types `ThereIsNoCowLevel` (case-insensitive) into the browser console while on the escape room page, a styled console message appears:

```
🐄🔥 M O O .

You found the secret. But you shouldn't have come here.

The Lord of Terror sends his regards.
Sanctuary has fallen, hero. The Prime Evils walk the earth once more,
and not even the Horadrim can save you now.

Stay a while and listen... or better yet, get back to the puzzle.
You still have rooms to clear. 🐄🔥

(Secret unlocked: Cow Level Denier 🏆)
```

Styled with:
- `%c` formatting: large, red-orange gradient text for "M O O ."
- Dark background block for the body text (Diablo dungeon aesthetic)
- Smaller, warm gold text for the Deckard Cain quote ("Stay a while and listen")

**In-game effect**: After the command is entered, a small cow skull icon (🐄💀) appears in the inventory bar as a cosmetic badge labeled "Cow Level Denier." It has no gameplay effect but persists for the session. Clippy also reacts: "Wait... how do you know about that? That level doesn't exist! 📎"

### Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                   EscapeRoom.razor Page                               │
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐    │
│  │ ConsoleWatcher (from 039)                                    │    │
│  │  - Detects DevTools open                                     │    │
│  │  - On escape room page: delegates to EscapeRoomStateService  │    │
│  │  - Suppresses generic toast in favor of Clippy reaction      │    │
│  └──────────────┬───────────────────────────────────────────────┘    │
│                  │ OnDevToolsDetected callback                       │
│                  ▼                                                   │
│  ┌──────────────────────────────────────────────────────────────┐    │
│  │ EscapeRoomStateService                                       │    │
│  │  - devToolsOpenCount (int, session-persisted)                │    │
│  │  - cowLevelUnlocked (bool, session-persisted)                │    │
│  │  - Triggers Clippy admonishment dialogue                     │    │
│  └──────────────┬───────────────────────────────────────────────┘    │
│                  │                                                   │
│                  ▼                                                   │
│  ┌──────────────────────────────────────────────────────────────┐    │
│  │ Clippy Component                                             │    │
│  │  - Suspicious/scolding pose animation                        │    │
│  │  - Escalating dialogue based on devToolsOpenCount            │    │
│  │  - Cow Level reaction dialogue                               │    │
│  └──────────────────────────────────────────────────────────────┘    │
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐    │
│  │ console-watcher.js (extended)                                │    │
│  │  - Registers window.__moo = function for cow level command   │    │
│  │  - Intercepts console input via Object.defineProperty on     │    │
│  │    window.ThereIsNoCowLevel                                  │    │
│  │  - Outputs styled Diablo-themed console message              │    │
│  │  - Invokes .NET callback to unlock cow badge                 │    │
│  └──────────────────────────────────────────────────────────────┘    │
└──────────────────────────────────────────────────────────────────────┘
```

### Console Command Detection

The `ThereIsNoCowLevel` input is detected by defining a getter on `window` that fires when the expression is evaluated in the console:

```javascript
Object.defineProperty(window, 'ThereIsNoCowLevel', {
    get() {
        triggerCowLevel();
        return '🐄 Moo.';
    },
    configurable: true
});
```

When the user types `ThereIsNoCowLevel` in the console and presses Enter, the getter fires, printing the styled message and invoking the .NET callback. A case-insensitive variant (`thereisnocowlevel`) is also registered.

---

## Vertical Slices

### Slice 1: Clippy DevTools Admonishment

**What changes:**

- **Client — Components**: Update `ConsoleWatcher.razor` to accept an `IsEscapeRoomActive` parameter (or detect the current route)
- **Client — Components**: When on the escape room page and DevTools is detected, invoke `EscapeRoomStateService.OnDevToolsOpened()` instead of showing the generic toast
- **Client — State**: Add `DevToolsOpenCount` to `EscapeRoomStateService`, persisted to sessionStorage
- **Client — Components**: Update `Clippy.razor` to support a "suspicious" pose and render the admonishment dialogue
- **Client — Data**: Add the admonishment dialogue pool and escalation logic
- **Client — JS**: Update `console-watcher.js` to skip the generic styled console message when on the escape room page (Clippy handles it in-game instead)

**Affected layers:** Client

**Acceptance criteria:**
- Opening DevTools on the escape room page triggers Clippy's admonishment dialogue, not the generic console toast
- First, second, and third+ openings produce escalating responses
- Clippy shows the suspicious/scolding pose during admonishment
- DevTools open count persists in sessionStorage
- The generic console toast still works on all other pages as per feature 039

---

### Slice 2: "There Is No Cow Level" Environmental References

**What changes:**

- **Client — Components**: Add a clickable book spine to the Library room's bookshelf hotspots: *"There Is No Cow Level: A Comprehensive History of Bovine Denial"*
- **Client — Data**: Add Clippy response dialogue for clicking the cow level book
- **Client — Assets**: Add faint "there is no cow level" graffiti text to the Garden room hedge maze background
- **Client — CSS**: Style the graffiti as barely visible mossy-green text that blends with the hedge

**Affected layers:** Client

**Acceptance criteria:**
- The book is visible on the Library bookshelf and clickable
- Clicking the book triggers a Clippy speech bubble dismissing the cow level
- The graffiti is present in the Garden maze but subtle enough to reward observant players
- Neither reference affects puzzle progression — they are purely cosmetic Easter eggs

---

### Slice 3: Secret Console Command & Cow Level Badge

**What changes:**

- **Client — JS**: Extend `console-watcher.js` (or `escape-room.js`) to register `Object.defineProperty` on `window.ThereIsNoCowLevel` (and case variant) when on the escape room page
- **Client — JS**: On trigger, output the styled Diablo-themed console message and invoke a .NET callback
- **Client — State**: Add `CowLevelUnlocked` boolean to `EscapeRoomStateService`, persisted to sessionStorage
- **Client — Components**: Update `InventoryBar.razor` to display the cosmetic cow skull badge (🐄💀 "Cow Level Denier") when unlocked
- **Client — Components**: Trigger a Clippy reaction ("Wait... how do you know about that?") when the badge is first unlocked

**Affected layers:** Client

**Acceptance criteria:**
- Typing `ThereIsNoCowLevel` in the console on the escape room page prints the styled Diablo message
- The cow skull badge appears in the inventory bar after the command is triggered
- The badge is cosmetic only — no gameplay effect
- Clippy reacts in-game when the badge is first unlocked
- The secret persists for the session (badge doesn't disappear on room navigation)
- The command does nothing on non-escape-room pages
- Typing the command a second time shows a shorter message: "You already found the secret. The cows remember. 🐄"

---

## Design Decisions

1. **Page-aware console detection**: The `ConsoleWatcher` component checks whether the escape room is active before deciding how to handle DevTools detection. This avoids Clippy appearing on non-game pages and keeps the generic 039 behavior intact elsewhere.
2. **Escalating dialogue, not punishment**: Opening DevTools never penalizes the player (no puzzle resets, no locked doors). The escalation is purely comedic — Clippy gets progressively more resigned rather than angry.
3. **`Object.defineProperty` for console command**: This is the standard technique for detecting when a user types an identifier in the console. It's non-intrusive and doesn't require polling or eval hooks. The getter returns a string (`'🐄 Moo.'`) so the console also shows a brief return value.
4. **Cosmetic badge, not gameplay advantage**: The cow skull badge rewards curiosity without breaking puzzle balance. It cannot be "used" on any hotspot.
5. **Diablo tone, not Diablo IP**: The Easter egg references are thematic homages (Prime Evils, Sanctuary, Horadrim, "Stay a while and listen") rather than using specific trademarked character names or artwork. This keeps it in tribute/parody territory.
6. **Case-insensitive command**: Both `ThereIsNoCowLevel` and `thereisnocowlevel` (and any mixed case) are handled by registering the property in a case-insensitive manner via multiple property definitions.

---

## Success Criteria

- [ ] Opening DevTools on the escape room page triggers Clippy's admonishment (not the generic toast)
- [ ] Admonishment dialogue escalates across repeated DevTools openings
- [ ] "There Is No Cow Level" book is clickable in the Library room with Clippy response
- [ ] Faint cow level graffiti is visible in the Garden maze
- [ ] Typing `ThereIsNoCowLevel` in the console prints the styled Diablo message
- [ ] Cow skull badge appears in inventory bar after console command
- [ ] All Easter eggs are cosmetic only — no gameplay impact
- [ ] Generic console detection (039) still works on non-escape-room pages
- [ ] All tests passing with 90%+ coverage
