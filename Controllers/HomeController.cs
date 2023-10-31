using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LoginAndRegistration.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;


namespace LoginAndRegistration.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private MyContext _context;

    public HomeController(ILogger<HomeController> logger, MyContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        List<User> AllUsers = _context.Users.ToList();
        return View();
    }

    [HttpPost]
    [Route("procesa/registro")]
    public IActionResult ProcesaRegistro(User newUser)
    {
        if (ModelState.IsValid)
        {
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
            _context.Users.Add(newUser);
            _context.SaveChanges();
            HttpContext.Session.SetString("UserName", newUser.FirstName);
            return RedirectToAction("Success");
        }
        return View("Index");
    }
    [HttpGet("success")]
    [SessionCheck]
    public IActionResult Success()
    {
        return View("Success");
    }

    [HttpPost]
    [Route("procesa/login")]
    public IActionResult ProcesaLogin(LoginUser loginUser)
    {
        if (ModelState.IsValid)
        {
            User? user = _context.Users.FirstOrDefault(us => us.Email == loginUser.EmailLogin);

            if (user != null)
            {
                PasswordHasher<LoginUser> Hasher = new PasswordHasher<LoginUser>();
                var result = Hasher.VerifyHashedPassword(loginUser, user.Password, loginUser.PasswordLogin);

                if (result != 0)
                {
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    return RedirectToAction("Success");
                }
                ModelState.AddModelError("PasswordLogin", "Credenciales incorrectas");


            }
            ModelState.AddModelError("EmailLogin", "El correo electronico no existe ne la base de datos.");
            return View("Index");
        }
        return View("Index");
    }

    [HttpGet("logout")]
    public IActionResult LogOut()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }





    public IActionResult Privacy()
    {
        return View();
    }

    public class SessionCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string? email = context.HttpContext.Session.GetString("UserEmail");
            if (email == null)
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }






    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
