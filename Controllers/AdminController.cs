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
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly ApplicationDbContext _db;

        public AdminController(ILogger<AdminController> logger, ApplicationDbContext db/*, IWebHostEnvironment hostEnvironment*/)
        {
            _logger = logger;
            _db = db;
            /*_hostEnvironment = hostEnvironment;*/
        }

        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("AdminId") != null)
            {
                return RedirectToAction("Index", "LabAdmin");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Admin admin)
        {
            Admin loggedInAdmin = await _db.Admin.Where(a => a.UserName == admin.UserName && a.Password == Md5.GetMd5Hash(admin.Password)).FirstOrDefaultAsync();
            if (loggedInAdmin == null)
            {
                ViewBag.MessageError = "The Username or Password is Incorrect.";
                return View(admin);
            }
            HttpContext.Session.SetInt32("AdminId", loggedInAdmin.Id);
            return RedirectToAction("Index", "LabAdmin");
        }

        public IActionResult Logout()
        {
            if (HttpContext.Session.GetInt32("Id") != null)
            {
                int id = (int)HttpContext.Session.GetInt32("Id");
                HttpContext.Session.Clear();
                HttpContext.Session.SetInt32("Id", id);
            }
            else
            {
                HttpContext.Session.Clear();
            }
            return RedirectToAction("Login", "Admin");
        }

        public IActionResult Blacklist()
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            IEnumerable<User> users = _db.User.Where(user => user.BlackList == true);
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Blacklist([FromForm] int id)
        {
            if (HttpContext.Session.GetInt32("AdminId") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            User user = await _db.User.FindAsync(id);

            if (user == null)
            {
                return BadRequest("The User not found.");
            }

            user.BlackList = false;
            _db.User.Update(user);
            await _db.SaveChangesAsync();

            return RedirectToAction("Blacklist");
        }

    }
}
