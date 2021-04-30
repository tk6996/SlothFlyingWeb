using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SlothFlyingWeb.Models
{
    public class ApiUser
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey("Lab")]
        public int LabId { get; set; }

        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; }

        public string ApiKey { get; set; }

        public bool Enable { get; set; }
    }
}