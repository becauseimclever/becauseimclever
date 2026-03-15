# 042 - Escape Room Decorative Hotspot Feedback

## Status: Planned

## Feature Description

Clicking decorative hotspots in the escape room (e.g., the foyer's "Entry Table" and "Dusty Mirror") produces no visible feedback. Players see `cursor: pointer` and a hover label, suggesting interactivity, but clicking does nothing. This applies to all `InteractionType.Decorative` hotspots across every room — not just the foyer.

## Goals

1. Clicking any decorative hotspot triggers a Clippy flavor-text response, giving the player feedback that they interacted with an object.
2. Each decorative hotspot has a unique, thematic flavor line.
3. The `InteractionType.Item` enum value is also handled so that item-granting hotspots work if added in the future.

## Root Cause

In `EscapeRoom.razor`, the `HandleHotspotClicked` method only branches on:
- A hard-coded ID check for `library-cowlevel-book`
- `InteractionType.Puzzle` with a non-null `PuzzleId`

There is no branch for `InteractionType.Decorative` or `InteractionType.Item`, so those clicks silently return `Task.CompletedTask`.

## Affected Hotspots

| Room    | Hotspot ID                  | Label                     | Type       |
|---------|-----------------------------|---------------------------|------------|
| Foyer   | `foyer-table`               | Entry Table               | Decorative |
| Foyer   | `foyer-mirror`              | Dusty Mirror              | Decorative |
| Library | `library-shelves`           | Bookshelves               | Decorative |
| Library | `library-globe`             | Old Globe                 | Decorative |
| Kitchen | `kitchen-stove`             | Cast Iron Stove           | Decorative |
| Kitchen | `kitchen-pantry`            | Pantry Shelf              | Decorative |
| Study   | `study-desk`                | Writing Desk              | Decorative |
| Study   | `study-fireplace`           | Fireplace                 | Decorative |
| Garden  | `garden-fountain`           | Stone Fountain            | Decorative |
| Garden  | `garden-cowlevel-graffiti`  | there is no cow level     | Decorative |
| Garden  | `garden-bench`              | Garden Bench              | Decorative |

> `library-cowlevel-book` is already handled via a special-case ID check and is not affected.

## Technical Approach

### Vertical Slice

This is a single vertical slice touching only the Client layer.

#### Client — `ClippyHints.cs`

Add a new `Dictionary<string, ClippyHint>` mapping each decorative hotspot ID to a unique flavor-text `ClippyHint`. Add a public `GetDecorativeHint(string hotspotId)` method that returns the matching hint or a generic fallback.

**Flavor text examples:**

| Hotspot ID                 | Clippy Text |
|----------------------------|-------------|
| `foyer-table`              | "An old entry table. There's a guest book, but all the names are smudged. Mysterious! 📎" |
| `foyer-mirror`             | "You peer into the dusty mirror... and something peers back. Just kidding — it's you! 📎" |
| `library-shelves`          | "So many books! I'd recommend 'Escaping Rooms for Dummies,' but it seems to be checked out. 📎" |
| `library-globe`            | "The globe is stuck on a continent that doesn't exist. Typical labyrinth cartography. 📎" |
| `kitchen-stove`            | "The stove is cold. Whatever was cooking here was finished a long time ago. 📎" |
| `kitchen-pantry`           | "The pantry is full of jars with strange labels. 'Pickled Moonbeams'? I'll pass. 📎" |
| `study-desk`               | "Notes and equations are scattered everywhere. Whoever worked here was brilliant — or mad. 📎" |
| `study-fireplace`          | "The embers are still warm. Someone was here recently... 📎" |
| `garden-fountain`          | "The fountain has a coin at the bottom. You could make a wish, but I'd wish for the exit code instead. 📎" |
| `garden-cowlevel-graffiti` | "Faint text on the hedge reads 'there is no cow level.' How did that get there? 📎" |
| `garden-bench`             | "A weathered bench. A good place to sit and think — if you weren't trapped in a labyrinth. 📎" |

**Generic fallback:** "Interesting, but it doesn't seem useful for escaping. Keep looking! 📎"

#### Client — `EscapeRoom.razor`

Update `HandleHotspotClicked` to add two new branches after the existing puzzle branch:

1. **`InteractionType.Decorative`** — call `ClippyHints.GetDecorativeHint(hotspot.Id)` and show the result via `ShowClippyHint`.
2. **`InteractionType.Item`** — if `hotspot.ItemId` is not null, call `State.AddItem(hotspot.ItemId)`, show a Clippy hint ("You picked up an item!"), and save state.

### Testing

#### Unit Tests — `ClippyHintsTests.cs`

- `GetDecorativeHint_WithKnownHotspotId_ReturnsUniqueHint` — verify each hotspot ID from the table above returns a non-null, non-empty hint.
- `GetDecorativeHint_WithUnknownHotspotId_ReturnsFallbackHint` — verify an unknown ID returns the generic fallback.

#### Unit Tests — `EscapeRoomTests.cs` (or equivalent)

- `HandleHotspotClicked_DecorativeHotspot_ShowsClippyHint` — verify clicking a decorative hotspot triggers Clippy with the correct flavor text.
- `HandleHotspotClicked_ItemHotspot_AddsItemToInventory` — verify clicking an item hotspot adds the item and saves state.

## Design Decisions

- Decorative hotspots keep `cursor: pointer` and their hover labels — they ARE interactive (Clippy responds), just not puzzle-gated. No CSS changes needed.
- The `library-cowlevel-book` special case is left as-is since it already works and has a unique behavior (separate from the generic decorative flow).
- `garden-cowlevel-graffiti` gets its own flavor text in the decorative hints dictionary rather than a special case, keeping the pattern consistent.
