using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using stitalizator01.Models;
using System.Net.Mail;
using System.Net;

namespace stitalizator01
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static System.Timers.Timer timer = new System.Timers.Timer(60000); // This will raise the event every one minute.
        public static System.Timers.Timer timer10 = new System.Timers.Timer(10*60000); 
        public static ApplicationDbContext db = new ApplicationDbContext();
        public static TimeSpan utcMoscowShift = TimeSpan.FromHours(3);
        private DateTime lastWarningTimeStamp = DateTime.UtcNow-TimeSpan.FromDays(1);

        protected void Application_Start()
        {

            AreaRegistration.RegisterAllAreas();
            //var ctx = new StitalizatorContext();
            //ctx.Database.Initialize(true);
            //initializeBetsDB(new ApplicationDbContext());
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            timer.Enabled = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer10.Enabled = true;
            timer10.Elapsed += new System.Timers.ElapsedEventHandler(timer10_Elapsed);


        }

        private void Contact()
        {
            DateTime curTime = DateTime.UtcNow;
            List<ApplicationUser> users = db.Users.ToList();
            List<Bet> userBets = new List<Bet>();
            //var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
            var message = new MailMessage();

            foreach (ApplicationUser user in users)
            {
                userBets = db.Bets.Where(b => b.ApplicationUser.UserName == user.UserName & b.Program.TvDate == curTime.Date & b.BetSTIplus == 0).ToList();

                if (user.UserName != "admin")
                {
                    if (userBets.Count() > 0)
                    {
                        message.To.Add(new MailAddress(user.Email));
                    }
                }
            }
            if (message.To.Count() > 0)
            {
                //message.To.Add(new MailAddress("amosendz@gmail.com"));  // replace with valid value 
                message.From = new MailAddress("stitalizator@gmail.com");  // replace with valid value
                message.Subject = "Нужно сделать ставки на " + curTime.Date.ToString("dd.MM.yyyy");
                message.Body = "В далеком светлом будущем здесь будет перечень ставок.";
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = "stitalizator@gmail.com",  // replace with valid value
                        Password = "945549Co"  // replace with valid value
                    };
                    smtp.Credentials = credential;
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.Send(message);
                    //return RedirectToAction("Sent");
                    //return Content("Sent!");
                }
            }
            else
            {
               // return Content("Все молодцы! Отправлять некому!");
            }
        }


        private void timer10_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DateTime now = DateTime.UtcNow + utcMoscowShift;
            List<Bet> burningBets = db.Bets.Where(b => b.Program.TimeStart < (now+TimeSpan.FromHours(1))).ToList();
            if (lastWarningTimeStamp.Date < now.Date)
            {                
                if (burningBets.Count > 0)
                {
                    Contact();
                    lastWarningTimeStamp = now;    
                }
            }

        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Московское время
            DateTime now = DateTime.UtcNow + utcMoscowShift;

            var expiredList = db.Bets.Where(b => b.Program.TimeStart < now);
            foreach (Bet bet in expiredList)
            {
                bet.IsLocked = true;
            }
            db.SaveChanges();
            
        }

        /*
        private void initializeBetsDB(ApplicationDbContext context)
        {
            var programs = new List<Program>
            {
                new Program{ProgTitle="Вечерние новости", TvDate=DateTime.Parse("04.04.2017"), TimeStart=DateTime.Parse("04.04.2017 18:00:00"), TimeEnd=DateTime.Parse("04.04.2017 18:25:00"), ChannelCode=10},
                new Program{ProgTitle="Первая студия", TvDate=DateTime.Parse("04.04.2017"), TimeStart=DateTime.Parse("04.04.2017 18:25:00"), TimeEnd=DateTime.Parse("04.04.2017 21:00:00"), ChannelCode=10},
                new Program{ProgTitle="Время", TvDate=DateTime.Parse("04.04.2017"), TimeStart=DateTime.Parse("04.04.2017 21:00:00"), TimeEnd=DateTime.Parse("04.04.2017 21:30:00"), ChannelCode=10},
                new Program{ProgTitle="Волчье солнце", TvDate=DateTime.Parse("04.04.2017"), TimeStart=DateTime.Parse("04.04.2017 21:30:00"), TimeEnd=DateTime.Parse("04.04.2017 23:35:00"), ChannelCode=10},
            };
            programs.ForEach(p => context.Programs.Add(p));
            context.SaveChanges();
        }
         */
    }
}
