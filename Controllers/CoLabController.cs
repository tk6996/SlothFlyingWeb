using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SlothFlyingWeb.Utils;
using SlothFlyingWeb.Models;

namespace SlothFlyingWeb.Controllers
{
    public class CoLabController : Controller
    {
        private readonly ILogger<CoLabController> _logger;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        public CoLabController(ILogger<CoLabController> logger, IConfiguration config, IMemoryCache cache)
        {
            _logger = logger;
            _config = config;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("Id") == null && HttpContext.Session.GetInt32("AdminId") == null)
            {
                return RedirectToAction("Login", "User");
            }

            IEnumerable<CoLab> coLabDetails = await _cache.GetOrCreateAsync<IEnumerable<CoLab>>("CoApiLab", async entry =>
            {
                HttpClient client = new HttpClient();

                client.BaseAddress = new Uri(_config.GetValue<string>("CoLabApi:ApiUri"));

                HttpResponseMessage response = await client.GetAsync(_config.GetValue<string>("CoLabApi:GetLabPath"));

                if (response.IsSuccessStatusCode)
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                    return await Task.FromResult(await JsonSerializer.DeserializeAsync<IEnumerable<CoLab>>(await response.Content.ReadAsStreamAsync()));
                }
                else
                {
                    entry.SlidingExpiration = null;
                    return null;
                }
            });
            return View(coLabDetails);
        }

        public async Task<IActionResult> Lab(int id)
        {
            if (HttpContext.Session.GetInt32("Id") == null && HttpContext.Session.GetInt32("AdminId") == null)
            {
                return RedirectToAction("Login", "User");
            }

            DateTime startDate = BangkokDateTime.now().Date;
            DateTime cacheDate;
            CoLabBooking coLabBooking;

            Func<DateTime, object, bool> checkDateCurrent = (cacheDate, key) =>
             {
                 if (cacheDate < startDate)
                 {
                     _cache.Remove(key);
                     return true;
                 }
                 return false;
             };

            do
            {
                (coLabBooking, cacheDate) = await _cache.GetOrCreateAsync<(CoLabBooking, DateTime)>($"CoApiBooking_{id}", async entry =>
                 {
                     HttpClient client = new HttpClient();

                     client.BaseAddress = new Uri(_config.GetValue<string>("CoLabApi:ApiUri"));

                     HttpResponseMessage response = await client.GetAsync($"{_config.GetValue<string>("CoLabApi:GetLabBookingPath")}/{id}");

                     if (response.IsSuccessStatusCode)
                     {
                         entry.SlidingExpiration = TimeSpan.FromMinutes(1);
                         return await Task.FromResult((await JsonSerializer.DeserializeAsync<CoLabBooking>(await response.Content.ReadAsStreamAsync()), startDate));
                     }
                     else
                     {
                         entry.SlidingExpiration = null;
                         return (null, startDate);
                     }
                 });
            } while (checkDateCurrent(cacheDate, $"CoApiBooking_{id}"));

            if(coLabBooking == null)
            {
                return NotFound();
            }
            
            ViewBag.startDate = DateTime.Parse(coLabBooking.timeStamp).Date;
            return View(coLabBooking);
        }
    }
}