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

        public async Task<IActionResult> Booking(int? Id)
        {
            if (SessionExtensions.GetInt32(HttpContext.Session, "Id") == null)
            {
                return RedirectToAction("Login", "User");
            }

            if (Id == null)
            {
                return RedirectToAction("Index");
            }

            Lab lab = await _db.Lab.FindAsync(Id);
            if (lab == null)
            {
                return NotFound();
            }

            DateTime startDate = DateTime.Now.Date;
            DateTime endDate = startDate.AddDays(14);

            IEnumerable<BookSlot> bookSlots = _db.BookSlot.Where(bookSlot => bookSlot.LabId == lab.Id)
                                                          .Where(bookSlot => startDate <= bookSlot.Date && bookSlot.Date < endDate);

            lab.BookSlotTable = new int[9, 14];

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 14; c++)
                {
                    lab.BookSlotTable[r, c] = lab.Amount - bookSlots.Where(bookSlot => bookSlot.Date == startDate.AddDays(c))
                                                                    .Count(bookSlot => bookSlot.TimeSlot == r + 1);
                }
            }

            ViewBag.startDate = startDate;
            return View(lab);
        }
    }
}