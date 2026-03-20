using Asp.Versioning;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Categories.Commands;
using NettoRetail.Application.CQRS.Categories.Queries;
using NettoRetail.Application.Models.Request;
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

        public async Task<IActionResult> GetCategories([FromQuery] PaginationRequest pageModel, CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();

            var query = new GetCategoriesQuery(pageModel, shopId);

            var response = await mediator.Send(query, cancellationToken);

            return Ok(new ApiResponse<GetCategoriesResponse>(response));
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddCategory(AddCategoryRequest model, CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();

            var command = new AddCategoryCommand(model, shopId);

            var response = await mediator.Send(command, cancellationToken);

            return Created(string.Empty, new ApiResponse<CategoryResponse>(response));
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
        {
            await mediator.Send(new DeleteCategoryCommand(id), cancellationToken);

            return NoContent();
        }

        [Authorize]
        [HttpPatch("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditCategory(int id, UpdateCategoryRequest model, CancellationToken cancellationToken)
        {
            var command = model.Adapt<UpdateCategoryCommand>();
            var shopId = HttpContext.GetUserShopId();
            command = command with { Id = id, ShopId = shopId };

            var response = await mediator.Send(command, cancellationToken);

            return Ok(new ApiResponse<CategoryResponse>(response));
        }

        [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
        [HttpGet("section")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoriesSection(CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();
            var query = new GetCategoriesSectionQuery(shopId);

            var response = await mediator.Send(query, cancellationToken);

            return Ok(new ApiResponse<GetCategoriesSectionResponse>(response));
        }

        [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
        [HttpGet("section/search")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<CategorySummaryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchCategoriesForSection(
            [FromQuery] string? query,
            [FromQuery] List<int>? excludeIds,
            [FromQuery] PaginationRequest pageModel,
            CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();
            var searchQuery = new SearchCategoriesForSectionQuery(
                shopId,
                query,
                excludeIds ?? [],
                pageModel);

            var response = await mediator.Send(searchQuery, cancellationToken);

            return Ok(new ApiResponse<PaginatedResponse<CategorySummaryResponse>>(response));
        }

        [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
        [HttpPatch("{shopId:int}/section")]
        [HttpPatch("section")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCategoriesSection(
            [FromBody] EditCategoriesSectionRequest request,
            CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();
            var command = new UpdateCategoriesSectionCommand(
                shopId,
                request.Name,
                request.Visibility,
                request.CategoryIds);

            await mediator.Send(command, cancellationToken);

            return NoContent();
        }

        [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
        [HttpGet("get-category-product-statuses/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<GetCategoryProductStatusesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoryProductStatuses(int id, CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();

            var command = new GetCategoryProductStatusesCommand(shopId, id);

            var result = await mediator.Send(command,cancellationToken);

            return Ok(new ApiResponse<GetCategoryProductStatusesResponse>(result));
        }
    }
}
