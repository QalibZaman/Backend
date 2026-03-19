using Mapster;
using MapsterMapper;
using NettoRetail.Application.CQRS.Orders.Queries;
using NettoRetail.Application.Models.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.OrderTests
{
    public class GetSearchOptionsQueryHandlerTest
    {
        private static OrderEntity Order(int id, DateTime creationDate, string creator, OrderStatusEnum orderStatus, 
            OrderDeliveryTypeEnum deliveryType, Guid custimerId, int shopId) =>
            new OrderEntity
            {
                Id = id,
                CreationDate = creationDate,
                Creator = creator,
                OrderStatus = orderStatus,
                DeliveryType = deliveryType,
                CustomerId = custimerId,
                ShopId = shopId,
                OrderItems = Substitute.For<ICollection<OrderItemEntity>>(),
                Customer = Substitute.For<CustomerEntity>()
            };

        private readonly CustomerEntity _customer;
        private readonly OrderEntity _orderEntity;
        private readonly IMapper _mapper;
        public GetSearchOptionsQueryHandlerTest() 
        {
            _orderEntity = Order(id: 1, DateTime.Now, creator: "Hleb Malusheu", OrderStatusEnum.InProgress,
            OrderDeliveryTypeEnum.SelfDelivery, Guid.NewGuid(), shopId: 1);
            _customer = Substitute.For<CustomerEntity>();
            _customer.Id = Guid.NewGuid();
            _orderEntity.Customer = _customer;
            _customer.FirstName = "Hleb";
            _customer.LastName = "Malusheu";
            _customer.Email = "hlebMal@gmail.com";
            _mapper = MapperFactory.Create();

        }

        static GetSearchOptionsQueryHandlerTest()
        {

            TypeAdapterConfig.GlobalSettings.Scan(typeof(GetSearchOptionsQueryHandler).Assembly);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList()
        {
            //Arrange
            SearchFieldModel searchFieldModel = Substitute.ForPartsOf<SearchFieldModel>();
            searchFieldModel.Field = "customer";
            searchFieldModel.SearchTerm = "hlebMal@gmail.com";
            GetSearchOptionsQuery getSearchOptionsQuery = new(searchFieldModel, 1);
            var data = new List<OrderEntity>
            {
                _orderEntity
            };
            var repo = Substitute.For<IOrderRepository>();
            repo.GetAllBySpecAsync(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>())
                .Returns(data);
            GetSearchOptionsQueryHandler handler = new GetSearchOptionsQueryHandler(repo, _mapper);
            //Act & Assert
            Assert.Empty(await handler.Handle(getSearchOptionsQuery, CancellationToken.None));
        }
        [Fact]
        public async Task Handle_ShouldReturnOrderUsingEmail()
        {
            //Arrange
            SearchFieldModel searchFieldModel = Substitute.For<SearchFieldModel>();
            searchFieldModel.Field = "customeremail";
            searchFieldModel.SearchTerm = "hlebMal@gmail.com";
            GetSearchOptionsQuery getSearchOptionsQuery = new(searchFieldModel, 1);
            var data = new List<OrderEntity>
            {
                _orderEntity
            };
            var repo = Substitute.For<IOrderRepository>();
            repo.GetAllBySpecAsync(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>())
                .Returns(data);
            GetSearchOptionsQueryHandler handler = new(repo, _mapper);
            //Act
            var result = await handler.Handle(getSearchOptionsQuery, CancellationToken.None);
            //Assert
            Assert.Equal(_orderEntity.Id, result.First().Id);
            Assert.Equal(_orderEntity.Creator, result.First().Creator);
            Assert.Equal(_orderEntity.Customer.Email, result.First().Email);
            Assert.Equal(_orderEntity.Customer.FullName, result.First().FullName);
            Assert.Equal(_orderEntity.CreationDate, result.First().CreationDate);
            Assert.Equal(_orderEntity.OrderStatus, result.First().OrderStatus);
        }

        [Fact]
        public async Task Handle_ShouldReturnOrderUsingUserId()
        {
            //Arrange
            SearchFieldModel searchFieldModel = Substitute.For<SearchFieldModel>();
            searchFieldModel.Field = "orderid";
            searchFieldModel.SearchTerm = $"{_orderEntity.Id.ToString()}";
            GetSearchOptionsQuery getSearchOptionsQuery = new(searchFieldModel, 1);
            var data = new List<OrderEntity>
            {
                _orderEntity
            };
            var repo = Substitute.For<IOrderRepository>();
            repo.GetAllBySpecAsync(Arg.Any<Specification<OrderEntity>>(), Arg.Any<CancellationToken>())
                .Returns(data);
            GetSearchOptionsQueryHandler handler = new(repo, _mapper);
            //Act 
            var result = await handler.Handle(getSearchOptionsQuery, CancellationToken.None);
            //Assert
            Assert.Equal(_orderEntity.Id, result.First().Id);
            Assert.Equal(_orderEntity.Creator, result.First().Creator);
            Assert.Equal(_orderEntity.Customer.Email, result.First().Email);
            Assert.Equal(_orderEntity.Customer.FullName, result.First().FullName);
            Assert.Equal(_orderEntity.CreationDate, result.First().CreationDate);
            Assert.Equal(_orderEntity.OrderStatus, result.First().OrderStatus);
        }
    }
}
