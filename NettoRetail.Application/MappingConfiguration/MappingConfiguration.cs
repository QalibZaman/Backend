using Mapster;
using NettoRetail.Application.CQRS.Users.Commands;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Customer;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.NewProductsSection;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Request.Brand;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.Application.Models.Response.Roles;
using NettoRetail.Application.Models.Subscriptions;
using NettoRetail.Application.Utilities;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.MappingConfiguration;

public class MappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CustomerEntity, CustomerContactInfoResponse>()
            .Map(dest => dest.AccountId, src => src.AccountId)
            .Map(dest => dest.CustomerName, src => src.Account.FirstName + " " + src.Account.LastName)
            .Map(dest => dest.Email, src => src.Account.Email)
            .Map(dest => dest.Phone, src => src.Account.PhoneNumber)
            .Map(dest => dest.LoyaltyId, src => src.LoyaltyId)
            .Map(dest => dest.Remarks, src => src.Remarks);

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
            .Map(dest => dest.ProductCategory, src => src.Category.Name)
            .Map(dest => dest.Brand, src => src.Brand != null ? src.Brand.Name : null);

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
                string.IsNullOrEmpty(src.Unit)
                    ? null
                    : (ProductDetailUnitEnum?)Enum.Parse<ProductDetailUnitEnum>(src.Unit));

        config.NewConfig<ProductDetailsEntity, ProductDetailsResponse>()
            .Map(dest => dest.Type, src => src.TypeEnum.ToString());

        config.NewConfig<CustomerEntity, CustomerInfoResponse>()
            .Map(dest => dest.FirstName, src => src.Account.FirstName)
            .Map(dest => dest.LastName, src => src.Account.LastName)
            .Map(dest => dest.Email, src => src.Account.Email);

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
            .Map(dest => dest.CanEditData,
                src => src.OrderStatus == OrderStatusEnum.New || src.OrderStatus == OrderStatusEnum.Processing);

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
            .Map(dest => dest.CustomerEmail, src => src.Customer.Account.Email)
            .Map(dest => dest.OrderItems, src => src.OrderItems);

        config.NewConfig<OrderItemEntity, OrderItemResponse>()
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Map(dest => dest.Price, src => src.InventoryItem.Price)
            .Map(dest => dest.ProductName, src => src.InventoryItem.ProductName)
            .Map(dest => dest.ProductUPC, src => src.InventoryItem.ProductUpc.ToString())
            .Map(dest => dest.AvailableAmount, src => src.InventoryItem.AvailableAmount)
            .Map(dest => dest.IsPromoted, src => src.InventoryItem.IsPromoted)
            .Map(dest => dest.PromotionPrice, src => src.InventoryItem.PromotionPrice ?? 0.0m)
            .AfterMapping((src, dest) =>
            {
                dest.Total = dest.IsPromoted
                    ? dest.PromotionPrice * dest.Quantity
                    : dest.Price * dest.Quantity;
            });

        config.NewConfig<OrderEntity, PrintOrderDetailsModel>()
            .Map(dest => dest.CustomerFirstName, src => src.Customer.Account.FirstName)
            .Map(dest => dest.CustomerLastName, src => src.Customer.Account.LastName)
            .Map(dest => dest.CustomerEmail, src => src.Customer.Account.Email)
            .Map(dest => dest.InventoryItems, src => src.OrderItems.Select(model => model.InventoryItem))
            .Map(dest => dest.OrderStatus, src => src.OrderStatus.ToString())
            .Map(dest => dest.DeliveryType, src => src.DeliveryType.ToString());

        config.NewConfig<UpdateBrandRequest, BrandEntity>()
            .IgnoreNullValues(true);

        config.NewConfig<ShopEntity, ClientsShopInfoResponse>()
            .Map(dest => dest.ShopEmail, src => src.Email)
            .Map(dest => dest.Country, src => src.Country.Name);

        config.NewConfig<AccountEntity, StaffCsvModel>()
            .Map(dest => dest.Role, src => src.Role.Name)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName);

        config.NewConfig<InventoryItemEntity, InventoryItemInfoFullResponse>()
            .Map(dest => dest.Status, src => src.Status.ToString());

        config.NewConfig<OrderStatusHistoryEntity, OrderStatusHistoryResponse>()
            .Map(dest => dest.OrderStatus, src => src.NewStatus.ToString())
            .Map(dest => dest.UpdatedBy, src => src.UpdatedBy.FullName);

        config.NewConfig<ShopEntity, AllShopsInfoResponse>();

        config.NewConfig<SubscriptionPlanEntity, SubscriptionPlanResponse>()
            .Map(dest => dest.Name, src => src.Name);

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

        config.NewConfig<InventoryItemEntity, InventoryItemCsvModel>()
            .Map(dest => dest.ProductUpc, src => src.ProductUpc)
            .Map(dest => dest.ProductName, src => src.ProductName)
            .Map(dest => dest.ProductLocation, src => src.ProductLocation)
            .Map(dest => dest.AvailableAmount, src => src.AvailableAmount)
            .Map(dest => dest.ReservedAmount, src => src.ReservedAmount)
            .Map(dest => dest.OnHandAmount, src => src.OnHandAmount)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.Description, src => src.Description)
            .Ignore(dest => dest.CategoryId)
            .Ignore(dest => dest.ShopId)
            .Ignore(dest => dest.Brand);

        config.NewConfig<InventoryItemCsvModel, InventoryItemEntity>()
            .Ignore(dest => dest.Brand)
            .Ignore(dest => dest.Category);

        config.NewConfig<AddInventoryItemModel, InventoryItemEntity>()
            .Ignore(nameof(InventoryItemEntity.ProductDetails));

        config.NewConfig<ProductDetailModel, ProductDetailEntity>()
            .Map(dest => dest.TypeEnum, src => src.Type);

        config.NewConfig<UpsertProductDetailModel, ProductDetailEntity>()
            .Map(dest => dest.TypeEnum, src => src.Type);

        config.NewConfig<ProductDetailEntity, ProductDetailModel>()
            .Map(dest => dest.Type, src => src.TypeEnum);

        config.NewConfig<UpdatePersonalInformationRequest, UpdatePersonalInformationCommand>()
            .Map(dest => dest.State, src => string.IsNullOrWhiteSpace(src.State) ? null : src.State);

        config.NewConfig<InventoryItemEntity, InventoryItemCardResponse>()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ProductName, s => s.ProductName)
            .Map(d => d.CategoryName, s => s.Category != null ? s.Category.Name : string.Empty)
            .Map(d => d.ThumbnailUrl, s =>
                s.Images != null
                    ? s.Images.OrderBy(i => i.Order).Select(i => i.BlobUrl).FirstOrDefault() ?? string.Empty
                    : string.Empty)
            .Map(d => d.Price, s => s.Price)
            .Map(d => d.PromotionPrice, s => s.PromotionPrice)
            .Map(d => d.AvailableAmount, s => s.AvailableAmount)
            .Map(d => d.IsOutOfStock, s => s.AvailableAmount <= 0);

        config.NewConfig<PaymentHistoryEntity, PaymentHistoryResponse>()
            .Map(dest => dest.ShopName, src => src.Account.Shop != null ? src.Account.Shop.Name : null);

        config.NewConfig<OrderEntity, OrderProductsResponse>()
            .Map(dest => dest.DeliveryType, src => src.DeliveryType.ToString())
            .Map(dest => dest.OrderStatus, src => src.OrderStatus)
            .Map(dest => dest.OrderItems, src => src.OrderItems)
            .IgnoreNonMapped(true)
            .AfterMapping((src, dest) =>
            {
                dest.Subtotal = dest.OrderItems.Sum(i => i.Price * i.Quantity);
                dest.Sale = dest.OrderItems.Sum(i => i.IsPromoted ? i.PromotionPrice * i.Quantity : 0.0m);
                dest.Total = dest.OrderItems.Sum(i => i.Total);
            });

        config.NewConfig<OrderEntity, OrderGeneralInfoResponse>()
            .Map(dest => dest.Creator, src => src.Creator)
            .Map(dest => dest.OrderId, src => src.Id)
            .Map(dest => dest.CustomerEmail, src => src.Customer.Account.Email)
            .Map(dest => dest.DeliveryType, src => src.DeliveryType)
            .Map(dest => dest.DateCreated, src => src.CreationDate)
            .Map(dest => dest.OrderStatus, src => src.OrderStatus)
            .Map(dest => dest.FirstName, src => src.Customer.Account.FirstName)
            .Map(dest => dest.LastName, src => src.Customer.Account.LastName);

        config.NewConfig<CartEntity, GetCartItemsResponse>()
            .Map(dest => dest.Items, src => src.Items)
            .Map(dest => dest.TotalPrice, src => src.TotalPrice)
            .Map(dest => dest.SubtotalPrice, src => src.Items.Sum(i => i.Quantity * i.Price))
            .Map(dest => dest.TotalQuantity, src => src.TotalQuantity);

        config.NewConfig<CartItemEntity, GetCartItemResponse>()
            .Map(dest => dest.Status, src => src.InventoryItem.Status)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Map(dest => dest.ProductUpc, src => src.ProductUpc)
            .Map(dest => dest.ProductName, src => src.ProductName)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.PromotionPrice, src => src.PromotionPrice)
            .Map(dest => dest.ImageUrl, src => src.InventoryItem.Images.FirstOrDefault().BlobUrl ?? string.Empty)
            .AfterMapping((src, dest) => dest.SubtotalPrice = dest.Price * dest.Quantity);

        config.NewConfig<CreatePlatformAdminModel, AccountEntity>()
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber == null ? null : src.PhoneNumber.Trim())
            .AfterMapping(dest =>
            {
                dest.Status = StaffStatusEnum.CreatePassword;
                dest.PasswordHash = string.Empty;
            })
            .Ignore(dest => dest.Role);

        config.NewConfig<ContentEntity, ContentBlockResponse>()
            .Map(dest => dest.SectionType, src => src.SectionType.ToString());

        // ===== MERGED CONFLICT PART (KEEP BOTH) =====

        config.NewConfig<AccountEntity, ClientsShopInfoResponse>()
            .Map(dest => dest.AdminsEmail, src => src.Email)
            .Map(dest => dest.AdminsFullName, src =>
                string.Join(
                    " ",
                    new[] { src.FirstName, src.LastName }
                        .Where(x => !string.IsNullOrWhiteSpace(x))));

        config.NewConfig<CategoryEntity, GetCategoryProductStatusesResponse>()
            .Map(dest => dest.Available, src => src.InventoryItems
                .Count(i => i.Status == InventoryItemStatusEnum.Available))
            .Map(dest => dest.Committed, src => src.InventoryItems
                .Count(i => i.Status == InventoryItemStatusEnum.Committed))
            .Map(dest => dest.Unavailable, src => src.InventoryItems
                .Count(i => i.Status == InventoryItemStatusEnum.Unavailable))
            .Map(dest => dest.Incoming, src => src.InventoryItems
                .Count(i => i.Status == InventoryItemStatusEnum.Incoming));

        config.NewConfig<RoleEntity, GetRolePermissionsResponse>()
            .Map(dest => dest.RoleId, src => src.Id)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.RoleName, src => src.Name)
            .AfterMapping((src, dest) =>
            {
                var rolePermissionTitles = src.PermissionRoles
                    .Where(pr => pr.Permission != null)
                    .Select(pr => pr.Permission!.Title)
                    .ToHashSet();

                foreach (var permission in PermissionProvider.All)
                {
                    dest.Permissions[permission] = rolePermissionTitles.Contains(permission);
                }
            });
    }
}
