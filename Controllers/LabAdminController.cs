using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SlothFlyingWeb.Data;
using SlothFlyingWeb.Models;
using SlothFlyingWeb.Utils;

namespace SlothFlyingWeb.Controllers
{
    public class LabAdminController : Controller
    {
        private readonly ILogger<LabAdminController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IMemoryCache _cache;
        public LabAdminController(ILogger<LabAdminController> logger, ApplicationDbContext db, IWebHostEnvironment hostEnvironment, IMemoryCache cache)
        {
            _logger = logger;
            _db = db;
            _hostEnvironment = hostEnvironment;
            _cache = cache;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            IEnumerable<Lab> labs = _db.Lab;
            return View(labs);
        }

        [HttpGet("/LabAdmin/ViewItem")]
        public async Task<IActionResult> ViewItem(int? id)
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
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

            ViewBag.startDate = startDate;
            return View(lab);
        }

        [HttpPost("/LabAdmin/ViewItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ViewItemPost([FromForm] int? id)
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (id == null || id < 0)
            {
                return BadRequest("Id is not valid.");
            }

            BookList bookList = await _db.BookList.FindAsync(id);

            if (bookList == null)
            {
                return BadRequest("The Booklist not found.");
            }

            if (bookList.Status == BookList.StatusType.FINISHED ||
                bookList.Status == BookList.StatusType.CANCEL ||
                bookList.Status == BookList.StatusType.EJECT)
            {
                return BadRequest("This booklist can't eject.");
            }

            bookList.Status = BookList.StatusType.EJECT;
            _db.BookList.Update(bookList);
            IEnumerable<BookSlot> bookSlots = _db.BookSlot.Where(bookSlot => bookSlot.BookListId == bookList.Id);
            foreach (BookSlot bookSlot in bookSlots)
            {
                _db.BookSlot.Remove(bookSlot);
            }
            await _db.SaveChangesAsync();
            _cache.Remove($"BookSlotTable_{bookList.LabId}");
            _cache.Remove($"UserBooked_{bookList.LabId}_{bookList.UserId}");

            return RedirectToAction("ViewItem", "LabAdmin");
        }

        // API
        public IActionResult UserBookList([FromQuery(Name = "labId")] int? labId, [FromQuery(Name = "date")] long? date, [FromQuery(Name = "timeslot")] int? timeslot)
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
            {
                return Unauthorized();
            }

            if (labId == null || date == null || timeslot == null)
            {
                return BadRequest();
            }

            DateTime dateValue = BangkokDateTime.millisecondToDateTime((long)date).Date;

            Func<BookList, User, BookList> joinUser = (bookList, user) =>
              {
                  bookList.FullName = $"{user.FirstName} {user.LastName}";
                  bookList.UserImageUrl = user.ImageUrl;
                  return bookList;
              };

            IEnumerable<BookList> bookLists = _db.BookSlot.Where(bs => bs.LabId == labId && bs.Date == dateValue && bs.TimeSlot == timeslot)
                                                                .Join(
                                                                _db.BookList,
                                                                bs => bs.BookListId,
                                                                bl => bl.Id,
                                                                (bs, bl) => bl)
                                                                .Join(_db.User,
                                                                bookList => bookList.UserId,
                                                                user => user.Id,
                                                                joinUser);
            DateTime dateNow = BangkokDateTime.now();
            foreach (BookList bookList in bookLists)
            {
                if (bookList.Status == BookList.StatusType.COMING)
                {
                    if (bookList.Date.AddHours(bookList.From) <= dateNow && dateNow < bookList.Date.AddHours(bookList.To))
                    {
                        bookList.Status = BookList.StatusType.USING;
                        _db.BookList.Update(bookList);
                    }
                    else if (dateNow >= bookList.Date.AddHours(bookList.To))
                    {
                        bookList.Status = BookList.StatusType.FINISHED;
                        _db.BookList.Update(bookList);
                    }
                }
                if (bookList.Status == BookList.StatusType.USING)
                {
                    if (dateNow >= bookList.Date.AddHours(bookList.To))
                    {
                        bookList.Status = BookList.StatusType.FINISHED;
                        _db.BookList.Update(bookList);
                    }
                }
            }

            return Json(bookLists.Select(bl => new BookListLabAdmin
            {
                BooklistId = bl.Id,
                UserId = bl.UserId,
                UserImageUrl = Url.Content(bl.UserImageUrl != "" ? bl.UserImageUrl : "~/assets/images/brand.jpg"),
                FullName = bl.FullName,
                From = bl.From,
                To = bl.To,
                Status = (int)bl.Status,
            }));
        }

        public async Task<IActionResult> EditItem(int? id)
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
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

            return View(lab);
        }

        [HttpPost]
        public async Task<IActionResult> EditItem(Lab lab)
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            Lab latestlab = await _db.Lab.FindAsync(lab.Id);
            if (latestlab == null)
            {
                return BadRequest("The id not found.");
            }

            if (!ModelState.IsValid)
            {
                return View(latestlab);
            }

            latestlab.Amount = lab.Amount;

            if (lab.ImageFile != null)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string fileName = $"{BangkokDateTime.now().ToString("yyyyMMddhhmmssffff")}_{lab.ImageFile.FileName}";
                string path = $"{wwwRootPath}/images/labs/{fileName}";
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await lab.ImageFile.CopyToAsync(fs);
                }
                latestlab.ImageUrl = $"~/images/labs/{fileName}";
            }

            _db.Lab.Update(latestlab);
            await _db.SaveChangesAsync();
            return RedirectToAction("EditItem", "LabAdmin", lab.Id);
        }
    }
}