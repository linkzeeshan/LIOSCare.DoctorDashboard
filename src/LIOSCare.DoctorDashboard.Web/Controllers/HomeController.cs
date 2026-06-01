using Microsoft.AspNetCore.Mvc;

namespace LIOSCare.DoctorDashboard.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Dashboard");
    [Route("home/error")]
    public IActionResult Error() => View();
}
