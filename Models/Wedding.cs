using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Models
{
    public class NoPastDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if ((DateTime)value < DateTime.Now)
            {
                return new ValidationResult("Wedding must be in the future");
            }
            return ValidationResult.Success;
        }
    }
    public class Wedding
    {
        [Key]
        public int WeddingId {get; set;}

        [Required]
        public string WedderOne {get; set;}

        [Required]
        public string WedderTwo {get; set;}

        [Required]
        [NoPastDate]
        public DateTime Date {get; set;}

        [Required]
        public string Address {get; set;}

        public DateTime CreatedAt {get; set;} = DateTime.Now;
        public DateTime UpdatedAt {get; set;} = DateTime.Now;
        public List<Association> Users {get; set;}
        public string Creator {get; set;}
    }
}