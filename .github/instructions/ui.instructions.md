---
applyTo: "src/Arbol.UI/**"
---

# UI and rendering

These rules govern shared UI and, through the repository-wide instructions,
Razor, styling, navigation, and rendering work in the server, browser host, and
tests. Keep the repository-wide project boundaries and validation requirements in
force, and the [C# and Razor conventions](csharp.instructions.md) for code-behind.
For account behavior, also read the [account guide](account.instructions.md) and
the [server guide](server.instructions.md), including when shared UI changes can
affect static Identity pages. See the [root README](../../README.md) for the
styling and layout architecture and [build rules](../../build/README.md) for
enforcement.

## Razor components

- Every page/component must have a matching `.razor.cs` partial class, including
  markup-only components. `_Imports.razor` does not need code-behind.
- Put component members and logic in code-behind. Do not add `@code` or `@functions`
  blocks. Rendering expressions and control flow remain in `.razor` markup.
- Match the generated component's namespace, class name, and generic parameters.
  `_Imports.razor` using directives do not apply to `.razor.cs` files; add the C#
  imports the code-behind needs.
- Preserve Razor routes, rendering, authorization, injection, and component
  parameter contracts when moving code. Do not edit generated C# files.
- Mark caller-supplied `[Parameter]` properties with `[EditorRequired]` when the
  component needs them to serve its purpose. Keep optional inputs optional and
  assess route, query, form, and cascading inputs separately. Use ordinary
  auto-properties, not C# `required` or `init`. This is a Razor call-site check,
  not runtime validation or a guarantee of non-null values.
- Treat `[Parameter]` properties as caller-owned inputs. Keep editable local state
  separate and use `EventCallback` to request parent updates; reconcile incoming
  parameter changes deliberately. `MA0121` is build-enforced for parameter
  overwrites, including when the parameter is `[EditorRequired]`. Keep Blazor
  `[CascadingParameter]` inputs on the same terms.
- Components that own asynchronous work cancel superseded operations and discard
  stale results instead of overwriting newer state. Component lifetime is the
  cancellation owner for interactive work; the repository-wide
  [cancellation rules](../copilot-instructions.md#validation) own the shared token
  requirements, including the ban on serializing tokens or reusing a completed
  prerender request's token in a circuit.

## Styling and layout

- This solution has no UI component library. Build the interface from semantic
  HTML with CSS Grid for layout and CSS for styling. Do not add a component
  framework, a CSS framework, a CSS preprocessor, or utility classes.
- Declare reusable values as custom properties in the `:root` block of
  `src/Arbol/wwwroot/app.css` and reference them from every stylesheet. Use the
  existing `--color-*`, `--space-*`, `--radius-*`, `--shadow-card`, and
  `--font-sans` tokens instead of new literal colours and sizes; add a token when
  a value is reused.
- Keep the shared baseline and reusable form, button, card, navigation, notice,
  and table patterns in `app.css` (`account-form`, `form-field`, `checkbox-field`,
  `actions`, `button-primary`, `button-subtle`, `card`, `card-links`, `nav-list`,
  `nav-section-label`, `notice`, `table-container`). Keep component-specific rules
  in adjacent `.razor.css` files, with narrowly scoped `::deep` selectors only
  when child component markup requires them.
- A malformed `.razor.css` rule is not a build error. It reaches the generated
  scoped-CSS bundle as written, and an unterminated block swallows every following
  rule in the assembly, breaking unrelated pages. Verify scoped-CSS edits with a
  browser test.
- Use descriptive classes rather than utility classes. Do not reintroduce
  Bootstrap, `!important`, or ornamental animations, and do not hand-write inline
  styles for ordinary layout or spacing. Prefer normal flow and CSS Grid.
- The shell is one CSS Grid in `MainLayout.razor.css` with named `header`,
  `navigation`, `content`, and `footer` areas. At 48rem and wider the navigation is
  a side column; below it the grid collapses to one column and the navigation
  becomes the `#site-menu-toggle` disclosure panel. Keep the toggle a real
  focusable input with a visible `<label>`, keep `#main-content` and the layout IDs
  stable across server renders, and close the panel from `MainLayout.razor.js` on
  `enhancednavigationstart`. Do not force full reloads to close it.
- Keep enhanced navigation enabled for shell links. Identity POSTs still perform
  normal navigation to refresh cookies and authentication state. Keep the shell on
  document scrolling so enhanced navigation can reset and restore scroll position;
  a separate layout scrollbar needs its own handling.
- Give each navigation region its own `<nav aria-label>` containing a `<ul>` of
  `NavLink`s. Native anchors are focusable without a `tabindex` override, and
  `NavLink` supplies `aria-current="page"`. The shell provides the grid cell, not
  a second navigation landmark.
- Keep native Identity inputs and submit buttons: their static SSR form mapping,
  browser autofill, and passkey submitter contracts must work without an
  interactive form. Do not replace them with interactive components.
- Make layouts shrink and wrap on narrow screens, preserve visible keyboard
  focus, and put form labels before their controls. Preserve semantic links,
  buttons, tables, and all Blazor/Identity form and reconnect hooks.
- Shared notices use `class="notice"` with `data-kind="success"`, `"error"`,
  `"warning"`, or `"information"`. Keep meaning in the message, not just color.
- Opt plain GET forms into enhanced handling with `data-enhance` when they target
  a Blazor page. Keep Identity, logout, and other cookie-changing POSTs as normal
  navigations.

## Progressive web app

- `service-worker.js` and the asset manifest are generated at build time from
  hashed inputs. Do not hand-edit generated files, and do not add caching
  strategies outside `build/PwaAssets.targets` and `build/PwaWorker.js`.
- Registration lives in `src/Arbol.UI/wwwroot/pwa.js`. Offer an update only
  with user consent, and reload only the tab that requested the update; never
  refresh every client to apply a new version.

## Rendering and first paint
- An interactively rendered page must not replace prerendered content with an
  empty loading state when its renderer starts. Use a structural skeleton when a
  read is really pending rather than a single loading line, and keep
  display-only first-paint state out of server-owned data you would then have to
  re-query.
- Do not ship large prerendered state. Prerendered state travels inside the
  circuit-start message, which has a 32 KiB SignalR receive limit; values are
  data-protected, Base64-encoded, serialized, and Base64-encoded again, so raw
  JSON is a poor proxy for transport size. Measure the transport form with the
  persistence serializer's options, including null-valued properties, keep a
  budget with headroom, and fall back to the skeleton when it does not fit.
- Mirror a resolved client theme onto the root element when browser chrome
  depends on it; a body-only theme leaves the document scrollbar in the other
  scheme.

## Asynchronous component lifetime

- Components owning asynchronous I/O derive from `CancellableComponentBase` and
  link each operation through `CreateOperation()`; cancel superseded reads, keep
  stale-result checks, and dispose each source when its operation finishes.
  Override `DisposeCoreAsync` for cleanup instead of hiding disposal. The base
  type reads the `RequestAborted` cascade that the static `App` supplies, so
  server-rendered work stops with its request while interactive work is owned by
  the component lifetime. The repository-wide
  [cancellation rules](../copilot-instructions.md#validation) own the shared token
  requirements, including the ban on serializing tokens or reusing a completed
  prerender request's token in a circuit.
