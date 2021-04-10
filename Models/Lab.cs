using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SlothFlyingWeb.Models
{
    public class Lab
    {

        [Key]
        public int Id { get; set; }

        public string ItemName { get; set; }

        public int Amount { get; set; }

        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; }

        [NotMapped]
        public int[,] BookSlotTable { get; set; }
    }
}