// Copyright (c) 2021, Mapache Digital
// Version: 1.2
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using System.ComponentModel.DataAnnotations;

namespace BlogAdecco.Areas.Admin.Models.ApplicationUserViewModels
{
    public class ApplicationUserDetailsViewModel
    {
        public string Id { get; set; } = default!;

        [Display(Name = "Firstname")]
        public string Firstname { get; set; } = default!;

        [Display(Name = "Lastname")]
        public string Lastname { get; set; } = default!;

        [Display(Name = "Company")]
        public string Company { get; set; } = default!;

        [Display(Name = "Mobile Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [Phone]
        public string MobilePhone { get; set; } = default!;

        [Display(Name = "Position")]
        public string? Position { get; set; }

        [Display(Name = "I agree with Grupo Adecco privacy policy")]
        public bool AcceptTermsOfService { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Display(Name = "Role")]
        public string Role { get; set; } = default!;

        [Display(Name = "Enabled")]
        public bool Enabled { get; set; }

        [Display(Name = "Approved")]
        public bool Approved { get; set; }
    }
}
