using System.Net;
using Microsoft.AspNetCore.Http;
using NettoRetail.API.Infrastructure.Helpers.ErrorHandlers;

namespace NettoRetail.Tests.UnitTests.ContentManagementTests.ErrorHandling;

public class ApiExceptionHandlerContentTests
{
    [Fact]
    public void Handle_ContentGetException_Returns_Em125()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/v1/content/block/slug/1-OurStory";

        var result = ApiExceptionHandler.Handle(new Exception("boom"), context);

        Assert.Equal((int)HttpStatusCode.InternalServerError, result.Status);
        Assert.Equal(
            "We cannot load [WYSIWYG editor name] at this time. Your content is preserved but cannot be modified until resolved.",
            result.Result.Message);
        Assert.NotNull(result.Result.Meta);
        Assert.True(result.Result.Meta!.ContainsKey("timestamp"));
    }

    [Fact]
    public void Handle_ContentUpdateException_Returns_Em126()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Put;
        context.Request.Path = "/api/v1/content/block/1-OurStory";

        var result = ApiExceptionHandler.Handle(new Exception("boom"), context);

        Assert.Equal((int)HttpStatusCode.InternalServerError, result.Status);
        Assert.StartsWith("Something went wrong. Editor Recovery Needed. We saved your work at ", result.Result.Message);
        Assert.NotNull(result.Result.Meta);
        Assert.True(result.Result.Meta!.ContainsKey("timestamp"));
    }
}
