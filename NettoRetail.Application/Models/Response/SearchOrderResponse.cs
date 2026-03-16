using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response;

public class SearchOrderResponse
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderStatusEnum OrderStatus { get; set; }
    public string Creator { get; set; }
}
