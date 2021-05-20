using System.Collections.Generic;

namespace SlothFlyingWeb.Models
{
    public class CoLabBooking
    {
        public int id { get; set; }

        public string itemName { get; set; }

        public string imageUrl { get; set; }

        public string startDate { get; set; }

        public string endDate { get; set; }

        public int from { get; set; }

        public int to { get; set; }

        public int[][] bookSlotTable { get; set; }

        public string timeStamp { get; set; }
    }
}