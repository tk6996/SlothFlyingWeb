using System.Linq;
using SlothFlyingWeb.Models;
using SlothFlyingWeb.Utils;

namespace SlothFlyingWeb.Data
{
    public class ApplicationDbInitializer
    {
        public static void Initializer(ApplicationDbContext db)
        {
            db.Database.EnsureCreated();

            //Look for any Admin
            if (db.Admin.Any())
            {
                return; // Database has been seeded
            }

            Admin admin = new Admin { UserName = "admin", Password = Md5.GetMd5Hash("admin") };
            db.Admin.Add(admin);
            db.SaveChanges();

            Lab[] labs = new Lab[]
            {
                new Lab{Id = 1, ItemName = "Osciloscope",ImageUrl = "~/images/labs/Oscilloscope.png", Amount = 10},
                new Lab{Id = 2, ItemName = "Multimeter",ImageUrl = "~/images/labs/Digital-multimeter.jpg", Amount = 10},
                new Lab{Id = 3, ItemName = "Rasberry Pi",ImageUrl = "~/images/labs/Rasberry_Pi.jpg", Amount = 10},
                new Lab{Id = 4, ItemName = "Arduino",ImageUrl = "~/images/labs/Arduino.jpg", Amount = 10},
                new Lab{Id = 5, ItemName = "Router",ImageUrl = "~/images/labs/Router.jpg", Amount = 10}
            };
            foreach (Lab lab in labs)
            {
                db.Lab.Add(lab);
            }
            db.SaveChanges();
        }
    }
}