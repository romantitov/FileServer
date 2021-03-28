using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FileServer.Api.Models
{
  public class FileDataRequest
  {
    [Required]
    public IFormFile File { get; set; }
  }
}