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

            //Look for any  Admin
            if (db.Admin.Any())
            {
                return; // Database has been seeded
            }

            Admin admin = new Admin { UserName = "admin", Password = Md5.GetMd5Hash("admin") };
            db.Admin.Add(admin);
            db.SaveChanges();

            Lab[] labs = new Lab[]
            {
                new Lab{Id = 1, ItemName = "Osciloscope",ImageUrl = "~/assets/images/Oscilloscope.png", Amount = 0},
                new Lab{Id = 2, ItemName = "Multimeter",ImageUrl = "~/assets/images/Digital-multimeter.jpg", Amount = 0},
                new Lab{Id = 3, ItemName = "Rasberry Pi",ImageUrl = "~/assets/images/Rasberry_Pi.jpg", Amount = 0},
                new Lab{Id = 4, ItemName = "Arduino",ImageUrl = "~/assets/images/Arduino.jpg", Amount = 0},
                new Lab{Id = 5, ItemName = "Router",ImageUrl = "~/assets/images/Router.jpg", Amount = 0}
            };
            foreach (Lab lab in labs)
            {
                db.Lab.Add(lab);
            }
            db.SaveChanges();
        }
    }
}