namespace Patcheetah.Tests.Models.RFC7396
{
    public class Post
    {
        public string Title { get; set; }

        public string PhoneNumber { get; set; }

        public Author Author { get; set; }

        public string[] Tags { get; set; }

        public string Content { get; set; }
    }
}
