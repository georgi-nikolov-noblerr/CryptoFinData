namespace CryptoFinData.Core.Exeptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}