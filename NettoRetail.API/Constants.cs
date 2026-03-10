namespace NettoRetail.API
{
    public static class Constants
    {
        public static class Validation
        {
            public static class Content
            {
                public const int MaxContentLength = 50 * 1024; // 50 KB
                public const string IsRequired = "Content is required.";
                public const string MaxContentLengthMessage = "Content cannot exceed {MaxLength} characters.";
                public const string ContentVisibilityRequiredMessage = "Visibility must be false when content is empty or whitespace.";
            }
        }
    }
}
