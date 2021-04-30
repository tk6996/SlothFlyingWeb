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

        public async Task<IActionResult> GetBooking([FromHeader] string ApiKey)
        {
            ApiUser apiUser = _db.ApiUser.Where(apiUser => apiUser.Enable && apiUser.ApiKey == ApiKey).FirstOrDefault();

            if (apiUser == null)
            {
                return StatusCode(403);
            }

            Lab lab = await _db.Lab.FindAsync(apiUser.LabId);

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

                    IEnumerable<ApiBookSlot> apiBookSlot = _db.ApiBookSlot.Where(bookSlot => bookSlot.LabId == lab.Id &&
                                                                                 startDate <= bookSlot.Date && bookSlot.Date < endDate);
                    for (int r = 0; r < 9; r++)
                    {
                        for (int c = 0; c < 14; c++)
                        {
                            if (bookSlotTable[r, c] < lab.Amount) // bookSlotTable is Full
                            {
                                bookSlotTable[r, c] += apiBookSlot.Where(bookSlot => bookSlot.Date == startDate.AddDays(c).Date)
                                                                  .Count(bookSlot => bookSlot.TimeSlot == r + 1);
                            }
                            // else
                            // {
                            //     // api check amount
                            //     bookSlotTable[r, c] = lab.Amount - 1;
                            // }
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
                    arr[c] = lab.BookSlotTable[c, r];
                }
                result[r] = arr;
            }

            return Json(new
            {
                LabName = lab.ItemName,
                StartDate = startDate.ToString("dd-MMM-yyyy"),
                endDate = endDate.ToString("dd-MMM-yyyy"),
                From = 8,
                To = 17,
                BookSlotTable = result,
                TimeStamp = BangkokDateTime.now().ToString("dd-MMM-yyyy HH:mm:ss")
            });
        }

        [HttpPost]
        public IActionResult SetBooking([FromHeader] string ApiKey, [FromBody] IEnumerable<BookRange> bookRanges)
        {
            ApiUser apiUser = _db.ApiUser.Where(apiUser => apiUser.Enable && apiUser.ApiKey == ApiKey).FirstOrDefault();

            if (apiUser == null)
            {
                return StatusCode(403);
            }

            if (bookRanges == null)
            {
                return BadRequest("Body is not valid.");
            }

            Lab lab = _db.Lab.Find(apiUser.LabId);

            if (lab == null)
            {
                throw new Exception("Lab not found.");
            }

            List<object> bookListResult = new List<object>();
            DateTime startDate = BangkokDateTime.now().Date;
            int[,] BookSlotTable = new int[14, 9];
            lock (BookingLock._lock)
            {

                foreach (BookRange bookRange in bookRanges)
                {
                    if (bookRange.Date == 0 || bookRange.From == 0 || bookRange.To == 0)
                    {
                        // Please complete all field.
                        return BadRequest("Please complete all field.");
                    }

                    DateTime dateValue = BangkokDateTime.millisecondToDateTime(bookRange.Date);
                    int fromValue = bookRange.From;
                    int toValue = bookRange.To;

                    if (dateValue < startDate || dateValue >= startDate.AddDays(14))
                    {
                        // The date is out of the boundary that you can book.
                        return BadRequest("The date is out of the boundary that you can book.");
                    }

                    if (dateValue.DayOfWeek == DayOfWeek.Saturday || dateValue.DayOfWeek == DayOfWeek.Sunday)
                    {
                        // You cannot book an item on Weekend.
                        return BadRequest("You cannot book an item on Weekend.");
                    }

                    if (fromValue >= toValue)
                    {
                        // You entered the wrong period.
                        return BadRequest("You entered the wrong period.");
                    }

                    if (dateValue == startDate && BangkokDateTime.now().Hour > fromValue)
                    {
                        // The time is out of the boundary that you can book.
                        return BadRequest("The time is out of the boundary that you can book.");
                    }

                    for (int slot = fromValue; slot < toValue; slot++)
                    {
                        if (BookSlotTable[(dateValue - startDate).Days, slot - 8] == 1)
                        {
                            // You cannot enter the period that overlaps.
                            return BadRequest("You cannot enter the period that overlaps.");
                        }

                        int amountBooking = _db.BookSlot.Where(bs => bs.LabId == lab.Id && bs.Date == dateValue).Count(bs => bs.TimeSlot == slot - 7);
                        int amountApiBooking = _db.ApiBookSlot.Where(bs => bs.LabId == apiUser.LabId && bs.Date == dateValue).Count(bs => bs.TimeSlot == slot - 7);
                        amountBooking += amountApiBooking;

                        if (amountBooking >= lab.Amount)
                        {
                            // You cannot enter the period that items full.
                            return BadRequest("You cannot enter the period that items full.");
                        }
                        BookSlotTable[(dateValue - startDate).Days, slot - 8] = 1;
                    }
                }

                for (int date = 0; date < 14; date++)
                {
                    int start = 0;
                    int end = 0;
                    for (int timeslot = 0; timeslot < 9; timeslot++)
                    {
                        if (BookSlotTable[date, timeslot] == 1 && start == 0)
                        {
                            start = timeslot + 8;
                        }
                        if (BookSlotTable[date, timeslot] == 1 && start > 0)
                        {
                            end = timeslot + 9;
                        }
                        if (BookSlotTable[date, timeslot] == 0 && start > 0)
                        {
                            ApiBookList apiBookList = new ApiBookList()
                            {
                                ApiUserId = apiUser.Id,
                                LabId = lab.Id,
                                Date = startDate.AddDays(date),
                                From = start,
                                To = end,
                                Status = ApiBookList.StatusType.COMING
                            };
                            _db.ApiBookList.Add(apiBookList);
                            _db.SaveChanges();
                            bookListResult.Add(new
                            {
                                BookListId = apiBookList.Id,
                                Date = apiBookList.Date.ToString("dd-MMM-yyyy"),
                                From = apiBookList.From,
                                To = apiBookList.To,
                            });
                            for (int slot = start; slot < end; slot++)
                            {
                                _db.ApiBookSlot.Add(new ApiBookSlot()
                                {
                                    ApiBookListId = apiBookList.Id,
                                    LabId = lab.Id,
                                    Date = startDate.AddDays(date),
                                    TimeSlot = slot - 7
                                });
                            }
                            _db.SaveChanges();
                            start = end = 0;
                        }
                    }
                    if (start > 0)
                    {
                        ApiBookList apiBookList = new ApiBookList()
                        {
                            ApiUserId = apiUser.Id,
                            LabId = lab.Id,
                            Date = startDate.AddDays(date),
                            From = start,
                            To = end,
                            Status = ApiBookList.StatusType.COMING
                        };
                        _db.ApiBookList.Add(apiBookList);
                        _db.SaveChanges();
                        bookListResult.Add(new
                        {
                            BookListId = apiBookList.Id,
                            Date = apiBookList.Date.ToString("dd-MMM-yyyy"),
                            From = apiBookList.From,
                            To = apiBookList.To,
                        });
                        for (int slot = start; slot < end; slot++)
                        {
                            _db.ApiBookSlot.Add(new ApiBookSlot()
                            {
                                ApiBookListId = apiBookList.Id,
                                LabId = lab.Id,
                                Date = startDate.AddDays(date),
                                TimeSlot = slot - 7
                            });
                        }
                        _db.SaveChanges();
                    }
                }
            }
            _cache.Remove($"BookSlotTable_{lab.Id}");
            return Json(new
            {
                LabName = lab.ItemName,
                bookListResult,
                TimeStamp = BangkokDateTime.now().ToString("dd-MMM-yyyy HH:mm:ss")
            });
        }

        [HttpPost]
        public async Task<IActionResult> CancelBooking([FromHeader] string ApiKey, [FromBody] int? id)
        {
            ApiUser apiUser = _db.ApiUser.Where(apiUser => apiUser.Enable && apiUser.ApiKey == ApiKey).FirstOrDefault();

            if (apiUser == null)
            {
                return StatusCode(403);
            }

            if (id == null || id < 0)
            {
                return BadRequest("Id is not valid.");
            }

            ApiBookList apiBookList = await _db.ApiBookList.FindAsync(id);

            if (apiBookList == null)
            {
                return BadRequest("The Booklist not found.");
            }

            if (apiBookList.ApiUserId != apiUser.Id)
            {
                return BadRequest("You not permission to cancel this booklist.");
            }

            if (apiBookList.Status == ApiBookList.StatusType.FINISHED ||
                apiBookList.Status == ApiBookList.StatusType.CANCEL ||
                apiBookList.Status == ApiBookList.StatusType.EJECT)
            {
                return BadRequest("This booklist can't cancel.");
            }

            apiBookList.Status = ApiBookList.StatusType.CANCEL;
            _db.ApiBookList.Update(apiBookList);
            IEnumerable<ApiBookSlot> apiBookSlots = _db.ApiBookSlot.Where(apiBookSlot => apiBookSlot.ApiBookListId == apiBookList.Id);
            foreach (ApiBookSlot apiBookSlot in apiBookSlots)
            {
                _db.ApiBookSlot.Remove(apiBookSlot);
            }
            await _db.SaveChangesAsync();

            _cache.Remove($"BookSlotTable_{apiBookList.LabId}");
            return Json(new
            {
                ItemName = apiBookList.LabId,
                Date = apiBookList.Date.ToString("dd-MMM-yyyy"),
                From = apiBookList.From,
                To = apiBookList.To,
                Status = apiBookList.Status,
                TimeStamp = BangkokDateTime.now().ToString("dd-MMM-yyyy HH:mm:ss")
            });
        }

        public async Task<IActionResult> GetBookList([FromHeader] string ApiKey)
        {
            ApiUser apiUser = _db.ApiUser.Where(apiUser => apiUser.Enable && apiUser.ApiKey == ApiKey).FirstOrDefault();

            if (apiUser == null)
            {
                return StatusCode(403);
            }

            DateTime dateNow = BangkokDateTime.now();

            Func<ApiBookList, Lab, ApiBookList> joinItemName = (apiBookList, lab) =>
              {
                  apiBookList.ItemName = lab.ItemName;
                  return apiBookList;
              };

            IEnumerable<ApiBookList> apiBookLists = _db.ApiBookList.Where(apiBookList => apiBookList.ApiUserId == apiUser.Id)
                                                                   .Join(_db.Lab,
                                                                         apiBookList => apiBookList.LabId,
                                                                         lab => lab.Id,
                                                                         joinItemName);

            foreach (ApiBookList apiBookList in apiBookLists)
            {
                if (apiBookList.Status == ApiBookList.StatusType.COMING)
                {
                    if (apiBookList.Date.AddHours(apiBookList.From) <= dateNow && dateNow < apiBookList.Date.AddHours(apiBookList.To))
                    {
                        apiBookList.Status = ApiBookList.StatusType.USING;
                        _db.ApiBookList.Update(apiBookList);
                    }
                    else if (dateNow >= apiBookList.Date.AddHours(apiBookList.To))
                    {
                        apiBookList.Status = ApiBookList.StatusType.FINISHED;
                        _db.ApiBookList.Update(apiBookList);
                    }
                }
                if (apiBookList.Status == ApiBookList.StatusType.USING)
                {
                    if (dateNow >= apiBookList.Date.AddHours(apiBookList.To))
                    {
                        apiBookList.Status = ApiBookList.StatusType.FINISHED;
                        _db.ApiBookList.Update(apiBookList);
                    }
                }
            }
            await _db.SaveChangesAsync();

            IEnumerable<ApiBookList> apibl = apiBookLists.OrderBy(bl => bl.Status)
                                                  .ThenByDescending(bl => bl.Date)
                                                  .ThenBy(bl => bl.From)
                                                  .ThenBy(bl => bl.To)
                                                  .ThenBy(bl => bl.LabId)
                                                  .ToList();

            return Json(new
            {
                BookLists = apibl.Select(bl => new
                {
                    Id = bl.Id,
                    LabName = bl.ItemName,
                    Date = bl.Date.ToString("dd-MMM-yyyy"),
                    From = bl.From,
                    To = bl.To,
                    Status = bl.Status.ToString()
                }),
                TimeSpan = BangkokDateTime.now().ToString("dd-MMM-yyyy HH:mm:ss")
            });
        }
    }
}