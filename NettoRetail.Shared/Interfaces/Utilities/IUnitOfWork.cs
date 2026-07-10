using System.Data;

namespace NettoRetail.Shared.Interfaces.Utilities;

public interface IUnitOfWork
{
    IDbTransaction BeginTransaction();
}