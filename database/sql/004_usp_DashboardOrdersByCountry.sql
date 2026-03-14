CREATE OR ALTER PROCEDURE dbo.usp_DashboardOrdersByCountry
    @Year INT,
    @TopN INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SafeTopN INT = CASE WHEN @TopN < 1 THEN 1 WHEN @TopN > 25 THEN 25 ELSE @TopN END;

    SELECT TOP (@SafeTopN)
        ISNULL(o.ShipCountry, 'Unknown') AS ShipCountry,
        COUNT(*) AS TotalOrders
    FROM Orders o
    WHERE o.OrderDate IS NOT NULL
      AND YEAR(o.OrderDate) = @Year
    GROUP BY o.ShipCountry
    ORDER BY COUNT(*) DESC, ISNULL(o.ShipCountry, 'Unknown');
END;
