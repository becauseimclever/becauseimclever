# 043 - Kitchen Recipe Puzzle Hotspot Alignment

## Status: 🚧 In Progress

## Feature Description

In the Kitchen room of the escape room labyrinth, the room description mentions "a mysterious recipe pinned to the wall," and the SVG artwork includes a detailed recipe board on the **left wall**. However, all three kitchen hotspots are misaligned with their corresponding SVG elements, making it unclear where to click. Most critically, the `kitchen-recipe` puzzle hotspot — the primary interactive element in the room — doesn't overlay the visible recipe board, so players have no visual indication of where the recipe is or how to interact with it.

## Goals

1. Align all three kitchen hotspots (`kitchen-recipe`, `kitchen-stove`, `kitchen-pantry`) to accurately overlay their SVG artwork.
2. Players can see the recipe board and intuitively click on it to start the sequence puzzle.
3. The stove and pantry hotspots visually correspond to the stove and pantry shelf in the SVG.

## Root Cause

The hotspot coordinates in `RoomMap.cs` for the Kitchen room do not match the positions of the visual elements in `kitchen.svg`. The SVG viewBox is `0 0 800 350`, but the percentage-based hotspot positions were set incorrectly:

| Hotspot          | SVG Element Position (px)  | Correct % (x, y, w, h)       | Current % (x, y, w, h) |
|------------------|----------------------------|-------------------------------|-------------------------|
| `kitchen-recipe` | (30, 25) — 160×120         | ~4%, ~7%, ~20%, ~34%          | 25%, 10%, 50%, 30%      |
| `kitchen-stove`  | (330, 130) — 140×130       | ~41%, ~37%, ~18%, ~37%        | 60%, 50%, 25%, 30%      |
| `kitchen-pantry` | (600, 30) — 170×220        | ~75%, ~9%, ~21%, ~63%         | 10%, 45%, 20%, 35%      |

The pantry hotspot is on the wrong side entirely (left instead of right), and the recipe board hotspot is shifted far from the actual artwork.

## Design Decisions

- **Coordinates**: Use values computed directly from the SVG element transforms and the 800×350 viewBox, with small padding for comfortable click targets.
- **No SVG changes**: The SVG artwork is correct; only the hotspot overlay positions need adjustment.

---

## Vertical Slices

### Slice 1: Fix Kitchen Hotspot Coordinates

**Goal:** Align all kitchen hotspots to their SVG counterparts so the recipe board, stove, and pantry are clickable where the player visually sees them.

#### Client

- **`Models/EscapeRoom/RoomMap.cs`** — Update the three Kitchen hotspot coordinates:
  - `kitchen-recipe`: `(4, 7, 20, 34)` (left wall, over recipe board)
  - `kitchen-stove`: `(41, 37, 18, 37)` (center, over stove)
  - `kitchen-pantry`: `(75, 9, 21, 63)` (right wall, over pantry shelf)

#### Tests

- Verify the hotspot positions are within the expected ranges.
- Existing `PuzzleRandomizerTests` remain unaffected (puzzle logic unchanged).

---

## Dependencies

None.

## Success Criteria

- [ ] Clicking the visible recipe board on the left wall of the kitchen opens the sequence puzzle.
- [ ] The stove hotspot overlays the stove artwork in the center.
- [ ] The pantry hotspot overlays the pantry shelf on the right wall.
- [ ] All existing escape room tests pass.
