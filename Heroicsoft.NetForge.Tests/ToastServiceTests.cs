using Heroicsoft.NetForge.Components.Toasts;

namespace Heroicsoft.NetForge.Tests;

public class ToastServiceTests
{
    [Fact]
    public void ShowBulk_WhenEverythingSucceeded_SaysSoWithPluralisation()
    {
        using var toasts = new ToastService();

        toasts.ShowBulk(1, 0, "Deleted");
        toasts.ShowBulk(3, 0, "Moved");

        var messages = toasts.Toasts.Select(toast => toast.Message).ToList();
        Assert.Contains("Deleted 1 item.", messages);
        Assert.Contains("Moved 3 items.", messages);
        Assert.All(toasts.Toasts, toast => Assert.Equal("success", toast.Level));
    }

    [Fact]
    public void ShowBulk_WhenNothingSucceeded_ReportsAnErrorWithTheFirstFailure()
    {
        using var toasts = new ToastService();

        toasts.ShowBulk(0, 2, "Deleted", "Access denied.");

        var toast = Assert.Single(toasts.Toasts);
        Assert.Equal("danger", toast.Level);
        Assert.Equal("None of the items were deleted. Access denied.", toast.Message);
    }

    [Fact]
    public void ShowBulk_WhenPartlySuccessful_WarnsWithCounts()
    {
        using var toasts = new ToastService();

        toasts.ShowBulk(2, 1, "Copied", "Disk full.");

        var toast = Assert.Single(toasts.Toasts);
        Assert.Equal("warning", toast.Level);
        Assert.Equal("Copied 2 of 3 items. 1 failed: Disk full.", toast.Message);
    }
}