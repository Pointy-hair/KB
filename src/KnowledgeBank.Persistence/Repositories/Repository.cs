using KnowledgeBank.Domain;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace KnowledgeBank.Persistence.Repositories
{
    public class Repository<T> : IRepository<T> where T : Entity
	{
		protected readonly ApplicationDbContext _dbContext;

		public Repository(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public virtual IDbContextTransaction BeginTransaction() => _dbContext.Database.BeginTransaction();

		public virtual T Get(long id) => _dbContext.Find<T>(id);

		public virtual IQueryable<T> Read() => _dbContext.Set<T>();
		public virtual IQueryable<T> Read(Expression<Func<T, bool>> predicate) => Read().Where(predicate);

		public virtual void AddRange(IEnumerable<T> entities)
		{
			_dbContext.AddRange(entities);
			_dbContext.SaveChanges();
		}
		public virtual void Update<TViewModel>(T entity, TViewModel viewModel)
		{
			_dbContext.Entry(entity).CurrentValues.SetValues(viewModel);
			_dbContext.SaveChanges();
		}

		public virtual long Create(T entity)
		{
			_dbContext.Add(entity);
			_dbContext.SaveChanges();
			return entity.Id;
		}

		public void Update(T entity)
		{
			_dbContext.Update(entity);
			_dbContext.SaveChanges();
		}

		public void Delete(T entity)
		{
			_dbContext.Remove(entity);
			_dbContext.SaveChanges();
		}

        public long GetTenantId()
        {
            return _dbContext.TenantId;
        }
	}
}
