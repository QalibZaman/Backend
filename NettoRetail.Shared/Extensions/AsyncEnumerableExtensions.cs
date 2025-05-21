namespace NettoRetail.Shared.Extensions
{
    public static class AsyncEnumerableExtensions
    {
        public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
        {
            await foreach (var item in source.WithCancellation(cancellationToken))
            {
                return true;
            }

            return false;
        }
    }
}
