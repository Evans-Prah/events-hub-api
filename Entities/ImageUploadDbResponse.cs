namespace Entities
{
    public class ImageUploadDbResponse
    {
        public string ResponseMessage { get; set; }
        public string PublicId { get; set; }
        public string Url { get; set; }
        public bool IsMainPhoto { get; set; }
    }
}
