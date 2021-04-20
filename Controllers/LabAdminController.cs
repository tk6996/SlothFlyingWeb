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
    public class LabAdminController : Controller
    {
        private readonly ILogger<LabAdminController> _logger;
        private readonly ApplicationDbContext _db;
        public LabAdminController(ILogger<LabAdminController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            IEnumerable<Lab> labs = _db.Lab;
            return View(labs);
        }

        public async Task<IActionResult> ViewItem(int? id)
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "AdminId") == null)
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
            ViewBag.startDate = startDate.Date;
            return View(lab);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ViewItem([FromForm] int id)
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
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
                return BadRequest("This booklist be canceled.");
            }

            bookList.Status = BookList.StatusType.EJECT;
            _db.BookList.Update(bookList);
            IEnumerable<BookSlot> bookSlots = _db.BookSlot.Where(bookSlot => bookSlot.BookListId == bookList.Id);
            foreach (BookSlot bookSlot in bookSlots)
            {
                _db.BookSlot.Remove(bookSlot);
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("ViewItem", "LabAdmin");
        }

        public IActionResult UserBookList([FromQuery(Name = "labId")] int? labId, [FromQuery(Name = "date")] long date, [FromQuery(Name = "timeslot")] int? timeslot)
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (labId == null || date == 0 || timeslot == null)
            {
                return RedirectToAction("Index");
            }

            DateTime dateValue = BangkokDateTime.millisecondToDateTime(date);

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

            return Json(bookLists.Select(bl => new
            {
                booklistId = bl.Id,
                UserId = bl.UserId,
                UserImageUrl = Url.Content(bl.UserImageUrl != "" ? bl.UserImageUrl : "~/assets/images/brand.jpg"),
                Fullname = bl.FullName,
                From = bl.From,
                To = bl.To,
                Status = bl.Status,
            }));
        }

        public async Task<IActionResult> EditItem(int? id)
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "AdminId") == null)
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
        public async Task<IActionResult> EditItem(int? id, [FromForm] int? amount)
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (id == null)
            {
                return BadRequest("This link must have id.");
            }

            if (amount == null || amount < 0)
            {
                return BadRequest("Amount have to zero or more");
            }

            Lab lab = await _db.Lab.FindAsync(id);
            if (lab == null)
            {
                return BadRequest("The id not found.");
            }

            lab.Amount = (int)amount;

            _db.Lab.Update(lab);
            await _db.SaveChangesAsync();
            return RedirectToAction("EditItem", "LabAdmin", id);
        }
    }
}