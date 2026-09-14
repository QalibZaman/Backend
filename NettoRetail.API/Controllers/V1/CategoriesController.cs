using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Categories.Commands;
using NettoRetail.Application.CQRS.Categories.Queries;
using NettoRetail.Application.Models.Request.Category;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1
{
    [ApiController]
    [ApiVersion(1)]
    [Route("api/v{v:ApiVersion}/[controller]")]
    public class CategoriesController(IMediator mediator) : ControllerBase
    {
        [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<GetCategoriesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategories([FromQuery] GetCategoriesRequest pageModel, CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();

            var query = new GetCategoriesQuery(pageModel, shopId);

            var response = await mediator.Send(query, cancellationToken);

            return Ok(new ApiResponse<GetCategoriesResponse>(response));
        }

        [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryRequest model, CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();

            var command = new AddCategoryCommand(model, shopId);

            var response = await mediator.Send(command, cancellationToken);

            return Created(string.Empty, new ApiResponse<CategoryResponse>(response));
        }

        [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
        [HttpGet("parent-options")]
        [ProducesResponseType(typeof(ApiResponse<List<ParentCategoryOptionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetParentCategoryOptions([FromQuery] string? searchTerm, CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();

            var query = new GetParentCategoryOptionsQuery(searchTerm, shopId);

            var response = await mediator.Send(query, cancellationToken);

            return Ok(new ApiResponse<List<ParentCategoryOptionResponse>>(response));
        }

        [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
        [HttpGet("check-name")]
        [ProducesResponseType(typeof(ApiResponse<CheckCategoryNameResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckCategoryName([FromQuery] string name, CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();

            var query = new CheckCategoryNameQuery(name, shopId);

            var response = await mediator.Send(query, cancellationToken);

            return Ok(new ApiResponse<CheckCategoryNameResponse>(response));
        }

        [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();

            var command = new DeleteCategoryCommand(id, shopId);

            await mediator.Send(command, cancellationToken);

            return NoContent();
        }
    }
}