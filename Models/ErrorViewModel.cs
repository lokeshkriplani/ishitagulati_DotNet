namespace ishitagualti.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "null";

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "null";

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}


public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }="null";

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }="null";

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }="null";
}





