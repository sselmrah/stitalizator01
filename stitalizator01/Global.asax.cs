using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using stitalizator01.Models;
using Microsoft.Bot.Connector;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace stitalizator01
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //int x = 0;
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }

    public class MvcApplication : System.Web.HttpApplication
    {
        public static System.Timers.Timer timer = new System.Timers.Timer(60000); // This will raise the event every one minute.

        public static ApplicationDbContext db = new ApplicationDbContext();
        public static TimeSpan utcMoscowShift = TimeSpan.FromHours(3);
        private int minutesElapsed = 0;
        //public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Global.asax");


        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            timer.Enabled = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            log.Info("Application Started");
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Московское время
            DateTime now = DateTime.UtcNow + utcMoscowShift;
            DateTime later = now + TimeSpan.FromMinutes(60);
            var expiredList = db.Bets.Where(b => b.Program.TimeStart < now & !b.IsLocked);
            if (expiredList != null)
            {
                if (expiredList.Count() > 0)
                {
                    foreach (Bet bet in expiredList)
                    {
                        bet.IsLocked = true;
                    }
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        log.Error("Bets Locker error.",ex);
                    }

                }
            }

            minutesElapsed++;
            //log.Info("Minutes elapsed: " + minutesElapsed.ToString());

            if (minutesElapsed >= 30)
            {
                log.Info("30 minutes elapsed.");
                List<Bet> allbets = db.Bets.Where(b => b.BetSTIplus == 0 & !b.IsLocked & b.Program.TvDate==now.Date).ToList();
                List<ConversationStarter> css = db.CSs.ToList();

                if (css.Count() > 0)
                {
                    foreach (ConversationStarter cs in css)
                    {
                        if (cs.ChannelId == "telegram")
                        {
                            Activity a = new Activity();
                            a.ServiceUrl = cs.ServiceUrl;
                            stitalizator01.Controllers.MessagesController c = new stitalizator01.Controllers.MessagesController();
                            

                            List<Bet> userBets = allbets.Where(b => b.ApplicationUser.UserName == cs.ApplicationUser.UserName).ToList();
                            int burningBetsCount = userBets.Where(b => b.Program.TimeStart <= later).Count();
                            log.Info("User " + cs.ApplicationUser.TelegramUserName + " bets \"total/soon\": "+ userBets.Count().ToString()+"/"+burningBetsCount.ToString());

                            //if (userBets.Where(b => b.Program.TimeStart <= later).Count() > 0)
                            if (burningBetsCount > 0 )
                            {
                                //if (cs.LastTimeUsed <= now - TimeSpan.FromHours(3))
                                //{
                                try
                                {
                                    c.manualTeleSend(cs.ApplicationUser.TelegramUserName, a, userBets);
                                    log.Info("Message sent to " + cs.ApplicationUser.TelegramUserName + ".");
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Something went wrong with sending a reminder.",ex);
                                }
                                //cs.LastTimeUsed = now;
                                //db.SaveChanges(); //Добавлено
                                //}
                            }


                            
                        }
                    }
                }
                minutesElapsed = 0;
            }

        }
        

    }


}
