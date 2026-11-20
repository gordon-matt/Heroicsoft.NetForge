using Heroicsoft.NetForge.Components.Navigation;

namespace Heroicsoft.NetForge.Tests;

public class NetForgeMenuBarTests
{
    [Fact]
    public void Hover_WhenIdle_DoesNotOpen()
    {
        var bar = new NetForgeMenuBar();
        object item = new();

        bar.Hover(item);

        Assert.False(bar.AnyOpen);
        Assert.False(bar.IsOpen(item));
    }

    [Fact]
    public void Toggle_OpensAndClickingTheSameItemCloses()
    {
        var bar = new NetForgeMenuBar();
        object item = new();

        bar.Toggle(item);
        Assert.True(bar.IsOpen(item));

        bar.Toggle(item);
        Assert.False(bar.AnyOpen);
        Assert.False(bar.IsActive);
        bar.Hover(item);
        Assert.False(bar.AnyOpen);
    }

    [Fact]
    public void Hover_WhenOpen_SwitchesAndLeavesOnlyOneOpen()
    {
        var bar = new NetForgeMenuBar();
        object first = new();
        object second = new();

        bar.Toggle(first);
        bar.Hover(second);

        Assert.False(bar.IsOpen(first));
        Assert.True(bar.IsOpen(second));
    }

    [Fact]
    public void Hover_OntoAnItemWithoutADropdown_HidesThePanelButStaysArmed()
    {
        var bar = new NetForgeMenuBar();
        object first = new();
        object second = new();

        bar.Toggle(first);
        bar.Hover(null);

        Assert.False(bar.AnyOpen);
        Assert.True(bar.IsActive);

        bar.Hover(second);

        Assert.True(bar.IsOpen(second));
    }

    [Fact]
    public void Close_ResetsSoTheNextHoverDoesNotOpen()
    {
        var bar = new NetForgeMenuBar();
        object first = new();
        object second = new();

        bar.Toggle(first);
        bar.Close();
        bar.Hover(second);

        Assert.False(bar.AnyOpen);
    }
}
