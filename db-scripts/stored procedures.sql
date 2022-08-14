DROP FUNCTION IF EXISTS event."GetEventAttendees"(INTEGER);
CREATE FUNCTION event."GetEventAttendees"("reqEventId" INTEGER)
    RETURNS TABLE
            (
                "Username"    CHARACTER VARYING,
                "DisplayName" CHARACTER VARYING,
                "Bio"         CHARACTER VARYING,
                "Image"       CHARACTER VARYING
            )
    LANGUAGE plpgsql
AS
$$
BEGIN
    RETURN QUERY SELECT DISTINCT ua."Username",
                                 ua."DisplayName",
                                 ua."Bio",
                                 (SELECT ph."Url"
                                  FROM event."Photos" ph
                                  WHERE ph."IsMain" = TRUE
                                    AND ea."UserAccountId" = ph."UserAccountId")
                 FROM event."UserAccount" ua
                          LEFT JOIN event."EventAttendees" ea ON ea."UserAccountId" = ua."AccountId"
                          LEFT JOIN event."Photos" p ON ea."UserAccountId" = p."UserAccountId"
                 WHERE ea."EventId" = "reqEventId";
END
$$;


DROP FUNCTION IF EXISTS event."GetEvents"();
CREATE FUNCTION event."GetEvents"()
    RETURNS TABLE
            (
                "EventUuid"    CHARACTER VARYING,
                "Title"        CHARACTER VARYING,
                "Description"  CHARACTER VARYING,
                "Category"     CHARACTER VARYING,
                "City"         CHARACTER VARYING,
                "Venue"        CHARACTER VARYING,
                "Date"         TIMESTAMP WITHOUT TIME ZONE,
                "HostUsername" CHARACTER VARYING,
                "IsCancelled"  BOOLEAN,
                "Attendees"    JSONB
            )
    LANGUAGE plpgsql
AS
$$
BEGIN
    RETURN QUERY SELECT DISTINCT ev."EventUuid",
                                 ev."Title",
                                 ev."Description",
                                 ev."Category",
                                 ev."City",
                                 ev."Venue",
                                 ev."Date",
                                 ua."Username",
                                 ev."IsCancelled",
                                 (SELECT jsonb_agg(v) FROM event."GetEventAttendees"(ea."EventId"::INTEGER) v)::JSONB
                 FROM event."Events" ev
                          LEFT JOIN event."EventAttendees" ea ON ev."Id" = ea."EventId"
                          LEFT JOIN event."UserAccount" ua ON ua."AccountId" = ev."HostId"
                 ORDER BY ev."Date";
END
$$;


DROP FUNCTION IF EXISTS event."GetEventDetails"(CHARACTER VARYING);
CREATE FUNCTION event."GetEventDetails"("reqEventUuid" CHARACTER VARYING)
    RETURNS TABLE
            (
                "EventUuid"    CHARACTER VARYING,
                "Title"        CHARACTER VARYING,
                "Description"  CHARACTER VARYING,
                "Category"     CHARACTER VARYING,
                "City"         CHARACTER VARYING,
                "Venue"        CHARACTER VARYING,
                "Date"         TIMESTAMP WITHOUT TIME ZONE,
                "HostUsername" CHARACTER VARYING,
                "IsCancelled"  BOOLEAN,
                "Attendees"    JSONB,
                "Likes"        BIGINT,
                "Comments"     BIGINT
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    _event_id INTEGER;
BEGIN

    SELECT e."Id"::INTEGER INTO _event_id FROM event."Events" e WHERE e."EventUuid" = "reqEventUuid" LIMIT 1;

    RETURN QUERY SELECT ev."EventUuid",
                        ev."Title",
                        ev."Description",
                        ev."Category",
                        ev."City",
                        ev."Venue",
                        ev."Date",
                        ua."Username",
                        ev."IsCancelled",
                        (SELECT jsonb_agg(v) FROM event."GetEventAttendees"(ea."EventId"::INTEGER) v)::JSONB,
                        (SELECT COUNT(l."Id") FROM event."EventLikes" l WHERE l."EventId" = _event_id),
                        (SELECT COUNT(ec."CommentId") FROM event."EventComments" ec WHERE ec."EventId" = _event_id)
                 FROM event."Events" ev
                          LEFT JOIN event."EventAttendees" ea ON ev."Id" = ea."EventId"
                          LEFT JOIN event."UserAccount" ua ON ua."AccountId" = ea."UserAccountId"
                 WHERE ev."Id"::INTEGER = _event_id
                 LIMIT 1;
END
$$;


DROP FUNCTION IF EXISTS event."CreateEvent"(CHARACTER VARYING, CHARACTER VARYING, CHARACTER VARYING, CHARACTER VARYING,
                                            CHARACTER VARYING,
                                            CHARACTER VARYING, CHARACTER VARYING, TIMESTAMP WITHOUT TIME ZONE);
CREATE FUNCTION event."CreateEvent"("reqUsername" CHARACTER VARYING, "reqEventUuid" CHARACTER VARYING,
                                    "reqTitle" CHARACTER VARYING,
                                    "reqDescription" CHARACTER VARYING, "reqCategory" CHARACTER VARYING,
                                    "reqCity" CHARACTER VARYING, "reqVenue" CHARACTER VARYING,
                                    "reqDate" TIMESTAMP WITHOUT TIME ZONE)
    RETURNS CHARACTER VARYING
    LANGUAGE plpgsql
AS
$$
DECLARE
    _account_id INTEGER;
    _event_id   INTEGER;
BEGIN
    SELECT uc."AccountId"::INTEGER
    INTO _account_id
    FROM event."UserAccount" uc
    WHERE uc."Username" = "reqUsername"
    LIMIT 1;

    IF _account_id IS NULL THEN
        RETURN 'You need to be a user to be able to create an event'::CHARACTER VARYING;
    END IF;

    INSERT INTO event."Events"("EventUuid", "Title", "Description", "Category", "City", "Venue", "Date", "HostId")
    VALUES ("reqEventUuid", "reqTitle", "reqDescription", "reqCategory", "reqCity", "reqVenue", "reqDate", _account_id)
    RETURNING "Id"::INTEGER INTO _event_id;

    INSERT INTO event."EventAttendees"("UserAccountId", "EventId", "IsHost") VALUES (_account_id, _event_id, TRUE);

    RETURN 'Event created successfully'::CHARACTER VARYING;
END
$$;


DROP FUNCTION IF EXISTS event."UpdateEvent"(CHARACTER VARYING, CHARACTER VARYING, CHARACTER VARYING, CHARACTER VARYING,
                                            CHARACTER VARYING,
                                            CHARACTER VARYING, CHARACTER VARYING, TIMESTAMP WITHOUT TIME ZONE);
CREATE FUNCTION event."UpdateEvent"("reqUsername" CHARACTER VARYING, "reqEventUuid" CHARACTER VARYING,
                                    "reqTitle" CHARACTER VARYING,
                                    "reqDescription" CHARACTER VARYING, "reqCategory" CHARACTER VARYING,
                                    "reqCity" CHARACTER VARYING, "reqVenue" CHARACTER VARYING,
                                    "reqDate" TIMESTAMP WITHOUT TIME ZONE)
    RETURNS CHARACTER VARYING
    LANGUAGE plpgsql
AS
$$
DECLARE
    _request RECORD;
    _user_id INTEGER;
BEGIN

    SELECT U."AccountId"::INTEGER INTO _user_id FROM event."UserAccount" u WHERE u."Username" = "reqUsername" LIMIT 1;

    SELECT ev."Id"::INTEGER, ev."HostId"
    FROM event."Events" ev
    WHERE ev."EventUuid" = "reqEventUuid"
    LIMIT 1
    INTO _request;

    IF _request."Id" IS NULL THEN
        RETURN 'Event does not exist, check and try again'::CHARACTER VARYING;
    END IF;


    IF _user_id IS NULL THEN
        RETURN 'User account does not exist, check and try again'::CHARACTER VARYING;
    END IF;

    IF _user_id IS NOT NULL AND _user_id != _request."HostId" THEN
        RETURN 'Sorry, only the host of this event can edit the event details'::CHARACTER VARYING;
    END IF;


    UPDATE event."Events"
    SET "Title"        = "reqTitle",
        "Description"  = "reqDescription",
        "Category"     = "reqCategory",
        "City"         = "reqCity",
        "Venue"        = "reqVenue",
        "Date"         = "reqDate",
        "DateModified" = NOW() AT TIME ZONE 'UTC'
    WHERE "EventUuid" = "reqEventUuid";

    RETURN ''::CHARACTER VARYING;
END
$$;


DROP FUNCTION IF EXISTS event."DeleteEvent"(CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."DeleteEvent"("reqUsername" CHARACTER VARYING, "reqEventUuid" CHARACTER VARYING)
    RETURNS CHARACTER VARYING
    LANGUAGE plpgsql
AS
$$
DECLARE
    _request RECORD;
    _user_id INTEGER;
BEGIN

    SELECT U."AccountId"::INTEGER INTO _user_id FROM event."UserAccount" u WHERE u."Username" = "reqUsername" LIMIT 1;

    SELECT ev."Id"::INTEGER, ev."HostId"
    FROM event."Events" ev
    WHERE ev."EventUuid" = "reqEventUuid"
    LIMIT 1
    INTO _request;

    IF _request."Id" IS NULL THEN
        RETURN 'Event does not exist, check and try again'::CHARACTER VARYING;
    END IF;

    IF _user_id IS NULL THEN
        RETURN 'User account does not exist, check and try again'::CHARACTER VARYING;
    END IF;

    IF _user_id IS NOT NULL AND _user_id != _request."HostId" THEN
        RETURN 'Sorry, only the host of this event can remove the event'::CHARACTER VARYING;
    END IF;

    DELETE FROM event."Events" WHERE "EventUuid" = "reqEventUuid";

    RETURN ''::CHARACTER VARYING;
END
$$;


DROP FUNCTION IF EXISTS event."UpdateEventAttendance"(CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."UpdateEventAttendance"("reqEventUuid" CHARACTER VARYING, "reqUsername" CHARACTER VARYING)
    RETURNS TABLE
            (
                "Message"      CHARACTER VARYING,
                "ResponseCode" INTEGER
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    _event_id        INTEGER;
    _event_cancelled BOOLEAN;
    _account_id      INTEGER;
    _attendee        CHARACTER VARYING;
    _attendance_id   INTEGER;
    _host_account_id INTEGER;
BEGIN

    SELECT ev."Id"::INTEGER, ev."IsCancelled"
    INTO _event_id, _event_cancelled
    FROM event."Events" ev
    WHERE ev."EventUuid" = "reqEventUuid"
    LIMIT 1;

    IF _event_id IS NULL THEN
        RETURN QUERY SELECT 'Event does not exist, check and try again'::CHARACTER VARYING,
                            400; -- Code 400 -> Event not found
        RETURN;
    END IF;

    SELECT ua."AccountId"::INTEGER
    INTO _account_id
    FROM event."UserAccount" ua
    WHERE ua."Username" = "reqUsername"
    LIMIT 1;

    IF _account_id IS NULL THEN
        RETURN QUERY SELECT 'You need to be a user to be able to attend an event'::CHARACTER VARYING,
                            402; -- Code 402 -> User not found
        RETURN;
    END IF;

    SELECT ea."UserAccountId"
    INTO _host_account_id
    FROM event."EventAttendees" ea
    WHERE ea."EventId" = _event_id
      AND ea."IsHost" = TRUE
    LIMIT 1;

    SELECT ea."Id"::INTEGER, u."Username"
    INTO _attendance_id, _attendee
    FROM event."EventAttendees" ea
             LEFT JOIN event."UserAccount" u ON u."AccountId" = ea."UserAccountId"
    WHERE ea."EventId" = _event_id
      AND ea."IsHost" = FALSE
    LIMIT 1;

    IF _host_account_id IS NOT NULL AND _host_account_id = _account_id THEN
        UPDATE event."Events" SET "IsCancelled" = NOT _event_cancelled WHERE "Id" = _event_id;
        RETURN QUERY SELECT 'Event status changed successfully'::CHARACTER VARYING,
                            100; -- Code 100 -> Event cancelled status
        RETURN;
    END IF;

    IF _attendee IS NOT NULL AND _host_account_id != _account_id THEN
        DELETE FROM event."EventAttendees" WHERE "Id" = _attendance_id;
        RETURN QUERY SELECT 'You  have been removed from the attendees of the event'::CHARACTER VARYING,
                            102; -- Code 102 -> User removed from attendees
    END IF;

    IF _attendee IS NULL THEN
        INSERT INTO event."EventAttendees"("UserAccountId", "EventId", "IsHost") VALUES (_account_id, _event_id, FALSE);
    end if;


    RETURN QUERY SELECT ''::CHARACTER VARYING,
                        107; -- Code 17 -> No message
END
$$;


DROP FUNCTION IF EXISTS event."RegisterUser"(CHARACTER VARYING, CHARACTER VARYING, CHARACTER VARYING,
                                             CHARACTER VARYING, CHARACTER VARYING, CHARACTER VARYING,
                                             CHARACTER VARYING);
CREATE FUNCTION event."RegisterUser"("reqAccountUuid" CHARACTER VARYING,
                                     "reqUsername" CHARACTER VARYING,
                                     "reqDisplayName" CHARACTER VARYING,
                                     "reqEmail" CHARACTER VARYING,
                                     "reqPassword" CHARACTER VARYING,
                                     "reqPhoneNumber" CHARACTER VARYING,
                                     "reqEmailConfirmationToken" CHARACTER VARYING)
    RETURNS CHARACTER VARYING
    LANGUAGE plpgsql AS
$$
DECLARE
    account RECORD;
BEGIN


    SELECT uc."AccountUuid", uc."Username", uc."Email", uc."PhoneNumber"
    FROM event."UserAccount" uc
    WHERE uc."AccountUuid" = "reqAccountUuid"
       OR upper(uc."Username") = upper("reqUsername")
       OR upper(uc."Email") = upper("reqEmail")
       OR uc."PhoneNumber" = "reqPhoneNumber"
    LIMIT 1
    INTO account;

    IF account."Username" IS NOT NULL AND account."Username" = "reqUsername" THEN
        RETURN CONCAT(account."Username", ' already exists, use a different username.')::CHARACTER VARYING;
    END IF;

    IF account."Email" IS NOT NULL AND account."Email" = "reqEmail" THEN
        RETURN CONCAT(ACCOUNT."Email", ' already exists, use a different email.')::CHARACTER VARYING;
    END IF;

    IF account."PhoneNumber" IS NOT NULL AND account."PhoneNumber" = "reqPhoneNumber" THEN
        RETURN 'Phone number already exists, use a different Phone number.'::CHARACTER VARYING;
    END IF;

    INSERT INTO event."UserAccount"("AccountUuid", "Username", "DisplayName", "Password", "Email", "PhoneNumber",
                                    "IsActive", "EmailConfirmationToken")
    VALUES ("reqAccountUuid", trim("reqUsername"), "reqDisplayName", trim("reqPassword"), "reqEmail", "reqPhoneNumber",
            TRUE, "reqEmailConfirmationToken");

    RETURN '':: CHARACTER VARYING;
END
$$;


DROP FUNCTION IF EXISTS event."VerifyEmail"(CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."VerifyEmail"("reqEmail" CHARACTER VARYING, "reqEmailConfirmationToken" CHARACTER VARYING)
    RETURNS CHARACTER VARYING
    LANGUAGE plpgsql
AS
$$
DECLARE
    _request RECORD;
BEGIN

    SELECT uc."Email", uc."EmailConfirmationToken"
    FROM event."UserAccount" uc
    WHERE lower(uc."Email") = lower("reqEmail")
    LIMIT 1
    INTO _request;

    IF _request."Email" IS NULL THEN
        RETURN 'Email address is not associated with any account, check and try again'::CHARACTER VARYING;
    END IF;

    IF _request."EmailConfirmationToken" != "reqEmailConfirmationToken" THEN
        RETURN 'Could not verify email address, check and try again'::CHARACTER VARYING;
    END IF;

    UPDATE event."UserAccount" uc SET "EmailConfirmed" = TRUE WHERE "Email" = "reqEmail";

    RETURN ''::CHARACTER VARYING;
END
$$;


DROP FUNCTION IF EXISTS event."ResendEmailConfirmationLink"(CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."ResendEmailConfirmationLink"("reqEmail" CHARACTER VARYING,
                                                    "reqEmailConfirmationToken" CHARACTER VARYING)
    RETURNS CHARACTER VARYING
    LANGUAGE plpgsql
AS
$$
DECLARE
    _email CHARACTER VARYING;
BEGIN

    SELECT uc."Email"
    INTO _email
    FROM event."UserAccount" uc
    WHERE lower(uc."Email") = lower("reqEmail")
    LIMIT 1;

    IF _email IS NULL THEN
        RETURN 'Email address is not associated with any account, check and try again'::CHARACTER VARYING;
    END IF;

    UPDATE event."UserAccount" uc SET "EmailConfirmationToken" = "reqEmailConfirmationToken" WHERE "Email" = "reqEmail";

    RETURN ''::CHARACTER VARYING;
END
$$;


DROP FUNCTION IF EXISTS event."UserLogin"(CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."UserLogin"("reqUsername" CHARACTER VARYING, "reqPassword" CHARACTER VARYING)
    RETURNS TABLE
            (
                "ResponseMessage" CHARACTER VARYING,
                "Username"        CHARACTER VARYING,
                "DisplayName"     CHARACTER VARYING,
                "ProfilePicture"  CHARACTER VARYING,
                "LastLogin"       TIMESTAMP WITHOUT TIME ZONE
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    userRecord  RECORD;
    _user_photo CHARACTER VARYING;
BEGIN

    SELECT uc."IsActive",
           uc."AccountId",
           uc."Username",
           uc."DisplayName",
           uc."LastLogin",
           uc."EmailConfirmed"
    FROM event."UserAccount" uc
    WHERE lower(uc."Username") = lower("reqUsername")
      AND uc."Password" = "reqPassword"
    LIMIT 1
    INTO userRecord;

    IF userRecord."AccountId" IS NULL THEN
        RETURN QUERY SELECT 'Invalid login credentials, check and try again'::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::TIMESTAMP WITHOUT TIME ZONE;
        RETURN;
    END IF;

    IF userRecord."IsActive" = FALSE THEN
        RETURN QUERY SELECT 'Your account is disabled, contact support for further assistance or create a new account'::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::TIMESTAMP WITHOUT TIME ZONE;
        RETURN;
    END IF;

    IF userRecord."EmailConfirmed" = FALSE THEN
        RETURN QUERY SELECT 'Email not confirmed, confirm your email and sign in'::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::TIMESTAMP WITHOUT TIME ZONE;
        RETURN;
    END IF;

    UPDATE event."UserAccount" uc SET "LastLogin" = NOW() AT TIME ZONE 'UTC' WHERE "AccountId" = userRecord."AccountId";

    SELECT p."Url"
    INTO _user_photo
    FROM event."Photos" p
    WHERE p."IsMain" = TRUE
      AND p."UserAccountId" = userRecord."AccountId"::INTEGER
    LIMIT 1;

    RETURN QUERY SELECT ''::CHARACTER VARYING,
                        userRecord."Username",
                        userRecord."DisplayName",
                        _user_photo,
                        userRecord."LastLogin";
END
$$;


DROP FUNCTION IF EXISTS event."UploadImage"(CHARACTER VARYING, CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."UploadImage"("reqUsername" CHARACTER VARYING, "reqPublicId" CHARACTER VARYING,
                                    "reqFile" CHARACTER VARYING)
    RETURNS TABLE
            (
                "ResponseMessage" CHARACTER VARYING,
                "PublicId"        CHARACTER VARYING,
                "Url"             CHARACTER VARYING,
                "IsMainPhoto"     BOOLEAN
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    _user_id       INTEGER;
    _is_main       BOOLEAN;
    _public_id     CHARACTER VARYING;
    _url           CHARACTER VARYING;
    _is_main_photo BOOLEAN;
BEGIN

    SELECT U."AccountId"::INTEGER INTO _user_id FROM event."UserAccount" u WHERE u."Username" = "reqUsername" LIMIT 1;

    IF _user_id IS NULL THEN
        RETURN QUERY SELECT 'User account does not exist, check and try again'::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            FALSE;
        RETURN;
    END IF;


    INSERT INTO event."Photos"("PublicId", "UserAccountId", "Url") VALUES ("reqPublicId", _user_id, "reqFile");

    SELECT e."IsMain" INTO _is_main FROM event."Photos" e WHERE e."UserAccountId" = _user_id;

    IF NOT _is_main THEN
        UPDATE event."Photos" p SET "IsMain" = TRUE WHERE p."PublicId" = "reqPublicId";
    END IF;

    SELECT ph."PublicId", ph."Url", ph."IsMain"
    INTO _public_id, _url, _is_main_photo
    FROM event."Photos" ph
    WHERE ph."PublicId" = "reqPublicId"
    LIMIT 1;

    RETURN QUERY SELECT ''::CHARACTER VARYING, _public_id, _url, _is_main_photo;
END
$$;


DROP FUNCTION IF EXISTS event."DeleteImage"(CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."DeleteImage"("reqUsername" CHARACTER VARYING, "reqPublicId" CHARACTER VARYING)
    RETURNS CHARACTER VARYING
    LANGUAGE plpgsql
AS
$$
DECLARE
    _user_id       INTEGER;
    _public_id     CHARACTER VARYING;
    _is_main_photo BOOLEAN;
BEGIN

    SELECT U."AccountId"::INTEGER INTO _user_id FROM event."UserAccount" u WHERE u."Username" = "reqUsername" LIMIT 1;

    IF _user_id IS NULL THEN
        RETURN 'User account does not exist, check and try again'::CHARACTER VARYING;
    END IF;

    SELECT ph."PublicId", ph."IsMain"
    INTO _public_id, _is_main_photo
    FROM event."Photos" ph
    WHERE ph."PublicId" = "reqPublicId"
      AND ph."UserAccountId" = _user_id
    LIMIT 1;

    IF _public_id IS NULL THEN
        RETURN 'Image does not exist, check and try again'::CHARACTER VARYING;
    END IF;

    IF _is_main_photo = TRUE THEN
        RETURN 'You cannot delete your main photo'::CHARACTER VARYING;
    END IF;

    DELETE FROM event."Photos" p WHERE p."PublicId" = "reqPublicId" AND p."UserAccountId" = _user_id;

    RETURN ''::CHARACTER VARYING;
END
$$;


DROP FUNCTION IF EXISTS event."SetProfilePicture"(CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."SetProfilePicture"("reqUsername" CHARACTER VARYING, "reqPublicId" CHARACTER VARYING)
    RETURNS CHARACTER VARYING
    LANGUAGE plpgsql
AS
$$
DECLARE
    _user_id             INTEGER;
    _public_id           CHARACTER VARYING;
    _current_profile_pic BOOLEAN;
BEGIN

    SELECT U."AccountId"::INTEGER INTO _user_id FROM event."UserAccount" u WHERE u."Username" = "reqUsername" LIMIT 1;

    IF _user_id IS NULL THEN
        RETURN 'User account does not exist, check and try again'::CHARACTER VARYING;
    END IF;

    SELECT ph."PublicId"
    INTO _public_id
    FROM event."Photos" ph
    WHERE ph."PublicId" = "reqPublicId"
      AND ph."UserAccountId" = _user_id
    LIMIT 1;

    IF _public_id IS NULL THEN
        RETURN 'Image does not exist, check and try again'::CHARACTER VARYING;
    END IF;

    SELECT p."IsMain"
    INTO _current_profile_pic
    FROM event."Photos" p
    WHERE p."UserAccountId" = _user_id
      AND p."IsMain" = TRUE
    LIMIT 1;

    IF _current_profile_pic = TRUE THEN
        UPDATE event."Photos" SET "IsMain" = FALSE WHERE "IsMain" = TRUE AND "UserAccountId" = _user_id;
    END IF;

    UPDATE event."Photos" SET "IsMain" = TRUE WHERE "PublicId" = "reqPublicId" AND "UserAccountId" = _user_id;

    RETURN ''::CHARACTER VARYING;
END
$$;


DROP FUNCTION IF EXISTS event."GetUserPhotos"(INTEGER);
CREATE FUNCTION event."GetUserPhotos"("reqAccountId" INTEGER)
    RETURNS TABLE
            (
                "PublicId"    CHARACTER VARYING,
                "Url"         CHARACTER VARYING,
                "IsMainPhoto" BOOLEAN
            )
    LANGUAGE plpgsql
AS
$$
BEGIN
    RETURN QUERY SELECT p."PublicId", p."Url", p."IsMain"
                 FROM event."Photos" p
                 WHERE p."UserAccountId" = "reqAccountId";
END
$$;


DROP FUNCTION IF EXISTS event."GetUserProfile"(CHARACTER VARYING);
CREATE FUNCTION event."GetUserProfile"("reqUsername" CHARACTER VARYING)
    RETURNS TABLE
            (
                "ResponseMessage" CHARACTER VARYING,
                "Username"        CHARACTER VARYING,
                "DisplayName"     CHARACTER VARYING,
                "Bio"             CHARACTER VARYING,
                "Image"           CHARACTER VARYING,
                "FollowingCount"  BIGINT,
                "FollowersCount"  BIGINT,
                "Photos"          JSONB
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    _user_id    INTEGER;
    _profile_id INTEGER;
BEGIN

    SELECT u."AccountId"::INTEGER INTO _user_id FROM event."UserAccount" u WHERE u."Username" = "reqUsername" LIMIT 1;

    SELECT ff."ObserverAccountId"
    INTO _profile_id
    FROM event."UserAccount" uc
             LEFT JOIN event."UserFollowings" ff ON ff."TargetAccountId" = uc."AccountId"
    WHERE uc."Username" = "reqUsername"
    LIMIT 1;

    IF _user_id IS NULL THEN
        RETURN QUERY SELECT CONCAT("reqUsername",
                                   ' does not match any user profile, check and try again')::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            0::BIGINT,
                            0::BIGINT,
                            NULL::JSONB;
        RETURN;
    END IF;

    RETURN QUERY SELECT ''::CHARACTER VARYING,
                        ua."Username",
                        ua."DisplayName",
                        ua."Bio",
                        (SELECT p."Url"
                         FROM event."Photos" p
                         WHERE p."UserAccountId" = _user_id
                           AND p."IsMain" = TRUE
                         LIMIT 1),
                        (SELECT COUNT(*)
                         FROM event."UserFollowings" f
                         WHERE f."ObserverAccountId" = ua."AccountId"),
                        (SELECT COUNT(*)
                         FROM event."UserFollowings" ufo
                         WHERE ufo."TargetAccountId" = ua."AccountId"),
                        (SELECT jsonb_agg(v) FROM event."GetUserPhotos"(_user_id) v)::JSONB
                 FROM event."UserAccount" ua
                          LEFT JOIN event."UserFollowings" uf ON uf."ObserverAccountId" = ua."AccountId"
                 WHERE ua."Username" = "reqUsername"
                 LIMIT 1;

END
$$;


DROP FUNCTION IF EXISTS event."UpdateUserProfile"(CHARACTER VARYING, CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."UpdateUserProfile"("reqUsername" CHARACTER VARYING, "reqDisplayName" CHARACTER VARYING,
                                          "reqBio" CHARACTER VARYING)
    RETURNS CHARACTER VARYING
    LANGUAGE plpgsql
AS
$$
DECLARE
    _user_id INTEGER;
BEGIN

    SELECT U."AccountId"::INTEGER INTO _user_id FROM event."UserAccount" u WHERE u."Username" = "reqUsername" LIMIT 1;

    IF _user_id IS NULL THEN
        RETURN 'User account does not exist, check and try again'::CHARACTER VARYING;
    END IF;


    UPDATE event."UserAccount" SET "DisplayName" = "reqDisplayName", "Bio" = "reqBio" WHERE "AccountId" = _user_id;

    RETURN ''::CHARACTER VARYING;
END
$$;


DROP FUNCTION IF EXISTS event."AddEventComment"(CHARACTER VARYING, CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."AddEventComment"("reqUsername" CHARACTER VARYING, "reqEventUuid" CHARACTER VARYING,
                                        "reqComment" CHARACTER VARYING)
    RETURNS CHARACTER VARYING
    LANGUAGE plpgsql
AS
$$
DECLARE
    _user_id  INTEGER;
    _event_id INTEGER;
BEGIN

    SELECT U."AccountId"::INTEGER INTO _user_id FROM event."UserAccount" u WHERE u."Username" = "reqUsername" LIMIT 1;

    SELECT ev."Id"::INTEGER INTO _event_id FROM event."Events" ev WHERE ev."EventUuid" = "reqEventUuid" LIMIT 1;

    IF _user_id IS NULL THEN
        RETURN 'User account does not exist, check and try again'::CHARACTER VARYING;
    END IF;

    IF _event_id IS NULL THEN
        RETURN 'Event does not exist, check and try again'::CHARACTER VARYING;
    END IF;

    INSERT INTO event."EventComments"("UserAccountId", "EventId", "Comment") VALUES (_user_id, _event_id, "reqComment");

    RETURN ''::CHARACTER VARYING;
END
$$;


DROP FUNCTION IF EXISTS event."ReplyOnComment"(CHARACTER VARYING, INTEGER, CHARACTER VARYING);
CREATE FUNCTION event."ReplyOnComment"("reqUsername" CHARACTER VARYING, "reqCommentId" INTEGER,
                                       "reqReply" CHARACTER VARYING)
    RETURNS CHARACTER VARYING
    LANGUAGE plpgsql
AS
$$
DECLARE
    _user_id    INTEGER;
    _comment_id INTEGER;
    _event_id   INTEGER;
BEGIN

    SELECT U."AccountId"::INTEGER INTO _user_id FROM event."UserAccount" u WHERE u."Username" = "reqUsername" LIMIT 1;

    SELECT ec."CommentId"::INTEGER, ec."EventId"
    INTO _comment_id, _event_id
    FROM event."EventComments" ec
    WHERE ec."CommentId" = "reqCommentId"
    LIMIT 1;

    IF _user_id IS NULL THEN
        RETURN 'User account does not exist, check and try again'::CHARACTER VARYING;
    END IF;

    IF _comment_id IS NULL THEN
        RETURN 'Comment does not exist, check and try again'::CHARACTER VARYING;
    END IF;

    INSERT INTO event."EventComments"("UserAccountId", "EventId", "Comment", "ParentId")
    VALUES (_user_id, _event_id, "reqReply", _comment_id);

    RETURN ''::CHARACTER VARYING;
END
$$;

DROP FUNCTION IF EXISTS event."LikeOrUnlikeComment"(CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."LikeOrUnlikeComment"("reqUsername" CHARACTER VARYING, "reqEventCommentId" INTEGER)
    RETURNS TABLE
            (
                "Message"      CHARACTER VARYING,
                "ResponseCode" INTEGER
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    _user_id               INTEGER;
    _event_comment_id      INTEGER;
    _event_comment_like_id INTEGER;
    _already_liked         BOOLEAN;
BEGIN

    SELECT U."AccountId"::INTEGER INTO _user_id FROM event."UserAccount" u WHERE u."Username" = "reqUsername" LIMIT 1;

    IF _user_id IS NULL THEN
        RETURN QUERY SELECT 'User account does not exist, check and try again'::CHARACTER VARYING,
                            400; -- Code 400: Not a user
        RETURN;
    END IF;

    SELECT ec."CommentId"::INTEGER
    INTO _event_comment_id
    FROM event."EventComments" ec
    WHERE ec."CommentId"::INTEGER = "reqEventCommentId"
    LIMIT 1;

    IF _event_comment_id IS NULL THEN
        RETURN QUERY SELECT 'Comment does not exist, check and try again'::CHARACTER VARYING,
                            401; -- Code 400: Comment not found
        RETURN;
    END IF;

    SELECT ecl."Id", ecl."Like"
    INTO _event_comment_like_id, _already_liked
    FROM event."EventCommentLikes" ecl
    WHERE ecl."EventCommentId" = _event_comment_id
      AND ecl."UserAccountId" = _user_id
      AND ecl."Like" = TRUE;

    IF _already_liked = TRUE THEN
        DELETE FROM event."EventCommentLikes" WHERE "Id"::INTEGER = _event_comment_like_id;
        RETURN QUERY SELECT 'You unliked the comment'::CHARACTER VARYING, 100; -- Code 100: Unlike event
        RETURN;
    END IF;

    INSERT INTO event."EventCommentLikes"("UserAccountId", "EventCommentId", "Like")
    VALUES (_user_id, _event_comment_id, TRUE);

    RETURN QUERY SELECT ''::CHARACTER VARYING, 101; -- Code 101: Comment liked
END
$$;

DROP FUNCTION IF EXISTS event."GetCommentLiker"(INTEGER);
CREATE FUNCTION event."GetCommentLiker"("reqCommentId" INTEGER)
    RETURNS TABLE
            (
                "LikerUsername"    CHARACTER VARYING,
                "LikerDisplayName" CHARACTER VARYING,
                "MainPhoto"        CHARACTER VARYING
            )
    LANGUAGE plpgsql
AS
$$
BEGIN
    RETURN QUERY SELECT uc."Username",
                        uc."DisplayName",
                        (SELECT ph."Url"
                         FROM event."Photos" ph
                         WHERE ph."UserAccountId" = ecl."UserAccountId"
                           AND ph."IsMain" = TRUE
                         LIMIT 1)
                 FROM event."EventCommentLikes" ecl
                          LEFT JOIN event."UserAccount" uc ON uc."AccountId" = ecl."UserAccountId"
                 WHERE ecl."EventCommentId" = "reqCommentId";
END
$$;

DROP FUNCTION IF EXISTS event."GetEventComments"(CHARACTER VARYING);
CREATE FUNCTION event."GetEventComments"("reqEventUuid" CHARACTER VARYING)
    RETURNS TABLE
            (
                "CommentId"     INTEGER,
                "Author"        CHARACTER VARYING,
                "Comment"       CHARACTER VARYING,
                "Replies"       JSONB,
                "NumberOfLikes" BIGINT,
                "Likes"         JSONB
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    _event_id INTEGER;
BEGIN

    SELECT ev."Id"::INTEGER INTO _event_id FROM event."Events" ev WHERE ev."EventUuid" = "reqEventUuid" LIMIT 1;

    RETURN QUERY SELECT ec."CommentId"::INTEGER,
                        uc."Username",
                        ec."Comment",
                        (SELECT DISTINCT jsonb_agg(v) FROM event."GetCommentReplies"(ec."CommentId"::INTEGER) v)::JSONB,
                        (SELECT COUNT(*) FROM event."EventCommentLikes" li WHERE li."EventCommentId" = ec."CommentId"),
                        (SELECT jsonb_agg(v) FROM event."GetCommentLiker"(ec."CommentId"::INTEGER) v)::JSONB
                 FROM event."EventComments" ec
                          LEFT JOIN event."UserAccount" uc ON uc."AccountId" = ec."UserAccountId"
                 WHERE ec."EventId" = _event_id
                   AND ec."ParentId" IS NULL
                 ORDER BY ec."DateCreated";
END
$$;

DROP FUNCTION IF EXISTS event."GetCommentReplies"(INTEGER);
CREATE FUNCTION event."GetCommentReplies"("reqCommentId" INTEGER)
    RETURNS TABLE
            (
                "RepliedByUsername"    CHARACTER VARYING,
                "RepliedByDisplayName" CHARACTER VARYING,
                "MainPhoto"            CHARACTER VARYING,
                "Reply"                CHARACTER VARYING,
                "NumberOfLikes"        BIGINT,
                "Likes"                JSONB
            )
    LANGUAGE plpgsql
AS
$$
BEGIN
    RETURN QUERY SELECT uc."Username",
                        uc."DisplayName",
                        (SELECT ph."Url"
                         FROM event."Photos" ph
                         WHERE ph."UserAccountId" = ec."UserAccountId"
                           AND ph."IsMain" = TRUE
                         LIMIT 1),
                        ec."Comment",
                        (SELECT COUNT(*) FROM event."EventCommentLikes" li WHERE li."EventCommentId" = ec."CommentId"),
                        (SELECT jsonb_agg(v) FROM event."GetCommentLiker"(ec."CommentId"::INTEGER) v)::JSONB
                 FROM event."EventComments" ec
                          LEFT JOIN event."UserAccount" uc ON uc."AccountId" = ec."UserAccountId"
                 WHERE ec."ParentId" = "reqCommentId";
END
$$;

DROP FUNCTION IF EXISTS event."FollowOrUnfollowUser"(CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."FollowOrUnfollowUser"("reqObserverUsername" CHARACTER VARYING,
                                             "reqTargetUsername" CHARACTER VARYING)
    RETURNS TABLE
            (
                "ResponseMessage" CHARACTER VARYING,
                "ResponseCode"    INTEGER
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    _observer_user_id INTEGER;
    _target_user_id   INTEGER;
    _following        BOOLEAN;
BEGIN

    SELECT U."AccountId"::INTEGER
    INTO _observer_user_id
    FROM event."UserAccount" u
    WHERE u."Username" = "reqObserverUsername"
    LIMIT 1;

    SELECT U."AccountId"::INTEGER
    INTO _target_user_id
    FROM event."UserAccount" u
    WHERE u."Username" = "reqTargetUsername"
    LIMIT 1;

    IF _observer_user_id IS NULL THEN
        RETURN QUERY SELECT 'User account does not exist, check and try again'::CHARACTER VARYING,
                            300; -- Code 400 -> Observer account does not exist
        RETURN;
    END IF;

    IF _target_user_id IS NULL THEN
        RETURN QUERY SELECT CONCAT("reqTargetUsername", ' does not exist, check and try again')::CHARACTER VARYING,
                            301; -- Code 400 -> Target account does not exist
        RETURN;
    END IF;

    SELECT uf."Status"
    INTO _following
    FROM event."UserFollowings" uf
    WHERE uf."ObserverAccountId" = _observer_user_id
      AND uf."TargetAccountId" = _target_user_id
    LIMIT 1;

    IF _following = TRUE THEN
        DELETE
        FROM event."UserFollowings"
        WHERE "ObserverAccountId" = _observer_user_id
          AND "TargetAccountId" = _target_user_id;
        RETURN QUERY SELECT CONCAT('You have unfollowed ', "reqTargetUsername")::CHARACTER VARYING,
                            100; -- Code 100 -> Unfollowed
        RETURN;
    END IF;

    INSERT INTO event."UserFollowings"("ObserverAccountId", "TargetAccountId", "Status")
    VALUES (_observer_user_id, _target_user_id, TRUE);

    RETURN QUERY SELECT ''::CHARACTER VARYING, 101; -- Code 101 -> Following
END
$$;

DROP FUNCTION IF EXISTS event."GetUserFollowings"(CHARACTER VARYING);
CREATE FUNCTION event."GetUserFollowings"("reqUsername" CHARACTER VARYING)
    RETURNS TABLE
            (
                "Username"       CHARACTER VARYING,
                "DisplayName"    CHARACTER VARYING,
                "Bio"            CHARACTER VARYING,
                "Image"          CHARACTER VARYING,
                "Following"      BOOLEAN,
                "FollowingCount" BIGINT,
                "FollowersCount" BIGINT,
                "Photos"         JSONB
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    _user_id INTEGER;
BEGIN

    SELECT U."AccountId"::INTEGER INTO _user_id FROM event."UserAccount" u WHERE u."Username" = "reqUsername" LIMIT 1;


    RETURN QUERY SELECT ua."Username",
                        ua."DisplayName",
                        ua."Bio",
                        (SELECT p."Url"
                         FROM event."Photos" p
                         WHERE p."UserAccountId" = uf."TargetAccountId"
                           AND p."IsMain" = TRUE
                         LIMIT 1),
                        uf."Status",
                        (SELECT COUNT(*)
                         FROM event."UserFollowings" f
                         WHERE f."ObserverAccountId" = ua."AccountId"),
                        (SELECT COUNT(*)
                         FROM event."UserFollowings" ufo
                         WHERE ufo."TargetAccountId" = ua."AccountId"),
                        (SELECT jsonb_agg(v) FROM event."GetUserPhotos"(uf."TargetAccountId") v)::JSONB
                 FROM event."UserAccount" ua
                          LEFT JOIN event."UserFollowings" uf ON uf."TargetAccountId" = ua."AccountId"
                 WHERE uf."ObserverAccountId" = _user_id;
END
$$;

DROP FUNCTION IF EXISTS event."GetUserFollowers"(CHARACTER VARYING);
CREATE FUNCTION event."GetUserFollowers"("reqUsername" CHARACTER VARYING)
    RETURNS TABLE
            (
                "Username"       CHARACTER VARYING,
                "DisplayName"    CHARACTER VARYING,
                "Bio"            CHARACTER VARYING,
                "Image"          CHARACTER VARYING,
                "Following"      BOOLEAN,
                "FollowingCount" BIGINT,
                "FollowersCount" BIGINT,
                "Photos"         JSONB
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    _user_id INTEGER;
BEGIN

    SELECT U."AccountId"::INTEGER INTO _user_id FROM event."UserAccount" u WHERE u."Username" = "reqUsername" LIMIT 1;


    RETURN QUERY SELECT ua."Username",
                        ua."DisplayName",
                        ua."Bio",
                        (SELECT p."Url"
                         FROM event."Photos" p
                         WHERE p."UserAccountId" = uf."ObserverAccountId"
                           AND p."IsMain" = TRUE
                         LIMIT 1),
                        uf."Status",
                        (SELECT COUNT(*)
                         FROM event."UserFollowings" f
                         WHERE f."ObserverAccountId" = ua."AccountId"),
                        (SELECT COUNT(*)
                         FROM event."UserFollowings" ufo
                         WHERE ufo."TargetAccountId" = ua."AccountId"),
                        (SELECT jsonb_agg(v) FROM event."GetUserPhotos"(uf."ObserverAccountId") v)::JSONB
                 FROM event."UserAccount" ua
                          LEFT JOIN event."UserFollowings" uf ON uf."ObserverAccountId" = ua."AccountId"
                 WHERE uf."TargetAccountId" = _user_id;

END
$$;

DROP FUNCTION IF EXISTS event."LikeOrUnlikeEvent"(CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."LikeOrUnlikeEvent"("reqUsername" CHARACTER VARYING, "reqEventUuid" CHARACTER VARYING)
    RETURNS TABLE
            (
                "Message"      CHARACTER VARYING,
                "ResponseCode" INTEGER
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    _user_id       INTEGER;
    _event_id      INTEGER;
    _event_like_id INTEGER;
    _already_liked BOOLEAN;
BEGIN

    SELECT U."AccountId"::INTEGER INTO _user_id FROM event."UserAccount" u WHERE u."Username" = "reqUsername" LIMIT 1;

    SELECT ev."Id"::INTEGER INTO _event_id FROM event."Events" ev WHERE ev."EventUuid" = "reqEventUuid" LIMIT 1;

    SELECT el."Id", el."Like"
    INTO _event_like_id, _already_liked
    FROM event."EventLikes" el
    WHERE el."EventId" = _event_id
      AND el."UserAccountId" = _user_id
      AND el."Like" = TRUE;

    IF _user_id IS NULL THEN
        RETURN QUERY SELECT 'User account does not exist, check and try again'::CHARACTER VARYING,
                            400; -- Code 400: Not a user
        RETURN;
    END IF;

    IF _event_id IS NULL THEN
        RETURN QUERY SELECT 'Event does not exist, check and try again'::CHARACTER VARYING,
                            401; -- Code 400: Event not found
        RETURN;
    END IF;

    IF _already_liked = TRUE THEN
        DELETE FROM event."EventLikes" WHERE "Id"::INTEGER = _event_like_id;
        RETURN QUERY SELECT 'You unliked the event'::CHARACTER VARYING, 100; -- Code 100: Unlike event
        RETURN;
    END IF;

    INSERT INTO event."EventLikes"("UserAccountId", "EventId", "Like") VALUES (_user_id, _event_id, TRUE);

    RETURN QUERY SELECT ''::CHARACTER VARYING, 101; -- Code 101: Event liked
END
$$;

DROP FUNCTION IF EXISTS event."GetEventLikes"(CHARACTER VARYING);
CREATE FUNCTION event."GetEventLikes"("reqEventUuid" CHARACTER VARYING)
    RETURNS TABLE
            (
                "Username" CHARACTER VARYING
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    _event_id INTEGER;
BEGIN

    SELECT ev."Id"::INTEGER INTO _event_id FROM event."Events" ev WHERE ev."EventUuid" = "reqEventUuid" LIMIT 1;

    RETURN QUERY SELECT uc."Username"
                 FROM event."EventLikes" el
                          LEFT JOIN event."UserAccount" uc ON uc."AccountId" = el."UserAccountId"
                 WHERE el."EventId" = _event_id
                 ORDER BY el."DateCreated";
END
$$;

DROP FUNCTION IF EXISTS event."ForgotPassword"(CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."ForgotPassword"("reqEmail" CHARACTER VARYING, "reqPasswordResetCode" CHARACTER VARYING)
    RETURNS TABLE
            (
                "Message"         CHARACTER VARYING,
                "Username"        CHARACTER VARYING,
                "Email"           CHARACTER VARYING,
                "ResetCode"       CHARACTER VARYING,
                "ResetCodeExpiry" CHARACTER VARYING
            )
    LANGUAGE plpgsql
AS

$$
DECLARE
    userRecord      RECORD;
    resetCodeExpiry CHARACTER VARYING;
BEGIN

    SELECT uc."AccountId", uc."Username", uc."Email", uc."IsActive", uc."EmailConfirmed"
    FROM event."UserAccount" uc
    WHERE uc."Email" = "reqEmail"
    LIMIT 1
    INTO userRecord;

    IF userRecord."IsActive" = FALSE THEN
        RETURN QUERY SELECT 'Sorry, your account has been disabled. Contact support for assistance.'::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING;
        RETURN;
    END IF;

    IF userRecord."EmailConfirmed" = FALSE THEN
        RETURN QUERY SELECT 'Sorry, you need to confirm your email first. Request for email confirmation link.'::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING;
        RETURN;
    END IF;

    IF userRecord."Email" IS NULL THEN
        RETURN QUERY SELECT 'Sorry, we could not find a registered user with that email.'::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING;
        RETURN;
    END IF;

    UPDATE event."UserAccount" u
    SET "PasswordResetCode"       = "reqPasswordResetCode",
        "PasswordResetCodeExpiry" = NOW() AT TIME ZONE 'UTC' + INTERVAL '24 hour'
        --"PasswordResetCodeExpiry" = NOW() AT TIME ZONE 'UTC' + (2 * INTERVAL '1 minute')
    WHERE "AccountId" = userRecord."AccountId"
    RETURNING u."PasswordResetCodeExpiry" INTO resetCodeExpiry;

    RETURN QUERY SELECT ''::CHARACTER VARYING,
                        userRecord."Username",
                        userRecord."Email",
                        "reqPasswordResetCode",
                        resetCodeExpiry;

END
$$;

DROP FUNCTION IF EXISTS event."VerifyPasswordReset"(CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."VerifyPasswordReset"("reqEmail" CHARACTER VARYING, "reqPasswordResetCode" CHARACTER VARYING)
    RETURNS CHARACTER VARYING
    LANGUAGE plpgsql
AS
$$
DECLARE
    _request RECORD;
BEGIN

    SELECT uc."Email", uc."PasswordResetCode", uc."PasswordResetCodeExpiry"
    FROM event."UserAccount" uc
    WHERE lower(uc."Email") = lower("reqEmail")
    LIMIT 1
    INTO _request;

    IF _request."Email" IS NULL THEN
        RETURN 'Sorry, we could not find a registered user with that email.'::CHARACTER VARYING;
    END IF;

    IF _request."PasswordResetCode" != "reqPasswordResetCode" THEN
        RETURN 'Could not verify password reset code - invalid code provided, check and try again'::CHARACTER VARYING;
    END IF;

    IF _request."PasswordResetCodeExpiry" < NOW() AT TIME ZONE 'UTC' THEN
        RETURN 'Password reset code has expired. Request for a new reset code'::CHARACTER VARYING;
    END IF;

    RETURN ''::CHARACTER VARYING;
END
$$;


DROP FUNCTION IF EXISTS event."ResetPassword"(CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."ResetPassword"("reqEmail" CHARACTER VARYING, "reqNewPassword" CHARACTER VARYING)
    RETURNS CHARACTER VARYING
    LANGUAGE plpgsql
AS
$$
DECLARE
    _request RECORD;
BEGIN

    SELECT uc."Email", uc."Password"
    FROM event."UserAccount" uc
    WHERE uc."Email" = "reqEmail"
    LIMIT 1
    INTO _request;

    IF _request."Email" IS NULL THEN
        RETURN 'Sorry, we could not find a registered user with that email.'::CHARACTER VARYING;
    END IF;

    IF _request."Password" IS NOT NULL AND _request."Password" = "reqNewPassword" THEN
        RETURN 'Your new password should be different from previous ones. Try a different password'::CHARACTER VARYING;
    END IF;

    UPDATE event."UserAccount" SET "Password" = "reqNewPassword" WHERE "Email" = "reqEmail";

    RETURN ''::CHARACTER VARYING;
END
$$;