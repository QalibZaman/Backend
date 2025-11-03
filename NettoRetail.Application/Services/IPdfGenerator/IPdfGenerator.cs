using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Models.Order;

namespace NettoRetail.Application.Services.IPdfGenerator;

public interface IPdfGenerator
{
    Task<Stream> PrintOrderDetails(PrintOrderDetailsModel model);
}
