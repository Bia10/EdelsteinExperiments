using System.Threading.Tasks;
using Edelstein.Protocol.Database;
using LiteDB;

namespace Edelstein.Protocol.Database.LiteDB
{
    public class LiteDBDataSession : IDataSession
    {
        private readonly LiteRepository _repository;

        public LiteDBDataSession(LiteRepository repository)
            => _repository = repository;

        public IDataQuery<T> Query<T>() where T : class, IDataEntity
            => new LiteDBDataQuery<T>(_repository.Query<T>());

        public IDataBatch Batch()
            => new QueuedDataBatch(this);

        public T Retrieve<T>(int id) where T : class, IDataEntity
            => _repository.SingleById<T>(id);

        public void Insert<T>(T entity) where T : class, IDataEntity
            => _repository.Insert<T>(entity);

        public void Update<T>(T entity) where T : class, IDataEntity
            => _repository.Update<T>(entity);

        public void Delete<T>(T entity) where T : class, IDataEntity
            => _repository.Delete<T>(entity.ID);

        public void Delete<T>(int id) where T : class, IDataEntity
            => _repository.Delete<T>(id);

        public Task<T> RetrieveAsync<T>(int id) where T : class, IDataEntity
            => Task.FromResult(Retrieve<T>(id));

        public Task InsertAsync<T>(T entity) where T : class, IDataEntity
            => Task.Run(() => Insert(entity));

        public Task UpdateAsync<T>(T entity) where T : class, IDataEntity
            => Task.Run(() => Update(entity));

        public Task DeleteAsync<T>(T entity) where T : class, IDataEntity
            => Task.Run(() => Delete(entity));

        public Task DeleteAsync<T>(int id) where T : class, IDataEntity
            => Task.Run(() => Delete<T>(id));

        public void Dispose()
        {
            // Do nothing
        }
    }
}