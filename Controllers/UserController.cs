using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SlothFlyingWeb.Models;
using SlothFlyingWeb.Data;
using SlothFlyingWeb.Utils;

namespace SlothFlyingWeb.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IMemoryCache _cache;

        public UserController(ILogger<UserController> logger, ApplicationDbContext db, IWebHostEnvironment hostEnvironment, IMemoryCache cache)
        {
            _logger = logger;
            _db = db;
            _hostEnvironment = hostEnvironment;
            _cache = cache;
        }
        public IActionResult Register()
        {
            if (HttpContext.Session.GetInt32("Id") != null)
            {
                return RedirectToAction("Index", "Lab");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            // _logger.LogInformation("User {user}", user);
            user.Password = Md5.GetMd5Hash(user.Password);
            user.CreateAt = BangkokDateTime.now();
            user.Phone = String.Join("", user.Phone.Split("-"));

            User registeredUser = await _db.User.Where(u => u.Email == user.Email).FirstOrDefaultAsync();
            if (registeredUser != null)
            {
                ViewBag.MessageError = "The Email already exists.";
                return View(user);
            }
            _db.User.Add(user);
            await _db.SaveChangesAsync();
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("Id") != null)
            {
                return RedirectToAction("Index", "Lab");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User user)
        {
            User loggedInUser = await _db.User.Where(u => u.Email == user.Email && u.Password == Md5.GetMd5Hash(user.Password)).FirstOrDefaultAsync();
            if (loggedInUser == null)
            {
                ViewBag.MessageError = "The Email or Password is Incorrect.";
                return View(user);
            }
            HttpContext.Session.SetInt32("Id", loggedInUser.Id);
            TempData["BlackList"] = loggedInUser.BlackList;
            TempData.Save();
            return RedirectToAction("Index", "Lab");
        }

        public IActionResult Logout()
        {
            int id = HttpContext.Session.GetInt32("Id") ?? 0;
            if (HttpContext.Session.GetInt32("AdminId") != null)
            {
                int adminId = (int)HttpContext.Session.GetInt32("AdminId");
                HttpContext.Session.Clear();
                HttpContext.Session.SetInt32("AdminId", adminId);
            }
            else
            {
                HttpContext.Session.Clear();
            }
            if (id > 0)
            {
                foreach (int labId in _db.Lab.Select(lab => lab.Id))
                {
                    _cache.Remove($"UserBooked_{labId}_{id}");
                }
                _cache.Remove($"UserBooklist_{id}");
            }
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Profile()
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login");
            }

            User loggedInUser = await _db.User.FindAsync(HttpContext.Session.GetInt32("Id"));
            if (loggedInUser == null)
            {
                throw new Exception("User not found");
            }
            return View(loggedInUser);
        }

        public async Task<IActionResult> EditProfile()
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login");
            }

            User loggedInUser = await _db.User.FindAsync(HttpContext.Session.GetInt32("Id"));
            if (loggedInUser == null)
            {
                throw new Exception("User not found");
            }
            return View(loggedInUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(User user)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login");
            }

            Match mf = Regex.Match(user.FirstName ?? "", "^[A-Za-z]+$");
            Match ml = Regex.Match(user.LastName ?? "", "^[A-Za-z]+$");
            Match mp = Regex.Match(user.Phone ?? "", @"^\d{3}-?\d{3}-?\d{3,4}$");
            if (!mf.Success || !ml.Success || !mp.Success)
            {
                // _logger.LogInformation("{user}",user);
                return View(user);
            }

            User loggedInUser = await _db.User.FindAsync(HttpContext.Session.GetInt32("Id"));
            if (loggedInUser == null)
            {
                throw new Exception("User not found");
            }
            loggedInUser.FirstName = user.FirstName;
            loggedInUser.LastName = user.LastName;
            loggedInUser.Phone = String.Join("", user.Phone.Split("-"));

            if (user.ImageFile != null)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string fileName = $"{BangkokDateTime.now().ToString("yyyyMMddhhmmssffff")}_{user.ImageFile.FileName}";
                string path = $"{wwwRootPath}/images/users/{fileName}";
                // _logger.LogInformation(path);
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    await user.ImageFile.CopyToAsync(fs);
                }
                loggedInUser.ImageUrl = $"~/images/users/{fileName}";
            }

            _db.User.Update(loggedInUser);
            await _db.SaveChangesAsync();
            return RedirectToAction("Profile");
        }

        [HttpGet("/User/Booklist")]
        public async Task<IActionResult> Booklist()
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login");
            }
            DateTime dateNow = BangkokDateTime.now();
            int userId = (int)HttpContext.Session.GetInt32("Id");

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

            List<BookList> bl = _cache.Set<List<BookList>>($"UserBooklist_{userId}", bookLists.Where(bl => bl.Status <= BookList.StatusType.COMING)
                                                                                              .OrderBy(bl => bl.Date)
                                                                                              .ThenBy(bl => bl.From)
                                                                                              .ThenBy(bl => bl.To)
                                                                                              .ThenBy(bl => bl.LabId)
                                                                                              .Concat(
                                                                                     bookLists.Where(bl => bl.Status > BookList.StatusType.COMING)
                                                                                              .OrderByDescending(bl => bl.Date)
                                                                                              .ThenBy(bl => bl.From)
                                                                                              .ThenBy(bl => bl.To)
                                                                                              .ThenBy(bl => bl.LabId))
                                                                                              .ToList(), new MemoryCacheEntryOptions()
                                                                                              {
                                                                                                  SlidingExpiration = TimeSpan.FromMinutes(1)
                                                                                              });

            return View(bl.GetRange(0, Math.Min(bl.Count, 10)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Booklist([FromForm] int? id)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login");
            }
            int userId = (int)HttpContext.Session.GetInt32("Id");

            if (id == null || id <= 0)
            {
                return BadRequest();
            }

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
                return BadRequest("This booklist can't cancel.");
            }

            bookList.Status = BookList.StatusType.CANCEL;
            _db.BookList.Update(bookList);
            IEnumerable<BookSlot> bookSlots = _db.BookSlot.Where(bookSlot => bookSlot.BookListId == bookList.Id);
            foreach (BookSlot bookSlot in bookSlots)
            {
                _db.BookSlot.Remove(bookSlot);
            }
            await _db.SaveChangesAsync();

            _cache.Remove($"BookSlotTable_{bookList.LabId}");
            _cache.Remove($"UserBooked_{bookList.LabId}_{userId}");
            return Redirect("Booklist");
        }

        // API
        [HttpGet("/User/Booklist/{round:int}")]
        public IActionResult BooklistApi([FromRoute] int round)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return Unauthorized();
            }

            int userId = (int)HttpContext.Session.GetInt32("Id");
            List<BookList> bookList = _cache.Get<List<BookList>>($"UserBooklist_{userId}");

            if (bookList == null || round < 0 || bookList.Count <= round * 10)
            {
                return Json(new object[] { });
            }

            return Json(bookList.GetRange(round * 10, Math.Min(bookList.Count - round * 10, 10)).Select(bl =>
                new
                {
                    Id = bl.Id,
                    LabId = bl.LabId,
                    ItemName = bl.ItemName,
                    Date = bl.Date.ToString("ddd dd MMM yyyy"),
                    From = $"{bl.From.ToString("D2")}.00",
                    To = $"{bl.To.ToString("D2")}.00",
                    Status = bl.Status
                }
            ));
        }
    }
}
