using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DataAccess.Executors
{
    public interface IStoredProcedureExecutor
    {
        Task ExecuteStoredProcedure(Connection _connection, string storeProcedureName, List<StoreProcedureParameter> parameters, Action<IDataReader> callback = null);
        Task<List<T>> ExecuteStoredProcedure<T>(Connection connection, string storeProcedureName, List<StoreProcedureParameter> parameters);
    }
}
