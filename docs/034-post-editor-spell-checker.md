# 034 - Post Editor Spell Checker

## Status: 📋 Planned

## Feature Description

Add an integrated spell checking feature to the blog post editor that works alongside (and doesn't conflict with) the browser's native spell checker. This feature will provide enhanced spell checking with custom dictionary support, technical term awareness, and markdown-aware checking.

## Current State

- The `MarkdownEditor` component uses a standard `<textarea>` with `spellcheck="true"` (browser native)
- Browser spell checkers (Chrome, Firefox, etc.) provide basic spell checking
- No custom dictionary for technical terms, brand names, or blog-specific words
- No way to ignore markdown syntax during spell checking
- No persistent word ignore list per user

## Goals

1. Provide enhanced spell checking without conflicting with browser native spell check
2. Support a custom dictionary for technical terms and names
3. Markdown-aware spell checking (ignore code blocks, inline code, URLs, etc.)
4. User-configurable ignore list that persists
5. Clear visual differentiation from browser spell check underlines
6. Performance-optimized for long blog posts

---

## Technical Design

### Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                        Post Editor UI                                 │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │                    MarkdownEditor Component                     │  │
│  │  ┌──────────────────┐  ┌─────────────────────────────────────┐ │  │
│  │  │   Editor Pane    │  │          SpellCheckOverlay          │ │  │
│  │  │   (textarea)     │  │  - Highlights misspelled words      │ │  │
│  │  │   spellcheck=    │  │  - Custom styling (not red underline)│ │  │
│  │  │   "false"        │  │  - Click for suggestions            │ │  │
│  │  └──────────────────┘  └─────────────────────────────────────┘ │  │
│  └────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌──────────────────────────────────────────────────────────────────────┐
│                      SpellCheckService (Client)                       │
│  - Debounced spell check requests                                     │
│  - Word tokenization (markdown-aware)                                 │
│  - Local cache of checked words                                       │
│  - User ignore list management                                        │
└──────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌──────────────────────────────────────────────────────────────────────┐
│                         Spell Check API (Server)                      │
│  - POST /api/spellcheck                                               │
│  - Uses Hunspell or similar library                                   │
│  - Custom dictionary support                                          │
│  - Returns misspellings + suggestions                                 │
└──────────────────────────────────────────────────────────────────────┘
```

### Browser Spell Check Conflict Prevention

To prevent conflicts with Chrome's native spell checker:

1. **Disable browser spell check** on the textarea: `spellcheck="false"`
2. **Use custom visual indicators** (not the standard red wavy underline):
   - Option A: Dotted purple/blue underline
   - Option B: Highlighted background color
   - Option C: Gutter markers (like VS Code)
3. **Overlay approach**: Use a positioned overlay div to render highlights without affecting the textarea

### Markdown-Aware Tokenization

The spell checker will ignore:
- Code blocks (``` ... ```)
- Inline code (` ... `)
- URLs and links (`[text](url)`, `https://...`)
- Image references (`![alt](src)`)
- HTML tags
- Front matter (if any)
- Code language identifiers (```csharp, ```javascript, etc.)

### API Design

#### Spell Check Endpoint

```
POST /api/spellcheck
```

**Request:**
```json
{
  "words": ["recieve", "teh", "occured", "JavaScript"],
  "language": "en-US"
}
```

**Response:**
```json
{
  "results": [
    {
      "word": "recieve",
      "correct": false,
      "suggestions": ["receive", "relieve", "review"]
    },
    {
      "word": "teh",
      "correct": false,
      "suggestions": ["the", "tea", "ten"]
    },
    {
      "word": "occured",
      "correct": false,
      "suggestions": ["occurred"]
    },
    {
      "word": "JavaScript",
      "correct": true,
      "suggestions": []
    }
  ]
}
```

### Custom Dictionary

Store custom dictionary words in:
1. **Server-side**: `wwwroot/dictionaries/custom.dic` for shared technical terms
2. **User-specific**: LocalStorage for personal ignore list

Default custom dictionary entries:
- Programming languages: JavaScript, TypeScript, C#, csharp, F#, fsharp, etc.
- Frameworks: Blazor, ASP.NET, .NET, React, Vue, Angular
- Technical terms: API, JSON, REST, GraphQL, OAuth, JWT, etc.
- Common proper nouns in tech

---

## Components

### New Components

1. **SpellCheckOverlay.razor** - Renders spell check highlights over the textarea
2. **SpellSuggestionPopup.razor** - Popup showing suggestions when clicking a misspelled word

### Modified Components

1. **MarkdownEditor.razor** - Integrate spell check overlay, disable browser spell check
2. **PostEditor.razor** - Add spell check toggle in toolbar or settings

### New Services

1. **ISpellCheckService** (Application) - Interface for spell checking
2. **SpellCheckService** (Infrastructure) - Server-side Hunspell implementation
3. **ClientSpellCheckService** (Client) - Client-side service with caching and debouncing

---

## UI/UX Design

### Spell Check Indicators

- Misspelled words highlighted with **dotted blue underline** (distinct from browser's red wavy)
- Hover shows tooltip: "Possible spelling error. Click for suggestions."
- Click opens suggestion popup

### Suggestion Popup

```
┌─────────────────────────┐
│ "recieve"               │
├─────────────────────────┤
│ ○ receive               │
│ ○ relieve               │
│ ○ review                │
├─────────────────────────┤
│ [Add to Dictionary]     │
│ [Ignore All]            │
└─────────────────────────┘
```

### Toolbar Integration

Add spell check toggle to MarkdownEditor toolbar:
- Icon: ✓ABC or similar
- Tooltip: "Toggle Spell Check"
- State persisted in LocalStorage

---

## Implementation Plan

### Phase 1: Server-Side Spell Check API
1. Add Hunspell NuGet package (e.g., `WeCantSpell.Hunspell`)
2. Create `ISpellCheckService` interface in Application layer
3. Implement `SpellCheckService` in Infrastructure layer
4. Add `/api/spellcheck` endpoint in Server
5. Include en-US dictionary files

### Phase 2: Client-Side Integration
1. Create `ClientSpellCheckService` with debouncing and caching
2. Create `SpellCheckOverlay` component
3. Create `SpellSuggestionPopup` component
4. Modify `MarkdownEditor` to integrate overlay

### Phase 3: Markdown-Aware Tokenization
1. Implement markdown parser to extract checkable text
2. Map positions back to original text for overlay
3. Handle edge cases (nested formatting, etc.)

### Phase 4: Custom Dictionary & User Preferences
1. Add custom dictionary file with tech terms
2. Implement "Add to Dictionary" functionality
3. Implement "Ignore All" functionality
4. Persist user preferences in LocalStorage

---

## Dependencies

- **WeCantSpell.Hunspell** (or similar) - Hunspell spell checking library for .NET
- **en-US dictionary files** - Included in wwwroot or embedded resources

---

## Testing Strategy

### Unit Tests
- Markdown tokenizer correctly identifies checkable vs. ignored regions
- Spell check service returns correct suggestions
- Client service properly debounces requests
- Custom dictionary words are recognized

### Integration Tests
- API endpoint returns correct responses
- Full flow from editor to API and back

### E2E Tests
- Misspelled words are highlighted in editor
- Clicking suggestion applies correction
- "Add to Dictionary" prevents future highlights
- Spell check toggle works correctly

---

## Performance Considerations

1. **Debounce spell checks** - Wait 500ms after typing stops before checking
2. **Batch word checks** - Send multiple words in single API request
3. **Client-side caching** - Cache known correct/incorrect words
4. **Incremental checking** - Only check changed text regions
5. **Virtual scrolling** - Only render overlays for visible text

---

## Future Enhancements

- Grammar checking integration
- Multiple language support
- AI-powered suggestions for context-aware corrections
- Autocomplete for commonly misspelled words
- Statistics: "You commonly misspell X as Y"
