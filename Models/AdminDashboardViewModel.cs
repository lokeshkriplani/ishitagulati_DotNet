namespace ishitagualti.Models
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;


public class AdminDashboardViewModel
{
    public IEnumerable<MakeupBooking> Bookings { get; set; }
    public IEnumerable<PdfLink> PdfLinks { get; set; }
    public SelectList Users { get; set; }
    public IFormFile PdfFile { get; set; }
    public string SelectedUserId { get; set; }
    public string CurrentUserId { get; set; } // Add this property
        public Dictionary<string, string> QrCodes { get; set; }

}

public class GuestDashboardViewModel
{
    public IEnumerable<PdfLink> PdfLinks { get; set; }
    public string CurrentUserId { get; set; } // Add this property
}



    public class LinkPdfViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        [DataType(DataType.Upload)]
        public IFormFile PdfFile { get; set; }

        public SelectList Users { get; set; }
    }


    public class PdfLink
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FilePath { get; set; }
    }
}
