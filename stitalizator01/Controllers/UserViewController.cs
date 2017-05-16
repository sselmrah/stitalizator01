using stitalizator01.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace stitalizator01.Controllers
{
    
    public class UserViewController : Controller
    {
        ApplicationDbContext context;
        UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
        
        public UserViewController()
        {
            context = new ApplicationDbContext();
        }
        // GET: UserView
        public ActionResult Index()
        {
            var allUsers = context.Users.ToList();

            var userVM = allUsers.Select(user => new UserViewModel { Username = user.UserName, Roles = userManager.GetRoles(user.Id).ToList() }).ToList();
            List<UserViewModel> userVM2 = new List<UserViewModel>();
            var model = new GroupedUserViewModel { Users = userVM };
            
            return View(model);
        }

        public ActionResult Delete(string userName)
        {
            var thisUser = context.Users.Where(r => r.UserName.Equals(userName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            var bets = context.Bets.Where(b => b.ApplicationUser.Id == thisUser.Id).ToList();
            context.Bets.RemoveRange(bets);
            context.SaveChanges();
            context.Users.Remove(thisUser);
            context.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}