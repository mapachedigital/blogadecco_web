// Copyright (c) 2021, Mapache Digital
// Version: 1.2
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using MDWidgets.Utils.ModelAttributes;
using System.ComponentModel.DataAnnotations;

namespace BlogAdecco.Areas.Admin.Models.ApplicationUserViewModels
{
    public class ApplicationUserEditViewModel
    {
        public string Id { get; set; } = default!;

        [Required(ErrorMessage = "The '{0}' field is required.")]
        [StringLength(80, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
        [Display(Name = "Firstname")]
        public string Firstname { get; set; } = default!;

        [Required(ErrorMessage = "The '{0}' field is required.")]
        [StringLength(80, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
        [Display(Name = "Lastname")]
        public string Lastname { get; set; } = default!;

        [Display(Name = "Display Name")]
        [StringLength(80, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
        public string? DisplayName { get; set; }

        [Required(ErrorMessage = "The '{0}' field is required.")]
        [StringLength(80, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
        [Display(Name = "Company")]
        public string Company { get; set; } = default!;

        [Required(ErrorMessage = "The '{0}' field is required.")]
        [StringLength(20, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
        [Display(Name = "Mobile Phone Number")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "The Phone number field is not a valid phone number.")]
        [Phone(ErrorMessage = "Invalid Phone Number.")]
        public string? MobilePhone { get; set; }

        [Display(Name = "Position")]
        [StringLength(80, ErrorMessage = "The '{0}' field must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string? Position { get; set; }

        [Required(ErrorMessage = "The '{0}' field is required.")]
        [RequiredCheck(ErrorMessage = "You must accept the privacy policy.")]
        [Display(Name = "I agree with Grupo Adecco privacy policy")]
        public bool AcceptTermsOfService { get; set; }

        [Required(ErrorMessage = "The '{0}' field is required.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        [StringLength(256, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; } = default!;

        [Required(ErrorMessage = "The '{0}' field is required.")]
        [StringLength(80, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
        [Display(Name = "Role")]
        public string Role { get; set; } = default!;

        [StringLength(100, ErrorMessage = "The '{0}' field must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Enabled")]
        public bool Enabled { get; set; }

        [Display(Name = "Approved")]
        public bool Approved { get; set; }
    }
}
