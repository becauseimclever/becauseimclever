# 034 - Post Editor Spell Checker

## Status: ✅ Completed

## Feature Description

Add an integrated spell checking feature to the blog post editor. The feature provides server-side spell checking via Hunspell, markdown-aware tokenization so code blocks and URLs are not checked, a custom dictionary for technical terms, and a visual overlay with suggestion popup in the editor.

## Current State

- All 4 slices are complete and fully tested
- `SpellCheckResult` and `SpellCheckRequest` domain records created
- `ISpellCheckService` and `IClientSpellCheckService` interfaces defined
- `SpellCheckService` implemented with `WeCantSpell.Hunspell` (lazy-loaded dictionary, suggestion limit of 5)
- `SpellCheckController` with `[Authorize(Policy = "PostManagement")]` wired up
- `ClientSpellCheckService` calling `POST /api/spellcheck` via `HttpClient`
- MarkdownEditor toolbar has a "Check Spelling" button showing misspelled word count badge
- DI wiring in Server and Client `Program.cs`
- **Slice 2**: `TextRegion` value object and `MarkdownTextExtractor` static utility in Domain
- **Slice 2**: `MarkdownTextExtractor` skips YAML front matter, fenced code blocks (backtick + tilde), indented code blocks, inline code (single + double backtick), image references, link URLs, bare URLs, and HTML tags
- **Slice 2**: `CheckMarkdownAsync` added to `IClientSpellCheckService` — accepts raw markdown, extracts prose via `MarkdownTextExtractor`, sends only unique prose words to API
- **Slice 3**: `AddWordRequest` domain record for custom dictionary API requests
- **Slice 3**: `ISpellCheckService` extended with `GetCustomDictionaryAsync` and `AddToDictionaryAsync`
- **Slice 3**: `SpellCheckService` loads/saves custom dictionary from `custom.dic` file, custom words checked before Hunspell, idempotent adds, sorted file output
- **Slice 3**: `SpellCheckController` extended with `GET /api/spellcheck/dictionary` and `POST /api/spellcheck/dictionary` endpoints
- **Slice 3**: `IClientSpellCheckService` extended with `AddToDictionaryAsync`, `IgnoreWord`, `IsIgnored`
- **Slice 3**: `ClientSpellCheckService` implements custom dictionary add (POST to API), session-scoped ignore list (in-memory `HashSet` with case-insensitive comparison), `CheckMarkdownAsync` filters ignored words from results
- **Slice 4**: `SpellCheckOverlay` component — positioned overlay rendering dotted blue underlines under misspelled words, segments content by word boundaries, supports click-to-show-popup, accepts `OverlayId` for JS scroll sync
- **Slice 4**: `SpellSuggestionPopup` component — popup with misspelled word, suggestion buttons, "Add to Dictionary" and "Ignore All" actions, fires replacement/close callbacks
- **Slice 4**: MarkdownEditor integration — browser spellcheck disabled, overlay and popup embedded, `CheckSpelling` uses `CheckMarkdownAsync`, `HandleSpellReplace` replaces words with Regex and re-checks, `HandleSpellWordClick` opens popup
- **Slice 4**: Scroll sync — `syncOverlayScroll` and `unregisterScrollSync` JS functions sync overlay scroll position with textarea, registered after first spell check, cleaned up on dispose
- **Slice 4**: Debounced auto spell check — 500ms debounce after typing stops triggers automatic spell check, `CancellationTokenSource`-based timer resets on each keystroke, cleaned up on dispose
- 932 total unit tests passing across Application (7), Domain (173), Infrastructure (229), Server (102), Client (421)
- Production Hunspell dictionary files (en-US) not yet deployed — must be provided at server's `Dictionaries/` path

## Goals

1. Server-side spell checking with suggestion support
2. Markdown-aware tokenization (ignore code blocks, inline code, URLs, etc.)
3. Custom dictionary for technical terms and names
4. Visual spell check overlay in the editor with suggestion popup
5. Maintain 90%+ code coverage across all projects

---

## Vertical Slices

### Slice 1: Basic Spell Check API (End-to-End MVP)

**Goal:** A working spell check round-trip — send words to the server, get back results, display a summary in the editor toolbar. Independently shippable: an author can check their post for misspellings via a toolbar button.

#### Domain

- `SpellCheckResult` value object — holds a word, whether it's correct, and a list of suggestions.
- `SpellCheckRequest` record — holds the list of words and a language code.

#### Application

- `ISpellCheckService` interface with `Task<IEnumerable<SpellCheckResult>> CheckWordsAsync(IEnumerable<string> words, string language, CancellationToken cancellationToken)`.

#### Infrastructure

- `SpellCheckService` implementation using `WeCantSpell.Hunspell` NuGet package.
- Loads the en-US `.dic` / `.aff` dictionary files from an embedded or file-system path.
- Returns `SpellCheckResult` for each submitted word.
- Constructor takes `ILogger<SpellCheckService>` and a dictionary file path option.

#### Server

- `SpellCheckController` with `[Route("api/spellcheck")]` and `[Authorize(Policy = "PostManagement")]`.
- `[HttpPost]` endpoint accepting `SpellCheckRequest`, returning `IEnumerable<SpellCheckResult>`.

#### Client

- `ClientSpellCheckService` — calls `POST /api/spellcheck` via `HttpClient`, returns results.
- `IClientSpellCheckService` interface registered in DI.
- Basic toolbar button added to `MarkdownEditor` — "Check Spelling" — that sends the current content (split into words) and shows a count of misspellings (e.g., badge on the button or a simple alert/toast).

#### Tests (maintain 90%+ coverage)

| Layer | Tests |
|-------|-------|
| Domain | `SpellCheckResult` construction, equality |
| Application | N/A (interface only) |
| Infrastructure | `SpellCheckService` — correct words, misspelled words, suggestions returned, null/empty input, constructor validation |
| Server | `SpellCheckController` — valid request returns results, empty request, unauthorized returns 401 |
| Client | `ClientSpellCheckService` — successful HTTP call, error handling; MarkdownEditor toolbar button renders |

---

### Slice 2: Markdown-Aware Tokenization

**Goal:** The spell checker only checks prose text — code blocks, inline code, URLs, image references, and HTML tags are skipped. Independently shippable: authors no longer see false positives for code snippets.

#### Domain

- `MarkdownTextExtractor` — static utility that parses markdown content and returns only the checkable text regions with their positions (offset + length).
- `TextRegion` value object — start position, length, extracted text.

#### Application

- No changes (reuses `ISpellCheckService`).

#### Infrastructure

- No changes (reuses `SpellCheckService`).

#### Server

- Optionally: new `[HttpPost] CheckMarkdown` endpoint that accepts raw markdown, tokenizes server-side, and returns results with positions. Alternative: tokenize client-side and reuse existing endpoint. Decision: **tokenize client-side** to keep the API simple and reduce payload size.

#### Client

- Update `ClientSpellCheckService` to accept raw markdown, extract checkable text via `MarkdownTextExtractor`, send only prose words to the API, and map results back to positions in the original content.

#### Tests

| Layer | Tests |
|-------|-------|
| Domain | `MarkdownTextExtractor` — skips fenced code blocks, inline code, URLs, image references, HTML tags, front matter; handles nested formatting; returns correct positions |
| Client | `ClientSpellCheckService` — sends only prose words, not code; position mapping is accurate |

---

### Slice 3: Custom Dictionary & Ignore List

**Goal:** Authors can add words to a custom dictionary so technical terms and brand names are not flagged. An ignore list allows temporarily dismissing words. Independently shippable: authors can teach the spell checker about their domain vocabulary.

#### Domain

- No new entities needed — the custom dictionary is a simple word list stored as a file.

#### Application

- Extend `ISpellCheckService` with:
  - `Task<IEnumerable<string>> GetCustomDictionaryAsync(CancellationToken cancellationToken)`
  - `Task AddToDictionaryAsync(string word, CancellationToken cancellationToken)`

#### Infrastructure

- Extend `SpellCheckService` to load a custom dictionary file (`custom.dic`) alongside the Hunspell dictionary.
- `AddToDictionaryAsync` appends the word to the custom dictionary file.
- Custom dictionary words are checked before Hunspell — if a word is in the custom dictionary, it's treated as correct.
- Ship a default custom dictionary with common tech terms (programming languages, frameworks, acronyms).

#### Server

- Add `[HttpGet("dictionary")]` and `[HttpPost("dictionary")]` endpoints to `SpellCheckController`.

#### Client

- Add "Add to Dictionary" button to the suggestion popup (Slice 4 provides the popup, but the client service method is wired here).
- `ClientSpellCheckService` gains `AddToDictionaryAsync(word)` and caches known custom words locally to avoid redundant API calls.
- Session-scoped ignore list (words dismissed for the current editing session) stored in the client service's in-memory state.

#### Tests

| Layer | Tests |
|-------|-------|
| Infrastructure | `SpellCheckService` — custom dictionary words are recognized; adding a word persists; duplicate adds are idempotent |
| Server | `SpellCheckController` — GET dictionary returns words; POST dictionary adds word; authorization enforced |
| Client | `ClientSpellCheckService` — add to dictionary calls API; ignore list prevents re-flagging |

---

### Slice 4: Spell Check Overlay & Suggestion Popup

**Goal:** Misspelled words are visually highlighted in the editor with a dotted blue underline, and clicking a highlighted word shows a suggestion popup with correction options. Independently shippable: the full spell check UX is complete.

#### Client

- **`SpellCheckOverlay.razor`** — a positioned overlay `<div>` rendered on top of the textarea that draws dotted blue underlines under misspelled words. Uses position data from `MarkdownTextExtractor` (Slice 2) to align highlights with textarea content. Scrolls in sync with the textarea.
- **`SpellSuggestionPopup.razor`** — popup shown on click of a highlighted word. Displays the misspelled word, up to 5 suggestions, and buttons for "Add to Dictionary" (Slice 3) and "Ignore All." Clicking a suggestion replaces the word in the editor.
- **`MarkdownEditor.razor` updates** — disable browser spell check (`spellcheck="false"`), embed the overlay, debounce spell check requests (500ms after typing stops), wire suggestion popup to word replacement.

#### JavaScript Interop

- Helper functions for:
  - Getting textarea scroll position and character positioning for overlay alignment.
  - Replacing text at a specific position in the textarea.

#### Tests

| Layer | Tests |
|-------|-------|
| Client | `SpellCheckOverlay` — renders highlights for misspelled words; no highlights when all correct; scrolls with textarea. `SpellSuggestionPopup` — displays word and suggestions; click suggestion fires replacement callback; "Add to Dictionary" calls service; "Ignore All" adds to ignore list. `MarkdownEditor` — spell check toggle disables browser spellcheck attribute; debounces check calls. |

---

## Coverage Strategy

Each slice must ship with tests that keep all projects at 90%+. Before merging any slice:

1. Run `dotnet test --settings coverage.runsettings --collect:"XPlat Code Coverage"`
2. Generate report with `reportgenerator` using existing assembly/class filters
3. Verify no project drops below 90%

New code introduced per slice:

| Slice | New Production Code | Required Tests |
|-------|-------------------|----------------|
| 1 | Domain VOs, Application interface, Infrastructure service, Server controller, Client service + toolbar button | Domain VO tests, Infrastructure service tests, Server controller tests, Client service + component tests |
| 2 | Domain text extractor | Comprehensive tokenizer tests (many edge cases) |
| 3 | Infrastructure dictionary methods, Server endpoints, Client service methods | Dictionary persistence tests, controller tests, client tests |
| 4 | Client overlay + popup components, JS interop | Component rendering tests, interaction tests |

---

## API Design

### Spell Check Endpoint

```
POST /api/spellcheck
Authorization: Bearer {token}
```

**Request:**
```json
{
  "words": ["recieve", "teh", "JavaScript"],
  "language": "en-US"
}
```

**Response:**
```json
[
  {
    "word": "recieve",
    "isCorrect": false,
    "suggestions": ["receive", "relieve"]
  },
  {
    "word": "teh",
    "isCorrect": false,
    "suggestions": ["the", "tea", "ten"]
  },
  {
    "word": "JavaScript",
    "isCorrect": true,
    "suggestions": []
  }
]
```

### Custom Dictionary Endpoint

```
GET /api/spellcheck/dictionary → string[]
POST /api/spellcheck/dictionary { "word": "Blazor" } → 201 Created
```

---

## Dependencies

- **WeCantSpell.Hunspell** — .NET Hunspell spell checking library (Infrastructure project)
- **en-US dictionary files** (`.dic` + `.aff`) — bundled in Infrastructure or Server wwwroot

---

## Performance Considerations

1. **Debounce** — wait 500ms after typing stops before sending a check request
2. **Batch** — send all unique words in a single API call, not per-word
3. **Client cache** — cache known correct/incorrect words for the session to avoid redundant API calls
4. **Incremental** — only check changed text regions (Slice 4 optimization)
5. **Dictionary preload** — load Hunspell dictionary once at startup, not per-request

---

## Design Decisions

1. **Client-side tokenization** — markdown parsing happens in the client to keep the API payload small (just words) and to retain position information for the overlay. The `MarkdownTextExtractor` lives in Domain so it can be tested independently and reused.
2. **Server-side spell checking** — Hunspell runs on the server because dictionary files are large (~5MB) and loading them in WASM would hurt startup time.
3. **Disable browser spell check** — set `spellcheck="false"` when our spell checker is active to avoid confusing double-underlines.
4. **Dotted blue underline** — visually distinct from the browser's red wavy underline, clearly communicating "this is our tool, not the browser."
5. **Authorization required** — spell check is only available to authenticated post editors, keeping the endpoint protected.
6. **Vertical slices** — each slice is independently shippable. Slice 1 delivers basic value (check my post for typos); Slice 2 eliminates false positives; Slice 3 adds personalization; Slice 4 polishes the UX.
