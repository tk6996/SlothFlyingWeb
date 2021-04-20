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