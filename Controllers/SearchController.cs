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
    public class SearchController : Controller
    {
        private readonly ILogger<SearchController> _logger;
        private readonly ApplicationDbContext _db;
        public SearchController(ILogger<SearchController> logger, ApplicationDbContext db)
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
            IEnumerable<User> users = _db.User;
            return View(users);
        }

        [HttpPost]
        public IActionResult Index([FromForm] int id)
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            TempData["userId"] = id;
            return RedirectToAction("UserProfile");
        }

        public async Task<IActionResult> UserProfile()
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            int userId = (int)TempData["userId"];
            TempData.Keep("userId");
            User user = await _db.User.FindAsync(userId);
            return View(user);
        }

        public async Task<IActionResult> UserBooklist()
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            int userId = (int)TempData["userId"];
            TempData.Keep("userId");

            DateTime dateNow = BangkokDateTime.now();

            Func<BookList, Lab, BookList> joinItemName = (bookList, lab) =>
              {
                  bookList.ItemName = lab.ItemName;
                  return bookList;
              };

            IEnumerable<BookList> bookLists = _db.BookList.Where(bookList => bookList.UserId == userId)
                                                          .Join(_db.Lab,
                                                                bookList => bookList.LabId,
                                                                lab => lab.Id,
                                                                joinItemName);

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
            await _db.SaveChangesAsync();
            return View(bookLists.OrderBy(bl => bl.Status).ThenByDescending(bl => bl.Date).ThenBy(bl => bl.From).ThenBy(bl => bl.To).ThenBy(bl => bl.LabId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserBooklist([FromForm] int id)
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            int userId = (int)TempData["userId"];
            TempData.Keep("userId");
            BookList bookList = await _db.BookList.FindAsync(id);

            if (bookList == null)
            {
                return BadRequest("The Booklist not found.");
            }

            if (bookList.UserId != userId)
            {
                return BadRequest("You not permission to cancel this booklist.");
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
            return Redirect("UserBooklist");
        }
    }
}