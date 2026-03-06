using NettoRetail.Application.Exceptions;

namespace NettoRetail.Application.Utilities;

public static class PaginationHelper
{
    public static int CalculateSkip(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            throw new BadRequestException("PageNumber must be greater than or equal to 1.");
        }

        if (pageSize < 1)
        {
            throw new BadRequestException("PageSize must be greater than or equal to 1.");
        }

        var skip = ((long)pageNumber - 1) * pageSize;

        if (skip > int.MaxValue)
        {
            throw new BadRequestException("Pagination parameters are too large.");
        }

        return (int)skip;
    }

    public static bool IsSkipWithinIntRange(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return true;
        }

        var skip = ((long)pageNumber - 1) * pageSize;
        return skip <= int.MaxValue;
    }
}
