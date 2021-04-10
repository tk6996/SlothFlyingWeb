using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace SlothFlyingWeb.Models
{
    public class BookSlot
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Lab")]
        public int LabId { get; set; }

        [ForeignKey("BookList")]
        public int BookListId { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public int TimeSlot { get; set; }
    }
}