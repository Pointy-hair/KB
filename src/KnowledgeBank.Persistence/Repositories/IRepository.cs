using KnowledgeBank.Domain;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace KnowledgeBank.Persistence.Repositories
{
	public interface IRepository<T> where T : Entity
	{
		IDbContextTransaction BeginTransaction();
		T Get(long id);
		IQueryable<T> Read();
		IQueryable<T> Read(Expression<Func<T, bool>> predicate);
		void Update<TViewModel>(T entity, TViewModel viewModel);
		void Update(T entity);
		long Create(T entity);
		void Delete(T entity);
		void AddRange(IEnumerable<T> entities);
        long GetTenantId();
	}
}
