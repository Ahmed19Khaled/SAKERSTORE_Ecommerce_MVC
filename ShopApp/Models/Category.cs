using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ShopApp.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required,MaxLength(100)]
        public string NameDepartment { get; set; }
        [MaxLength(100)]
        public string? ImageDepartment { get; set; }
        [NotMapped]
        public IFormFile? formFile { get; set; }
        [ValidateNever]
        public ICollection<SubCategory> SubCategories { get; set; }

         }
}  

//[ValidateNever]
        //public  ICollection<Nurse>  Nurses { get; set; }
        //[ValidateNever]
        //public  ICollection<Doctor>  Doctors { get; set; }
        //[ValidateNever] 
        //public  ICollection<Stuff>  Stuffs { get; set; }
        //[ValidateNever] 
        //public  ICollection<Patient> Patients { get; set; }




