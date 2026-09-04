-- ============================================================================
-- Course: PROG6212 - Programming 2B | PoE Part 1
-- File: docs/RaceDay_Database.sql
-- Description: SQL script for the RaceDay database and seed data.
-- ===========================================================================+

USE master;
GO


IF EXISTS (SELECT name FROM sys.databases WHERE name = 'RaceDayDB')
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END
GO

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

-- ============================================================================
--  CREATE TABLES & DEFINING KEYS AND CONSTRAINTS
-- ============================================================================

-- 1. Organisers Table
CREATE TABLE Organisers (
    OrganisersId VARCHAR(50) NOT NULL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    Surname VARCHAR(100) NOT NULL
);

-- 2. Participants Table
CREATE TABLE Participants (
    participantsId VARCHAR(50) NOT NULL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Surname VARCHAR(100) NOT NULL,
    gender CHAR(1) NOT NULL CHECK (gender IN ('M', 'F', 'O')),
    Age VARCHAR(10) NOT NULL
);

-- 3. Categories Table
CREATE TABLE Categories (
    categoriesId VARCHAR(50) NOT NULL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(255) NOT NULL
);

-- 4. Route Table
CREATE TABLE Route (
    routeId VARCHAR(50) NOT NULL PRIMARY KEY,
    routeName VARCHAR(100) NOT NULL,
    duration INT NOT NULL, -- duration in minutes
    startlocation VARCHAR(150) NOT NULL,
    endlocation VARCHAR(150) NOT NULL,
    Description VARCHAR(255) NOT NULL,
    workspaceId VARCHAR(50) NOT NULL
);

-- 5. Events Table
CREATE TABLE Events (
    eventsId VARCHAR(50) NOT NULL PRIMARY KEY,
    cventsName VARCHAR(150) NOT NULL,
    categoriesId VARCHAR(50) NOT NULL,
    routeId VARCHAR(50) NOT NULL,
    teamId VARCHAR(50) NOT NULL,
    OrganisersId VARCHAR(50) NOT NULL,
    CONSTRAINT FK_Events_Categories FOREIGN KEY (categoriesId) REFERENCES Categories(categoriesId),
    CONSTRAINT FK_Events_Route FOREIGN KEY (routeId) REFERENCES Route(routeId),
    CONSTRAINT FK_Events_Organisers FOREIGN KEY (OrganisersId) REFERENCES Organisers(OrganisersId)
);

-- 6. Results Table (Represents Event Enrolments & Race Completion Details)
CREATE TABLE results (
    resultsId VARCHAR(50) NOT NULL PRIMARY KEY,
    eventsId VARCHAR(50) NOT NULL,
    participantsId VARCHAR(50) NOT NULL,
    finishTime DATETIME2 NULL, -- NULL until participant finishes race
    CONSTRAINT FK_Results_Events FOREIGN KEY (eventsId) REFERENCES Events(eventsId) ON DELETE CASCADE,
    CONSTRAINT FK_Results_Participants FOREIGN KEY (participantsId) REFERENCES Participants(participantsId) ON DELETE CASCADE,
    CONSTRAINT UQ_Event_Participant UNIQUE (eventsId, participantsId)
);
GO

-- ============================================================================
-- STEP 3: ADD SEED DATA
-- ============================================================================

-- Seed Organisers (Minimum 2)
INSERT INTO Organisers (OrganisersId, name, Surname) VALUES
('ORG-101', 'Sipho', 'Dlamini'),
('ORG-102', 'Jessica', 'Van Zyl');

-- Seed Participants (Minimum 2)
INSERT INTO Participants (participantsId, Name, Surname, gender, Age) VALUES
('PAR-201', 'Lethabo', 'Mokoena', 'M', '24'),
('PAR-202', 'Sarah', 'Jenkins', 'F', '29'),
('PAR-203', 'David', 'Naidoo', 'M', '31');

-- Seed Categories
INSERT INTO Categories (categoriesId, name, description) VALUES
('CAT-301', 'Marathon', 'Full distance 42.2km endurance road running event.'),
('CAT-302', 'Half Marathon', '21.1km distance road race for intermediate runners.'),
('CAT-303', 'Trail Run', 'Off-road 15km mountain trail run.');

-- Seed Routes
INSERT INTO Route (routeId, routeName, duration, startlocation, endlocation, Description, workspaceId) VALUES
('RTE-401', 'Coastal Sprint', 180, 'Camps Bay Beach', 'V&A Waterfront', 'Scenic coastal road along Victoria Road', 'WS-801'),
('RTE-402', 'Mountain Ridge Trail', 120, 'Kirstenbosch Gate 2', 'Constantia Nek', 'Challenging uphill mountain trail route', 'WS-802'),
('RTE-403', 'City Loop', 90, 'Green Point Park', 'Green Point Park', 'Flat circular city loop through Urban Park', 'WS-803');

-- Seed Events (Minimum 3)
INSERT INTO Events (eventsId, cventsName, categoriesId, routeId, teamId, OrganisersId) VALUES
('EVT-501', 'Cape Town Marathon 2026', 'CAT-301', 'RTE-401', 'TEAM-ALPHA', 'ORG-101'),
('EVT-502', 'Table Mountain Trail Challenge', 'CAT-303', 'RTE-402', 'TEAM-BRAVO', 'ORG-101'),
('EVT-503', 'Green Point 10k City Dash', 'CAT-302', 'RTE-403', 'TEAM-CHARLIE', 'ORG-102');

-- Seed Sample Enrolments and Results
INSERT INTO results (resultsId, eventsId, participantsId, finishTime) VALUES
('RES-601', 'EVT-501', 'PAR-201', '2026-09-04 10:42:15'),
('RES-602', 'EVT-501', 'PAR-202', '2026-09-04 11:15:30'),
('RES-603', 'EVT-502', 'PAR-202', '2026-09-04 12:05:00'),
('RES-604', 'EVT-503', 'PAR-203', NULL); -- Enrolled, but has not completed/finished race yet
GO

-- ============================================================================
-- VERIFICATION QUERIES
-- ============================================================================

SELECT 'Organisers' AS Entity, COUNT(*) AS RecordCount FROM Organisers
UNION ALL
SELECT 'Participants', COUNT(*) FROM Participants
UNION ALL
SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL
SELECT 'Route', COUNT(*) FROM Route
UNION ALL
SELECT 'Events', COUNT(*) FROM Events
UNION ALL
SELECT 'results (Enrolments)', COUNT(*) FROM results;
GO
