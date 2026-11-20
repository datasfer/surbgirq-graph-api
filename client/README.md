# HolyGraph web client (Angular)

The web UI of [HolyGraph](../README.md): read the Bible chapter by chapter, and browse the Classical Armenian word roots and
their word forms. Angular 16 with Bootstrap 5; it was generated with Angular CLI 16.2.11.

## Pages

| Page | What it shows |
|---|---|
| **Scriptures** (`/books`) | Pick a translation, a book and a chapter, then read the verses. "With Sections" groups them under their section titles. Previous and next chapter buttons; optional paging. |
| **Armats** (`/armats`, `/armats/:letter`) | The word roots, filtered by first letter with an alphabet bar; paged |
| **Word forms** (`/armat-tokens/:armat`) | Every form of one root, with its grammar tags |
| Home, Tokens, book detail | Stubs ("… works!") |

The navigation bar still has a few items from the Bootstrap template.

## Requirements

- **Node.js:** Angular 16 officially supports Node.js 16.14+ and 18.10+. Newer Node versions haven't been tried with this
  client.
- **The HolyGraph API**, running; see the [root README](../README.md).

## Install

```bash
cd client
npm ci
```

## The development certificate

`ng serve` uses HTTPS: `angular.json` sets `"ssl": true`, with `ssl/localhost.pem` and `ssl/localhost-key.pem`. The
certificate isn't in the repository (`client/ssl/` is git-ignored), so create your own, for example with
[mkcert](https://github.com/FiloSottile/mkcert):

```bash
mkcert -install      # once: trust mkcert's local certificate authority
mkdir -p ssl
mkcert -cert-file ssl/localhost.pem -key-file ssl/localhost-key.pem localhost 127.0.0.1
```

## The API address

- **Development:** `apiUrl` in `src/environments/environment.development.ts`. It ships with a placeholder (a documentation
  address); set it to your API, for example `http://localhost:33010/api/`.
- **Production build:** `src/environments/environment.ts` uses the relative `api/`, for a client served from the same host
  as the API.
- `src/app/app.component.ts` also has a hard-coded `host` for one request.

The dev page runs on HTTPS. Browsers let it call an API on `http://localhost`, but block plain-HTTP calls to other hosts
(mixed content). So use a local API, or one served over HTTPS.

## Run and build

```bash
npm start            # ng serve: https://localhost:4200
npm run build        # into dist/
```

## Known issues

- Angular 16 is out of support; an upgrade is planned when the project runs again.
- The default translation is `e1994-T`, which isn't public (see the root README, "Data"). Pick another one in the list.
- The hard-coded `host` in `app.component.ts`, and the stub pages above.
