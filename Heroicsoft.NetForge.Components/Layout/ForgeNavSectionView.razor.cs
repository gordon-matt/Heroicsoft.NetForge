using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Layout;

public partial class ForgeNavSectionView
{
    [Parameter, EditorRequired]
    public ForgeNavSection Section { get; set; } = default!;
}