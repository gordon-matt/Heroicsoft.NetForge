using Ardalis.Result;
using Heroicsoft.NetForge.Results;

namespace Heroicsoft.NetForge.Tests;

public class ResultExtensionsTests
{
    [Fact]
    public void To_CarriesNotFoundOntoTheDestinationType()
    {
        IResult source = Result.NotFound("gone");

        var converted = source.To<int>();

        Assert.Equal(ResultStatus.NotFound, converted.Status);
        Assert.Equal("gone", converted.Errors.Single());
    }

    [Fact]
    public void To_RefusesToDropASuccessfulValue()
    {
        IResult source = Result.Success();

        Assert.Throws<InvalidOperationException>(source.To<int>);
    }

    [Fact]
    public void GetDisplayMessage_PrefersTheErrorThenAGenericFallback()
    {
        Assert.Equal("nope", Result.Forbidden("nope").GetDisplayMessage());
        Assert.Equal("That item no longer exists.", Result.NotFound().GetDisplayMessage());
        Assert.Equal("The page is gone.", Result.NotFound().GetDisplayMessage("The page is gone."));
    }
}