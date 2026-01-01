# 024 - Monopoly Theme

## Feature Description
Add a new "Monopoly" theme to the application, inspired by the classic board game.

## Goals
- Provide a fun, nostalgic theme option for users.
- Implement distinct visual styles for the Monopoly theme (colors, fonts, shadows).

## Technical Approach
- Add `Monopoly` to the `Theme` domain entity in `BecauseImClever.Domain`.
- Add CSS variables and specific component styles for `[data-theme="monopoly"]` in `site.css`.

## Affected Components
- `BecauseImClever.Domain.Entities.Theme`
- `BecauseImClever.Client/wwwroot/css/site.css`

## Design Decisions
- **Colors**:
  - Background: Cream (#FBFBEA) to resemble property cards.
  - Secondary Background: Board Green (#CDE6D0).
  - Accent: Hotel Red (#ED1B24).
  - Buttons: Chance Orange (#F7941D).
- **Typography**: Verdana/Geneva for body text, Courier New for monospace.
- **Visuals**: Hard black shadows and borders to mimic the printed board game aesthetic.
