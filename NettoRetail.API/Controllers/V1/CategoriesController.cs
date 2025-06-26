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

namespace NettoRetail.API.Controllers.V1
{
    [ApiController]
    [ApiVersion(1)]
    [Route("api/v{v:ApiVersion}/[controller]")]
    public class CategoriesController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] PaginationRequest pageModel,
            [FromQuery] GetCategoriesRequest model, CancellationToken cancellationToken)
        {
            var query = new GetCategoriesQuery(pageModel, model.ShopId);

            var response = await mediator.Send(query, cancellationToken);

            return Ok(new ApiResponse<GetCategoriesResponse>(response));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddCategory(AddCategoryRequest model, CancellationToken cancellationToken)
        {
            var command = model.Adapt<AddCategoryCommand>();

            var response = await mediator.Send(command, cancellationToken);

            return Created(string.Empty, new ApiResponse<CategoryResponse>(response));
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
        {
            await mediator.Send(new DeleteCategoryCommand(id), cancellationToken);

            return NoContent();
        }

        [Authorize]
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> EditCategory(int id, UpdateCategoryRequest model,
            CancellationToken cancellationToken)
        {
            var command = model.Adapt<UpdateCategoryCommand>();
            var shopId = HttpContext.GetUserShopId();
            command = command with { Id = id, ShopId = shopId };

            var response = await mediator.Send(command, cancellationToken);

            return Ok(new ApiResponse<CategoryResponse>(response));
        }

        [HttpGet("section")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoriesSectionForEdit([FromQuery] int shopId, CancellationToken cancellationToken)
        {
            var query = new GetCategoriesSectionForEditQuery(shopId);

            var response = await mediator.Send(query, cancellationToken);

            return Ok(new ApiResponse<EditCategoriesSectionRequest>(response));
        }

        [Authorize]
        [HttpPatch("section")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCategoriesSection([FromBody] EditCategoriesSectionRequest request, CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId();

            var command = new UpdateCategoriesSectionCommand(
                shopId,
                request.Name,
                request.Preview,
                request.Visibility);

            await mediator.Send(command, cancellationToken);

            return NoContent();
        }
    }
}