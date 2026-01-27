using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Application.Models.Response
{
    public record ToggleLinkProductsResponse(
     Guid Id,
     string Name,
     long Upc,
     bool IsLinked);
}
