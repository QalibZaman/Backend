using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using NettoRetail.DataAccess.Context;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.DataAccess.UOW;

public class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public IDbTransaction BeginTransaction()
    {
        var transaction = dbContext.Database.BeginTransaction();

        return transaction.GetDbTransaction();
    }
}
