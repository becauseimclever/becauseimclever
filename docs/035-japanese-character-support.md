# 035 - Japanese Character Support

## Status: ✅ Completed

## Feature Description

Add support for Japanese characters (Hiragana, Katakana, and Kanji) throughout the site to enable blog posts and content with Japanese text. This enhancement supports the author's journey learning Japanese and allows for bilingual or Japanese-focused content.

## Goals

- Enable proper rendering of Japanese characters across all pages
- Ensure Japanese text displays correctly in blog posts
- Maintain readability and aesthetic consistency with existing themes
- Support mixed English/Japanese content seamlessly

## Design Decisions

- **Font Choice**: Noto Sans JP chosen for broad character coverage and Google Fonts availability.
- **Loading Strategy**: Use `font-display: swap` to prevent invisible text during font load. Preconnect to Google Fonts origin for faster resolution.
- **No RTL Support Needed**: Japanese is read left-to-right (horizontal) or top-to-bottom (vertical); vertical layout is out of scope.
- **Theme Strategy**: Append Noto Sans JP as a fallback to each theme's existing `--font-body` stack rather than replacing primary fonts, preserving each theme's visual identity.
- **CJK Word Breaking**: Use `overflow-wrap: anywhere` on content areas to handle long unbroken Japanese strings without disrupting English text flow.

---

## Vertical Slices

### Slice 1 — Base Font Loading & Default Theme Typography

**Goal**: Load the Noto Sans JP web font and integrate it into the default theme so Japanese characters render correctly site-wide using the base font stack.

**Independently shippable**: Yes — after this slice, any page using the default theme will correctly display Japanese text.

#### Client — `wwwroot/index.html`
- Add a `<link rel="preconnect">` to `fonts.googleapis.com` and `fonts.gstatic.com` (crossorigin) in `<head>`.
- Add a `<link rel="stylesheet">` to load Noto Sans JP (weights 400, 700) from Google Fonts.

#### Client — `wwwroot/css/base/_variables.css`
- Append `"Noto Sans JP"` to the end of the `--font-body` variable (before the generic `sans-serif` keyword) so it acts as a CJK fallback without affecting English text rendering.

#### Testing
- Visual verification: create or use a test blog post containing Hiragana, Katakana, Kanji, and mixed English/Japanese text.
- Confirm font loads via browser DevTools Network tab (Noto Sans JP woff2 files requested).
- Confirm no layout shift (CLS) regression using Lighthouse or manual observation.

---

### Slice 2 — CJK Text Layout (Word Breaking & Line Height)

**Goal**: Ensure Japanese text wraps correctly and has appropriate line spacing in blog content areas.

**Independently shippable**: Yes — improves text layout for any CJK content already renderable after Slice 1.

#### Client — `wwwroot/css/components/_blog.css`
- Add `overflow-wrap: anywhere;` to the blog content container so long Japanese strings wrap rather than overflow.
- Add `word-break: normal;` to preserve default word-breaking behavior for English while allowing the `overflow-wrap` to handle CJK overflow.

#### Client — `wwwroot/css/base/_typography.css`
- Evaluate whether the current `line-height: 1.6` is sufficient for Japanese text. If adjustment is needed, introduce a slightly increased line-height (e.g., `1.8`) scoped to CJK content via a utility class or the blog content container, rather than globally.

#### Testing
- Verify long Japanese strings (no spaces) wrap correctly in blog post content.
- Verify mixed English/Japanese paragraphs break naturally.
- Verify no horizontal scrollbar appears on blog posts with Japanese text.
- Test on mobile viewport widths (375px, 414px) for proper wrapping.

---

### Slice 3 — Retro OS Theme Font Fallbacks

**Goal**: Update all 10 retro OS theme files to include Noto Sans JP in their `--font-body` overrides, ensuring Japanese text renders correctly regardless of selected theme.

**Independently shippable**: Yes — after Slice 1 delivers base support, this extends coverage to every theme.

#### Client — `wwwroot/css/themes/` (all theme files)

For each theme file, append `"Noto Sans JP"` to the `--font-body` variable value, placed just before the generic family keyword (`sans-serif`, `monospace`):

| Theme File | Current `--font-body` ending | After change |
|---|---|---|
| `_dungeon.css` | `... monospace` | `... "Noto Sans JP", monospace` |
| `_retro.css` | `... monospace` | `... "Noto Sans JP", monospace` |
| `_macos7.css` | `... sans-serif` | `... "Noto Sans JP", sans-serif` |
| `_macos9.css` | `... sans-serif` | `... "Noto Sans JP", sans-serif` |
| `_win95.css` | `... sans-serif` | `... "Noto Sans JP", sans-serif` |
| `_winxp.css` | `... sans-serif` | `... "Noto Sans JP", sans-serif` |
| `_vista.css` | `... sans-serif` | `... "Noto Sans JP", sans-serif` |
| `_raspberry-pi.css` | `... sans-serif` | `... "Noto Sans JP", sans-serif` |
| `_geocities.css` | `... sans-serif` | `... "Noto Sans JP", sans-serif` |
| `_monopoly.css` | `... sans-serif` | `... "Noto Sans JP", sans-serif` |

#### Testing
- Switch through each theme and verify sample Japanese text renders correctly.
- Verify each theme's primary English font is unaffected (Noto Sans JP should only activate for CJK characters).
- Pay special attention to monospace themes (Dungeon, Retro) where the Japanese font will look different from the monospaced English text — confirm this is acceptable.

---

## Sample Test Content

Use the following content in a test blog post to validate rendering across all slices:

```markdown
## 日本語テスト (Japanese Test)

こんにちは世界！(Hello World!)

日本語を勉強しています。(I am studying Japanese.)

### Character Sets

- ひらがな: あいうえお かきくけこ
- カタカナ: アイウエオ カキクケコ
- 漢字: 日本語 東京 勉強

### Mixed Content

This paragraph mixes English and 日本語 in the same sentence to verify that both scripts render properly together.

### Long Unbroken String

あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん
```

## Future Enhancements (Out of Scope)

- Vertical text layout support
- Ruby annotations (furigana) for kanji readings
- Language toggle for bilingual posts
- Japanese UI translations
