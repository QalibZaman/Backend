using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using NettoRetail.API.Controllers.V1;
using NettoRetail.Domain.Enums;

namespace NettoRetail.API.Infrastructure.Swagger;

public sealed class CreateOrderExampleOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(OrdersController) ||
            !string.Equals(context.MethodInfo.Name, nameof(OrdersController.CreateOrder), StringComparison.Ordinal))
        {
            return;
        }

        if (operation.RequestBody?.Content == null)
        {
            return;
        }

        if (!operation.RequestBody.Content.TryGetValue("application/json", out var content))
        {
            return;
        }

        content.Example = new OpenApiObject
        {
            ["customerId"] = new OpenApiString("11111111-1111-1111-1111-111111111111"),
            ["deliveryType"] = new OpenApiInteger((int)OrderDeliveryTypeEnum.Courier),
            ["products"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["inventoryItemId"] = new OpenApiString("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    ["amount"] = new OpenApiInteger(2)
                },
                new OpenApiObject
                {
                    ["inventoryItemId"] = new OpenApiString("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    ["amount"] = new OpenApiInteger(1)
                }
            }
        };
    }
}
