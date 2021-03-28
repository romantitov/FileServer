namespace FileServer.Core.Exceptions
{
  public class ResourceNotFoundException : DomainException
  {
    public ResourceNotFoundException(int errorCode) : this(errorCode, string.Empty)
    {
    }
    public ResourceNotFoundException(int errorCode, string message):base(errorCode,message)
    {
    }
  }
}