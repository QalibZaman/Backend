namespace NettoRetail.API.Infrastructure.Helpers.ErrorHandlers;

public static class ContentEditorErrorCatalog
{
    public const string InitializationErrorMessage =
        "We cannot load [WYSIWYG editor name] at this time. Your content is preserved but cannot be modified until resolved.";

    public static string BuildRecoveryErrorMessage(DateTime timestampUtc) =>
        $"Something went wrong. Editor Recovery Needed. We saved your work at {timestampUtc:O}.";
}
