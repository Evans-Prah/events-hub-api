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