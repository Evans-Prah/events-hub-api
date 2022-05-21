namespace Entities
{
    public class ServiceResponse
    {
        public bool Successful { get; set; }
        public string? ResponseMessage { get; set; }
        public dynamic? Data { get; set; }
    }
}
