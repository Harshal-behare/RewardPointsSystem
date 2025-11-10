-- =============================================
-- Stored Procedure: Get Top 3 Employees with Highest Rewards
-- Description: Returns the top 3 employees/users with the highest total earned points
-- =============================================

-- Drop procedure if it exists
IF OBJECT_ID('dbo.SP_GetTop3EmployeesByRewards', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_GetTop3EmployeesByRewards;
GO

CREATE PROCEDURE dbo.SP_GetTop3EmployeesByRewards
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Get top 3 employees with highest total earned points
        SELECT TOP 3
            u.Id AS EmployeeId,
            u.FirstName,
            u.LastName,
            u.Email,
            pa.TotalEarned AS TotalRewardPoints,
            pa.CurrentBalance AS CurrentBalance,
            pa.TotalRedeemed AS TotalRedeemed,
            pa.LastUpdatedAt AS LastPointsUpdate,
            u.IsActive AS IsActive,
            -- Calculate total events participated
            (SELECT COUNT(*) 
             FROM dbo.EventParticipants ep 
             WHERE ep.UserId = u.Id) AS TotalEventsParticipated,
            -- Calculate total events won (with points awarded)
            (SELECT COUNT(*) 
             FROM dbo.EventParticipants ep 
             WHERE ep.UserId = u.Id 
             AND ep.PointsAwarded IS NOT NULL 
             AND ep.PointsAwarded > 0) AS TotalEventsWon,
            -- Calculate total redemptions
            (SELECT COUNT(*) 
             FROM dbo.Redemptions r 
             WHERE r.UserId = u.Id) AS TotalRedemptions
        FROM 
            dbo.Users u
        INNER JOIN 
            dbo.PointsAccounts pa ON u.Id = pa.UserId
        WHERE 
            u.IsActive = 1  -- Only active employees
        ORDER BY 
            pa.TotalEarned DESC,  -- Primary sort by total earned points
            pa.CurrentBalance DESC,  -- Secondary sort by current balance
            u.FirstName ASC;  -- Tertiary sort by first name
            
    END TRY
    BEGIN CATCH
        -- Error handling
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

PRINT 'Stored Procedure SP_GetTop3EmployeesByRewards created successfully!';
GO
