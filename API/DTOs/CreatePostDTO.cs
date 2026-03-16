using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace API.DTOs
{
    public class CreatePostDTO
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        
        [Required]
        public string Content { get; set; } = string.Empty;

        
    }
}