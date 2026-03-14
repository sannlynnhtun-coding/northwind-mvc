CREATE OR ALTER PROCEDURE dbo.usp_DashboardTopProducts
    @TopN INT = 5
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SafeTopN INT = CASE WHEN @TopN < 1 THEN 1 WHEN @TopN > 25 THEN 25 ELSE @TopN END;

    SELECT TOP (@SafeTopN)
        p.ProductName,
        SUM(od.Quantity) AS TotalQuantity
    FROM [Order Details] od
    INNER JOIN Products p ON p.ProductID = od.ProductID
    GROUP BY p.ProductName
    ORDER BY SUM(od.Quantity) DESC, p.ProductName;
END;
