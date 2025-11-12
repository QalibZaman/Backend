using Mapster;
using Microsoft.EntityFrameworkCore;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.NewProductsSection;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Utilities;
using static System.Net.Mime.MediaTypeNames;

namespace NettoRetail.Application.MappingConfiguration;

public class MappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AdminSignUpModel, AccountEntity>()
            .Map(dest => dest.PasswordHash, src => src.Password);

        config.NewConfig<CategoryEntity, CategoryResponse>()
            .Map(dest => dest.ItemsAmount, src => src.InventoryItems.Count());

        config.NewConfig<InventoryItemEntity, InventoryItemListResponse>()
            .Map(dest => dest.ProductCategory, src => src.Category.Name)
            .Map(dest => dest.BrandName, src => src.Brand!.Name);

        config.NewConfig<RoleEntity, RoleResponse>()
            .Map(dest => dest.UserCount, src => src.Accounts.Count());

        config.NewConfig<InventoryItemEntity, InventoryItemCsvModel>()
            .Map(dest => dest.ProductCategory, src => src.Category.Name);

        config.NewConfig<AccountEntity, StaffListResponse>()
            .Map(dest => dest.Role, src => src.Role.Name);

        config.NewConfig<CustomerEntity, CustomerResponse>()
            .Map(dest => dest.FirstName, src => src.Account.FirstName)
            .Map(dest => dest.LastName, src => src.Account.LastName)
            .Map(dest => dest.Email, src => src.Account.Email);

        config.NewConfig<UpdateInventoryItemModel, InventoryItemEntity>()
            .IgnoreNullValues(true);

        config.NewConfig<ProductDetailsModel, ProductDetailsEntity>()
            .Map(dest => dest.TypeEnum, src => Enum.Parse<ProductDetailTypeEnum>(src.Type))
            .Map(dest => dest.Unit, src =>
                string.IsNullOrEmpty(src.Unit) ? null : (ProductDetailUnitEnum?)Enum.Parse<ProductDetailUnitEnum>(src.Unit));

        config.NewConfig<ProductDetailsEntity, ProductDetailsResponse>()
            .Map(dest => dest.Type, src => src.TypeEnum.ToString());

        config.NewConfig<CustomerEntity, CustomerInfoResponse>()
            .Map(dest => dest.FirstName, src => src.Account.FirstName)
            .Map(dest => dest.LastName, src => src.Account.LastName)
            .Map(dest => dest.Email, src => src.Account.Email);

        config.NewConfig<OrderEntity, OrderResponse>()
            .Map(dest => dest.CustomerFirstName, src => src.Customer.Account.FirstName)
            .Map(dest => dest.CustomerLastName, src => src.Customer.Account.LastName)
            .Map(dest => dest.CustomerEmail, src => src.Customer.Account.Email);

        config.NewConfig<UpdateCustomerRequest, CustomerEntity>()
            .IgnoreNullValues(true);

        config.NewConfig<UpdateCustomerRequest, AccountEntity>()
            .IgnoreNullValues(true);

        config.NewConfig<OrderEntity, OrderListResponse>()
            .Map(dest => dest.CustomerFirstName, src => src.Customer.Account.FirstName)
            .Map(dest => dest.CustomerLastName, src => src.Customer.Account.LastName)
            .Map(dest => dest.CustomerEmail, src => src.Customer.Account.Email)
            .Map(dest => dest.OrderStatus, src => src.OrderStatus.ToString())
            .Map(dest => dest.DeliveryType, src => src.DeliveryType.ToString())
            .Map(dest => dest.CanEditData, src => src.OrderStatus == OrderStatusEnum.New || src.OrderStatus == OrderStatusEnum.Processing);

        config.NewConfig<UpdateStaffRequest, AccountEntity>()
            .IgnoreNullValues(true);

        config.NewConfig<UpdateOrderModel, OrderEntity>()
            .IgnoreNullValues(true);

        config.NewConfig<OrderEntity, OrderResponse>()
            .Map(dest => dest.CustomerFirstName, src => src.Customer.Account.FirstName)
            .Map(dest => dest.CustomerLastName, src => src.Customer.Account.LastName)
            .Map(dest => dest.CustomerEmail, src => src.Customer.Account.Email)
            .Map(dest => dest.OrderStatus, src => src.OrderStatus.ToString())
            .Map(dest => dest.DeliveryType, src => src.DeliveryType.ToString())
            .Map(dest => dest.CustomerEmail, src => src.Customer.Account.Email);

        config.NewConfig<OrderItemEntity, OrderItemResponse>()
            .Map(dest => dest.ProductName, src => src.InventoryItem.ProductName)
            .Map(dest => dest.ProductUPC, src => src.InventoryItem.ProductUpc)
            .Map(dest => dest.AvailableAmount, src => src.InventoryItem.AvailableAmount)
            .Map(dest => dest.Price, src => src.InventoryItem.Price)
            .Map(dest => dest.Quantity, src => src.Quantity);

        config.NewConfig<OrderEntity, OrderPrintingResponse>()
            .Map(dest => dest.CustomerId, src => src.CustomerId)
            .Map(dest => dest.CreationDate, src => DateOnly.FromDateTime(src.CreationDate))
            .Map(dest => dest.CustomerFirstName, src => src.Customer.Account.FirstName)
            .Map(dest => dest.CustomerLastName, src => src.Customer.Account.LastName)
            .Map(dest => dest.CustomerEmail, src => src.Customer.Account.Email)
            .Map(dest => dest.InventoryItems, src => src.OrderItems.Select(model => model.InventoryItem))
            .Map(dest => dest.OrderStatus, src => src.OrderStatus.ToString())
            .Map(dest => dest.DeliveryType, src => src.DeliveryType.ToString());

        config.NewConfig<OrderEntity, OrderResponse>()
           .Map(dest => dest.CustomerEmail, src => src.Customer.Account.Email)
           .Map(dest => dest.CustomerFirstName, src => src.Customer.Account.FirstName)
           .Map(dest => dest.CustomerLastName, src => src.Customer.Account.LastName);

        config.NewConfig<UpdateBrandRequest, BrandEntity>()
            .IgnoreNullValues(true);

        config.NewConfig<ShopEntity, ClientsShopInfoResponse>()
            .Map(dest => dest.ShopEmail, src => src.Email)
            .Map(dest => dest.Country, src => src.Country.Name)
            .Map(dest => dest.Subscription, src => src.Subscription.ToString());

        config.NewConfig<AccountEntity, StaffCsvModel>()
            .Map(dest => dest.Role, src => src.Role.Name)
            .Map(dest => dest.Status, src => src.Status.ToString());

        config.NewConfig<InventoryItemEntity, InventoryItemInfoResponse>()
            .Map(dest => dest.Status, src => src.Status.ToString());

        config.NewConfig<OrderEntity, OrderHistoryResponse>()
            .Map(dest => dest.DeliveryType, src => src.DeliveryType.ToString())
            .Map(dest => dest.OrderStatus, src => src.OrderStatus.ToString());

        config.NewConfig<ShopEntity, AllShopsInfoResponse>()
            .Map(dest => dest.Subscription, src => src.Subscription.ToString());

        config.NewConfig<StaffCsvModel, AccountEntity>()
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Status, src => Enum.Parse<StaffStatusEnum>(src.Status, true))
            .Ignore(dest => dest.Role)
            .Ignore(dest => dest.PasswordHash);

        config.NewConfig<FeedbackEntity, FeedbackResponse>()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.ShopName, src => src.Account.Shop != null ? src.Account.Shop.Name : null);

        config.NewConfig<NewProductsSectionEntity, GetNewProductsSectionResponse>()
            .Map(dest => dest.SelectionType, src => src.SelectionType.ToString())
            .Map(dest => dest.Products, src => src.Products.Select(p => p.InventoryItemId).ToList());

        config.NewConfig<AccountEntity, PlatformAdminResponse>()
            .Map(
                dest => dest.LastActivityTime,
                src => src.Sessions != null && src.Sessions.Any()
                ? src.Sessions.Max(s => s.LastActivityTime)
                : (DateTime?)null)
            .Map(
                dest => dest.Permissions,
                src => src.Role != null && src.Role.PermissionRoles != null
                ? src.Role.PermissionRoles.Select(p => p.PermissionId).ToList()
                : new List<int>());

        config.NewConfig<FavouriteListItemEntity, AddToFavouritesResponse>();

        config.NewConfig<CartItemEntity, CartItemResponse>()
            .Map(dest => dest.ProductId, src => src.InventoryItemId);

        config.NewConfig<BrandEntity, BrandResponse>();
    }
}