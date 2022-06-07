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
    "IsCancelled"  BOOLEAN                     DEFAULT FALSE,
    "HostId"       INTEGER,
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


DROP TABLE IF EXISTS event."EventAttendees";
CREATE TABLE event."EventAttendees"
(
    "Id"            BIGSERIAL PRIMARY KEY NOT NULL,
    "UserAccountId" INTEGER,
    "EventId"       INTEGER,
    "IsHost"        BOOLEAN DEFAULT FALSE
);

DROP TABLE IF EXISTS event."Photos";
CREATE TABLE event."Photos"
(
    "PublicId"      CHARACTER VARYING PRIMARY KEY,
    "UserAccountId" INTEGER,
    "Url"           CHARACTER VARYING,
    "IsMain"        BOOLEAN DEFAULT FALSE
);

DROP TABLE IF EXISTS event."EventComments";
CREATE TABLE event."EventComments"
(
    "CommentId"     BIGSERIAL PRIMARY KEY NOT NULL,
    "UserAccountId" INTEGER,
    "EventId"       INTEGER,
    "Comment"       CHARACTER VARYING,
    "DateCreated"   TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'UTC')
);

DROP TABLE IF EXISTS event."UserFollowings";
CREATE TABLE event."UserFollowings"
(
    "Id"                BIGSERIAL PRIMARY KEY NOT NULL,
    "ObserverAccountId" INTEGER,
    "TargetAccountId"   INTEGER,
    "Status"            BOOLEAN DEFAULT FALSE
);

DROP TABLE IF EXISTS event."EventLikes";
CREATE TABLE event."EventLikes"
(
    "Id"            BIGSERIAL PRIMARY KEY NOT NULL,
    "UserAccountId" INTEGER,
    "EventId"       INTEGER,
    "Like"          BOOLEAN DEFAULT FALSE,
    "DateCreated"   TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'UTC')
);