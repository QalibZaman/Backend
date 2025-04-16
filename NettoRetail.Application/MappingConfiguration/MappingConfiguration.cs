using Mapster;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;

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
            .Map(dest => dest.CustomerEmail, src => src.Customer.Account.Email);
    }
}