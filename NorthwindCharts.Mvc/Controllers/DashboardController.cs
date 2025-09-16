using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindCharts.Database.AppDbContextModels;
using NorthwindCharts.Mvc.Dtos;

namespace NorthwindCharts.Mvc.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetBestSellingProducts()
        {
            // ရောင်းရအရေအတွက်အလိုက် အရောင်းရဆုံး product ၅ ခုကို ဆွဲထုတ်မယ့် Raw SQL Query ပါ။
            var sqlQuery = @"
                SELECT TOP 5
                    P.ProductName,
                    SUM(OD.Quantity) AS TotalQuantity
                FROM
                    [Order Details] OD
                JOIN
                    Products P ON OD.ProductID = P.ProductID
                GROUP BY
                    P.ProductName
                ORDER BY
                    TotalQuantity DESC";

            // FromSqlRaw ကိုသုံးပြီး Query ကရလာတဲ့ Data တွေကို ကျွန်တော်တို့ရဲ့ DTO ထဲကို ထည့်ပါမယ်။
            var topProducts = await _db.Database
                                       .SqlQueryRaw<BestSellingProductDto>(sqlQuery)
                                       .ToListAsync();

            return Json(topProducts);
        }
    }
}
