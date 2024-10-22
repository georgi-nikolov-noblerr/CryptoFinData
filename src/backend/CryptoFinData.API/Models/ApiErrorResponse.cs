namespace CryptoFinData.API.Models;

public class ApiErrorResponse
{
    public string TraceId { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public string Details { get; set; }
    public DevelopmentErrorDetails DevelopmentDetails { get; set; }
}
