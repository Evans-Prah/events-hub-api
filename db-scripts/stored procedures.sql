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
    RETURN QUERY SELECT ua."Username",
                        ua."DisplayName",
                        ua."Bio",
                        ua."Image"
                 FROM event."UserAccount" ua
                          LEFT JOIN event."EventAttendees" ea ON ea."UserAccountId" = ua."AccountId"
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
                "Attendees"    JSONB
            )
    LANGUAGE plpgsql
AS
$$
BEGIN
    RETURN QUERY SELECT ev."EventUuid",
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
                          LEFT JOIN event."UserAccount" ua ON ua."AccountId" = ea."UserAccountId"
                 WHERE ev."EventUuid" = "reqEventUuid"
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
                                             CHARACTER VARYING, CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."RegisterUser"("reqAccountUuid" CHARACTER VARYING,
                                     "reqUsername" CHARACTER VARYING,
                                     "reqDisplayName" CHARACTER VARYING,
                                     "reqEmail" CHARACTER VARYING,
                                     "reqPassword" CHARACTER VARYING,
                                     "reqPhoneNumber" CHARACTER VARYING)
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
                                    "IsActive")
    VALUES ("reqAccountUuid", trim("reqUsername"), "reqDisplayName", trim("reqPassword"), "reqEmail", "reqPhoneNumber",
            TRUE);

    RETURN '':: CHARACTER VARYING;
END
$$;


DROP FUNCTION IF EXISTS event."UserLogin"(CHARACTER VARYING, CHARACTER VARYING);
CREATE FUNCTION event."UserLogin"("reqUsername" CHARACTER VARYING, "reqPassword" CHARACTER VARYING)
    RETURNS TABLE
            (
                "ResponseMessage" CHARACTER VARYING,
                "Username"        CHARACTER VARYING,
                "DisplayName"     CHARACTER VARYING,
                "LastLogin"       TIMESTAMP WITHOUT TIME ZONE
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    userRecord RECORD;
BEGIN

    SELECT uc."IsActive",
           uc."AccountId",
           uc."Username",
           uc."DisplayName",
           uc."LastLogin"
    FROM event."UserAccount" uc
    WHERE lower(uc."Username") = lower("reqUsername")
      AND uc."Password" = "reqPassword"
    LIMIT 1
    INTO userRecord;

    IF userRecord."AccountId" IS NULL THEN
        RETURN QUERY SELECT 'Invalid login credentials, check and try again'::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::TIMESTAMP WITHOUT TIME ZONE;
        RETURN;
    END IF;

    IF userRecord."IsActive" = FALSE THEN
        RETURN QUERY SELECT 'Your account is disabled, contact support for further assistance or create a new account'::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::CHARACTER VARYING,
                            NULL::TIMESTAMP WITHOUT TIME ZONE;
        RETURN;
    END IF;

    UPDATE event."UserAccount" uc SET "LastLogin" = NOW() AT TIME ZONE 'UTC' WHERE "AccountId" = userRecord."AccountId";

    RETURN QUERY SELECT ''::CHARACTER VARYING,
                        userRecord."Username",
                        userRecord."DisplayName",
                        userRecord."LastLogin";
END
$$;
