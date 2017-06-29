﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Web.Http;

using System.Web.Optimization;
using System.Web.Routing;
using stitalizator01.Models;
using System.Net.Mail;
using System.Net;
using System.ComponentModel;
using System.Diagnostics;


using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;

using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading.Tasks;

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
        
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            timer.Enabled = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
        }
       
        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Московское время
            DateTime now = DateTime.UtcNow + utcMoscowShift;
            DateTime later = now + TimeSpan.FromMinutes(30);
            var expiredList = db.Bets.Where(b => b.Program.TimeStart < now & !b.IsLocked);
            if (expiredList != null)
            {
                if (expiredList.Count() > 0)
                {
                    foreach (Bet bet in expiredList)
                    {
                        bet.IsLocked = true;
                    }
                    db.SaveChanges();

                }
            }
            
            minutesElapsed++;
             
            if (minutesElapsed == 30)
            {
                List<Bet> allbets = db.Bets.Where(b => b.BetSTIplus == 0 & !b.IsLocked).ToList();
                foreach (ConversationStarter cs in db.CSs)
                {
                    if (cs.ChannelId=="telegram")
                    {
                        List<Bet> userBets = allbets.Where(b => b.ApplicationUser.UserName == cs.ApplicationUser.UserName).ToList();
                        if (userBets.Where(b => b.Program.TimeStart<later).Count()>0)
                        {
                            //Дописать, возможно, добавив lastTimeWarned юзеру
                        }
                    }
                }
                minutesElapsed = 0;
            }
            
        }
    }
}
