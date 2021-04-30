namespace SlothFlyingWeb.Models
{
    public class BookListLabAdmin
    {
        public int BooklistId { get; set; }
        public int UserId { get; set; }
        public string UserImageUrl { get; set; }
        public string FullName { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public int Status { get; set; }
    }
}