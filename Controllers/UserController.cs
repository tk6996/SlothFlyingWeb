using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        public UserController(ILogger<UserController> logger, ApplicationDbContext db, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _db = db;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Register()
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "Id") != null)
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
            user.CreateAt = DateTime.Now;
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
            if (SessionExtensions.GetInt32(HttpContext.Session, "Id") != null)
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
            SessionExtensions.SetInt32(HttpContext.Session, "Id", loggedInUser.Id);
            return RedirectToAction("Index", "Lab");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Profile()
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "Id") == null)
            {
                return RedirectToAction("Login");
            }
            
            User loggedInUser = await _db.User.FindAsync(SessionExtensions.GetInt32(HttpContext.Session, "Id"));
            if (loggedInUser == null)
            {
                throw new Exception("User not found");
            }
            return View(loggedInUser);
        }

        public async Task<IActionResult> EditProfile()
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "Id") == null)
            {
                return RedirectToAction("Login");
            }

            User loggedInUser = await _db.User.FindAsync(SessionExtensions.GetInt32(HttpContext.Session, "Id"));
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
            if (SessionExtensions.GetInt32(HttpContext.Session, "Id") == null)
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

            User loggedInUser = await _db.User.FindAsync(SessionExtensions.GetInt32(HttpContext.Session, "Id"));
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
                string fileName = $"{DateTime.Now.ToString("yyyyMMMddhhmmssffff")}_{user.ImageFile.FileName}";
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
    }
}
