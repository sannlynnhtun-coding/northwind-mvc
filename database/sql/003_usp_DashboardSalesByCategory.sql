CREATE OR ALTER PROCEDURE dbo.usp_DashboardSalesByCategory
    @Year INT,
    @TopN INT = 8
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SafeTopN INT = CASE WHEN @TopN < 1 THEN 1 WHEN @TopN > 25 THEN 25 ELSE @TopN END;

    SELECT TOP (@SafeTopN)
        c.CategoryName,
        CAST(SUM(od.UnitPrice * od.Quantity * (1 - CAST(od.Discount AS DECIMAL(9,4)))) AS DECIMAL(18,2)) AS TotalSales
    FROM Orders o
    INNER JOIN [Order Details] od ON od.OrderID = o.OrderID
    INNER JOIN Products p ON p.ProductID = od.ProductID
    LEFT JOIN Categories c ON c.CategoryID = p.CategoryID
    WHERE o.OrderDate IS NOT NULL
      AND YEAR(o.OrderDate) = @Year
    GROUP BY c.CategoryName
    ORDER BY TotalSales DESC, c.CategoryName;
END;
