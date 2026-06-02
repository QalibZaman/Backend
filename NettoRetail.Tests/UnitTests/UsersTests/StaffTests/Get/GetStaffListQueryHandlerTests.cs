using FluentAssertions;
using MapsterMapper;
using NettoRetail.Application.CQRS.Users.Queries.Staff;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.UsersTests.StaffTests.Get;

public class GetStaffListQueryHandlerTests
{
    private readonly IStaffRepository _staffRepository;
    private readonly IMapper _mapper;
    private readonly GetStaffListQueryHandler _handler;
    public GetStaffListQueryHandlerTests()
    {
        _staffRepository = Substitute.For<IStaffRepository>();
        _mapper = MapperFactory.Create();
        _handler = new GetStaffListQueryHandler(_staffRepository, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResponse_WithMappedItemsAndTotalCount()
    {
        var entities = new List<StaffEntity>
        {
            new StaffEntity
            {
                Id = Guid.Empty,
                FirstName = "Test",
                Email = "test@example.com",
                LastName = "Test",
                PasswordHash = "hashedpassword",
                Status = StaffStatusEnum.Active,
                Role = new RoleEntity
                {
                    Name = "Admin"
                }
            }
        };

        var mapped = new List<StaffListResponse> { new StaffListResponse() { Id = Guid.Empty, FirstName = "Test", Email = "test@example.com", LastName = "Test", Status = "Active", Role = "Admin", PasswordUpdatedAt = default } };
        SetupRepository(entities, 42);

        var result = await _handler.Handle(BuildQuery(), CancellationToken.None);

        result.Items.Should().BeEquivalentTo(mapped);
        result.TotalAmount.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyItems_WhenRepositoryReturnsNoResults()
    {
        SetupRepository([], 0);

        var result = await _handler.Handle(BuildQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalAmount.Should().Be(0);
    }

    private static GetStaffListQuery BuildQuery(
      int shopId = 1,
      int pageNumber = 1,
      int pageSize = 10,
      string? firstName = null,
      string? lastName = null,
      string? email = null,
      int? roleId = null,
      string? status = null,
      List<SortingOption>? sortingOptions = null) =>
      new(
          new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize },
          new GetFilteredStaffRequest
          {
              FirstName = firstName,
              LastName = lastName,
              Email = email,
              RoleId = roleId,
              Status = status,
              SortingOptions = sortingOptions
          },
          shopId);

    private void SetupRepository(IEnumerable<StaffEntity> items, int totalCount)
    {
        _staffRepository
            .GetStaffAccountsPaginated(
                Arg.Any<Specification<StaffEntity>>(),
                    Arg.Any<CancellationToken>())
                .Returns((items.ToList(), totalCount));
    }
}
