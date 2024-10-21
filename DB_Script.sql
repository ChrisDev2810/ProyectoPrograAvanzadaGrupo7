----------------------------DATABASE---------------------

IF NOT EXISTS(SELECT name FROM sys.databases WHERE name = 'WMRDB')
BEGIN
 CREATE DATABASE WMRDB;
END

USE WMRDB;

---------------TABLES----------------------

CREATE TABLE [room] (
  [roomID] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
  [name] VARCHAR(255) NOT NULL,
  [capacity] INT NOT NULL,
  [location] VARCHAR(255) NOT NULL,
  [equipmentID] INT NOT NULL,
  [availability_start] TIME NOT NULL,
  [availability_end] TIME NOT NULL,
  [isActive] BIT NOT NULL
)
GO

CREATE TABLE [user] (
  [userID] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
  [email] VARCHAR(255) NOT NULL,
  [password] VARCHAR(255),
  [isAdmin] BIT NOT NULL
)
GO

CREATE TABLE [equipment] (
  [equipmentID] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
  [equimentDescription] VARCHAR(100) NOT NULL
)
GO

CREATE TABLE [roomsEquipment] (
  [roomsEquipmentID] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
  [roomID] INT NOT NULL,
  [equipmentID] INT NOT NULL
)
GO

CREATE TABLE [UsageStatistic] (
  [statsID] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
  [roomID] INT NOT NULL,
  [DATE] DATE NOT NULL,
  [hoursBooked] DECIMAL (5,2) NOT NULL,
  [porcentageUsed] DECIMAL (5,2) NOT NULL
)
GO

CREATE TABLE [reservation] (
  [reservationID] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
  [roomId] INT NOT NULL,
  [userID] INT NOT NULL,
  [startTime] DATETIME NOT NULL,
  [endtime] DATETIME NOT NULL,
  [statusID] INT NOT NULL
)
GO

CREATE TABLE [status] (
  [statusID] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
  [statusDescription] VARCHAR(100) NOT NULL
)
GO

----------------------CONSTRAINTS----------------------------------------

ALTER TABLE roomsEquipment ADD FOREIGN KEY ([roomID]) REFERENCES [rooms] ([roomID])
GO

ALTER TABLE roomsEquipment ADD FOREIGN KEY ([equipmentID]) REFERENCES [equipment] ([equipmentID])
GO

ALTER TABLE [UsageStatistic] ADD FOREIGN KEY ([roomID]) REFERENCES [rooms] ([roomID])
GO

ALTER TABLE [reservation] ADD FOREIGN KEY ([roomID]) REFERENCES [rooms] ([roomID])
GO

ALTER TABLE [reservation] ADD FOREIGN KEY ([userID]) REFERENCES [users] ([userID])
GO

ALTER TABLE [reservation] ADD FOREIGN KEY ([statusID]) REFERENCES [status] ([statusID])
GO