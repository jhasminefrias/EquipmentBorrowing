
-- ============================================================================
-- ITSD 81: Laboratory Activity 3 - Basic SQL Demonstration
-- Campus Equipment Borrowing System (SQLite)
-- ============================================================================

-- 1. Basic Retrieval: Retrieve all equipment records
SELECT
    Id,
    AssetTag,
    Name,
    IsAvailable
FROM Equipment;

-- 2. Filtering: Retrieve only currently available equipment
SELECT
    Id,
    AssetTag,
    Name,
    IsAvailable
FROM Equipment
WHERE IsAvailable = 1;

-- 3. Join: Retrieve active borrowings with student and equipment information
SELECT
    s.FullName AS Student,
    s.StudentNumber AS [Student ID],
    e.Name AS Equipment,
    e.AssetTag AS [Asset Tag],
    b.DateBorrowed AS [Date Borrowed],
    b.ExpectedReturnDate AS [Due Date]
FROM Borrowings b
INNER JOIN Students s ON b.StudentId = s.Id
INNER JOIN Equipment e ON b.EquipmentId = e.Id
WHERE b.Status = 0; -- 0 represents BorrowingStatus.Active

-- 4. Aggregate Query: Count of active borrowings per student
SELECT
    s.Id AS StudentId,
    s.FullName AS StudentName,
    s.MaximumActiveBorrowings AS BorrowingLimit,
    COUNT(b.Id) AS ActiveBorrowingsCount
FROM Students s
LEFT JOIN Borrowings b 
    ON s.Id = b.StudentId AND b.Status = 0
GROUP BY s.Id, s.FullName, s.MaximumActiveBorrowings;

-- Additional Aggregate: Total available vs borrowed equipment count
SELECT
    CASE WHEN IsAvailable = 1 THEN 'Available' ELSE 'Borrowed' END AS [Status],
    COUNT(*) AS [Total Items]
FROM Equipment
GROUP BY IsAvailable;

-- 5. Update: Update availability of an equipment item when returned
UPDATE Equipment
SET IsAvailable = 1
WHERE Id = 2;

-- Update: Mark a borrowing record as returned with return date
UPDATE Borrowings
SET Status = 1, -- 1 represents BorrowingStatus.Returned
    DateReturned = '2026-09-26'
WHERE Id = 1;
