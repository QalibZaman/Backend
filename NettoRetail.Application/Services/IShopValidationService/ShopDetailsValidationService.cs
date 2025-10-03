using System.Text;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Shop;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.IShopValidationService
{
    public class ShopDetailsValidationService(IShopRepository shopRepository) : IShopDetailsValidationService
    {
        public async Task ValidateShopOnCreate(ShopValidationModel model, CancellationToken cancellationToken)
        {
            var specificationBuilder = new SpecificationBuilder<ShopEntity>();
            specificationBuilder.Add<GetShopByEmailSpecification>(model.Email)
                .AddOr<GetShopByNameSpecification>(model.Name)
                .AddOr<GetShopByPhoneNumberSpecification>(model.PhoneNumber);

            await ValidateAsync(model, specificationBuilder, cancellationToken);
        }

        public async Task ValidateShopOnUpdate(ShopValidationModel model, int shopId, CancellationToken cancellationToken)
        {
            var specificationBuilder = new SpecificationBuilder<ShopEntity>();
            specificationBuilder.Add<GetShopByEmailSpecification>(model.Email)
                .AddOr<GetShopByNameSpecification>(model.Name)
                .AddOr<GetShopByPhoneNumberSpecification>(model.PhoneNumber)
                .Add<ExceptShopByIdSpecification>(shopId);

            await ValidateAsync(model, specificationBuilder, cancellationToken);
        }

        private async Task ValidateAsync(ShopValidationModel model, SpecificationBuilder<ShopEntity> specificationBuilder, CancellationToken cancellationToken)
        {
            var shopsWithRepeatingProps = await shopRepository.GetManyBySpecAsync(specificationBuilder.Build(), cancellationToken);

            if (!shopsWithRepeatingProps.IsNullOrEmpty())
            {
                StringBuilder exceptionMessageBuilder = new StringBuilder();
                foreach (var violatedShop in shopsWithRepeatingProps)
                {
                    if (model.Name == violatedShop.Name)
                    {
                        exceptionMessageBuilder.AppendLine($"Shop name must be unique");
                    }

                    if (model.PhoneNumber == violatedShop.PhoneNumber)
                    {
                        exceptionMessageBuilder.AppendLine($"Shop phone number must be unique");
                    }

                    if (model.Email == violatedShop.Email)
                    {
                        exceptionMessageBuilder.AppendLine($"Shop email must be unique");
                    }
                }

                throw new ConflictException(exceptionMessageBuilder.ToString());
            }


        }
    }
}
