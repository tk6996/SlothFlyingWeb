using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SlothFlyingWeb.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Username")]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}