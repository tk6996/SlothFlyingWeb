using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SlothFlyingWeb.Models
{
    public class Lab
    {

        [Key]
        public int Id { get; set; }

        public string ItemName { get; set; }

        public uint Amount { get; set; }

        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; }

        public ICollection<BookList> BookLists { get; set; }
    }
}