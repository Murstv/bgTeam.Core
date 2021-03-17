namespace bgTeam.DataAccess.Impl.Dapper
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using global::Dapper;
    using global::Dapper.Contrib.Extensions;

    public class RepositoryDapper : IRepository
    {
        private readonly int _commandTimeout = 300;
        private readonly IConnectionFactory _factory;

        public RepositoryDapper(IConnectionFactory factory)
        {
            _factory = factory;
        }

        public T Get<T>(ISqlObject obj, IDbConnection connection = null, IDbTransaction transaction = null)
            => GetAll<T>(obj, connection, transaction).SingleOrDefault();

        public T Get<T>(string sql, object param = null, IDbConnection connection = null, IDbTransaction transaction = null)
            => GetAll<T>(sql, param, connection, transaction).SingleOrDefault();

        public T Get<T>(Expression<Func<T, bool>> predicate, IDbConnection connection = null, IDbTransaction transaction = null)
            where T : class
        {
            throw new NotSupportedException("Dapper.Contrib not support linq expressions");

            //if (connection == null)
            //{
            //    predicate.ToString();
            //    using (connection = _factory.Create())
            //    {
            //        return connection.Get<T>(predicate, commandTimeout: _commandTimeout);
            //    }
            //}
            //else
            //{
            //    return connection.Get<T>(predicate, transaction, commandTimeout: _commandTimeout);
            //}
        }

        public async Task<T> GetAsync<T>(ISqlObject obj, IDbConnection connection = null, IDbTransaction transaction = null)
            => (await GetAllAsync<T>(obj, connection, transaction)).SingleOrDefault();

        public async Task<T> GetAsync<T>(string sql, object param = null, IDbConnection connection = null, IDbTransaction transaction = null)
            => (await GetAllAsync<T>(sql, param, connection, transaction)).SingleOrDefault();

        public async Task<T> GetAsync<T>(Expression<Func<T, bool>> predicate, IDbConnection connection = null, IDbTransaction transaction = null)
            where T : class
        {
            throw new NotSupportedException("Dapper.Contrib not support linq expressions");

            //if (connection == null)
            //{
            //    using (connection = await _factory.CreateAsync())
            //    {
            //        return await connection.GetAsync<T>(predicate, commandTimeout: _commandTimeout);
            //    }
            //}
            //else
            //{
            //    return connection.Get<T>(predicate, transaction, commandTimeout: _commandTimeout);
            //}
        }

        public IEnumerable<T> GetAll<T>(ISqlObject obj, IDbConnection connection = null, IDbTransaction transaction = null)
        {
            if (connection == null)
            {
                using (connection = _factory.Create())
                {
                    return connection.Query<T>(obj.Sql, obj.QueryParams, commandTimeout: _commandTimeout);
                }
            }
            else
            {
                return connection.Query<T>(obj.Sql, obj.QueryParams, transaction, commandTimeout: _commandTimeout);
            }
        }

        public IEnumerable<T> GetAll<T>(string sql, object param = null, IDbConnection connection = null, IDbTransaction transaction = null)
        {
            if (connection == null)
            {
                using (connection = _factory.Create())
                {
                    return connection.Query<T>(sql, param, commandTimeout: _commandTimeout);
                }
            }
            else
            {
                return connection.Query<T>(sql, param, transaction, commandTimeout: _commandTimeout);
            }
        }

        public IEnumerable<T> GetAll<T>(Expression<Func<T, bool>> predicate = null, IDbConnection connection = null, IDbTransaction transaction = null)
            where T : class
        {
            throw new NotSupportedException("Dapper.Contrib not support linq expressions");

            //if (connection == null)
            //{
            //    using (connection = _factory.Create())
            //    {
            //        return connection.GetAll<T>(predicate, commandTimeout: _commandTimeout);
            //    }
            //}
            //else
            //{
            //    return connection.GetAll<T>(predicate, transaction: transaction, commandTimeout: _commandTimeout);
            //}
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(ISqlObject obj, IDbConnection connection = null, IDbTransaction transaction = null)
        {
            if (connection == null)
            {
                using (connection = await _factory.CreateAsync())
                {
                    return await connection.QueryAsync<T>(obj.Sql, obj.QueryParams, commandTimeout: _commandTimeout);
                }
            }
            else
            {
                return await connection.QueryAsync<T>(obj.Sql, obj.QueryParams, transaction, commandTimeout: _commandTimeout);
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(string sql, object param = null, IDbConnection connection = null, IDbTransaction transaction = null)
        {
            if (connection == null)
            {
                using (connection = await _factory.CreateAsync())
                {
                    return await connection.QueryAsync<T>(sql, param, commandTimeout: _commandTimeout);
                }
            }
            else
            {
                return await connection.QueryAsync<T>(sql, param, transaction, commandTimeout: _commandTimeout);
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(Expression<Func<T, bool>> predicate = null, IDbConnection connection = null, IDbTransaction transaction = null)
            where T : class
        {
            throw new NotSupportedException("Dapper.Contrib not support linq expressions");

            //if (connection == null)
            //{
            //    using (connection = await _factory.CreateAsync())
            //    {
            //        return await connection.GetAllAsync<T>(predicate, commandTimeout: _commandTimeout);
            //    }
            //}
            //else
            //{
            //    return await connection.GetAllAsync<T>(predicate, transaction: transaction, commandTimeout: _commandTimeout);
            //}
        }

        public IEnumerable<T> GetPage<T>(Expression<Func<T, bool>> predicate, IList<ISort> sort, int page, int resultsPerPage)
            where T : class
        {
            using (var connection = _factory.Create())
            {
                var builder = new SqlBuilder();
                //builder.
                var cmd = connection.CreateCommand();
                //connection.Query()
                predicate.Compile()
                return connection.GetPage<T>(predicate, sort, page, resultsPerPage, buffered: true);
            }
        }

        public async Task<IEnumerable<T>> GetPageAsync<T>(Expression<Func<T, bool>> predicate = null, IList<ISort> sort = null, int page = 1, int resultsPerPage = 10)
            where T : class
        {
            using (var connection = await _factory.CreateAsync())
            {
                return await connection.GetPageAsync<T>(predicate, sort, page, resultsPerPage);
            }
        }

        public async Task<PaginatedResult<T>> GetPaginatedResultAsync<T>(Expression<Func<T, bool>> predicate = null, IList<ISort> sort = null, int page = 1, int resultsPerPage = 10, IDbConnection connection = null, IDbTransaction transaction = null)
            where T : class
        {
            if (connection == null)
            {
                using (connection = await _factory.CreateAsync())
                {
                    return PaginatedResult<T>.Create(
                        await connection.CountAsync(predicate, transaction, _commandTimeout),
                        await connection.GetPageAsync(predicate, sort, page, resultsPerPage, transaction, _commandTimeout));
                }
            }
            else
            {
                return PaginatedResult<T>.Create(
                    await connection.CountAsync(predicate, transaction, _commandTimeout),
                    await connection.GetPageAsync(predicate, sort, page, resultsPerPage, transaction, _commandTimeout));
            }
        }

        //public async Task<Dapper.SqlMapper.GridReader> QueryMultipleAsync(string sql, object param = null)
        //{
        //    using (var connection = await _factory.CreateAsync())
        //    {
        //        return await connection.QueryMultipleAsync(sql, param, commandTimeout: _commandTimeout);
        //    }
        //}

        //public async Task<int> ExecuteAsync(string sql, object param = null)
        //{
        //    using (var connection = await _factory.CreateAsync())
        //    {
        //        return await connection.ExecuteAsync(sql, param, commandTimeout: _commandTimeout);
        //    }
        //}
    }
}
