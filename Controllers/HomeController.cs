using QRCoder;
using Newtonsoft.Json;
using System.Diagnostics;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using SkiaSharp;
using ZXing.SkiaSharp;
using SkiaSharp;
using Microsoft.AspNetCore.Mvc;
using ishitagualti.Models;
using ishitagualti.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Drawing; // For System.Drawing.Common
using System.Drawing.Imaging;
using System.IO;

namespace ishitagualti.Controllers
{

    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly BookingService _bookingService;

        private readonly ApplicationDbContext _context;

        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            IWebHostEnvironment webHostEnvironment,
            ILogger<HomeController> logger,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context  , BookingService bookingService
)
        {
             _webHostEnvironment = webHostEnvironment;
            _context = context;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _bookingService=bookingService;
        }

        public IActionResult Index()
        {
            return View();
        }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
public async Task<IActionResult> Register(RegisterViewModel model)
{
    if (ModelState.IsValid)
    {
        var user = new IdentityUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Handle successful registration
            return RedirectToAction("Index", "Home");
        }

        // Add errors to ModelState
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    // If we got this far, something failed, redisplay form
    return View(model);
}

[HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string returnUrl = "null")
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }


[HttpPost]
    public async Task<IActionResult> Delete_booking(int id)
    {
        var booking = await _context.MakeupBookings.FindAsync(id);
        if (booking != null)
        {
            _context.MakeupBookings.Remove(booking);
            await _context.SaveChangesAsync();
        }
            return RedirectToAction("AdminDashboard", "Home");
    }



[Authorize]
public async Task<IActionResult> AdminDashboard()
{
    var user = await _userManager.GetUserAsync(User);

    // Check if the user is the admin
    if (user != null && user.Email != "sample@sample.com")
    {
        return RedirectToAction("Login", "Home");
    }

    _logger.LogInformation("AdminDashboard action invoked.");

    IEnumerable<MakeupBooking> bookings;
    try
    {
        bookings = await _bookingService.GetBookingsAsync();
        _logger.LogInformation("Bookings fetched successfully.");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "An error occurred while fetching bookings.");
        return View("Error");
    }

    var users = await _context.Users.ToListAsync();
    var userSelectList = users.Select(u => new SelectListItem
    {
        Value = u.Id,
        Text = u.UserName
    }).ToList();

    var qrCodes = new Dictionary<string, string>();
    foreach (var u in users)
    {
        var url = Url.Action("GuestDashboard", "Home", new { id = u.Id }, protocol: Request.Scheme);
        qrCodes[u.Id] = GenerateQrCode(url);
    }

    var pdfLinksData = await _context.PdfLinks.ToListAsync();
    var pdfLinks = pdfLinksData.Select(p => new ishitagualti.Models.PdfLink
    {
        Id = p.Id,
        FilePath = p.FilePath,
        UserId = p.UserId
    }).ToList();

    var model = new AdminDashboardViewModel
    {
        Bookings = bookings,
        PdfLinks = pdfLinks,
        Users = new SelectList(userSelectList, "Value", "Text"),
        CurrentUserId = user.Id,
        QrCodes = qrCodes
    };

    return View(model);
}


public string GenerateQrCode(string text)
{
    if (string.IsNullOrEmpty(text))
    {
        throw new ArgumentException("QR code text cannot be null or empty", nameof(text));
    }

    var barcodeWriter = new ZXing.SkiaSharp.BarcodeWriter
    {
        Format = BarcodeFormat.QR_CODE,
        Options = new ZXing.Common.EncodingOptions
        {
            Height = 300,
            Width = 300
        }
    };

    using (var bitmap = barcodeWriter.Write(text))
    {
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        {
            return Convert.ToBase64String(data.ToArray());
        }
    }
}


[HttpPost]
public async Task<IActionResult> UploadPdf(AdminDashboardViewModel model)
{
    if (model.PdfFile != null && model.PdfFile.Length > 0)
    {
        var fileName = Path.GetFileName(model.PdfFile.FileName);
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await model.PdfFile.CopyToAsync(stream);
        }

        // Save the PDF link to the database
        var pdfLink = new PdfLink
        {
            FilePath = $"/uploads/{fileName}",
            UserId = model.SelectedUserId // Assuming AdminDashboardViewModel has a SelectedUserId property
        };

        _context.PdfLinks.Add(pdfLink);
        await _context.SaveChangesAsync();

        return RedirectToAction("AdminDashboard");
    }

    // Handle the case where the file is not provided or invalid
    ModelState.AddModelError("", "No file selected or file is invalid.");
    return View("AdminDashboard", model);
}





    

[Authorize]
public async Task<IActionResult> GuestDashboard()
{
    var user = await _userManager.GetUserAsync(User);

    // Check if the user is logged in
    if (user == null)
    {
        return RedirectToAction("Login", "Home");
    }

    _logger.LogInformation("GuestDashboard action invoked.");

    var pdfLinksData = await _context.PdfLinks.ToListAsync();

    // Convert ishitagualti.Data.PdfLink to ishitagualti.Models.PdfLink if necessary
    var pdfLinks = pdfLinksData.Select(p => new ishitagualti.Models.PdfLink
    {
        Id = p.Id,
        FilePath = p.FilePath,
        UserId = p.UserId
    }).ToList();

    var model = new GuestDashboardViewModel
    {
        PdfLinks = pdfLinks,
        CurrentUserId = user.Id // Add this line to set the current user's ID
    };

    return View(model);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model)
{
    if (ModelState.IsValid)
    {
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if(model.Email == "sample@sample.com"){
                return RedirectToAction("AdminDashboard", "Home");

            }
            return RedirectToAction("GuestDashboard", "Home");


        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }
    }

    // If we got this far, something failed; redisplay form
    return View(model);
}

    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
        public IActionResult Pricelist()
        {
            return View("pricelist");
        }

        public IActionResult BookNow()
        {
            return View("booknow");
        }

        [HttpPost]
        [Route("/submit")] // Ensure the route matches the one used in your fetch call
        public async Task<IActionResult> Submit([FromBody] MakeupBooking booking)
        {
            _logger.LogInformation("Submit action called.");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.MakeupBookings.Add(booking);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Booking saved successfully.");
                    return Ok(new { message = "Your booking request has been sent successfully!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving booking.");
                    return StatusCode(500, new { message = "Internal server error." });
                }
            }
            else
            {
                _logger.LogWarning("Invalid model state: {@ModelState}", ModelState);
            }

            return BadRequest(new { message = "There was a problem with your booking request. Please try again." });
        }
    
        public IActionResult PartyMakeup()
        {
            _logger.LogError("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@Invalid model state");
            return View("party_makeup");
        }

        public IActionResult BridalMakeup()
        {
            return View("bridal_booking");
        }

        public IActionResult Portfolio()
        {
            return View("portfolio");
        }

        public IActionResult Academy()
        {
            return View("academy");
        }

        public IActionResult About()
        {
            return View("about");
        }

        public IActionResult CallNow()
        {
            return View("callnow");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpGet]
    public async Task<IActionResult> LinkPdfToUser()
    {
        var users = await _userManager.Users.ToListAsync();
        var model = new LinkPdfViewModel
        {
            Users = new SelectList(users, "Id", "Email")
        };
        return View(model);
    }

    // POST: Handle the form submission
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LinkPdfToUser(LinkPdfViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            var fileName = Path.GetFileName(model.PdfFile.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.PdfFile.CopyToAsync(stream);
            }

            // Here, you can save the file path or any related information to the user's record in your database.
            // For example, if you have a property in your user class to store the PDF path, you can update it here.

            // Save changes to the database
            await _context.SaveChangesAsync();

            return RedirectToAction("AdminDashboard");
        }

        var users = await _userManager.Users.ToListAsync();
        model.Users = new SelectList(users, "Id", "Email");
        return View(model);
    }
    }
}
