using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            return View();
        }


        public async Task<IActionResult> UserList([FromQuery(Name = "search")] string search)
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            if (search == null)
                return Json(new object[] { });

            search = search.Trim();

            string[] words = search.Split();
            if (words.Length == 1)
            {
                int id;
                if (Int32.TryParse(words[0], out id))
                {
                    User user = await _db.User.FindAsync(id);
                    if (user == null)
                    {
                        return Json(new object[] { });
                    }
                    return Json(new object[]{
                        new {
                            Id = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            ImageUrl = Url.Content(user.ImageUrl != "" ? user.ImageUrl : "~/assets/images/brand.jpg")
                        }
                    });
                }

                IEnumerable<User> users = _db.User.Where(user => (user.FirstName.Length >= words[0].Length &&
                                                                  user.FirstName.Substring(0, words[0].Length).ToLower().Equals(words[0].ToLower())) ||
                                                                 (user.LastName.Length >= words[0].Length &&
                                                                  user.LastName.Substring(0, words[0].Length).ToLower().Equals(words[0].ToLower())));
                return Json(users.Select(user => new
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ImageUrl = Url.Content(user.ImageUrl != "" ? user.ImageUrl : "~/assets/images/brand.jpg")
                }));
            }

            if (words.Length == 2)
            {
                IEnumerable<User> users = _db.User.Where(user => ((user.FirstName.Length >= words[0].Length &&
                                                                   user.FirstName.Substring(0, words[0].Length).ToLower().Equals(words[0].ToLower())) &&
                                                                  (user.LastName.Length >= words[1].Length &&
                                                                   user.LastName.Substring(0, words[1].Length).ToLower().Equals(words[1].ToLower()))) ||
                                                                 ((user.FirstName.Length >= words[1].Length &&
                                                                   user.FirstName.Substring(0, words[1].Length).ToLower().Equals(words[1].ToLower())) &&
                                                                  (user.LastName.Length >= words[0].Length &&
                                                                  user.LastName.Substring(0, words[0].Length).ToLower().Equals(words[0].ToLower()))));
                return Json(users.Select(user => new
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ImageUrl = Url.Content(user.ImageUrl != "" ? user.ImageUrl : "~/assets/images/brand.jpg")
                }));
            }

            return Json(new object[] { });
        }

        public async Task<IActionResult> UserProfile(int? id)
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            User user = await _db.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.UserId = user.Id;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserProfile([FromForm] int id, bool blacklist)
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            User user = await _db.User.FindAsync(id);

            if (user == null)
            {
                return BadRequest("The User not found.");
            }

            user.BlackList = blacklist;
            _db.User.Update(user);
            await _db.SaveChangesAsync();

            return RedirectToAction("UserProfile");
        }

        public async Task<IActionResult> UserBooklist(int? id)
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            User user = await _db.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            DateTime dateNow = BangkokDateTime.now();

            Func<BookList, Lab, BookList> joinItemName = (bookList, lab) =>
              {
                  bookList.ItemName = lab.ItemName;
                  return bookList;
              };

            IEnumerable<BookList> bookLists = _db.BookList.Where(bookList => bookList.UserId == user.Id)
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
            ViewBag.UserId = user.Id;
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
            return RedirectToAction("UserBooklist", "Search", $"{bookList.UserId}");
        }
    }
}