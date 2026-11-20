# HolyGraph

A REST API (ASP.NET Core 8) and an Angular web client over **Surbgirq**, a Neo4j knowledge graph of Armenian Bible
translations, the King James Version, and Bible metadata: people, places, events, years and Easton's Bible Dictionary.
The graph also holds a lexicon of Classical Armenian word roots (*armat*) and their word forms.

*Surb Girq* (Սուրբ Գիրք) is Armenian for "Holy Book"; HolyGraph is the English name of the project's 2024 API rewrite.
The Telegram bot and the data pipeline that builds the graph live in a separate repository,
[`datasfer/surbgirq`](https://github.com/datasfer/surbgirq) (Haskell).

**Status:** the API builds and starts on .NET 8 (checked in November 2026). It hasn't been run against a current Neo4j
version yet. Read [Known issues / security](#known-issues--security) before exposing it anywhere.

## Repository layout

| Folder | What it is |
|---|---|
| `HolyGraph.API/` | The REST API: ASP.NET Core 8, the Neo4j .NET driver 5, Swagger |
| `client/` | The Angular 16 web client: read a chapter with its section titles; browse word roots and their forms ([client/README.md](client/README.md)) |
| `elm-client/` | An Elm 0.19 stub: one search box, no API calls yet |
| `src/HolyGraph.GUI/` | A WPF stub: an empty window |
| `docs/api.md` | Every route, with examples ([docs/api.md](docs/api.md)) |

## Run the API

You need the .NET 8 SDK (or a later SDK) and a Neo4j database with the Surbgirq graph (see [Data](#data)).

1. **Configure the database.** `HolyGraph.API/appsettings.Development.json` ships with placeholder values (a documentation
   address and `CHANGE_ME`):

   ```json
   "Neo4jConfig": {
     "instanceUri": "bolt://192.0.2.20:7687",
     "databaseName": "bibles",
     "username": "neo4j",
     "password": "CHANGE_ME"
   }
   ```

   Put your values there, or override them with environment variables: `Neo4jConfig__InstanceUri`,
   `Neo4jConfig__UserName`, `Neo4jConfig__Password`. (The database name is hard-coded for now; see Known issues.)
2. **Run it:**

   ```bash
   cd HolyGraph.API
   dotnet run
   ```

   The launch profile starts the API in the Development environment on port 33010, listening on all network interfaces.
   Swagger UI: <http://localhost:33010/swagger>.

All routes are `GET` under `api/` and return JSON:

| Routes | What they return |
|---|---|
| `api/translations` | translations, and the books of one translation |
| `api/books` | books, paged |
| `api/scriptures/…` | books of a translation, chapters, counts |
| `api/verses/…` | verses of a chapter (paged, optionally grouped by section), one verse, a verse range, sections |
| `api/armats`, `api/tokens` | Classical Armenian word roots and their word forms |
| `api/entity/…` | raw graph nodes: places, persons, events, Easton's entries, divisions, years and more |

Paged routes return a `Pagination` response header. The full list is in [docs/api.md](docs/api.md).

## Run the web clients

**Angular** (`client/`), in short:

```bash
cd client
npm ci
# create the dev server's HTTPS certificate in client/ssl/ first; see client/README.md
npm start            # https://localhost:4200
```

The dev server uses HTTPS, and its certificate isn't in the repository: [client/README.md](client/README.md) shows how to
create one with `mkcert`, and where to set the API address.

**Elm** (`elm-client/`, a stub). The page loads `index.js`, which is compiled from `src/Main.elm` and isn't in the repository:

```bash
npm install -g elm   # the Elm 0.19.1 compiler
cd elm-client
npm install
npm run dev          # elm-live compiles index.js and serves index.html, with live reload
```

## Data

- **This repository holds code only.** The API reads the Surbgirq graph from Neo4j. The graph is built by the data pipeline
  in `surbgirq` ([docs/data-pipeline.md](https://github.com/datasfer/surbgirq/blob/master/docs/data-pipeline.md)), not here.
- **The graph combines:**
  - Armenian Bible texts: Classical Armenian (Grabar) editions and a modern translation;
  - the King James Version;
  - Grigor Narekatsi's *Book of Lamentations* (Narek), in Classical Armenian and in a modern translation;
  - Theographic Bible Metadata: people, places, events, years, and Easton's Bible Dictionary (CC BY-SA 4.0);
  - a Classical Armenian lexicon: word roots (*armat*) and their word forms.

  The sources and licences of the data are listed in `surbgirq`'s
  [docs/data-sources.md](https://github.com/datasfer/surbgirq/blob/master/docs/data-sources.md).
- **The 1994 Etchmiadzin translation is not public:** not in this repository and not in its history. It's copyrighted by the
  Mother See of Holy Etchmiadzin and the Bible Society of Armenia, and so are its section headings. The code still knows its
  translation ID, `e1994-T` (the web client's default), because a private graph may contain it.
- **Optional private book metadata.** For that translation, the API can read chapter and verse counts and book abbreviations
  from YAML files in `HolyGraph.API/Data/YAML/e1994T/New/`. The folder is git-ignored; the build copies the files next to the
  app. Without them the API still starts, logs "No book metadata …", and uses default counts.

## History

- **2020: the first .NET API,** in its own repository `Surbgirq.GraphQL` (despite the name, a REST API with Swagger, plus an
  echo-stub Telegram bot). It was written because the Haskell side couldn't work with Neo4j 4 at the time.
- **2024: HolyGraph,** a rewrite of that API with more data and a web client. This repository's own line starts on 2024-01-17.
- **2026: published.** The 2020 API's history was merged in as `legacy/`, then removed from the tree. The history therefore has
  two starting points (2020-06-25 and 2024-01-17), joined by one merge commit dated when the repository was published. The old
  code stays one click away, at the tag
  [`legacy-api-final`](../../tree/legacy-api-final/legacy).
- **The history was rewritten before publishing.** Secrets and server addresses were replaced (by `CHANGE_ME`,
  `REMOVED-TELEGRAM-TOKEN` and `192.0.2.x` documentation addresses). A private key, compiled output, a third-party install
  script, old log files and the copyrighted data were removed from every commit. Dates, authors and messages are unchanged.
  - Some older commits don't build as checked out, because files they need were removed. For example, the HolyGraph commits
    from 2024-01-27 on import a namespace whose only file was removed.

## Known issues / security

Nothing runs this code in public at the moment. Fix these before it does:

| Issue | Planned fix |
|---|---|
| **Cypher injection:** queries are built by pasting request parameters into the query text. Sessions are read-only, so this exposes read access to the whole graph, not writes. | Parameterized queries, with dynamic labels on Neo4j 5.26+ |
| **No authentication, and CORS allows any origin.** | CORS limited to the web client; authentication if write routes ever appear |
| **Swagger is on in Development, and the launch profile runs Development on all interfaces** (`0.0.0.0:33010`). | Swagger off in production; a production profile |
| `Neo4jGraphRepository.Dispose()` throws `NotImplementedException`. | Dispose the driver properly |
| The database name is hard-coded (`bibles01`); `Neo4jConfig:databaseName` is ignored. | Read it from the configuration |
| Hard-coded totals: books (133), word roots (11,713); `…/verses/count` always returns 100. Without the private metadata, chapter and verse counts fall back to fixed defaults. | Count in Cypher |
| Some routes ignore a parameter or fail: `api/scriptures` ignores `scripture`; `api/scriptures/{translation}/books` fails for any `testament` other than `All`; `api/scriptures/bibles` and `api/scriptures/nareks` return every book. | Fix them, or remove them |
| The Telegram bot (`surbgirq`, branch `fix-api`) asks for one verse and for sections under `api/scriptures/…`, but this API serves them under `api/verses/…`. | Align the two when the bot runs again |
| The web client: Angular 16 is out of support; its default translation, `e1994-T`, isn't public; `app.component.ts` hard-codes an API address; the home, tokens and book-detail pages are stubs. | Upgrade and finish it with the revival |
| Unused template scaffolding: a JWT token service, an SQLite `DataContext`, the Neo4jClient package. The Elm and WPF clients are stubs. | Remove or build out while polishing |

## License

All code in this repository is licensed under the MIT License; see [LICENSE](LICENSE). The repository contains no Bible text
or other data; see [Data](#data).
