namespace FileServer.Core.Exceptions
{
  public class AccessNotAllowedException : DomainException
  {
    public AccessNotAllowedException(int errorCode) : this(errorCode, string.Empty)
    {
    }
    public AccessNotAllowedException(int errorCode, string message) : base(errorCode, message)
    {
    }
  }
}