namespace NettoRetail.Infrastructure.Models;

// I know it
// not the best idea to store models in infrastructure layer, but the architecture
// of the project (https://wiki.andersenlab.com/display/LNP/Architecture) basically forces me
// into storing models that are fetched in the infra level

public record Location(string? City, string? Country);
