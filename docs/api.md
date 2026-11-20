# HolyGraph API

All routes are `GET`, return JSON, and need no authentication. With the development launch profile, the base address is
`http://localhost:33010/`, and Swagger UI is at `/swagger`.

- **Identifiers.** `{translation}` is a translation ID, for example `kjv-T` (see `api/translations`). `{book}` is the book's
  English name as stored in the graph, for example `Matthew`.
- **Paging.** Routes marked *paged* take `pageNumber` (default 1) and `pageSize` (default 10, at most 50). They return a
  `Pagination` response header such as `{"currentPage":1,"itemsPerPage":10,"totalItems":133,"totalPages":14}`.
- **Errors.** Routes that run a query directly answer `500 Internal Server Error` when the query fails; the details go to the
  server log.

## Translations

| Route | Returns |
|---|---|
| `api/translations` | Every translation |
| `api/translations/{translation}/books` | The translation's books. For books in the optional private metadata (see the README, "Data"), `chapterCount`, `verseCount` and `abbreviations` are filled in from it. |

## Books

| Route | Returns |
|---|---|
| `api/books` | *Paged.* Books with their translation ID. Filters: `translation` (a translation ID), or `scripture` = `All` (default), `Bible` or `Narek` (this one filters on a node property named `Label`; untested). `translation` wins when both are given. The total in the header is hard-coded (133). |

## Scriptures: books, chapters, counts

| Route | Returns |
|---|---|
| `api/scriptures` | Every book node, paged by `pageNumber` and `pageSize`, without a `Pagination` header. The `scripture` parameter is ignored. |
| `api/scriptures/{translation}/books` | The translation's books, by testament, then book order. Only `testament=All` (the default) works. *Used by the Telegram bot.* |
| `api/scriptures/books/{book}` | The book, in every translation |
| `api/scriptures/{translation}/books/{book}` | One translation's book |
| `api/scriptures/books/{book}/chapters` | The book's chapters, in every translation |
| `api/scriptures/{translation}/books/{book}/chapters` | One translation's chapters of the book |
| `api/scriptures/{translation}/books/{book}/chapters/count` | The book's order, title and chapter count |
| `api/scriptures/{translation}/books/{book}/chapters/{num}/verses/count` | Always `100`: a stub. *Used by the Telegram bot.* |
| `api/scriptures/bibles`, `api/scriptures/nareks` | Stubs: every book node |

## Verses and sections

| Route | Returns |
|---|---|
| `api/verses` | *Paged.* The verses of one chapter: `translation`, `book`, `chapter`. With `withSections=true`, the verses are ordered by section, and each carries its `sectionID` and `sectionTitle`. *Used by the web client.* |
| `api/verses/{translation}/books/{book}/chapters/{chapter}` | Every verse of the chapter; `withSections=true` adds the section titles |
| `api/verses/{translation}/books/{book}/chapters/{chapter}/oneverse/{verseNum}/` | One verse |
| `api/verses/{translation}/books/{book}/chapters/{chapter}/verses/{v1}/?v2={v2}` | Verses `v1` to `v2` (a missing or zero `v2` means up to 100). *Used by the Telegram bot.* |
| `api/verses/{translation}/books/{book}/chapters/{chapter}/sections/` | The chapter's sections |
| `api/verses/{translation}/books/{book}/chapters/{chapter}/sections/{secnum}` | One section |
| `api/verses/{translation}/books/{book}/sections` | Every section of the book |

A verse from `api/verses` has `translationId`, `bookTitle`, `book`, `chapter`, `section`, `verse`, `verseID`, `verseOrder`,
`verseText`, and, with sections, `sectionID` and `sectionTitle`.

## Word roots and word forms

| Route | Returns |
|---|---|
| `api/armats` | *Paged.* Classical Armenian word roots (`id`, `armat`, `formID`). The total in the header is hard-coded (11,713). |
| `api/armats/{startsWith}?ignoreCase=false` | The roots that start with the given letters |
| `api/tokens?armat={armat}` | The word forms of a root (`id`, `armatID`, `armat`, `token`, `tags`) |
| `api/tokens/armatId?armatId={id}` | The same, by the root's ID |

## Raw graph nodes

`api/entity/{label}` returns the graph nodes with that label, where `{label}` is one of:
- `places` and `persons` (the first 50);
- `eastons`, Easton's Bible Dictionary entries, and `armats` (the first 100);
- `events`, `divisions`, `translations`, `scriptures`, `commentaries` and `years` (all).

## Other

- `GET /weatherforecast`: the project template's sample route; it's still there.
- `api/words` has no routes yet.

## Examples: the Telegram bot's inline queries

In the bot's inline mode, a user types a reference such as `Mat 1 4-10`, and the bot asks the API for the verses. These three
queries come from the maintainer's notes; the bot used the KJV (`kjv-T`):

| Inline query | The route that answers it |
|---|---|
| `Mat 1 -` | `GET api/verses/kjv-T/books/Matthew/chapters/1/verses/1/?v2=50` |
| `Mat 1 4` | `GET api/verses/kjv-T/books/Matthew/chapters/1/oneverse/4/` |
| `Mat 1 4-10` | `GET api/verses/kjv-T/books/Matthew/chapters/1/verses/4/?v2=10` |

The last call runs this query, with the parameters pasted into the text (see "Known issues" in the README):

```cypher
MATCH(t:Translation) where t.id = 'kjv-T'
MATCH(t)-[rt:CONTAINS]->(n:Book) where n.book = 'Matthew'
MATCH (n)<-[r:CONTAINS]->(ch:Chapter)-[:CONTAINS]->(v:Verse)
where ch.num = 1 and v.verse >= 4 and v.verse <= 10
return v, t.id as translation, n.title as bookTitle order by v.verseOrder
```

> **Note:** the bot's `fix-api` branch asks for one verse, and for sections, under `api/scriptures/…`. This API serves those
> routes under `api/verses/…`, as listed above, so those bot commands need one side changed first.
