-- Create database
CREATE DATABASE ForexDB;
GO

-- Use the database
USE ForexDB;
GO

-- Create table for storing transaction data
CREATE TABLE Transactions (
    Id INT PRIMARY KEY IDENTITY,
    Date DATE,
    Currency NVARCHAR(3),
    TransactionType NVARCHAR(4),
    Amount DECIMAL(18, 2)
);

-- Insert data 
