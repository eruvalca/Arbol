# Historical provenance

Arbol's initial application snapshot was generated from the `ArbolFluent.Solution`
template version 1.0.0 at source commit
`8da9d66a13015232c911956007e923d8a84cc81d`.

That starter was itself generated from the `Ruvents.Solution` template version
1.2.0 at source commit `7034699d7a13d017564264fb5b0e3d71139c818d`.

After generation, Arbol replaced Fluent UI Blazor with plain HTML and CSS Grid
styling and removed the Fluent-specific build assets and package references.
Acceptance results recorded in the earlier templates describe those starters. They
do not describe Arbol, and they do not describe applications generated from
`Arbol.Templates`. No separate Arbol historical acceptance matrix is recorded here.

Each candidate `Arbol.Templates` package must be validated by
`scripts/Test-Template.ps1` before installation. That receipt describes the exact
package that was tested; see [Template maintenance](README.md).
