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
    public class DinoLabController : Controller
    {
        private readonly ILogger<DinoLabController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        public DinoLabController(ILogger<DinoLabController> logger, IConfiguration config, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
            _config = config;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("Id") == null && HttpContext.Session.GetInt32("AdminId") == null)
            {
                return RedirectToAction("Login", "User");
            }

            List<DinoLab> dinoLabs = await _cache.GetOrCreateAsync<List<DinoLab>>("DinoLab", async entry =>
            {
                List<DinoLab> dinoLabList = new List<DinoLab>();
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(_config.GetValue<string>("DinoLabApi:ApiUri"));
                for (int id = 1; id <= 5; id++)
                {
                    HttpResponseMessage response = await client.GetAsync(_config.GetValue<string>("DinoLabApi:GetLabPath") + $"/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        dinoLabList.Add(await JsonSerializer.DeserializeAsync<DinoLab>(await response.Content.ReadAsStreamAsync()));
                    }
                }
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return dinoLabList;
            });


            return View(dinoLabs);
        }

        public async Task<IActionResult> Lab(int id)
        {

            if (HttpContext.Session.GetInt32("Id") == null && HttpContext.Session.GetInt32("AdminId") == null)
            {
                return RedirectToAction("Login", "User");
            }

            DateTime startDate = BangkokDateTime.now().Date;
            DinoLab dinoLabDetails = null;
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(_config.GetValue<string>("DinoLabApi:ApiUri"));

            HttpResponseMessage response = await client.GetAsync(_config.GetValue<string>("DinoLabApi:GetLabPath") + $"/{id}");

            if (response.IsSuccessStatusCode)
            {
                dinoLabDetails = await JsonSerializer.DeserializeAsync<DinoLab>(await response.Content.ReadAsStreamAsync());
            }

            if (dinoLabDetails == null)
            {
                return NotFound();
            }

            ViewBag.startDate = startDate;
            return View(dinoLabDetails);
        }
    }
}