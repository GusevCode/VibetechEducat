using System.Linq.Expressions;
using Vibetech.Educat.Common.Interfaces;

namespace Vibetech.Educat.DataAccess.Interfaces;

public interface IDataAccessRepository<T, TKey> : IRepository<T, TKey> where T : class
{
}

public interface IDataAccessRepository<T> : IDataAccessRepository<T, int>, IRepository<T> where T : class
{
} 