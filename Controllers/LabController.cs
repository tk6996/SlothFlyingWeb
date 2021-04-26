using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SlothFlyingWeb.Data;
using SlothFlyingWeb.Models;
using SlothFlyingWeb.Utils;

namespace SlothFlyingWeb.Controllers
{
    public class LabController : Controller
    {
        private readonly ILogger<LabController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IMemoryCache _cache;
        public LabController(ILogger<LabController> logger, ApplicationDbContext db, IMemoryCache cache)
        {
            _logger = logger;
            _db = db;
            _cache = cache;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "User");
            }
            IEnumerable<Lab> labs = _db.Lab;
            return View(labs);
        }

        public async Task<IActionResult> Booking(int? id)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "User");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            Lab lab = await _db.Lab.FindAsync(id);

            if (lab == null)
            {
                return NotFound();
            }

            int userId = (int)HttpContext.Session.GetInt32("Id");

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
            } while (checkDateCurrent(cacheDate, $"BookSlotTable_{id}"));

            int[,] userBooked;
            do
            {
                (userBooked, cacheDate) = await _cache.GetOrCreateAsync<(int[,], DateTime)>($"UserBooked_{lab.Id}_{userId}", entry =>
                {
                    IEnumerable<BookList> bookLists = _db.BookList.Where(bl => bl.UserId == userId &&
                                                                               bl.LabId == lab.Id &&
                                                                               startDate <= bl.Date && bl.Date < endDate &&
                                                                               bl.Status != BookList.StatusType.CANCEL && bl.Status != BookList.StatusType.EJECT);

                    int[,] booked = new int[9, 14];
                    foreach (BookList bl in bookLists)
                    {
                        for (int ts = bl.From; ts < bl.To; ts++)
                        {
                            booked[ts - 8, (bl.Date.Date - startDate.Date).Days] = 1;
                        }
                    }

                    entry.SlidingExpiration = TimeSpan.FromMinutes(1);
                    return Task.FromResult((booked, startDate));
                });
            } while (checkDateCurrent(cacheDate, $"UserBooked_{lab.Id}_{userId}"));

            User user = await _db.User.FindAsync(userId);

            ViewBag.userBlacklist = user.BlackList;
            ViewBag.userBooked = userBooked;
            ViewBag.startDate = startDate;
            return View(lab);
        }

        // API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Booking(int id, [FromBody] IEnumerable<BookRange> bookRanges)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                // "Your Session has Expired"
                return Unauthorized();
            }

            int userId = (int)HttpContext.Session.GetInt32("Id");
            int labId = id;
            DateTime startDate = BangkokDateTime.now().Date;
            int[,] BookSlotTable = new int[14, 9];
            lock (BookingLock._lock)
            {
                IEnumerable<BookList> bookLists = _db.BookList.Where(bl => bl.UserId == userId &&
                                                                           bl.LabId == id &&
                                                                           startDate.Date <= bl.Date && bl.Date < startDate.AddDays(14).Date &&
                                                                           bl.Status != BookList.StatusType.CANCEL && bl.Status != BookList.StatusType.EJECT);

                int[,] userBooked = new int[14, 9];
                foreach (BookList bl in bookLists)
                {
                    for (int ts = bl.From; ts < bl.To; ts++)
                    {
                        userBooked[(bl.Date.Date - startDate.Date).Days, ts - 8] = 1;
                    }
                }

                foreach (BookRange bookRange in bookRanges)
                {
                    if (bookRange.Date == 0 || bookRange.From == 0 || bookRange.To == 0)
                    {
                        // Please complete all field.
                        return BadRequest();
                    }

                    DateTime dateValue = BangkokDateTime.millisecondToDateTime(bookRange.Date);
                    int fromValue = bookRange.From;
                    int toValue = bookRange.To;

                    if (dateValue < startDate || dateValue >= startDate.AddDays(14))
                    {
                        // The date is out of the boundary that you can book.
                        return BadRequest();
                    }

                    if (dateValue.DayOfWeek == DayOfWeek.Saturday || dateValue.DayOfWeek == DayOfWeek.Sunday)
                    {
                        // You cannot book an item on Weekend.
                        return BadRequest();
                    }

                    if (fromValue >= toValue)
                    {
                        // You entered the wrong period.
                        return BadRequest();
                    }

                    if (dateValue == startDate && BangkokDateTime.now().Hour > fromValue)
                    {
                        // The time is out of the boundary that you can book.
                        return BadRequest();
                    }

                    for (int slot = fromValue; slot < toValue; slot++)
                    {
                        if (BookSlotTable[(dateValue - startDate).Days, slot - 8] == 1)
                        {
                            // You cannot enter the period that overlaps.
                            return BadRequest();
                        }
                        if (userBooked[(dateValue - startDate).Days, slot - 8] == 1)
                        {
                            // You have already booked this period.
                            return BadRequest();
                        }
                        if (_db.BookSlot.Where(bs => bs.LabId == labId).Where(bs => bs.Date == dateValue).Count(bs => bs.TimeSlot == slot - 7) >= _db.Lab.Find(labId).Amount)
                        {
                            // You cannot enter the period that items full.
                            return BadRequest();
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
                            BookList bl = new BookList()
                            {
                                UserId = userId,
                                LabId = labId,
                                Date = startDate.AddDays(date),
                                From = start,
                                To = end,
                                Status = BookList.StatusType.COMING
                            };
                            _db.BookList.Add(bl);
                            _db.SaveChanges();
                            for (int slot = start; slot < end; slot++)
                            {
                                _db.BookSlot.Add(new BookSlot()
                                {
                                    BookListId = bl.Id,
                                    LabId = labId,
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
                        BookList bl = new BookList()
                        {
                            UserId = userId,
                            LabId = labId,
                            Date = startDate.AddDays(date),
                            From = start,
                            To = end,
                            Status = BookList.StatusType.COMING
                        };
                        _db.BookList.Add(bl);
                        _db.SaveChanges();
                        for (int slot = start; slot < end; slot++)
                        {
                            _db.BookSlot.Add(new BookSlot()
                            {
                                BookListId = bl.Id,
                                LabId = labId,
                                Date = startDate.AddDays(date),
                                TimeSlot = slot - 7
                            });
                        }
                        _db.SaveChanges();
                    }
                }
            }
            _cache.Remove($"BookSlotTable_{labId}");
            _cache.Remove($"UserBooked_{labId}_{userId}");
            return Ok("Ok");
        }
    }
}