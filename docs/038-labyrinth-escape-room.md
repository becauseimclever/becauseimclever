# 038 - Labyrinth Escape Room

## Status: ✅ Complete

## Feature Description

Create an interactive labyrinth-style escape room experience as an Easter egg / experiment on the site. The experience is styled in the visual aesthetic of **Microsoft Bob** — friendly, cartoonish room-based navigation with clickable objects, speech-bubble hints, and a warm, early-90s UI feel. Visitors navigate through interconnected rooms, solving puzzles to "escape" the labyrinth. The feature is linked from the main navigation as an "Experiments" menu item.

## Goals

1. Build a multi-room escape room with 4–6 themed rooms connected in a non-linear labyrinth layout
2. Implement point-and-click interaction with objects in each room (Microsoft Bob style)
3. Include 4–6 puzzles of varying difficulty that gate progression through doors/exits
4. Use a Microsoft Bob–inspired visual style: isometric-ish room views, Clippy as the guide character, speech-bubble dialogue, chunky UI elements, pastel/warm color palette
5. Track puzzle completion state client-side so progress persists within a session
6. Add an "Experiments" link in the main navigation menu
7. Provide a completion screen with a congratulatory message and time taken
8. Randomize puzzle solutions each playthrough so the experience is infinitely replayable
9. Support seedable randomization for deterministic test scenarios
10. Provide a "Start Over" option that resets all state and generates new puzzles, with a visible attempt counter

## Design Inspiration — Microsoft Bob

- **Room-based navigation**: Each "screen" is a room rendered as a stylized illustration with clickable hotspots (doors, objects, drawers, paintings, etc.)
- **Guide character**: Clippy, the iconic paperclip assistant from Microsoft Office, gives hints via speech bubbles ("It looks like you're trying to escape! Would you like help?")
- **UI chrome**: Chunky window borders, large friendly buttons, rounded corners, drop shadows, pastel backgrounds
- **Typography**: Rounded, friendly sans-serif fonts (e.g., Comic Neue as a web-safe nod to Comic Sans)
- **Sound effects** (optional, off by default): Click sounds, puzzle-solved jingle, door opening

---

## Technical Design

### Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                        Client (Blazor WASM)                          │
│                                                                      │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │               EscapeRoom.razor (/experiments/escape-room)      │  │
│  │                                                                │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌─────────────────────┐  │  │
│  │  │  RoomView    │  │  GuidePanel  │  │  InventoryBar       │  │  │
│  │  │  Component   │  │  (speech     │  │  (collected items)  │  │  │
│  │  │  (SVG/HTML   │  │   bubbles)   │  │                     │  │  │
│  │  │   room scene)│  │              │  │                     │  │  │
│  │  └──────────────┘  └──────────────┘  └─────────────────────┘  │  │
│  │                                                                │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌─────────────────────┐  │  │
│  │  │ PuzzleModal  │  │ DoorTransit  │  │  CompletionScreen   │  │  │
│  │  │ (per-puzzle  │  │ (animation   │  │  (win state, time)  │  │  │
│  │  │  overlay)    │  │  between     │  │                     │  │  │
│  │  │              │  │  rooms)      │  │                     │  │  │
│  │  └──────────────┘  └──────────────┘  └─────────────────────┘  │  │
│  └────────────────────────────────────────────────────────────────┘  │
│                                                                      │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │                   EscapeRoomStateService                       │  │
│  │  - Current room                                                │  │
│  │  - Inventory (collected items)                                 │  │
│  │  - Solved puzzles set                                          │  │
│  │  - Unlocked doors set                                          │  │
│  │  - Timer (start time, elapsed)                                 │  │
│  │  - Puzzle seed (int, drives randomization)                     │  │
│  │  - Attempt counter (increments on each Start Over)             │  │
│  │  - Persisted to sessionStorage via JS interop                  │  │
│  └────────────────────────────────────────────────────────────────┘  │
│                                                                      │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │                   PuzzleRandomizer                             │  │
│  │  - Accepts seed (int) → deterministic Random instance          │  │
│  │  - Generates puzzle parameters per room from seed              │  │
│  │  - Same seed always produces identical puzzles (testable)      │  │
│  │  - New seed generated on each new attempt                      │  │
│  └────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────┘
```

### Room & Puzzle Design

The labyrinth consists of rooms connected by locked/unlocked doors. Each room has interactive objects and at least one puzzle. Solving a puzzle yields a key, code, or item needed to unlock a door to another room.

### Puzzle Randomization

Every puzzle is parameterized so that the *structure* stays the same but the *solution* changes each playthrough. A single integer **seed** (stored in session state) drives a `System.Random` instance that generates all puzzle parameters. This guarantees:

- **Replayability**: Starting over produces new solutions every time.
- **Session consistency**: Navigating away and returning within the same session preserves the same puzzles — the seed and solved-state are persisted in `sessionStorage`.
- **Testability**: Passing a known seed (e.g., via query string `?seed=42`) produces deterministic, reproducible puzzles for automated and manual testing.

**Randomization per puzzle:**

| Room | What's randomized | Seed-driven parameter |
|------|-------------------|----------------------|
| Foyer | The dates on the picture frames and their correct chronological order | Shuffled date set + ordering |
| Library | The cipher key and encoded message | Random substitution key + message selection |
| Kitchen | The ingredient list, their order, and the recipe clue text | Shuffled ingredient pool + sequence |
| Study | The logic grid constraints and solution | Generated constraint set from valid solution |
| Garden | The hedge maze layout (walls and path) | Procedurally generated maze from seed |
| Exit | The combined exit code digits | Derived from Study + Garden puzzle outputs |

**`PuzzleRandomizer` class:**

```csharp
public class PuzzleRandomizer
{
    private readonly Random _random;
    public int Seed { get; }

    public PuzzleRandomizer(int seed)
    {
        Seed = seed;
        _random = new Random(seed);
    }

    public static PuzzleRandomizer NewGame()
        => new(Environment.TickCount);

    public static PuzzleRandomizer FromSeed(int seed)
        => new(seed);

    public FoyerPuzzleParams GenerateFoyerPuzzle() { /* ... */ }
    public LibraryPuzzleParams GenerateLibraryPuzzle() { /* ... */ }
    // ... one method per puzzle
}
```

**Room Map (non-linear):**

```
                ┌──────────┐
                │  START   │
                │  Foyer   │
                └────┬─────┘
                     │
              ┌──────┴──────┐
              │              │
        ┌─────┴────┐  ┌─────┴────┐
        │  Library  │  │  Kitchen │
        └─────┬────┘  └─────┬────┘
              │              │
        ┌─────┴────┐  ┌─────┴────┐
        │  Study   │  │  Garden  │
        └─────┬────┘  └─────┬────┘
              │              │
              └──────┬───────┘
                     │
              ┌──────┴──────┐
              │    EXIT     │
              │  Front Door │
              └─────────────┘
```

**Puzzles (examples):**

| Room | Puzzle | Type | Reward |
|------|--------|------|--------|
| Foyer | Rearrange picture frames by date (dates randomized per seed) | Sorting/ordering | Opens Library & Kitchen doors |
| Library | Decode a message using a book cipher (cipher key randomized) | Cipher/decode | Brass key (opens Study) |
| Kitchen | Mix ingredients in correct order (recipe & ingredients randomized) | Sequence | Garden gate key |
| Study | Solve a logic grid puzzle on the chalkboard (constraints randomized) | Logic | Half of the exit code |
| Garden | Navigate a hedge maze mini-game (maze layout randomized) | Spatial | Other half of the exit code |
| Exit | Enter the combined exit code (derived from randomized puzzle outputs) | Combination | Escape! |

### Guide Character — Clippy

- Clippy (the paperclip assistant from Microsoft Office) rendered as a CSS-animated sprite positioned in the corner of the room
- Classic Clippy poses: idle bounce, thinking (tapping chin), pointing at objects, celebrating (confetti)
- Speaks through a speech-bubble `<div>` with typed-out text effect, using Clippy's signature phrasing ("It looks like you're trying to...")
- Offers contextual hints based on current room and puzzle state
- Idle animations when not speaking (eyebrow raise, subtle bounce)
- Clicking Clippy triggers a hint for the current room/puzzle

---

## Vertical Slices

### Slice 1: Navigation & Page Shell ✅

**What changes:**

- **Client — Layout**: Add "Experiments" link to the main navigation in `MainLayout.razor`, pointing to `/experiments/escape-room`
- **Client — Pages**: Create `Pages/Experiments/EscapeRoom.razor` page with route `/experiments/escape-room`
- **Client — Page Shell**: Render a Microsoft Bob–styled frame (chunky border, title bar, pastel background) with placeholder content

**Affected layers:** Client

**Acceptance criteria:**
- "Experiments" appears in the main navigation menu
- Clicking it navigates to `/experiments/escape-room`
- Page renders with Microsoft Bob–inspired visual chrome
- Page is accessible without authentication

---

### Slice 2: Room Rendering & Navigation ✅

**What changes:**

- **Client — Components**: Create `RoomView.razor` component that renders a room scene using HTML/CSS (or inline SVG) with clickable hotspot areas
- **Client — Components**: Create `DoorTransition.razor` for animated transitions between rooms
- **Client — State**: Create `EscapeRoomStateService` to track current room, solved puzzles, inventory, timer, puzzle seed, and attempt counter
- **Client — State**: Create `PuzzleRandomizer` class that accepts an integer seed and generates deterministic puzzle parameters
- **Client — Data**: Define room data model (rooms, connections, hotspot coordinates, descriptions) as static C# objects or JSON
- **Client — JS Interop**: Add `escape-room.js` module for sessionStorage persistence of game state (including seed and attempt count)

**Affected layers:** Client

**Acceptance criteria:**
- Rooms render with background art and clickable hotspot overlays
- Clicking a door navigates to the connected room (if unlocked) with a transition animation
- Locked doors display a speech-bubble hint from Clippy
- Game state (including puzzle seed and attempt count) persists across page refreshes within the same browser session
- Passing `?seed=42` in the URL produces deterministic puzzles for testing

---

### Slice 3: Clippy Guide & Speech Bubbles ✅

**What changes:**

- **Client — Components**: Create `Clippy.razor` with CSS sprite animation (idle bounce, thinking, pointing, celebrating)
- **Client — Components**: Create `SpeechBubble.razor` with typewriter text effect
- **Client — Assets**: Create Clippy sprite sheet (SVG or PNG) with key poses
- **Client — Data**: Define hint text per room and puzzle state, using Clippy's signature "It looks like you're trying to..." phrasing

**Affected layers:** Client

**Acceptance criteria:**
- Clippy appears in each room with an idle bounce animation
- Clippy speaks contextual hints when entering a room or when clicked
- Hints use Clippy's classic phrasing style
- Text appears with a typewriter effect
- Clippy does a celebration animation when a puzzle is solved

---

### Slice 4: Puzzle Framework & First Puzzle ✅

**What changes:**

- **Client — Components**: Create `PuzzleModal.razor` as an overlay container for puzzle UIs
- **Client — Components**: Create `SortingPuzzle.razor` (Foyer puzzle — drag-and-drop picture frame ordering, dates driven by `PuzzleRandomizer`)
- **Client — State**: Update `EscapeRoomStateService` with puzzle completion tracking

**Affected layers:** Client

**Acceptance criteria:**
- Clicking the interactive object in the Foyer opens the puzzle modal
- Player can drag/reorder picture frames
- Correct order triggers a "solved" animation and unlocks the Library and Kitchen doors
- Puzzle dates/order are different each playthrough (driven by seed)
- Puzzle state persists in session

---

### Slice 5: Remaining Puzzles ✅

**What changes:**

- **Client — Components**: Create puzzle components for each remaining room:
  - `CipherPuzzle.razor` (Library) — cipher wheel substitution decode, text input, case-insensitive
  - `SequencePuzzle.razor` (Kitchen) — click-to-select ingredient buttons in correct order
  - `LogicGridPuzzle.razor` (Study) — chalkboard with 3 solvable equations revealing a 3-digit code
  - `MazePuzzle.razor` (Garden) — 7×7 grid navigation from (0,0) to (6,6), reveals 3-digit code
  - `ExitCodePuzzle.razor` (Exit) — 6-digit combination lock (Study 3 digits + Garden 3 digits)
- **Client — Page**: Wired all 5 puzzles into `EscapeRoom.razor` hotspot/modal system
- **Client — Items**: Library puzzle grants `brass-key`, Kitchen puzzle grants `garden-key`

**Affected layers:** Client

**Acceptance criteria:**
- ✅ Each puzzle is interactive, solvable, and rewards the correct item/code
- ✅ Each puzzle's solution is randomized per seed — replaying with a different seed produces different solutions
- ✅ Solving all puzzles enables the player to reach and unlock the exit
- ✅ Clippy provides puzzle-specific hints when a puzzle modal is open (more targeted than room-level hints)

---

### Slice 6: Inventory Bar & Item System ✅

**What changes:**

- **Client — Components**: Create `InventoryBar.razor` displayed at the bottom of the screen (Microsoft Bob–style toolbar)
- **Client — Models**: Create `InventoryItemCatalog` static catalog mapping item IDs to display metadata (name, icon, description)
- **Client — State**: Add `SelectedItem` property and `SelectItem` method to `EscapeRoomStateService` for item selection/toggle
- **Client — State**: Add `UnlockedDoors` tracking and `UnlockDoor` method — item-gated doors now require explicit item use (select item + click door)
- **Client — Interaction**: Clicking a locked door with the correct item selected consumes the item and permanently unlocks the door

**Affected layers:** Client

**Acceptance criteria:**
- Collected items appear as icons in the inventory bar
- Player can select an item and click a door/object to "use" it
- Using the correct item on the correct target unlocks the interaction
- Inventory persists in session state

---

### Slice 7: Completion Screen & Timer ✅

**What changes:**

- ✅ **Client — Components**: Created `CompletionScreen.razor` with congratulatory message, elapsed time (M:SS / H:MM:SS), attempt number, and "Play Again" button
- ✅ **Client — Components**: "Start Over" button now shows a confirmation dialog ("Reset all progress?" with Yes/No) before resetting
- ✅ **Client — State**: On "Play Again" or "Start Over": increments attempt counter, generates a new seed, resets room/inventory/puzzle state, persists to sessionStorage
- ✅ **Client — Styling**: Microsoft Bob celebration screen with confetti animation (30 falling pieces) and Clippy celebrating (📎🎉)
- ✅ **Client — UI**: Attempt number displayed in completion screen stats
- ✅ **Client — Integration**: CompletionScreen wired into EscapeRoom.razor, shown when `IsGameComplete` is true; InventoryBar and Clippy hidden during completion

**Affected layers:** Client

**Acceptance criteria:**
- Entering the correct exit code shows the completion screen with elapsed time and attempt number
- "Play Again" generates a new seed, increments attempt counter, and returns to the Foyer with fresh puzzles
- "Start Over" button is available during gameplay, confirms before resetting
- Starting over increments the attempt counter and generates a new puzzle seed
- Attempt counter persists in sessionStorage and is visible in the game HUD
- Clippy does a celebration animation

---

### Slice 8: Visual Polish & Responsive Design ✅

**What changes:**

- ✅ **Client — Components**: Created `SmallScreenMessage.razor` with Clippy emoji, friendly suggestion to use desktop/tablet
- ✅ **Client — CSS**: Added responsive media queries to all components (768px–1024px tablet scaling, <768px hides main window)
- ✅ **Client — CSS**: Added hotspot discoverability pulse animation (subtle dashed border pulse on idle, solid on hover)
- ✅ **Client — CSS**: Added door discoverability (🚪 emoji indicator on unlocked doors)
- ✅ **Client — Responsive**: SmallScreenMessage shown only on screens <768px (via CSS `display: none`/`flex` media query)
- ✅ **Client — Responsive**: Main `.bob-window` hidden on screens <768px, scaled to 95% width on tablets
- ✅ **Client — Responsive**: All puzzle components, modal, Clippy, speech bubble, and completion screen scale gracefully on tablets

**Affected layers:** Client

**Acceptance criteria:**
- ✅ Consistent Microsoft Bob aesthetic across all rooms
- ✅ All hotspots are visually discoverable (subtle hover effects)
- ✅ Readable and playable on screens ≥ 768px wide
- ✅ Small-screen visitors see a friendly message suggesting desktop

---

## Design Decisions

1. **Client-only**: The escape room is entirely client-side with no server API. State is held in memory and sessionStorage. This keeps it lightweight and avoids unnecessary server complexity for what is an Easter egg feature.
2. **No leaderboard (initially)**: A server-side leaderboard could be added later but is out of scope for the initial implementation.
3. **SVG + HTML rooms over Canvas**: Using HTML/CSS/SVG for room rendering keeps the implementation within Blazor's component model and avoids heavy JS interop. Clickable hotspots are just positioned `<div>` or `<area>` elements.
4. **Comic Neue font**: Used as a web-safe, open-source nod to Comic Sans MS, which was a hallmark of the Microsoft Bob era.
5. **No audio by default**: Sound effects are opt-in to respect user preferences and accessibility.
6. **Seedable `System.Random`**: Using `new Random(seed)` ensures puzzle generation is fully deterministic for a given seed. The seed is an `int` generated from `Environment.TickCount` for real play. For tests, a fixed seed can be injected via `PuzzleRandomizer.FromSeed(seed)` or via the `?seed=` query string parameter.
7. **Session-scoped persistence**: All game state (seed, attempt counter, solved puzzles, inventory, current room, timer) is serialized to `sessionStorage`. This means the player can navigate away and return to exactly where they left off, but closing the browser tab starts completely fresh. This is intentional — the escape room is casual and ephemeral.
8. **Attempt counter never resets within a session**: The attempt counter only resets when the session is cleared (tab close / manual clear). This gives players a sense of how many tries it took without any server-side tracking.

---

## Dependencies

- Comic Neue font (Google Fonts) for Microsoft Bob–era typography
- No server-side dependencies — entirely client-side

## Success Criteria

- [x] "Experiments" link appears in navigation and loads the escape room page
- [x] 4–6 rooms render with interactive hotspots and room transitions
- [x] Each puzzle is solvable and gates progression through doors
- [x] Puzzle solutions are randomized per seed, producing different solutions each playthrough
- [x] Clippy provides contextual hints and celebrates puzzle completions
- [x] Inventory system allows collecting and using items
- [x] Completion screen shows elapsed time and attempt count
- [x] "Start Over" generates a new seed and increments the attempt counter
- [x] Game state persists in sessionStorage across page refreshes
- [x] All tests passing with 90%+ coverage on escape room components
