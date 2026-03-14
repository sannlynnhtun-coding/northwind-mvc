CREATE OR ALTER PROCEDURE dbo.usp_DashboardMonthlySales
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MONTH(o.OrderDate) AS SalesMonth,
        CAST(SUM(od.UnitPrice * od.Quantity * (1 - CAST(od.Discount AS DECIMAL(9,4)))) AS DECIMAL(18,2)) AS TotalSales
    FROM Orders o
    INNER JOIN [Order Details] od ON od.OrderID = o.OrderID
    WHERE o.OrderDate IS NOT NULL
      AND YEAR(o.OrderDate) = @Year
    GROUP BY MONTH(o.OrderDate)
    ORDER BY MONTH(o.OrderDate);
END;
