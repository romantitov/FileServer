using System;

namespace FileServer.Core.Exceptions
{
  public abstract class DomainException : ApplicationException
  {
    protected DomainException(int errorCode):this(errorCode, string.Empty)
    {
    }
    protected DomainException(int errorCode, string message) : base(message)
    {
      ErrorCode = errorCode;
    }
    public int ErrorCode { get; }
  }
}