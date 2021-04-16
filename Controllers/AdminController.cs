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
            if (SessionExtensions.GetInt32(HttpContext.Session, "AdminId") != null)
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
            SessionExtensions.SetInt32(HttpContext.Session, "AdminId", loggedInAdmin.Id);
            return RedirectToAction("Index", "LabAdmin");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Admin");
        }


    }
}
