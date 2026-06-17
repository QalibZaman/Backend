using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NettoRetail.Application.Models.Request;

namespace NettoRetail.Application.Utilities;

public class SortingOptionsModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        var value = valueProviderResult.FirstValue;

        if (string.IsNullOrWhiteSpace(value))
        {
            return Task.CompletedTask;
        }

        try
        {
            List<SortingOption>? result;

            if (value.TrimStart().StartsWith("["))
            {
                result = JsonSerializer.Deserialize<List<SortingOption>>(value, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                var singleOption = JsonSerializer.Deserialize<SortingOption>(value, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                result = singleOption != null ? new List<SortingOption> { singleOption } : new List<SortingOption>();
            }

            bindingContext.Result = ModelBindingResult.Success(result);
        }
        catch (JsonException)
        {
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }
}
