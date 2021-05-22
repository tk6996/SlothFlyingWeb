using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
using SlothFlyingWeb.Data;
using SlothFlyingWeb.Models;
using SlothFlyingWeb.Utils;

namespace SlothFlyingWeb.Controllers
{
    public class ApiController : Controller
    {
        private readonly ILogger<ApiController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IMemoryCache _cache;
        public ApiController(ILogger<ApiController> logger, ApplicationDbContext db, IMemoryCache cache)
        {
            _logger = logger;
            _db = db;
            _cache = cache;
        }

        public IActionResult GetLab()
        {
            IEnumerable<Lab> labs = _db.Lab;
            
            return Json(labs.Select(lab => new {
                Id = lab.Id,
                ItemName = lab.ItemName,
                ImageUrl = Url.Content(lab.ImageUrl)
            }));
        }


        public async Task<IActionResult> GetBooking(int? id)
        {
            Lab lab = await _db.Lab.FindAsync(id);

            if (lab == null)
            {
                return NotFound();
            }

            DateTime startDate = BangkokDateTime.now().Date;
            DateTime endDate = startDate.AddDays(14).Date;
            DateTime cacheDate;

            Func<DateTime, object, bool> checkDateCurrent = (cacheDate, key) =>
             {
                 if (cacheDate < startDate)
                 {
                     _cache.Remove(key);
                     return true;
                 }
                 return false;
             };

            do
            {
                (lab.BookSlotTable, cacheDate) = await _cache.GetOrCreateAsync<(int[,], DateTime)>($"BookSlotTable_{lab.Id}", entry =>
                {
                    IEnumerable<BookSlot> bookSlots = _db.BookSlot.Where(bookSlot => bookSlot.LabId == lab.Id &&
                                                                                     startDate <= bookSlot.Date && bookSlot.Date < endDate);

                    //Console.WriteLine(bookSlots.Count());

                    int[,] bookSlotTable = new int[9, 14];

                    for (int r = 0; r < 9; r++)
                    {
                        for (int c = 0; c < 14; c++)
                        {
                            bookSlotTable[r, c] = bookSlots.Where(bookSlot => bookSlot.Date == startDate.AddDays(c).Date)
                                                           .Count(bookSlot => bookSlot.TimeSlot == r + 1);
                        }
                    }

                    entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                    return Task.FromResult((bookSlotTable, startDate));
                });
            } while (checkDateCurrent(cacheDate, $"BookSlotTable_{lab.Id}"));

            int[][] result = new int[14][];

            for (int r = 0; r < 14; r++)
            {
                int[] arr = new int[9];
                for (int c = 0; c < 9; c++)
                {
                    arr[c] = lab.Amount > lab.BookSlotTable[c, r] ? lab.Amount - lab.BookSlotTable[c, r] : 0;
                }
                result[r] = arr;
            }

            return Json(new
            {
                Id = lab.Id,
                ItemName = lab.ItemName,
                ImageUrl = Url.Content(lab.ImageUrl),
                StartDate = startDate.ToString("dd-MMM-yyyy"),
                EndDate = endDate.ToString("dd-MMM-yyyy"),
                From = 8,
                To = 17,
                BookSlotTable = result,
                TimeStamp = BangkokDateTime.now().ToString("dd-MMM-yyyy HH:mm:ss")
            });
        }
    }
}