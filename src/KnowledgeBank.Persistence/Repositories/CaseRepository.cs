using KnowledgeBank.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KnowledgeBank.Persistence.Repositories
{
    public class CaseRepository : Repository<Case>
    {
        public CaseRepository(ApplicationDbContext dbContext) : base(dbContext)
        {

        }

        public virtual void UpdateCollection<T, TViewModel>(Case entity, Func<Case, ICollection<T>> accessor, IEnumerable<TViewModel> collection, Func<TViewModel, T> map, bool update = true) where T : Entity where TViewModel : IEntity
        {
            Delete(entity, accessor, collection);

            Add(entity, accessor, collection, map);

            if(update)
                Update(entity, accessor, collection);

            _dbContext.Update(entity);
            _dbContext.SaveChanges();
        }

        private void Delete<T, TViewModel>(Case entity, Func<Case, ICollection<T>> accessor, IEnumerable<TViewModel> collection)
            where T : Entity
            where TViewModel : IEntity
        {
            foreach (var item in accessor(entity))
            {
                bool isDeleted = !collection.Any(x => x.Id == item.Id);

                if (isDeleted)
                    _dbContext.Entry(item).State = EntityState.Deleted;
            }
        }

        private static void Add<T, TViewModel>(Case entity, Func<Case, ICollection<T>> accessor, IEnumerable<TViewModel> collection, Func<TViewModel, T> map)
            where T : Entity
            where TViewModel : IEntity
        {
            var newAttachments = collection.Where(x => x.Id == 0);

            foreach (var item in newAttachments)
            {
                accessor(entity).Add(map(item));
            }
        }

        private void Update<T, TViewModel>(Case entity, Func<Case, ICollection<T>> accessor, IEnumerable<TViewModel> collection)
            where T : Entity
            where TViewModel : IEntity
        {
            var joined = accessor(entity).Join(collection, x => x.Id, x => x.Id, (x, y) => new { db = x, dto = y }).ToArray();
            foreach (var item in joined)
            {
                _dbContext.Entry(item.db).CurrentValues.SetValues(item.dto);
            }
        }
    }
}
