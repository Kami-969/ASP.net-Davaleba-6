using C.R.E.A.M.Context;
using C.R.E.A.M.Models;
using C.R.E.A.M.ViewModel;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace C.R.E.A.M.Controllers
{
    
    public class AccountController : Controller
    {
        private StoreContext _storeContext = new StoreContext();
        

        // GET: Account
        public ActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignUp(RegisterUser newUser)
        {
            if(!ModelState.IsValid)
            {
                return View();
            }

            var result = _storeContext.Users.Where(x => x.Username == newUser.Username).FirstOrDefault();
            if(result != null)
            {     
                ModelState.AddModelError("Username", "ასეთი მომხმარებელი უკვე არსებობს");
                return View();
            }

            User user = new User()
            {
                Email = newUser.Email,
                Password = newUser.Password,
                Username = newUser.Username,
                ConfirmationCode = Guid.NewGuid()
            };

            Uri uri = new Uri(Request.Url.AbsoluteUri);

            var urlHost = uri.Scheme + Uri.SchemeDelimiter + uri.Host + ":" + uri.Port;

            var text = $"გთხოვთ ეწვიოთ ბმულს და დაასრულოთ რეგისტრაცია: {urlHost}/Account/Confirmation/{user.ConfirmationCode}";
            SendMail(user.Email, text);

            _storeContext.Users.Add(user);
            _storeContext.SaveChanges();


            return RedirectToAction("login");
        }

        private void SendMail(string to, string text)
        {
            string filename = @"C:\Users\Yakimas\Desktop\Register.txt";

            if(System.IO.File.Exists(filename))
            {
                System.IO.File.Delete(filename);
            }

            using (System.IO.FileStream fs = System.IO.File.Create(filename))
            {
                byte[] innerText = new UTF8Encoding(true).GetBytes(text);
                fs.Write(innerText, 0, innerText.Length);
            }
        }

        public ActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogIn(User user)
        {
            if(!ModelState.IsValid)
            {
                ModelState.AddModelError("", "მომხმარებელი ან პაროლი არასწორია");
                return View();
            }

            var result = _storeContext.Users.Where(x => x.Username == user.Username && x.Password == user.Password).FirstOrDefault();
           

            if (result == null)
            {
                ModelState.AddModelError("", "მომხმარებელი ან პაროლი არასწორია");
                return View();
            }

            Session["Username"] = result.Username;

            return RedirectToAction("index", "store");
        }

        public ActionResult Confirmation(string id)
        {
            var user = _storeContext.Users.Where(x => x.ConfirmationCode.ToString().ToLower() == id.ToLower()).FirstOrDefault();

            user.IsActive = true;
            _storeContext.SaveChanges();

            return RedirectToAction("login");
        }

        public ActionResult ChangePassword()
        {

            var userSession = (string)Session["Username"];
            if (userSession == null)
            {
                return RedirectToAction("Login", "account");
            }

            return View();
        }

         [HttpPost]
        public ActionResult ChangePassword(RegisterUser ChangedPassowrd )
        {
            var userSession = (string)Session["Username"];

            var result = _storeContext.Users.Where(x => x.Username == userSession).FirstOrDefault();

            result.Password = ChangedPassowrd.Password;
            _storeContext.SaveChanges();

            return View();


        }
    }
}