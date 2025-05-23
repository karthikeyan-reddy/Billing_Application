using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Data;
using ShoppingApp.Models;
using System.Security.Claims;
using System.Text;

namespace ShoppingApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ShoppingApplicationContext _context;

        public AccountController() { 

            _context = new ShoppingApplicationContext();
        }

        [HttpGet]
        public ActionResult Login()
        {
            var login = new Login();

            return View(login);
        }

        [HttpPost]
        public ActionResult Login(Login login)
        {
            var getUser = _context.Users.FirstOrDefault(u => u.UserName == login.UserName);

            if(getUser != null && getUser.Password == login.Password)
            {
                var claims = new List<Claim>()
                {
                     new Claim(ClaimTypes.Name, getUser.UserName),
                     new Claim("FullName", getUser.FirstName + " " + getUser.LastName),
                     new Claim("Business Name",getUser.BusinessName??"")
                }; 

                var claimsIdentity = new ClaimsIdentity(claims, "cookie" ); 
                var claimsPrincipal = new ClaimsPrincipal(new[] { claimsIdentity });
                HttpContext.SignInAsync(claimsPrincipal);


                return RedirectToAction("Index", "Home");
            }
            else
            {
                login.Error = "Invalid username or password";

                return View(login);
            }
                
        }

        [HttpGet]
        public ActionResult Register() => View();

        [HttpPost]
        public ActionResult Register(Register registerUser)
        {
            try
            {
                var checkBusinessName = _context.Users.FirstOrDefault(u => u.BusinessName == registerUser.user.BusinessName);
                var userNameCheck = _context.Users.FirstOrDefault(u => u.UserName == registerUser.user.UserName);

                if (checkBusinessName != null)
                {
                    registerUser.Error = "Business name already exists";
                    return View(registerUser);
                }
                else if (userNameCheck != null)
                {
                    registerUser.Error = "Username already exists";
                    return View(registerUser);
                }
                else
                {
                    _context.Users.Add(registerUser.user);
                    _context.SaveChanges();
                    return RedirectToAction("Login", "Account");
                }
            }

            catch (Exception ex)
            {
                registerUser.Error = "Error While Registering User";
                return View(registerUser);
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
