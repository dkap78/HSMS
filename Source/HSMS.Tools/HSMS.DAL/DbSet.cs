using HSMS.DB.Models;
using HSMS.Security;
using Npgsql;

namespace HSMS.DAL
{
    public abstract class DbSet<TEntity> where TEntity : BaseModel
    {
        #region Variables
        protected BaseDbContext Context { get; }
        protected IAppUserContext AppUser => Context.AppUser;
        #endregion

        #region Constructor
        protected DbSet(BaseDbContext context)
        {
            Context = context;
        }
        #endregion

        #region Generic Query Execute Methods
        protected Task<List<TEntity>> QueryAsync(string sp, params NpgsqlParameter[] p)
            => Context.QueryAsync<TEntity>(sp, System.Data.CommandType.StoredProcedure, p);
        protected Task<object?> ScalarAsync(string sp, params NpgsqlParameter[] p)
            => Context.QueryValueAsync(sp, System.Data.CommandType.StoredProcedure, p);
        protected Task<int> ExecuteAsync(string sp, params NpgsqlParameter[] p)
            => Context.ExecuteAsync(sp, System.Data.CommandType.StoredProcedure, p);
        #endregion

        #region Implementation of IDisposable Interface
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void Dispose(bool blnDisposing)
        {
            if (blnDisposing)
            {
                if (Context != null)
                {
                    Context.Dispose();
                }
            }
        }
        #endregion
    }
}
