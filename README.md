# Arbol

.NET 10 Blazor application with a hosted WebAssembly client, shared UI and kernel,
Aspire orchestration, and PostgreSQL-backed ASP.NET Core Identity.

## Local prerequisites

- .NET SDK **10.0.401** (selected by `global.json`).
- Aspire CLI **13.6.4** on `PATH` (the version the workflows pin), with
  the AppHost SDK on **13.6.4**.
  Follow the [Aspire installation guide](https://aspire.dev/get-started/install/).
- Docker Desktop running Linux containers, or another Aspire-supported container runtime.
- A trusted .NET development HTTPS certificate: `dotnet dev-certs https --trust`.
- Optional VS Code with this repository's recommended C# and Aspire extensions.
  The editor opens `Arbol.slnx` by default.

Run `dotnet --version`, `aspire --version`, and `aspire doctor` from the repository
root to verify the environment. The AppHost is explicitly located by the root
`aspire.config.json`.

## First run and application identity

Use PowerShell 7 for the scripts in this repository. From the solution root,
run `dotnet build Arbol.slnx`, then `aspire run`. The first build/start may
restore NuGet packages, download Aspire/EF tooling, and pull container images.
The initial migration creates an empty Identity schema; no accounts or local
credentials are included. Git initialization is optional and separate.

If this solution was generated, `.template-provenance.json` records its template
version and source commit. It is an independent snapshot: template updates do not
update this application. Review SDK/package/skill updates in this repository.

Use a unique application name for projects you run side by side. Different ports
do not isolate browser cookies on the same hostname. A generated secrets ID is
unique even when the application name is reused, but the development hostname is
name-based. Aspire's default data-volume name depends on the AppHost path; moving
or renaming a checkout can select a different volume. Keep the original secrets
and volume together if preserving development data.

## Run through Aspire

Aspire is the default entry point for running and debugging the application. It
starts PostgreSQL, applies migrations, supplies configuration, and starts the web
project. The hosted WebAssembly client runs through that web project.

For interactive development, run from the repository root:

```powershell
aspire run
```

The CLI builds the AppHost and its projects before starting them. For a background
run (including agent validation), use:

```powershell
aspire start --launch-profile https --non-interactive
aspire wait arbol-web --timeout 120 --non-interactive
aspire describe --non-interactive
```

`aspire start` prints the dashboard login URL. The dashboard and `aspire describe`
show the current application endpoints. Open the HTTPS endpoint for
`arbol.dev.localhost`; all ports are allocated dynamically and can change on
restart. Chromium browsers resolve `.localhost` names locally. Command-line
clients that do not resolve subdomains can use the internal `https://localhost`
endpoint shown by Aspire. Do not copy ports or dashboard login tokens into source.

The background command explicitly selects `https`; it is also the AppHost's first
and default launch profile. `AddProject` selects the
matching web profile and derives its endpoints from `applicationUrl`; there is no
AppHost override for the web profile or its ports. The web profiles retain
`arbol.dev.localhost:0`: the hostname is our local convention, and port `0` asks
Aspire to allocate an available port instead of using the template's fixed ports.
The other web launch settings preserve the template's environment and Blazor debugging.

The AppHost's `https` profile omits dashboard, OTLP, and resource-service addresses:
Aspire 13.6.4 supplies secure, dynamically allocated local endpoints. The optional
AppHost `http` profile sets `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true`, which opts into
HTTP endpoints with allocated ports. This opt-in applies only when that profile
is selected; the default `https` profile keeps Aspire's secure defaults.

Use the web resource's **Rebuild** command to apply compiled application changes.
For AppHost changes or a full solution build, stop first to release Windows file locks:

```powershell
aspire stop --non-interactive
dotnet build Arbol.slnx
aspire start --launch-profile https --non-interactive
```

Stop the application with `aspire stop --non-interactive` (or Ctrl+C for a foreground
run). PostgreSQL and any started pgAdmin container stop with Aspire. The database
volume survives. Aspire retains its generated local PostgreSQL password in AppHost
user secrets; preserve those secrets along with the volume when reusing local data.
No database password or connection string needs to be committed.

## Debug through Aspire

In VS Code, install the recommended Aspire and C# extensions and open the repository
root. Select **Aspire: Arbol** in Run and Debug, then press **F5**. The checked-in
`.vscode/launch.json` uses the Aspire debugger and discovers the AppHost through
`aspire.config.json`; no fixed ports or connection strings are needed. The extension
starts the resource graph and attaches the .NET debugger to the web project.
Use **Ctrl+F5** with the same configuration to run without debugging.

Stop any CLI-started instance with `aspire stop --non-interactive` before starting
an IDE debug session. Use the IDE's **Stop Debugging** action to end its session.
Open the dashboard through **Aspire: Open Dashboard** or the Aspire view; editor
settings control whether it opens automatically. This workspace sets
`aspire.dashboardBrowser` to `openExternalBrowser`, so the dashboard opens in the
configured external browser for both F5 and Ctrl+F5. This overrides a user-level
`debugEdge`/`debugChrome` preference; C# application debugging remains enabled with F5.

If VS Code reports **Unable to launch browser: "Unable to attach to browser"**,
check **Output → Aspire Extension**. An entry such as **Failed to start debug
browser (pwa-msedge), falling back to default browser** identifies the dashboard's
browser-debugger session. That session is separate from the AppHost and web C#
debuggers and is not needed to use the dashboard. Keep the workspace's
`openExternalBrowser` setting and remove any `dashboardBrowser: "debugEdge"` or
`"debugChrome"` override in the selected `launch.json` configuration, which takes
precedence over workspace settings. Then stop and restart the Aspire session.

Set server breakpoints in the web project's code-behind, such as the registration
handler in `src/Arbol/Features/Account/Pages/Register.razor.cs`, then exercise the
page through the HTTPS endpoint shown by Aspire. Inspect variables and the call
stack in the debugger, and use Aspire's logs and traces for the corresponding
HTTP and database activity. A breakpoint can temporarily interrupt health checks;
resume execution before diagnosing the paused resource as unhealthy.

In Visual Studio, set **Arbol.AppHost** as the startup project, select its
**https** profile, and use F5 (debug) or Ctrl+F5 (run). Launching `Arbol` or
`Arbol.Client` alone does not start the database/migration dependency graph.

The existing unit and component tests remain headless and can run/debug through
Test Explorer without Aspire.

See the [Aspire VS Code extension guide](https://aspire.dev/get-started/aspire-vscode-extension/).

## Styling and layout

The UI is plain HTML, CSS, and Blazor with no component library. Layout uses
CSS Grid, and styling uses CSS custom properties declared once in
`src/Arbol/wwwroot/app.css`. Do not add a component framework, a CSS framework,
or a CSS preprocessor; extend the shared tokens and classes instead. The shared
kernel has no UI dependency.

Application design tokens live in the `:root` block of `app.css`: `--color-*`
for text, surfaces, borders, accent, and status colours, `--space-*` for
spacing, `--radius-*` for corners, `--shadow-card` for elevation, and
`--font-sans` for the type family. Component stylesheets resolve every colour
and size from those tokens; add a token rather than a new literal when a value
is reused. The document selects a light colour scheme.

`App.razor` loads `Arbol.styles.css` (the generated scoped-CSS bundle) and then
`app.css`. Custom properties are resolved at runtime, so the order only decides
which declaration wins for the same property on the same element.

The shell is a single CSS Grid in `MainLayout.razor.css` with named `header`,
`navigation`, `content`, and `footer` areas. At 48rem and wider the navigation
is a fixed 15rem column beside the content; below that the same grid collapses
to one column and the navigation becomes a disclosure panel driven by the
`#site-menu-toggle` checkbox. No overlay, focus trap, or .NET event handler is
needed, so the shell stays compatible with static SSR. `Arbol.UI.lib.module.js`
registers the collocated `MainLayout.razor.js` handler through `afterWebStarted`,
and that handler clears the checkbox on `enhancednavigationstart` so the panel
collapses before Blazor patches in the next page.

Navigation uses ordinary `NavLink` anchors inside `<ul>` lists. Native anchors
preserve accessible names and keyboard order before JavaScript initializes, and
`NavLink` marks the current page with `aria-current="page"`. Give each navigation
region its own `<nav aria-label>` element: `NavMenu` and `ManageNavMenu` each own
one, and the shell provides only the grid cell around them.

Render boundaries are deliberate:

- `Routes`, `MainLayout`, Home, the authenticated summary, and Identity pages
  use static SSR. `Counter` opts into `InteractiveAuto` because its button needs
  .NET event handling. Counter actions are disabled during prerendering until
  their renderer becomes interactive. Static pages don't start a .NET interactive
  runtime simply to display navigation or cards.
- Shell links use Blazor's enhanced navigation: the server renders the destination
  and Blazor updates the existing document without restarting the page or
  reloading the runtime. Stable layout IDs preserve the drawer wiring across
  updates. The shell grows with its content and uses document scrolling, allowing
  Blazor to reset scroll position on navigation and restore it through browser
  history.
- Identity forms retain native inputs, submit buttons, form names, antiforgery,
  and passkey hooks. They use the shared tokens while preserving static POST
  mapping, autofill, and cookie redirects. These POSTs keep normal navigation so
  cookie changes refresh authentication state. Shared notices retain their
  `notice`/`data-kind` contract. The reconnect/error UI retains native controls
  because it must function while the Blazor circuit is unavailable.

Shared styles live in `src/Arbol/wwwroot/app.css`: the token block, element
baseline, forms (`account-form`, `form-field`, `checkbox-field`), buttons
(`button-primary`, `button-subtle`), cards (`card`, `card-links`), navigation
lists (`nav-list`, `nav-section-label`), action groups (`actions`), notices
(`notice` with a semantic `data-kind`), and table overflow (`table-container`).
Component-specific styles belong in adjacent `.razor.css` files. Use narrowly
scoped `::deep` selectors only when child component markup requires them.

Use normal flow for prose and semantic tables. Do not reintroduce Bootstrap or
utility classes, add `!important`, or add ornamental animations. Keep form labels
before their controls, provide a visible focus indicator, and preserve
Blazor/Identity behavior hooks when editing markup. Check narrow screens, keyboard
focus, and text wrapping when changing layouts. See the
[repository instructions](.github/copilot-instructions.md) and the
[UI instructions](.github/instructions/ui.instructions.md) for the authoring
conventions.

References: [CSS Grid layout](https://developer.mozilla.org/docs/Web/CSS/CSS_grid_layout),
[enhanced navigation](https://learn.microsoft.com/aspnet/core/blazor/fundamentals/navigation?view=aspnetcore-10.0#enhanced-navigation-and-form-handling),
and [JavaScript with static SSR](https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability/static-server-rendering?view=aspnetcore-10.0).

## Resource graph and database

```text
postgres (PostgreSQL 18.3, managed data volume)
  ├─ arboldb (physical database: arbol)
  │    └─ arbol-migrations (EF database update)
  │         └─ arbol-web (Blazor server + hosted WebAssembly)
  └─ pgadmin (explicit start)
```

After a normal startup, these dashboard states are expected:

| Resource | Expected state | Meaning |
| --- | --- | --- |
| `postgres`, `arboldb` | Running / Healthy | PostgreSQL and the application database are ready. |
| `arbol-migrations` | Finished | The one-shot migration command completed successfully. It is not a long-running service. |
| `arbol-web` | Running / Healthy | The web application is ready and its database readiness check passes. |
| `pgadmin` | Not started | Optional database UI; start it when needed. |

With hidden resources displayed, `arbol-web-rebuilder` can also be **Not started**
until the web project's Rebuild command is used, and the EF tool resource can be
**Finished**. These helper states do not indicate an application startup failure.
An **Exited**, **Failed**, or **Unhealthy** application/database resource, or web
startup blocked by failed migrations, does require investigation through its logs.

The PostgreSQL image version is the built-in default of
`Aspire.Hosting.PostgreSQL` 13.6.4. Container lifetime remains Aspire's default
session lifetime. Data uses an Aspire-managed named volume rather than a repository
file. This is a fresh PostgreSQL schema; the original SQLite scaffold is removed.

Aspire supplies `ConnectionStrings:arboldb` to both the application and migration
tool. The application uses Npgsql EF Core 10.0.3, EF Core 10.0.14, and Aspire's
Npgsql EF integration 13.6.4. Its non-pooled `IDbContextFactory<ApplicationDbContext>`
supports a context per Blazor operation; Identity can still resolve the scoped
context. Dispose factory-created contexts with `await using`.

Identity schema version 3 is retained, including the `AspNetUserPasskeys` table.
Development registration uses the existing no-op email sender: follow the confirmation
link on the registration confirmation page before logging in. This setup does not
configure a production email service or deployment infrastructure.

Before production, replace `IdentityNoOpEmailSender` and remove or deliberately
gate the scaffold confirmation link. The current shortcut checks the sender type,
not the hosting environment; it is not restricted to Development. External-login
provider credentials, production secrets, HTTPS/domain configuration, deployment,
and the desired exposure of health endpoints also require application-specific work.

## EF migrations

`Aspire.Hosting.EntityFrameworkCore` **13.6.4-preview.1.26523.11** manages
`arbol-migrations` and its **dotnet-ef 10.0.14** tool. It does not alter the machine's
global EF tool. On every start, migrations wait for PostgreSQL and the database;
the web application starts only after migration success. Failure blocks web startup.
Migrations use the actual web startup registration, keeping Identity's design-time
and runtime models aligned.

For a schema change:

1. Stop Aspire, change the model, and build the solution.
2. Start Aspire. If EF detects pending model changes, migration startup can fail and
   block the web resource; the migration authoring commands remain available.
3. On `arbol-migrations` in the dashboard, run **Add Migration...** and supply a
   descriptive name. Review the generated files in `src/Arbol/Data/Migrations`
   with namespace `Arbol.Migrations`.
4. Apply the repository's conventions to the handwritten migration class (file-scoped
   namespace and `internal sealed partial class`); leave generated designer code alone.
   Keep the model snapshot in the same migrations directory. EF can choose a directory
   from the legacy namespace when regenerating a missing snapshot, so check its location.
5. Stop Aspire, rebuild, and start again. The built-in migration resource applies the
   newly compiled migration before starting the web application.
6. Run **Get Database Status** on `arbol-migrations` to verify applied migrations and
   pending model changes.

CLI equivalents for inspecting and updating the current compiled model:

```powershell
aspire resource arbol-migrations ef-database-status --non-interactive
aspire resource arbol-migrations ef-database-update --non-interactive
aspire logs arbol-migrations --non-interactive
```

The built-in **Remove Migration**, **Drop Database**, and **Reset Database** commands
are also available. Drop/reset delete local application data; use them only when
that is intended. There is no application-startup migration routine or custom worker.
See [Aspire's EF migration integration](https://aspire.dev/integrations/databases/efcore/migrations/).

## Health, telemetry, and pgAdmin

The application references `Arbol.ServiceDefaults`. In Development, `/health`
checks readiness including PostgreSQL connectivity; `/alive` checks process liveness
independently of PostgreSQL. Aspire monitors `/health`. The database readiness check
has a five-second timeout so EF's transient retries do not hold an unhealthy response
open for minutes. Cancellation is cooperative: an in-flight Npgsql connection attempt
can delay the response (about 15 seconds in local outage validation). Normal
application operations retain Aspire's retry behavior.

`src/Arbol/Arbol.http` lists the Development-only probes and the
starter's public pages for manual HTTP checks. Replace its port placeholder with
the endpoint `aspire describe` reports. This starter ships no JSON API; add
requests there as endpoints are added. OpenAPI and problem details are wired for
that work: in Development, `/openapi/v1.json` serves a document for whatever
endpoints exist, and `IProblemDetailsService` is registered for API error
responses. Exposing or authorizing the document in a deployed application is an
application-specific decision. Never commit cookies or credentials.

The dashboard exposes server logs, request traces, Npgsql database spans, and metrics.
Useful CLI commands include `aspire logs arbol-web`, `aspire otel traces`, and
`aspire otel logs`. The Aspire MCP server provides the same resource and telemetry
visibility to agents. Database query telemetry can contain application information;
keep exports and runtime logs out of Git.

Aspire 13.6 retains dashboard run history automatically in **Run** persistence
mode. Use the header's run selector to compare the live run with completed runs;
historical runs are read-only. Up to ten unpinned runs are retained per application,
and pinning keeps a useful run. Console logs are only persisted after their stream
is viewed or exported, so capture needed console output before stopping.

Keep the default dashboard data directory, `<ASPIRE_HOME>/dashboard` (normally
`~/.aspire/dashboard`), outside the checkout. Persisted resource snapshots can
contain unredacted credentials even when the dashboard masks them. On Windows,
the directory inherits filesystem permissions; keep it private and do not share
its databases or backups. The existing Git exclusions cover `.aspire/`, `*.db`,
`*.db-wal`, and `*.db-shm`. No application telemetry changes are needed for run
history. See [dashboard persistence](https://aspire.dev/dashboard/data-persistence/).

For quick SQL inspection, select **REPL** on the running `postgres` resource. The
dashboard opens the container's bundled `psql` with its existing credentials, so
no local client or pgAdmin startup is required. Run `\connect arbol` to switch
from the initial `postgres` database to the application database. Exit with `\q`
before closing the tab; closing the viewer alone can leave the client running.
This shell has normal write permissions, not read-only access. Database reset or
drop still requires explicit intent to delete local data. `WithRepl()` uses our
existing PostgreSQL package without an experimental-warning suppression, while
the dashboard terminal infrastructure remains preview. See
[PostgreSQL REPLs](https://aspire.dev/integrations/databases/postgres/postgres-host/#open-an-interactive-repl).

pgAdmin is available only in run mode. Start `pgadmin` from its dashboard action or:

```powershell
aspire resource pgadmin start --non-interactive
aspire wait pgadmin --timeout 120 --non-interactive
```

Open its generated HTTP endpoint, expand **Servers → postgres → Databases → arbol**.
Aspire configures the connection and credentials. Stop it from the dashboard when done.

## Progressive web app

The server imports `build/PwaAssets.targets`, which generates a versioned
`service-worker.js` from hashed application inputs and serves it at a stable URL.
Installing from the browser adds the app to the home screen or desktop; the
install affordance is hidden when the app already runs standalone.

The worker caches only three public fallback assets (`offline.html`,
`pwa/offline.css`, `icons/icon-192.png`). Navigations are never cached: when a
page load or an enhanced-navigation GET fails, the app serves the offline screen
with a `503` so an interrupted load cannot be mistaken for a successful
operation. Signing in and saving data always need the server.

When a new version is ready the shell shows an update notice. Nothing reloads
until you choose **Reload to update**, and only the tab that asked reloads; other
tabs keep their state and prompt separately. See
[build/README.md](build/README.md#pwa-build-assets) for generation rules and the
[UI instructions](.github/instructions/ui.instructions.md) for the constraints.

Verify a publish includes the worker, manifest, icons, and fallback assets:

```powershell
dotnet publish src/Arbol/Arbol.csproj --configuration Release --output artifacts/pwa-publish
```

## Agent instructions, tools, and skills

Agent instructions are GitHub Copilot only. Repository-wide rules live in
[`.github/copilot-instructions.md`](.github/copilot-instructions.md), and
path-scoped rules live in
[`.github/instructions/*.instructions.md`](.github/instructions) with `applyTo`
globs. Copilot loads a path-scoped file only when a file in scope matches, so the
repository-wide file names every rule-owning file. Where a rule belongs, and which
instruction sources Copilot discovers, is documented in
[agent hook maintenance](build/agent-hooks.md#instruction-sources-and-scope).
There is no `AGENTS.md`, `CLAUDE.md`, or `GEMINI.md`; do not add one. Instruction
changes are not picked up by an active session — restart it.

Project hooks are registered in `.github/hooks/*.json`:
`documentation-review.json` runs the documentation review reminder and finishing
pass, and `validation.json` provides the opt-in Fast validation described in
[agent hook maintenance](build/agent-hooks.md). Hooks are advisory, not commit gates.

Shared repository skills live only in `.agents/skills`, which Copilot discovers.
The Aspire skills use the first-party **aspire-skills v0.0.3** bundle refreshed
with CLI 13.6.0. Local corrections document 13.6's Project v2 diagnostic changes
and the distinction between persistent-resource cleanup and volume deletion.
Reconcile those corrections when refreshing the skills; use installed package/API
evidence and current documentation when upstream guidance differs. Project v2
migration remains deferred; the AppHost continues to use `AddProject`.

The repository registers the Aspire MCP server in [`.mcp.json`](.mcp.json) with
`aspire agent mcp`. Keep a single entry per agent; do not add a duplicate user-level
entry for a machine already using this repository's registration. To use a
user-level entry instead, add a local/stdio server named `aspire` with command
`aspire` and arguments `agent mcp` at user scope through Copilot's MCP settings or
`/mcp add` in the CLI. Restart/reload the agent after configuration. Start the
application from this repository and use the Aspire MCP resource-list tool to
verify the connection. If several AppHosts are running, select this repository's
AppHost explicitly.

The checked-in skills require no additional installation on clone. To refresh the
Aspire skills, use `aspire agent init` with the **standard** skill location
(`.agents/skills`), then review the diff. Avoid creating alternate copies under
`.github` or VS Code agent configuration. Keep user secrets, dashboard tokens,
runtime `.aspire` state, telemetry exports, Playwright CLI configuration and
output, and temporary browser output out of source control.

References: [Aspire MCP](https://aspire.dev/reference/cli/commands/aspire-agent-mcp/),
[Copilot CLI skills](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-skills),
[Copilot custom instructions](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-custom-instructions),
[Copilot desktop](https://docs.github.com/en/copilot/how-tos/github-copilot-app/customize-github-copilot-app).

## Validation

The shared entry point selects the right checks for a scope and stops at the
first failure:

```powershell
pwsh ./scripts/Invoke-Validation.ps1 -Scope Quick          # staged whitespace checks only
pwsh ./scripts/Invoke-Validation.ps1 -Scope Fast           # format verify, build, unit + component tests
& ./scripts/Invoke-Validation.ps1 -Scope Infrastructure -Suite Integration,Aspire
pwsh ./scripts/Invoke-Validation.ps1 -Scope Full -Configuration Release
```

`Fast` is the default and needs no Docker, browser, or running Aspire instance.
Git hooks are opt-in and off by default; see
[build/README.md](build/README.md#local-validation-and-opt-in-git-hooks) to enable
them and for the full scope table.

For every change set, run formatting from the repository root, review the fixes,
and require a clean verification pass. Stop Aspire first on Windows. After code
changes, also build and run the unit and bUnit projects. All five test projects use
native Microsoft.Testing.Platform and Shouldly. The complete suite additionally
requires Docker, Aspire tooling, development HTTPS, and Playwright Chromium:

```powershell
dotnet format Arbol.slnx --severity warn
dotnet format Arbol.slnx --severity warn --verify-no-changes
dotnet build Arbol.slnx
pwsh ./tests/Arbol.PlaywrightTests/bin/Debug/net10.0/playwright.ps1 install chromium
dotnet test --solution Arbol.slnx
```

The unit and component projects can still run individually without external
processes. Testcontainers covers focused database integration, Aspire integration
covers the real resource graph and HTTP behavior, and Playwright covers browser
interaction against its own isolated Aspire application. Test resources are
disposable and never use the development database volumes. See
[tests/README.md](tests/README.md) for commands, prerequisites, layer selection, and
[build/README.md](build/README.md) for formatting scope, analyzer rules, and Razor
code-behind validation. Formatting does not replace checks for other file types.
