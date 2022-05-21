DROP TABLE IF EXISTS event."Events";
CREATE TABLE event."Events"
(
    "Id"           BIGSERIAL PRIMARY KEY NOT NULL,
    "EventUuid"    CHARACTER VARYING,
    "Title"        CHARACTER VARYING,
    "Description"  CHARACTER VARYING,
    "Category"     CHARACTER VARYING,
    "City"         CHARACTER VARYING,
    "Venue"        CHARACTER VARYING,
    "Date"         TIMESTAMP WITHOUT TIME ZONE,
    "DateCreated"  TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'UTC'),
    "DateModified" TIMESTAMP WITHOUT TIME ZONE
);


DROP TABLE IF EXISTS event."UserAccount";
CREATE TABLE event."UserAccount"
(
    "AccountId"   BIGSERIAL PRIMARY KEY NOT NULL,
    "AccountUuid" CHARACTER VARYING     NOT NULL,
    "Username"    CHARACTER VARYING     NOT NULL,
    "DisplayName" CHARACTER VARYING     NOT NULL,
    "Password"    CHARACTER VARYING     NOT NULL,
    "Email"       CHARACTER VARYING     NOT NULL,
    "PhoneNumber" CHARACTER VARYING,
    "Bio"         CHARACTER VARYING,
    "LastLogin"   TIMESTAMP WITHOUT TIME ZONE,
    "IsActive"    BOOLEAN                     DEFAULT TRUE,
    "DateCreated" TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'UTC')
);