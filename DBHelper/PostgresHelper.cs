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

        public async Task<string> CreateEvent(string eventUuid, string title, string description, string category, string city, string venue, DateTime date)
        {
            string response = "";

            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqEventUuid", Type = NpgsqlDbType.Varchar, Value = eventUuid},
                new StoreProcedureParameter { Name = "reqTitle", Type = NpgsqlDbType.Varchar, Value = title},
                new StoreProcedureParameter { Name = "reqDescription", Type = NpgsqlDbType.Varchar, Value = description},
                new StoreProcedureParameter { Name = "reqCategory", Type = NpgsqlDbType.Varchar, Value = category},
                new StoreProcedureParameter { Name = "reqCity", Type = NpgsqlDbType.Varchar, Value = city},
                new StoreProcedureParameter { Name = "reqVenue", Type = NpgsqlDbType.Varchar, Value = venue},
                new StoreProcedureParameter { Name = "reqDate", Type = NpgsqlDbType.Timestamp, Value = date}
            };

            await _storedProcedureExecutor.ExecuteStoredProcedure(_connectionStrings.Default, "\"CreateEvent\"", parameters, (reader) =>
            {
                if (reader.Read()) response = reader.GetString(0);
            });

            return response;
        }
        
        public async Task<string> UpdateEvent(string eventUuid, string title, string description, string category, string city, string venue, DateTime date)
        {
            string response = "";

            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqEventUuid", Type = NpgsqlDbType.Varchar, Value = eventUuid},
                new StoreProcedureParameter { Name = "reqTitle", Type = NpgsqlDbType.Varchar, Value = title},
                new StoreProcedureParameter { Name = "reqDescription", Type = NpgsqlDbType.Varchar, Value = description},
                new StoreProcedureParameter { Name = "reqCategory", Type = NpgsqlDbType.Varchar, Value = category},
                new StoreProcedureParameter { Name = "reqCity", Type = NpgsqlDbType.Varchar, Value = city},
                new StoreProcedureParameter { Name = "reqVenue", Type = NpgsqlDbType.Varchar, Value = venue},
                new StoreProcedureParameter { Name = "reqDate", Type = NpgsqlDbType.Timestamp, Value = date}
            };

            await _storedProcedureExecutor.ExecuteStoredProcedure(_connectionStrings.Default, "\"UpdateEvent\"", parameters, (reader) =>
            {
                if (reader.Read()) response = reader.GetString(0);
            });

            return response;
        }
        
        public async Task<string> DeleteEvent(string eventUuid)
        {
            string response = "";

            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqEventUuid", Type = NpgsqlDbType.Varchar, Value = eventUuid}
            };

            await _storedProcedureExecutor.ExecuteStoredProcedure(_connectionStrings.Default, "\"DeleteEvent\"", parameters, (reader) =>
            {
                if (reader.Read()) response = reader.GetString(0);
            });

            return response;
        }


        #endregion
    }
}
