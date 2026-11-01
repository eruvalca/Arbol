using Microsoft.AspNetCore.Components;

namespace Arbol.Components;

public sealed partial class App
{
    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;
}
