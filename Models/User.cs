using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http;

namespace SlothFlyingWeb.Models
{
    public class User
    {
        [Key]
        [DisplayName("User ID")]
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "The First Name must be contains A-Z and a-z.")]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "The Last Name must be contains A-Z and a-z.")]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(64, MinimumLength = 8, ErrorMessage = "The Password must be a string length 8 - 64.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [StringLength(64, MinimumLength = 8, ErrorMessage = "The ConfirmPassword must be a string length 8 - 64.")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        [NotMapped]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreateAt { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\d{3}-?\d{3}-?\d{3,4}$", ErrorMessage = "Incorrect phone number format.")]
        public string Phone { get; set; }

        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; } = "";

        [NotMapped]
        [DisplayName("Upload a Photo")]
        public IFormFile ImageFile { get; set; }

        public ICollection<BookList> BookLists { get; set; }

        public bool BlackList { get; set; } = false;

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var property in this.GetType().GetProperties())
            {
                sb.Append(property.Name);
                sb.Append(": ");
                if (property.GetIndexParameters().Length > 0)
                {
                    sb.Append("Indexed Property cannot be used");
                }
                else
                {
                    sb.Append(property.GetValue(this, null));
                }
                sb.Append(System.Environment.NewLine);
            }

            return sb.ToString();
        }
    }
}
