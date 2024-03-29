﻿using DataAccess.Executors;
using DataAccess.Models;
using Entities;
using Entities.Event;
using Entities.Profile;
using Entities.UserAccount;
using Entities.UserFollowing;
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

        public async Task<string> CreateEvent(string username, string eventUuid, string title, string description, string category, string city, string venue, DateTime date)
        {
            string response = "";

            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username},
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

        public async Task<string> UpdateEvent(string username, string eventUuid, string title, string description, string category, string city, string venue, DateTime date)
        {
            string response = "";

            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username},
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

        public async Task<string> DeleteEvent(string username, string eventUuid)
        {
            string response = "";

            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username},
                new StoreProcedureParameter { Name = "reqEventUuid", Type = NpgsqlDbType.Varchar, Value = eventUuid}
            };

            await _storedProcedureExecutor.ExecuteStoredProcedure(_connectionStrings.Default, "\"DeleteEvent\"", parameters, (reader) =>
            {
                if (reader.Read()) response = reader.GetString(0);
            });

            return response;
        }

        public async Task<DbResponse> UpdateEventAttendance(string eventUuid, string username)
        {
            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqEventUuid", Type = NpgsqlDbType.Varchar, Value = eventUuid},
                new StoreProcedureParameter { Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username}
            };

            var response = await _storedProcedureExecutor.ExecuteStoredProcedure<DbResponse>(_connectionStrings.Default, "\"UpdateEventAttendance\"", parameters);

            if (response.Count > 0) return response[0];

            return new DbResponse { Message = "An error occurred" };
        }


        public async Task<string> AddEventComment(string username, string eventUuid, string comment)
        {
            string response = "";

            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username},
                new StoreProcedureParameter { Name = "reqEventUuid", Type = NpgsqlDbType.Varchar, Value = eventUuid},
                new StoreProcedureParameter { Name = "reqComment", Type = NpgsqlDbType.Varchar, Value = comment},
            };

            await _storedProcedureExecutor.ExecuteStoredProcedure(_connectionStrings.Default, "\"AddEventComment\"", parameters, (reader) =>
            {
                if (reader.Read()) response = reader.GetString(0);
            });

            return response;
        }

        public async Task<List<EventComment>> GetEventComments(string eventUuid)
        {
            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter {Name = "reqEventUuid", Type = NpgsqlDbType.Varchar, Value = eventUuid}
            };

            return await _storedProcedureExecutor.ExecuteStoredProcedure<EventComment>(_connectionStrings.Default, "\"GetEventComments\"", parameters);

        }

        public async Task<DbResponse> LikeOrUnlikeEvent(string eventUuid, string username)
        {
            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqEventUuid", Type = NpgsqlDbType.Varchar, Value = eventUuid},
                new StoreProcedureParameter { Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username}
            };

            var response = await _storedProcedureExecutor.ExecuteStoredProcedure<DbResponse>(_connectionStrings.Default, "\"LikeOrUnlikeEvent\"", parameters);

            if (response.Count > 0) return response[0];

            return new DbResponse { Message = "An error occurred" };
        }
        
        public async Task<DbResponse> LikeOrUnlikeComment(int eventCommentId, string username)
        {
            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqEventCommentId", Type = NpgsqlDbType.Integer, Value = eventCommentId},
                new StoreProcedureParameter { Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username}
            };

            var response = await _storedProcedureExecutor.ExecuteStoredProcedure<DbResponse>(_connectionStrings.Default, "\"LikeOrUnlikeComment\"", parameters);

            if (response.Count > 0) return response[0];

            return new DbResponse { Message = "An error occurred" };
        }

        public async Task<List<EventLikes>> GetEventLikes(string eventUuid)
        {
            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter {Name = "reqEventUuid", Type = NpgsqlDbType.Varchar, Value = eventUuid}
            };

            return await _storedProcedureExecutor.ExecuteStoredProcedure<EventLikes>(_connectionStrings.Default, "\"GetEventLikes\"", parameters);

        }

        public async Task<string> ReplyOnComment(string username, int commentId, string reply)
        {
            string response = "";

            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username},
                new StoreProcedureParameter { Name = "reqCommentId", Type = NpgsqlDbType.Integer, Value = commentId},
                new StoreProcedureParameter { Name = "reqReply", Type = NpgsqlDbType.Varchar, Value = reply},
            };

            await _storedProcedureExecutor.ExecuteStoredProcedure(_connectionStrings.Default, "\"ReplyOnComment\"", parameters, (reader) =>
            {
                if (reader.Read()) response = reader.GetString(0);
            });

            return response;
        }

        #endregion


        #region UserAccount

        public async Task<string> RegisterUser(string accountUuid, string username, string displayName, string password, string email, string phoneNumber, string emailConfirmationToken)
        {
            string response = "";

            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqAccountUuid", Type = NpgsqlDbType.Varchar, Value = accountUuid},
                new StoreProcedureParameter { Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username},
                new StoreProcedureParameter { Name = "reqDisplayName", Type = NpgsqlDbType.Varchar, Value = displayName},
                new StoreProcedureParameter { Name = "reqPassword", Type = NpgsqlDbType.Varchar, Value = password},
                new StoreProcedureParameter { Name = "reqEmail", Type = NpgsqlDbType.Varchar, Value = email},
                new StoreProcedureParameter { Name = "reqPhoneNumber", Type = NpgsqlDbType.Varchar, Value = phoneNumber},
                new StoreProcedureParameter { Name = "reqEmailConfirmationToken", Type = NpgsqlDbType.Varchar, Value = emailConfirmationToken},
            };

            await _storedProcedureExecutor.ExecuteStoredProcedure(_connectionStrings.Default, "\"RegisterUser\"", parameters, (reader) =>
            {
                if (reader.Read()) response = reader.GetString(0);
            });

            return response;
        }
        
        public async Task<string> VerifyEmail(string email, string emailConfirmationToken)
        {
            string response = "";

            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqEmail", Type = NpgsqlDbType.Varchar, Value = email},
                new StoreProcedureParameter { Name = "reqEmailConfirmationToken", Type = NpgsqlDbType.Varchar, Value = emailConfirmationToken},
            };

            await _storedProcedureExecutor.ExecuteStoredProcedure(_connectionStrings.Default, "\"VerifyEmail\"", parameters, (reader) =>
            {
                if (reader.Read()) response = reader.GetString(0);
            });

            return response;
        }
        
        public async Task<string> ResendEmailConfirmationLink(string email, string emailConfirmationToken)
        {
            string response = "";

            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqEmail", Type = NpgsqlDbType.Varchar, Value = email},
                new StoreProcedureParameter { Name = "reqEmailConfirmationToken", Type = NpgsqlDbType.Varchar, Value = emailConfirmationToken},
            };

            await _storedProcedureExecutor.ExecuteStoredProcedure(_connectionStrings.Default, "\"ResendEmailConfirmationLink\"", parameters, (reader) =>
            {
                if (reader.Read()) response = reader.GetString(0);
            });

            return response;
        }

        public async Task<LoginResponse> UserLogin(string username, string password)
        {
            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username},
                new StoreProcedureParameter { Name = "reqPassword", Type = NpgsqlDbType.Varchar, Value = password},
            };

            var response = await _storedProcedureExecutor.ExecuteStoredProcedure<LoginResponse>(_connectionStrings.Default, "\"UserLogin\"", parameters);

            if (response.Count > 0) return response[0];

            return new LoginResponse { ResponseMessage = "An error occurred" };
        }
        
        public async Task<ForgotPasswordResponse> ForgotPassword(string email, string passwordResetCode)
        {
            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqEmail", Type = NpgsqlDbType.Varchar, Value = email},
                new StoreProcedureParameter { Name = "reqPasswordResetCode", Type = NpgsqlDbType.Varchar, Value = passwordResetCode},
            };

            var response = await _storedProcedureExecutor.ExecuteStoredProcedure<ForgotPasswordResponse>(_connectionStrings.Default, "\"ForgotPassword\"", parameters);

            if (response.Count > 0) return response[0];

            return new ForgotPasswordResponse { Message = "An error occurred" };
        }

        public async Task<string> VerifyPasswordReset(string email, string passwordResetCode)
        {
            string response = "";

            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqEmail", Type = NpgsqlDbType.Varchar, Value = email},
                new StoreProcedureParameter { Name = "reqPasswordResetCode", Type = NpgsqlDbType.Varchar, Value = passwordResetCode},
            };

            await _storedProcedureExecutor.ExecuteStoredProcedure(_connectionStrings.Default, "\"VerifyPasswordReset\"", parameters, (reader) =>
            {
                if (reader.Read()) response = reader.GetString(0);
            });

            return response;
        }
        
        public async Task<string> ResetPassword(string email, string newPassword)
        {
            string response = "";

            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqEmail", Type = NpgsqlDbType.Varchar, Value = email},
                new StoreProcedureParameter { Name = "reqNewPassword", Type = NpgsqlDbType.Varchar, Value = newPassword},
            };

            await _storedProcedureExecutor.ExecuteStoredProcedure(_connectionStrings.Default, "\"ResetPassword\"", parameters, (reader) =>
            {
                if (reader.Read()) response = reader.GetString(0);
            });

            return response;
        }


        #endregion


        #region ImageUpload

        public async Task<ImageUploadDbResponse> UploadImage(string username, string publicId, string file)
        {
            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username},
                new StoreProcedureParameter { Name = "reqPublicId", Type = NpgsqlDbType.Varchar, Value = publicId},
                new StoreProcedureParameter { Name = "reqFile", Type = NpgsqlDbType.Varchar, Value = file},
            };

            var response = await _storedProcedureExecutor.ExecuteStoredProcedure<ImageUploadDbResponse>(_connectionStrings.Default, "\"UploadImage\"", parameters);

            if (response.Count > 0) return response[0];

            return new ImageUploadDbResponse { ResponseMessage = "An error occurred" };
        }

        public async Task<string> DeleteImage(string username, string publicId)
        {
            string response = "";

            var parameters = new List<StoreProcedureParameter>
            {
               new StoreProcedureParameter { Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username},
               new StoreProcedureParameter { Name = "reqPublicId", Type = NpgsqlDbType.Varchar, Value = publicId},
            };

            await _storedProcedureExecutor.ExecuteStoredProcedure(_connectionStrings.Default, "\"DeleteImage\"", parameters, (reader) =>
            {
                if (reader.Read()) response = reader.GetString(0);
            });

            return response;
        }
        
        public async Task<string> SetProfilePicture(string username, string publicId)
        {
            string response = "";

            var parameters = new List<StoreProcedureParameter>
            {
               new StoreProcedureParameter { Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username},
               new StoreProcedureParameter { Name = "reqPublicId", Type = NpgsqlDbType.Varchar, Value = publicId},
            };

            await _storedProcedureExecutor.ExecuteStoredProcedure(_connectionStrings.Default, "\"SetProfilePicture\"", parameters, (reader) =>
            {
                if (reader.Read()) response = reader.GetString(0);
            });

            return response;
        }

        #endregion


        #region UserProfile

        public async Task<UserProfile?> GetUserProfile(string username)
        {
            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter {Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username}
            };

            var response = await _storedProcedureExecutor.ExecuteStoredProcedure<UserProfile>(_connectionStrings.Default, "\"GetUserProfile\"", parameters);

            if (response.Count > 0) return response[0];

            return null;
        }

        public async Task<string> UpdateUserProfile(string username, string displayName, string bio)
        {
            string response = "";

            var parameters = new List<StoreProcedureParameter>
            {
               new StoreProcedureParameter { Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username},
               new StoreProcedureParameter { Name = "reqDisplayName", Type = NpgsqlDbType.Varchar, Value = displayName},
               new StoreProcedureParameter { Name = "reqBio", Type = NpgsqlDbType.Varchar, Value = bio},
            };

            await _storedProcedureExecutor.ExecuteStoredProcedure(_connectionStrings.Default, "\"UpdateUserProfile\"", parameters, (reader) =>
            {
                if (reader.Read()) response = reader.GetString(0);
            });

            return response;
        }

        #endregion


        #region UserFollowing

        public async Task<FollowingDbResponse> FollowOrUnfollowUser(string observerUsername, string targetUsername)
        {
            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter { Name = "reqObserverUsername", Type = NpgsqlDbType.Varchar, Value = observerUsername},
                new StoreProcedureParameter { Name = "reqTargetUsername", Type = NpgsqlDbType.Varchar, Value = targetUsername}
            };

            var response = await _storedProcedureExecutor.ExecuteStoredProcedure<FollowingDbResponse>(_connectionStrings.Default, "\"FollowOrUnfollowUser\"", parameters);

            if (response.Count > 0) return response[0];

            return new FollowingDbResponse { ResponseMessage = "An error occurred" };
        }

        public async Task<List<Followings>> GetUserFollowings(string username)
        {
            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter {Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username}
            };

            return await _storedProcedureExecutor.ExecuteStoredProcedure<Followings>(_connectionStrings.Default, "\"GetUserFollowings\"", parameters);

        }
        
        public async Task<List<Followers>> GetUserFollowers(string username)
        {
            var parameters = new List<StoreProcedureParameter>
            {
                new StoreProcedureParameter {Name = "reqUsername", Type = NpgsqlDbType.Varchar, Value = username}
            };

            return await _storedProcedureExecutor.ExecuteStoredProcedure<Followers>(_connectionStrings.Default, "\"GetUserFollowers\"", parameters);

        }

        #endregion
    }
}
