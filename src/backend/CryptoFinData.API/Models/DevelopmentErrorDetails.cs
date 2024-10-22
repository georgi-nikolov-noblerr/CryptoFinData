namespace CryptoFinData.API.Models;

public class DevelopmentErrorDetails
{
    public string ExceptionType { get; set; }
    public string StackTrace { get; set; }
    public string RequestPath { get; set; }
    public string RequestMethod { get; set; }
    public DateTime Timestamp { get; set; }
}