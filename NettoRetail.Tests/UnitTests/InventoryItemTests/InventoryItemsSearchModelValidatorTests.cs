using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Tests.UnitTests.InventoryItemTests
{
    public class InventoryItemsSearchModelValidatorTests
    {
        private readonly InventoryItemsSearchModelValidator _validator;
        public InventoryItemsSearchModelValidatorTests()
        {
            _validator = new InventoryItemsSearchModelValidator();
        }

        [Fact]
        public void Validate_ShouldFail_WhenUpcIsInvalid()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                Upc = "12345"
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.Upc)
                .WithErrorMessage(ValidationErrorMessages.Inventory.UpcMinLengthErrorMessage);
        }

        [Theory]
        [MemberData(nameof(GetInvalidNameData))]
        public void Validate_ShouldFail_WhenProductNameIsTooShortOrTooLong(string productName)
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                ProductName = productName
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.ProductName);
        }

        [Fact]
        public void Validate_ShouldFail_WhenProductNameHasInvalidCharacters()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                ProductName = "Invalid@Name!"
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.ProductName)
                .WithErrorMessage(ValidationErrorMessages.Inventory.ProductNameAllowedCharactersRegex);
        }

        [Fact]
        public void Validate_ShouldFail_WhenProductLocationHasInvalidFormat()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                ProductLocation = "InvalidLocation_1"
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.ProductLocation)
                .WithErrorMessage(ValidationErrorMessages.Inventory.ProductLocationInvalidFormatMessage);
        }

        [Theory]
        [MemberData(nameof(GetInvalidProductLocationData))]
        public void Validate_ShouldFail_WhenProductLocationIsTooShortOrTooLong(string productLocation)
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                ProductLocation = productLocation
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.ProductLocation);
        }

        [Fact]
        public void Validate_ShouldFail_WhenCategoryIdIsNegative()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                CategoryId = -1
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.CategoryId);
        }

        [Fact]
        public void Validate_ShouldFail_WhenMinAvailableAmountIsNegative()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                MinAvailableAmount = -1
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.MinAvailableAmount);
        }

        [Fact]
        public void Validate_ShouldFail_WhenMaxAvailableAmountIsLessThanMinAvailableAmount()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                MinAvailableAmount = 10,
                MaxAvailableAmount = 5
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.MaxAvailableAmount)
                .WithErrorMessage(ValidationErrorMessages.Inventory.MaxAvailableAmountMinLengthMessage);
        }

        [Fact]
        public void Validate_ShouldFail_WhenMinReservedAmountIsNegative()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                MinReservedAmount = -1
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.MinReservedAmount);
        }

        [Fact]
        public void Validate_ShouldFail_WhenMaxReservedAmountIsLessThanMinReservedAmount()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                MinReservedAmount = 10,
                MaxReservedAmount = 5
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.MaxReservedAmount)
                .WithErrorMessage(ValidationErrorMessages.Inventory.MaxReservedAmountMinLengthMessage);
        }

        [Fact]
        public void Validate_ShouldFail_WhenMinOnHandAmountIsNegative()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                MinOnHandAmount = -1
            };
            
            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.MinOnHandAmount);
        }

        [Fact]
        public void Validate_ShouldFail_WhenMaxOnHandAmountIsLessThanMinOnHandAmount()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                MinOnHandAmount = 10,
                MaxOnHandAmount = 5
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.MaxOnHandAmount)
                .WithErrorMessage(ValidationErrorMessages.Inventory.MaxOnHandAmountMinLengthMessage);
        }

        [Fact]
        public void Validate_ShouldFail_WhenMinPriceIsNegative()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                MinPrice = -1
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.MinPrice);
        }

        [Fact]
        public void Validate_ShouldFail_WhenMinPriceHasMoreThanTwoDecimalPlaces()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                MinPrice = 10.123m
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.MinPrice)
                .WithErrorMessage(ValidationErrorMessages.Inventory.InvalidPriceScaleMessage);
        }

        [Fact]
        public void Validate_ShouldFail_WhenMaxPriceIsNegative()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                MaxPrice = -1
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.MaxPrice)
                .WithErrorMessage(ValidationErrorMessages.Inventory.MaxPriceMaxLengthErrorMessage);
        }

        [Fact]
        public void Validate_ShouldFail_WhenMaxPriceHasMoreThanTwoDecimalPlaces()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                MaxPrice = 10.123m
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.MaxPrice)
                .WithErrorMessage(ValidationErrorMessages.Inventory.InvalidPriceScaleMessage);
        }

        [Fact]
        public void Validate_ShouldFail_WhenStatusIsInvalid()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                Status = "InvalidStatus"
            };

            //Act
            var result = _validator.TestValidate(model);
            
            //Assert
            result.ShouldHaveValidationErrorFor(x => x.Status)
                .WithErrorMessage(ValidationErrorMessages.Inventory.InvalidStatusErrorMessage);
        }

        [Fact]
        public void Validate_ShouldFail_WhenBrandNameIsTooLong()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                BrandName = new string('a', 90 + 1)
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.BrandName);
        }

        [Fact]
        public void Validate_ShouldFail_WhenBrandNameHasInvalidCharacters()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                BrandName = "@Brand!"
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.BrandName)
                .WithErrorMessage(ValidationErrorMessages.Brand.BrandNameAllowedCharactersRegex);
        }

        [Fact]
        public void Validate_ShouldFail_WhenPageNumberIsLessThanOne()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                PageNumber = 0
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.PageNumber);
        }

        [Fact]
        public void Validate_ShouldFail_WhenPageSizeIsLessThanOrEqualToZero()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                PageSize = 0
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize);
        }

        [Fact]
        public void Validate_ShouldFail_WhenPageSizeIsGreaterThanMaxPageSize()
        {
            //Arrange
            var model = new InventoryItemsSearchModel
            {
                PageNumber = 1_073_741_825,
                PageSize = 2
            };

            //Act
            var result = _validator.TestValidate(model);

            //Assert
            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage(ValidationErrorMessages.Pagination.PaginationTooLargeErrorMessage);
        }

        public static IEnumerable<object[]> GetInvalidNameData()
        {
            yield return new object[] { "a" };
            yield return new object[] { new string('a', ValidationErrorMessages.Inventory.ProductNameMaxLength + 1) };
        }

        public static IEnumerable<object[]> GetInvalidProductLocationData()
        {
            yield return new object[] { "a" };
            yield return new object[] { new string('a', 101) };
        }
    }
}

