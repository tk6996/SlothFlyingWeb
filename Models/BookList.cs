using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace SlothFlyingWeb.Models
{
    public class BookList
    {
        public enum StatusType
        {
            USING,COMING,FINISHED,CANCEL,EJECT
        }

        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Lab")]
        public int LabId { get; set; }

        [NotMapped]
        public string ItemName { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public int From { get; set; }

        public int To { get; set; }

        public StatusType Status { get; set; }
    }
}