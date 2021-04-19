using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
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
        public LabController(ILogger<LabController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "Id") == null)
            {
                return RedirectToAction("Login", "User");
            }
            IEnumerable<Lab> labs = _db.Lab;
            return View(labs);
        }

        public async Task<IActionResult> Booking(int? id)
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "Id") == null)
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

            int userId = (int)SessionExtensions.GetInt32(HttpContext.Session, "Id");

            DateTime startDate = BangkokDateTime.now().Date;
            DateTime endDate = startDate.AddDays(14).Date;

            IEnumerable<BookSlot> bookSlots = _db.BookSlot.Where(bookSlot => bookSlot.LabId == lab.Id &&
                                                                             startDate <= bookSlot.Date && bookSlot.Date < endDate);

            //Console.WriteLine(bookSlots.Count());

            lab.BookSlotTable = new int[9, 14];

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 14; c++)
                {
                    lab.BookSlotTable[r, c] = lab.Amount - bookSlots.Where(bookSlot => bookSlot.Date == startDate.AddDays(c).Date)
                                                                    .Count(bookSlot => bookSlot.TimeSlot == r + 1);
                }
            }

            IEnumerable<BookList> bookLists = _db.BookList.Where(bl => bl.UserId == userId &&
                                                                       bl.LabId == lab.Id &&
                                                                       startDate <= bl.Date && bl.Date < endDate &&
                                                                       bl.Status != BookList.StatusType.CANCEL && bl.Status != BookList.StatusType.EJECT);

            int[,] userBooked = new int[9, 14];
            foreach (BookList bl in bookLists)
            {
                for (int ts = bl.From; ts < bl.To; ts++)
                {
                    userBooked[ts - 8, (bl.Date.Date - startDate.Date).Days] = 1;
                }
            }

            User user = await _db.User.FindAsync(userId);

            ViewBag.userBlacklist = user.BlackList;
            ViewBag.userBooked = userBooked;
            ViewBag.startDate = startDate.Date;
            return View(lab);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Booking(int id, [FromBody] IEnumerable<BookRange> bookRanges)
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "Id") == null)
            {
                return BadRequest("Your Session has Expired");
            }

            int userId = (int)SessionExtensions.GetInt32(HttpContext.Session, "Id");
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
                        return BadRequest("Please complete all field.");
                    }

                    DateTime dateValue = BangkokDateTime.millisecondToDateTime(bookRange.Date);
                    int fromValue = bookRange.From;
                    int toValue = bookRange.To;

                    if (dateValue < startDate || dateValue >= startDate.AddDays(14))
                    {
                        return BadRequest("The date is out of the boundary that you can book.");
                    }

                    if (dateValue.DayOfWeek == DayOfWeek.Saturday || dateValue.DayOfWeek == DayOfWeek.Sunday)
                    {
                        return BadRequest("You cannot book an item on Weekend.");
                    }

                    if (fromValue >= toValue)
                    {
                        return BadRequest("You entered the wrong period.");
                    }

                    if (dateValue == startDate && BangkokDateTime.now().Hour > fromValue)
                    {
                        return BadRequest("The time is out of the boundary that you can book.");
                    }

                    for (int slot = fromValue; slot < toValue; slot++)
                    {
                        if (BookSlotTable[(dateValue - startDate).Days, slot - 8] == 1)
                        {
                            return BadRequest("You cannot enter the period that overlaps.");
                        }
                        if (userBooked[(dateValue - startDate).Days, slot - 8] == 1)
                        {
                            return BadRequest("You have already booked this period.");
                        }
                        if (_db.BookSlot.Where(bs => bs.LabId == labId).Where(bs => bs.Date == dateValue).Count(bs => bs.TimeSlot == slot - 7) >= _db.Lab.Find(labId).Amount)
                        {
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
            return Content("{\"success\":\"true\"}", "application/json", Encoding.UTF8);
        }
    }
}