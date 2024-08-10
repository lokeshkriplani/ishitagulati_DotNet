namespace ishitagualti.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// Models/MakeupBooking.cs
public class MakeupBooking
{
    public int Id { get; set; } 
    public string Name { get; set; } = "null";
    public string Mobile { get; set; } = "null";
    public string RecipientEmail { get; set; } = "null";
    public DateTime EventDate { get; set; }
    public string MakeupType { get; set; } = "null";
    public int NumberOfMakeups { get; set; }
    public TimeSpan ReadyTime { get; set; }
    public string EventLocation { get; set; } = "null";
}




