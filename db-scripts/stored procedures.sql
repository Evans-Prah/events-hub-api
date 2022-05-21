DROP FUNCTION IF EXISTS event."GetEvents"();
CREATE FUNCTION event."GetEvents"()
    RETURNS TABLE
            (
                "EventUuid"   CHARACTER VARYING,
                "Title"       CHARACTER VARYING,
                "Description" CHARACTER VARYING,
                "Category"    CHARACTER VARYING,
                "City"        CHARACTER VARYING,
                "Venue"       CHARACTER VARYING,
                "Date"        TIMESTAMP WITHOUT TIME ZONE
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
                        ev."Date"
                 FROM event."Events" ev;
END
$$;


DROP FUNCTION IF EXISTS event."GetEventDetails"(CHARACTER VARYING);
CREATE FUNCTION event."GetEventDetails"("reqEventUuid" CHARACTER VARYING)
    RETURNS TABLE
            (
                "EventUuid"   CHARACTER VARYING,
                "Title"       CHARACTER VARYING,
                "Description" CHARACTER VARYING,
                "Category"    CHARACTER VARYING,
                "City"        CHARACTER VARYING,
                "Venue"       CHARACTER VARYING,
                "Date"        TIMESTAMP WITHOUT TIME ZONE
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
                        ev."Date"
                 FROM event."Events" ev
                 WHERE ev."EventUuid" = "reqEventUuid"
                 LIMIT 1;
END
$$;


DROP FUNCTION IF EXISTS event."CreateEvent"(CHARACTER VARYING, CHARACTER VARYING, CHARACTER VARYING, CHARACTER VARYING,
                                            CHARACTER VARYING, CHARACTER VARYING, TIMESTAMP WITHOUT TIME ZONE);
CREATE FUNCTION event."CreateEvent"("reqEventUuid" CHARACTER VARYING, "reqTitle" CHARACTER VARYING,
                                    "reqDescription" CHARACTER VARYING, "reqCategory" CHARACTER VARYING,
                                    "reqCity" CHARACTER VARYING, "reqVenue" CHARACTER VARYING,
                                    "reqDate" TIMESTAMP WITHOUT TIME ZONE)
    RETURNS CHARACTER VARYING
    LANGUAGE plpgsql
AS
$$
BEGIN
    INSERT INTO event."Events"("EventUuid", "Title", "Description", "Category", "City", "Venue", "Date")
    VALUES ("reqEventUuid", "reqTitle", "reqDescription", "reqCategory", "reqCity", "reqVenue", "reqDate");

    RETURN 'Event created successfully'::CHARACTER VARYING;
END
$$;


DROP FUNCTION IF EXISTS event."UpdateEvent"(CHARACTER VARYING, CHARACTER VARYING, CHARACTER VARYING, CHARACTER VARYING,
                                            CHARACTER VARYING, CHARACTER VARYING, TIMESTAMP WITHOUT TIME ZONE);
CREATE FUNCTION event."UpdateEvent"("reqEventUuid" CHARACTER VARYING, "reqTitle" CHARACTER VARYING,
                                    "reqDescription" CHARACTER VARYING, "reqCategory" CHARACTER VARYING,
                                    "reqCity" CHARACTER VARYING, "reqVenue" CHARACTER VARYING,
                                    "reqDate" TIMESTAMP WITHOUT TIME ZONE)
    RETURNS CHARACTER VARYING
    LANGUAGE plpgsql
AS
$$
DECLARE
    _event_id INTEGER;
BEGIN

    SELECT ev."Id"::INTEGER INTO _event_id FROM event."Events" ev WHERE ev."EventUuid" = "reqEventUuid" LIMIT 1;

    IF _event_id IS NULL THEN
        RETURN 'Event does not exist, check and try again'::CHARACTER VARYING;
    END IF;

    UPDATE event."Events"
    SET "Title"        = "reqTitle",
        "Description"  = "reqDescription",
        "Category"     ="reqCategory",
        "City"         = "reqCity",
        "Venue"        = "reqVenue",
        "Date"         = "reqDate",
        "DateModified" = NOW() AT TIME ZONE 'UTC'
    WHERE "EventUuid" = "reqEventUuid";

    RETURN ''::CHARACTER VARYING;
END
$$;


DROP FUNCTION IF EXISTS event."DeleteEvent"(CHARACTER VARYING);
CREATE FUNCTION event."DeleteEvent"("reqEventUuid" CHARACTER VARYING)
    RETURNS CHARACTER VARYING
    LANGUAGE plpgsql
AS
$$
DECLARE
    _event_id INTEGER;
BEGIN

    SELECT ev."Id"::INTEGER INTO _event_id FROM event."Events" ev WHERE ev."EventUuid" = "reqEventUuid" LIMIT 1;

    IF _event_id IS NULL THEN
        RETURN 'Event does not exist, check and try again'::CHARACTER VARYING;
    END IF;

    DELETE FROM event."Events" WHERE "EventUuid" = "reqEventUuid";

    RETURN ''::CHARACTER VARYING;
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