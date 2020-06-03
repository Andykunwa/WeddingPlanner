using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private HomeContext dbContext;
        
        public HomeController(HomeContext context)
        {
            dbContext = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("signin")]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(User register)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == register.Email))
                {
                    ModelState.AddModelError("Email", "Email is already in use.");
                    return View("Index");
                }
                else
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    register.Password = Hasher.HashPassword(register, register.Password);

                    dbContext.Users.Add(register);
                    dbContext.SaveChanges();
                    HttpContext.Session.SetString("UserEmail", register.Email);
                    HttpContext.Session.SetInt32("UserId", register.UserId);
                    return RedirectToAction("Dashboard");
                }
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost("login")]
        public IActionResult LogIn(LoginUser login)
        {
            if(ModelState.IsValid)
            {
                User userInDb = dbContext.Users.FirstOrDefault(u => u.Email == login.LoginEmail);
                if(userInDb == null)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Email or Password.");
                    return View("SignIn");
                }
                
                PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(login, userInDb.Password, login.LoginPassword);
                if(result == 0)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Email or Password.");
                    return View("SignIn");
                }
                HttpContext.Session.SetString("UserEmail", login.LoginEmail);
                HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("SignIn");
            }
        }


        ///////////////////////////////////////////WEDDING PLANNER/////////////////////////////////////////////////////////////////
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            User userInDb = dbContext.Users.FirstOrDefault(u => u.Email == HttpContext.Session.GetString("UserEmail"));
            if(userInDb == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Index");
            }
            List<Wedding> AllWeddings = dbContext.Weddings.Include(a => a.Users).ThenInclude(b => b.User).ToList();
            Association currAssociation = dbContext.Associations.FirstOrDefault(a => a.UserId == userInDb.UserId);
            ViewBag.Creator = HttpContext.Session.GetString("UserEmail").ToString();
            return View(AllWeddings);
        }

        ///////////////////////////////////////WEDDING PAGE, CREATION, AND DELETION////////////////////////////////////////////////
        [HttpGet("newweddingpage")]
        public IActionResult NewWeddingPage()
        {
            return View("NewWeddingPage");
        }

        [HttpPost("createwedding")]
        public IActionResult CreateWedding(Wedding newWedding)
        {
            if(ModelState.IsValid)
            {
                string loggedUser = HttpContext.Session.GetString("UserEmail");
                newWedding.Creator = loggedUser;
                dbContext.Weddings.Add(newWedding);
                dbContext.SaveChanges();
                ViewBag.Creator = loggedUser.ToString();
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("NewWeddingPage");
            }
        }

        [HttpGet("weddinginfo/{wed_id}")]
        public IActionResult WeddingInfo(int wed_id)
        {
            Wedding currentWedding = dbContext.Weddings.Include(a => a.Users).ThenInclude(b => b.User).FirstOrDefault(w => w.WeddingId == wed_id);
            return View(currentWedding);
        }

        [HttpGet("deletewedding")]
        public IActionResult DeleteWedding(int wed_id)
        {
            Wedding removedWedding = dbContext.Weddings.FirstOrDefault(w => w.WeddingId == wed_id);
            dbContext.Weddings.Remove(removedWedding);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }


        ////////////////////////////////////////RSVP & UN-RSVP//////////////////////////////////////////////////////////
        [HttpGet("rsvp")]
        public IActionResult RSVP(int wed_id)
        {
            Wedding rsvpWedding = dbContext.Weddings.FirstOrDefault(w => w.WeddingId == wed_id);
            User rsvper = dbContext.Users.FirstOrDefault(u => u.Email == HttpContext.Session.GetString("UserEmail"));
            Association rsvp  = new Association();
            rsvp.UserId = rsvper.UserId;
            rsvp.WeddingId = wed_id;
            dbContext.Associations.Add(rsvp);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("unrsvp")]
        public IActionResult UnRSVP(int wed_id)
        {
            IEnumerable <Association> users = dbContext.Associations.Where(a => a.WeddingId == wed_id);
            Association rsvpUser = users.FirstOrDefault(e => e.UserId == HttpContext.Session.GetInt32("UserId"));
            dbContext.Remove(rsvpUser);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }


        ////////////////////////////////////////////LOGOUT METHOD//////////////////////////////////////////////////////
        [HttpGet("logout")]
        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }




        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
