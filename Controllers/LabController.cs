using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SlothFlyingWeb.Data;
using SlothFlyingWeb.Models;

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
    }
}