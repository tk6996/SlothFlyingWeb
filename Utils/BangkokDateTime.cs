using System;

namespace SlothFlyingWeb.Utils
{
    public class BangkokDateTime
    {
        private static readonly int GMT = 7;

        public static DateTime now(){
            return DateTime.UtcNow.AddHours(GMT);
        }

        public static DateTime millisecondToDateTime(long mills){
            return (new DateTime(1970, 1, 1)).AddHours(GMT).AddMilliseconds(mills);
        }
    }
}