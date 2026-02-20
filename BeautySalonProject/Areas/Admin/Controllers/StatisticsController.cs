using BeautySalonProject.Areas.Admin.ViewModels.Statistics;
using BeautySalonProject.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautySalonProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public StatisticsController(ApplicationDbContext db)
        {
            _db = db;
        }
        [HttpGet]
        public async Task<IActionResult> Index(DateTime? from, DateTime? to)
        {
            var toDate = (to ?? DateTime.Today).Date;
            var fromDate = (from ?? toDate.AddDays(-29)).Date;

            if (fromDate > toDate)
            {
                (fromDate, toDate) = (toDate, fromDate);
            }

            var start = fromDate;
            var endExclusive = toDate.AddDays(1);

            const byte Completed = 2;
            var completedQ = _db.Appointments
                .AsNoTracking()
                .Include(a => a.Variant)
                .ThenInclude(v => v.Service)
                .Where(a => a.Status == Completed &&
                            a.StartAt >= start &&
                            a.StartAt < endExclusive);

            var visits = await completedQ.CountAsync();

            var revenue = await completedQ.SumAsync(a => (decimal?)a.FinalPrice) ?? 0m;

            var uniqueClients = await completedQ
               .Select(a => !string.IsNullOrEmpty(a.ClientUserId)
               ? a.ClientUserId
               : (a.GuestEmail ?? a.GuestPhone ?? a.GuestFullName ?? "guest"))
               .Distinct()
               .CountAsync();

            var perDay = await completedQ
               .GroupBy(a => a.StartAt.Date)
               .Select(g => new
               {
                   Day = g.Key,
                   Visits = g.Count(),
                   Revenue = g.Sum(x => x.FinalPrice)
               })
               .ToListAsync();

            var topServices = await completedQ
               .GroupBy(a => a.Variant.Service.Name)
               .Select(g => new AdminStatisticsVm.TopServiceRow
               {
                   ServiceName = g.Key,
                   Count = g.Count(),
                   Revenue = g.Sum(x => x.FinalPrice)
               })
               .OrderByDescending(x => x.Count)
               .ThenByDescending(x => x.Revenue)
               .Take(10)
               .ToListAsync();

            var labels = new List<string>();
            var visitsSeries = new List<int>();
            var revenueSeries = new List<decimal>();

            var map = perDay.ToDictionary(x => x.Day, x => x);

            for (var d = fromDate; d <= toDate; d = d.AddDays(1))
            {
                labels.Add(d.ToString("dd.MM"));
                if (map.TryGetValue(d, out var row))
                {
                    visitsSeries.Add(row.Visits);
                    revenueSeries.Add(row.Revenue);
                }
                else
                {
                    visitsSeries.Add(0);
                    revenueSeries.Add(0m);
                }
            }

            var vm = new AdminStatisticsVm
            {
                From = fromDate,
                To = toDate,

                VisitsCompleted = visits,
                RevenueCompleted = revenue,
                UniqueClientsCompleted = uniqueClients,

                Labels = labels,
                VisitsPerDay = visitsSeries,
                RevenuePerDay = revenueSeries,

                TopServices = topServices
            };

            return View(vm);
        }
    }
}
