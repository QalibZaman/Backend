using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Tests.UnitTests.PaginationTests
{
    public class PaginationRequestTests
    {
        private readonly PaginationRequestValidator _validator = new();

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        [InlineData(0, 0)]
        public void Should_Fail_When_Pagination_Is_Invalid(int pageNumber, int pageSize)
        {
            // Arrange
            var model = new PaginationRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Act
            var result = _validator.Validate(model);

            // Assert
            Assert.False(result.IsValid);
        }
    }

}
