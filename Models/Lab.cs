using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;

namespace SlothFlyingWeb.Models
{
    public class Lab
    {

        [Key]
        [Required]
        public int Id { get; set; }

        public string ItemName { get; set; }

        [Required]
        [Range(0,999)]
        public int Amount { get; set; }

        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; }

        [NotMapped]
        [DisplayName("Upload a Photo")]
        public IFormFile ImageFile { get; set; }

        [NotMapped]
        public int[,] BookSlotTable { get; set; }
    }
}