DROP TABLE IF EXISTS event."Events";
CREATE TABLE event."Events"
(
    "Id"          BIGSERIAL PRIMARY KEY NOT NULL,
    "EventUuid"   CHARACTER VARYING,
    "Title"       CHARACTER VARYING,
    "Description" CHARACTER VARYING,
    "Category"    CHARACTER VARYING,
    "City"        CHARACTER VARYING,
    "Venue"       CHARACTER VARYING,
    "Date"        TIMESTAMP WITHOUT TIME ZONE
);