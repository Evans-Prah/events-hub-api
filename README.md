This is an Event app where users can create and post events for other users to attend.
Users can view other people attending a particular event.Users can also follow other users on the app.
The API is built with Miscrosoft .NET 6. The project uses stored procedures implementation.
The Entities project contains the application models. The DBHelper project contains the connection to the database and all
relevent stored procedures implementations in C#. The Services project contains some business related logic and it
also serves as the mediator for the DBHelper and the API project.
The database used in this project is PostgreSQL. I have included the database tables and stored procedures scripts needed to run this project.
