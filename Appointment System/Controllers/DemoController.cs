using Microsoft.AspNetCore.Mvc;

namespace Appointment_System.Controllers
{
    public class DemoController : Controller
    {
        [HttpGet("/chat-demo")]
        public IActionResult ChatDemo()
        {
            return File("~/chat-demo.html", "text/html");
        }
    }
} 