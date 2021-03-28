using FluentValidation;

namespace FileServer.Api.Models
{
  public class PageRequest
  {
    public PageRequest()
    {
      Limit = 100;
    }

    public int Limit { get; set; }
    public int Offset { get; set; }
  }

  public class PageRequestValidator : AbstractValidator<PageRequest>
  {
    public PageRequestValidator()
    {
      RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);
      RuleFor(x => x.Limit).GreaterThanOrEqualTo(0);
    }
  }
}