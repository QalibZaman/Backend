namespace NettoRetail.DataAccess.Options;

public class CosmosOptions
{
    public required string AccountEndpoint { get; set; }
    public required string AccountKey { get; set; }
    public required string DatabaseName { get; set; }
    public required string ContainerName { get; set; }
}
