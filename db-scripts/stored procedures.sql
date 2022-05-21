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