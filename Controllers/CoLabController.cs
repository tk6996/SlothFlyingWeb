using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
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
            IEnumerable<CoLab> coLabDetails = null;

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(_config.GetValue<string>("CoLabApi:ApiUri"));

            HttpResponseMessage response = await client.GetAsync(_config.GetValue<string>("CoLabApi:GetLabPath"));

            if (response.IsSuccessStatusCode)
            {
                coLabDetails = await JsonSerializer.DeserializeAsync<IEnumerable<CoLab>>(await response.Content.ReadAsStreamAsync());
            }

            return View(coLabDetails);
        }

        public async Task<IActionResult> Lab(int id)
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(_config.GetValue<string>("CoLabApi:ApiUri"));

            HttpResponseMessage response = await client.GetAsync($"{_config.GetValue<string>("CoLabApi:GetLabBookingPath")}/{id}");

            if (response.IsSuccessStatusCode)
            {
                CoLabBooking coLabBooking = await JsonSerializer.DeserializeAsync<CoLabBooking>(await response.Content.ReadAsStreamAsync());
                ViewBag.startDate = DateTime.Parse(coLabBooking.timeStamp).Date;
                return View(coLabBooking);
            }

            return NotFound();
        }
    }
}