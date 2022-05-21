using DataAccess.Executors;
using DataAccess.Models;
using Entities.Event;
using NpgsqlTypes;

namespace DBHelper
{
    public class PostgresHelper : IPostgresHelper
    {
        private readonly DatabaseConnections _connectionStrings;
        private readonly IStoredProcedureExecutor _storedProcedureExecutor;

        public PostgresHelper(DatabaseConnections connectionStrings, IStoredProcedureExecutor storedProcedureExecutor)
        {
            _connectionStrings = connectionStrings;
            _storedProcedureExecutor = storedProcedureExecutor;
        }

        #region Events

        public async Task<List<EventsInfo>> GetEvents() => await _storedProcedureExecutor.ExecuteStoredProcedure<EventsInfo>(_connectionStrings.Default, "\"GetEvents\"", null);

        public async Task<EventDetails?> GetEventDetails(string eventUuid)
        {
            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter {Name = "reqEventUuid", Type = NpgsqlDbType.Varchar, Value = eventUuid}
            };

            var response = await _storedProcedureExecutor.ExecuteStoredProcedure<EventDetails>(_connectionStrings.Default, "\"GetEventDetails\"", parameters);

            if (response.Count > 0) return response[0];

            return null;
        }


        #endregion
    }
}
