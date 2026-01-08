# 035 - Japanese Character Support

## Feature Description

Add support for Japanese characters (Hiragana, Katakana, and Kanji) throughout the site to enable blog posts and content with Japanese text. This enhancement supports the author's journey learning Japanese and allows for bilingual or Japanese-focused content.

## Goals

- Enable proper rendering of Japanese characters across all pages
- Ensure Japanese text displays correctly in blog posts
- Maintain readability and aesthetic consistency with existing themes
- Support mixed English/Japanese content seamlessly

## Technical Approach

### 1. Font Configuration

Add web fonts that support Japanese character sets:

- **Noto Sans JP** - Clean sans-serif font for body text
- **Noto Serif JP** - Serif option for headings or emphasis
- Consider fallback fonts for optimal loading performance

### 2. CSS Updates

```css
/* Add Japanese font stack to existing typography */
font-family: 'Noto Sans JP', 'Hiragino Sans', 'Yu Gothic', sans-serif;
```

### 3. Considerations

- **Font Loading**: Use `font-display: swap` to prevent invisible text during load
- **File Size**: Japanese fonts are larger; consider subsetting or using variable fonts
- **Line Height**: Japanese text may require adjusted line-height for readability
- **Word Wrapping**: Implement `word-break: keep-all` or `overflow-wrap` rules for proper Japanese text wrapping

## Affected Components/Layers

| Layer | Component | Changes |
|-------|-----------|---------|
| Client | wwwroot/css | Add Japanese font imports and typography rules |
| Client | App.razor or _Host.cshtml | Add font preload links for performance |
| Client | Theme files | Update each theme's font stack |

## Implementation Steps

1. Add Google Fonts or self-hosted Japanese font files
2. Update base CSS with Japanese font fallbacks
3. Adjust typography settings (line-height, letter-spacing) for Japanese text
4. Test rendering across all themes (including retro OS themes)
5. Verify blog post markdown renders Japanese correctly
6. Test on mobile devices for proper display

## Design Decisions

- **Font Choice**: Noto Sans JP chosen for broad character coverage and Google Fonts availability
- **Loading Strategy**: Preload fonts to minimize layout shift
- **No RTL Support Needed**: Japanese is read left-to-right (horizontal) or top-to-bottom (vertical), but vertical layout is out of scope for this feature

## Testing Considerations

- Visual testing with sample Japanese text (Hiragana, Katakana, Kanji mix)
- Performance testing for font load times
- Cross-browser compatibility (Chrome, Firefox, Safari, Edge)
- Theme compatibility across all available themes

## Sample Test Content

```
こんにちは世界！(Hello World!)
日本語を勉強しています。(I am studying Japanese.)
ひらがな カタカナ 漢字
```

## Future Enhancements (Out of Scope)

- Vertical text layout support
- Ruby annotations (furigana) for kanji readings
- Language toggle for bilingual posts
- Japanese UI translations
