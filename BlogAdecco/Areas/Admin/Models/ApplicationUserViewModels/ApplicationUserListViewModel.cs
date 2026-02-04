// Copyright (c) 2021, Mapache Digital
// Version: 1.2
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Models;
using System.ComponentModel.DataAnnotations;

namespace BlogAdecco.Areas.Admin.Models.ApplicationUserViewModels
{
    public class ApplicationUserListViewModel
    {
        public string Id { get; set; } = default!;

        [Display(Name = "Name")]
        public string FullName { get; set; } = default!;

        [Display(Name = "Company")]
        public string Company { get; set; } = default!;

        [Display(Name = "Mobile Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [Phone]
        public string MobilePhone { get; set; } = default!;

        [Display(Name = "Position")]
        public string? Position { get; set; }

        [Required]
        [Display(Name = "I agree with Grupo Adecco privacy policy")]
        public bool AcceptTermsOfService { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Display(Name = "Last Access")]
        [DataType(DataType.DateTime)]
        public DateTime? LastAccess { get; set; }

        [Display(Name = "Role")]
        public string Role { get; set; } = default!;

        public bool CanBeEdited { get; set; }

        [Display(Name = "Enabled")]
        public bool Enabled { get; set; }

        [Display(Name = "Approved")]
        public bool Approved { get; set; }

        [Display(Name = "User")]
        public ApplicationUser ApplicationUser { get; set; } = default!;
    }
}
