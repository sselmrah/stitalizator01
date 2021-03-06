﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using stitalizator01.Models;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Text;

namespace stitalizator01.Controllers
{
    
    public class ProgramsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private static readonly log4net.ILog logP = log4net.LogManager.GetLogger("ProgramsController.cs");

        [HttpGet]
        public ActionResult Index(string date = "today")
        {
            DateTime curDay;
            if (date == "today")
            {
                curDay = DateTime.UtcNow.Date;
            }
            else
            {
                curDay = DateTime.Parse(date);
            }
            List<Program> todayProgList = new List<Program>();
            ViewBag.curDate = curDay.ToString("yyyy-MM-dd");
            todayProgList = db.Programs.Where(o => o.TvDate == curDay.Date).ToList();
            string msg = "";
            msg = "User " + System.Web.HttpContext.Current.User.Identity.Name + " has opened Programs/Index page";
            logP.Info(msg);
            

            return View(todayProgList);
        }



        // GET: Programs
        [HttpGet]
        public ActionResult ProgsByDate(string date = "today",string channelsListStr = "1TV", string filter="9-23")
        {

            DateTime curDay;
            if (date == "today")
            {
                curDay = DateTime.UtcNow.Date;
            }
            else
            {
                curDay = DateTime.Parse(date);
            }
            
            List<Program> todayProgList = new List<Program>();                        
            List<string> channelsList = getChannelTagsListFromString(channelsListStr);            

            if (channelsList.Count > 0)
            {
                foreach (string channelTag in channelsList)
                {
                    updateSchedule(curDay.ToString("dd.MM.yyyy"), channelTag, filter);
                }
            }
            //todayProgList = db.Programs.Where(o => o.TvDate == curDay.Date).ToList();
            //return View(todayProgList);
            string msg = "";
            msg = "User " + System.Web.HttpContext.Current.User.Identity.Name + " has opened Programs/ProgsByDate page for "+ curDay.ToString("dd.MM.yyyy");
            logP.Info(msg);
            return RedirectToAction("Index", new { date = curDay.ToString("dd.MM.yyyy") });
        }

        public List<string> getChannelTagsListFromString (string channelListStr)
        {
            if (channelListStr.Last() == ';') { channelListStr = channelListStr.Substring(0, channelListStr.Length - 1); }
            List<string> resultList = new List<string>();
            if (channelListStr.IndexOf(";")>0)
            {
                resultList = channelListStr.Split(';').ToList();
            }
            else
            {
                resultList.Add(channelListStr);
            }

            return resultList;
        }

        public ActionResult Clear(string date)
        {            
            DateTime curDate = DateTime.Parse(date);
            var listToRemove = db.Programs.Where(p => (p.IsBet == false & p.TvDate == curDate.Date));
            
            if (listToRemove.Count() > 0)
            {
                db.Programs.RemoveRange(listToRemove);
                db.SaveChanges();
            }
            string msg = "";
            msg = "User " + System.Web.HttpContext.Current.User.Identity.Name + " has cleared unused programs";
            logP.Info(msg);
            return RedirectToAction("Index");
        }


        // GET: Programs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Program program = db.Programs.Find(id);
            if (program == null)
            {
                return HttpNotFound();
            }
            return View(program);
        }

        // GET: Programs/Create
        public ActionResult Create()
        {
            List<Channel> channelList = db.Channels.ToList();
            List<string> channelNames = new List<string>();
            foreach(Channel c in channelList)
            {
                channelNames.Add(c.ChannelName);
            }
            var query = new SelectList(channelNames);
            ViewData["ChannelCode"] = query;
            return View();
        }

        // POST: Programs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProgramID,ChannelCode,ProgTitle,TvDate,TimeStart,TimeEnd,ProgCat,ProgDescr,ShareStiPlus,ShareStiMob,ShareSti,ShareMos18,ShareRus18")] Program program)
        {
            if (ModelState.IsValid)
            {
                db.Programs.Add(program);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            string msg = "";
            msg = "User " + System.Web.HttpContext.Current.User.Identity.Name + " has created program "+ program.ProgTitle;
            logP.Info(msg);
            return View(program);
        }

        // GET: Programs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Program program = db.Programs.Find(id);
            if (program == null)
            {
                return HttpNotFound();
            }
            return View(program);
        }

        // POST: Programs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProgramID,ChannelCode,ProgTitle,TvDate,TimeStart,TimeEnd,ProgCat,ProgDescr,ShareStiPlus,ShareStiMob,ShareSti,ShareMos18,ShareRus18,IsBet")] Program program)
        {
            if (ModelState.IsValid)
            {
                program.TimeStart = program.TvDate + TimeSpan.FromHours(program.TimeStart.Hour)+TimeSpan.FromMinutes(program.TimeStart.Minute);

                db.Entry(program).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            string msg = "";
            msg = "User " + System.Web.HttpContext.Current.User.Identity.Name + " has edited program "+program.ProgTitle;
            logP.Info(msg);
            return View(program);
        }
        

        [HttpPost]
        public ActionResult updateBetsUsers(int id)
        {
            Program curProg = db.Programs.Find(id);
            List<ApplicationUser> users = db.Users.ToList();
            List<Bet> bets = db.Bets.Where(b => b.Program.ProgramID == curProg.ProgramID).ToList();
            List<ApplicationUser> curUsers = new List<ApplicationUser>();
            int counter = 0;
            foreach (Bet b in bets)
            {
                if (!curUsers.Contains(b.ApplicationUser))
                {
                    curUsers.Add(b.ApplicationUser);
                }                
            }
            foreach (ApplicationUser u in users)
            {
                if (!curUsers.Contains(u))
                {
                    if (u.UserName != "Admin" & u.UserName !="Виктория")
                    {
                        Bet curBet = new Bet();
                        curBet.ProgramID = curProg.ProgramID;
                        curBet.Program = curProg;
                        curBet.TimeStamp = DateTime.UtcNow + MvcApplication.utcMoscowShift;
                        curBet.ApplicationUser = u;
                        db.Bets.Add(curBet);
                        counter++;
                    }
                }
            }
            db.SaveChanges();
            string msg = "";
            msg = "User " + System.Web.HttpContext.Current.User.Identity.Name + " has added a program "+curProg.ProgTitle+" to "+counter.ToString()+" users' bets";
            logP.Info(msg);
            return Content("Added bets");
        }
    

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult UpdateBet(int id)
        {
            Program curProg = db.Programs.Find(id);
            curProg.IsBet = !curProg.IsBet;
            db.Entry(curProg).State = EntityState.Modified;
            
            if (curProg.IsBet)
            {
                updateScoresGambled(curProg.TvDate, true, curProg.IsHorse); 
                foreach (ApplicationUser user in db.Users)
                {
                    if (user.UserName != "admin" & user.UserName !="Виктория")
                    {
                        Bet curBet = new Bet();
                        curBet.ProgramID = curProg.ProgramID;
                        curBet.Program = curProg;
                        curBet.TimeStamp = DateTime.UtcNow + MvcApplication.utcMoscowShift;
                        curBet.ApplicationUser = user;
                        db.Bets.Add(curBet);
                    }
                }
            }
            else
            {
                updateScoresGambled(curProg.TvDate, false, curProg.IsHorse);                
                var x = db.Bets.Where(o => o.ProgramID == curProg.ProgramID);
                db.Bets.RemoveRange(x);                

            }
            db.SaveChanges();
            string msg = "";
            msg = "User " + System.Web.HttpContext.Current.User.Identity.Name + " changed program's " + curProg.ProgTitle + " bet state to " + curProg.IsBet.ToString();
            logP.Info(msg);
            return PartialView(curProg);
        }

        public void updateScoresGambled (DateTime tvDate, bool add, bool isHorse)
        {
            var periods = db.Periods.Where(p => (p.BegDate <= tvDate) & (p.EndDate >= tvDate));
            int pointsToAdd = 3;
            if (isHorse) { pointsToAdd = pointsToAdd * 2; }
            if (!add) { pointsToAdd = pointsToAdd * (-1); }
            if (periods != null)
            {
                foreach (Period period in periods)
                { 
                    period.ScoresGambled += pointsToAdd;
                    db.Entry(period).State = EntityState.Modified;
                }
            }
        }

        [HttpPost]
        public ActionResult enterSingleResult(string ProgramID, string ShareStiPlus)
        {
            int prId = Convert.ToInt32(ProgramID);
            //21.10.17 switched next two lines
            //Program program = db.Programs.Where(p => p.ProgramID == prId).FirstOrDefault();
            Program program = db.Programs.Find(prId);
            program.ShareStiPlus = Convert.ToDouble(ShareStiPlus.Replace(".",","));
            db.Entry(program).State = EntityState.Modified;
            db.SaveChanges(); //added 21.10.17
            var betsList = db.Bets.Where(b => b.Program.ProgramID == program.ProgramID);
            foreach (Bet bet in betsList)
            {
                
                bet.ScoreClassic = calculateScoreClassic(bet);
                bet.ScoreOLS = calculateScoreOLS(bet.BetSTIplus, (float)bet.Program.ShareStiPlus);
                db.Entry(bet).State = EntityState.Modified;
            }
            db.SaveChanges(); //added 21.10.17
            List<Period> periodsList = db.Periods.Where(p => (p.BegDate <= program.TvDate & program.TvDate <= p.EndDate)).ToList();
            /*
            //This code is probably slowing down the entire application
            //disabled 22.10.17
            foreach (Period period in periodsList)
            {
                var userResults = db.Bets.Where(bet => (bet.Program.TvDate >= period.BegDate & bet.Program.TvDate <= period.EndDate))
                                         .GroupBy(b => b.ApplicationUser,
                                                  b => b.ScoreClassic,
                                                  (key, g) => new
                                                  {
                                                      PersonId = key,
                                                      Score = g.Sum()
                                                  })
                                         .OrderBy(p => p.Score)
                                                  ;

                period.ApplicationUser = userResults.First().PersonId;
            }
            db.SaveChanges();
            */

            string msg = "";
            msg = "User " + System.Web.HttpContext.Current.User.Identity.Name + " has entered a result of "+ShareStiPlus.ToString() +" to program " + program.ProgTitle;
            logP.Info(msg);
            return Content(program.ProgTitle + ": " + program.ShareStiPlus);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult enterResult([Bind(Include = "ProgramID,ChannelCode,ProgTitle,TvDate,TimeStart,TimeEnd,ProgDescr,ProgCat,ShareStiPlus,ShareStiMob,ShareSti,ShareMos18,ShareRus18, IsBet")] Program program)
        {
            if (ModelState.IsValid)
            {
                db.Entry(program).State = EntityState.Modified;
                var betsList = db.Bets.Where(b => b.Program.ProgramID == program.ProgramID);
                foreach (Bet bet in betsList)
                {
                    //bet.ScoreClassic=calculateScoreClassic(bet.BetSTIplus, (float)bet.Program.ShareStiPlus);
                    bet.ScoreClassic = calculateScoreClassic(bet);
                    bet.ScoreOLS = calculateScoreOLS(bet.BetSTIplus, (float)bet.Program.ShareStiPlus);
                    db.Entry(bet).State = EntityState.Modified;                    
                }
                List<Period> periodsList = db.Periods.Where(p => (p.BegDate <= program.TvDate & program.TvDate <= p.EndDate)).ToList();

                foreach (Period period in periodsList)
                {
                    var userResults = db.Bets.Where(bet => (bet.Program.TvDate >= period.BegDate & bet.Program.TvDate <= period.EndDate))
                                             .GroupBy(b => b.ApplicationUser,
                                                      b => b.ScoreClassic,
                                                      (key, g) => new
                                                      {
                                                          PersonId = key,
                                                          Score = g.Sum()
                                                      })
                                             .OrderBy(p => p.Score)
                                                      ;

                    period.ApplicationUser = userResults.First().PersonId;

                    

                }

                db.SaveChanges();
                string msg = "";
                msg = "User " + System.Web.HttpContext.Current.User.Identity.Name + " has entered a result of " + program.ShareStiPlus.ToString() + " to program " + program.ProgTitle;
                logP.Info(msg);
                //return RedirectToAction("Index");
            }

            return RedirectToAction("Index", new { date = program.TvDate.ToString("dd.MM.yyyy") });
        }


        //public float calculateScoreClassic(float bet, float result)
        public float calculateScoreClassic(Bet curBet)
        {
            float bet = curBet.BetSTIplus;
            float result = (float)curBet.Program.ShareStiPlus;
            //Округляем до 0.5
            bet = (float)(Math.Round(bet * 2) / 2);
            result = (float)(Math.Round(result * 2) / 2);
            float score = 0;
            float tempResult = 0;

            tempResult = Math.Abs(result - bet);
            if (tempResult <= 2) { score = 1; }
            if (tempResult <= 1) { score = 2; }
            if (tempResult <= 0.5) { score = 3; }
            
            if (curBet.Program.IsHorse) { score = score * 2; }

            return score;
        }

        //Ordinary least squares result
        public float calculateScoreOLS(float bet, float result)
        {
            float score = 0;
            score = (float)Math.Pow((result - bet),2);

            return score;
        }

        // GET: Programs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Program program = db.Programs.Find(id);
            if (program == null)
            {
                return HttpNotFound();
            }

            return View(program);
        }

        // POST: Programs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Program program = db.Programs.Find(id);
            db.Programs.Remove(program);
            if (program.IsBet)
            {
                updateScoresGambled(program.TvDate, false, program.IsHorse);
            }
            db.SaveChanges();
            string msg = "";
            msg = "User " + System.Web.HttpContext.Current.User.Identity.Name + " has deleted program " + program.ProgTitle;
            logP.Info(msg);
            return RedirectToAction("Index");
        }


        public void updateSchedule(string dateStr="05.04.2017", string chCodeStr = "1TV", string filter="9-23")
        {
            DateTime curDate = DateTime.Parse(dateStr);

            string curChannelName = "";

            //foreach(Tuple<string,string> ch in chDict)
            List<Channel> chList = db.Channels.ToList();
            foreach (Channel channel in chList)
            {
                if (channel.ChannelTag==chCodeStr)
                {
                    curChannelName = channel.ChannelName;
                    break;
                }
            }

            if (getScheduleByDateChannel(dateStr))
            {
                var progs = db.Programs.Where(o => o.TvDate == curDate & o.ChannelCode == chCodeStr);
                if (progs.Count()>0)
                {
                    db.Programs.RemoveRange(progs);
                }
                getScheduleByDateChannel(dateStr, chCodeStr);
                //string[] lines;
                var list = new List<string>();
                var fileStream = new FileStream(@Path.Combine(HttpContext.ApplicationInstance.Server.MapPath("~/App_Data"), "Temp", "result.txt"), FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream,Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        list.Add(line);
                    }
                }
                DateTime dt = DateTime.Parse("01/01/2017");
                List<Program> progList = new List<Program>();
                foreach (string l in list)
                {                    
                    if (l.Length > 5)
                    {
                        if (l.IndexOf('\t') >=0)
                        {
                            if (dt.Date == curDate.Date)
                            {
                                Program curProg = new Program();
                                string[] curProgString = l.Split('\t');
                                curProg.TimeStart = DateTime.Parse(dt.ToString("dd.MM.yyyy") + " " + curProgString[0].Trim());
                                curProg.TimeEnd = DateTime.Parse(dt.ToString("dd.MM.yyyy") + " " + curProgString[1].Trim());
                                curProg.ProgTitle = curProgString[2].Trim();
                                curProg.ProgCat = curProgString[3].Trim();
                                string[] curCatString = curProgString[3].Trim().Split(';');
                                if (curCatString.Count()>0)
                                {
                                    int num = 0;
                                    foreach (string c in curCatString)
                                    {
                                        num++;
                                        string curCatName = c.Trim();
                                        Category newCat = new Category();
                                        newCat.CatName = curCatName;
                                        newCat.CatNum = num;
                                        if (!db.Categories.Any(x=> x.CatName==curCatName && x.CatNum==num))
                                        {
                                            db.Categories.Add(newCat);
                                            db.SaveChanges();
                                        }
                                        
                                    }
                                }
                                curProg.TvDate = dt.Date;
                                curProg.ChannelCode = curChannelName;
                                //add categories here
                                if (curProg.ProgCat != null)
                                {

                                    Category newCat1 = new Category();
                                    Category newCat2 = new Category();
                                    if (curCatString.Count() > 0)
                                    {
                                        int num = 0;
                                        foreach (string c in curCatString)
                                        {
                                            string curCatName = curCatString[num];


                                            if (num == 0)
                                            {
                                                if (db.Categories.Where(x => x.CatName == curCatName.Trim()).FirstOrDefault() != null)
                                                {
                                                    curProg.Cat1 = db.Categories.Where(x => x.CatName == curCatName.Trim() && x.CatNum == 1).FirstOrDefault();
                                                }
                                                else
                                                {
                                                    newCat1.CatName = curCatString[num].Trim();
                                                    newCat1.CatNum = 1;
                                                    curProg.Cat1 = newCat1;
                                                }

                                            }
                                            else
                                            {
                                                if (db.Categories.Where(x => x.CatName == curCatName.Trim()).FirstOrDefault() != null)
                                                {
                                                    curProg.Cat2 = db.Categories.Where(x => x.CatName == curCatName.Trim() && x.CatNum == 2).FirstOrDefault();
                                                }
                                                else
                                                {
                                                    newCat2.CatName = curCatString[num].Trim();
                                                    newCat2.CatNum = 2;
                                                    curProg.Cat2 = newCat2;
                                                }
                                            }

                                            num++;
                                        }
                                    }
                                    //
                                }

                                    progList.Add(curProg);
                            }
                        }
                        else
                        {
                            //Дата
                            if (l.IndexOf(":") < 0)
                            {
                                dt = DateTime.Parse(l);
                            }
                        }
                    }
                }
                List<Program> filteredProgList = new List<Program>();
                switch (filter)
                {
                        /*
                    case "9-23":
                        foreach(Program p in progList)
                        {
                            if (p.TimeStart.Hour>=9 & p.TimeStart.Hour<23)
                            {
                                filteredProgList.Add(p);
                            }
                        }
                        break;
                         */ 
                    case "none":
                        filteredProgList = progList;
                        break;
                    default:
                        string timeStart = filter.Substring(0,filter.IndexOf("-"));
                        string timeEnd = filter.Substring(filter.IndexOf("-")+1);
                        foreach (Program p in progList)
                        {
                            if (p.TimeStart.Hour >= Convert.ToInt32(timeStart) & p.TimeStart.Hour < Convert.ToInt32(timeEnd))
                            {
                                filteredProgList.Add(p);
                            }
                        }
                        break;
                }
                db.Programs.AddRange(filteredProgList);
                db.SaveChanges();
                string msg = "";
                msg = "User " + System.Web.HttpContext.Current.User.Identity.Name + " has updated schedule for " + dateStr;
                logP.Info(msg);
                //lines = list.ToArray();
            }


        }


        public bool getScheduleByDateChannel(string dateStr = "05.04.2017", string chCodeStr = "1TV")
        {
            bool result = false;
            List<string> sched = new List<string>();
            string targetUrl = "";            
            List<Tuple<string, DateTime, string, string, string, DateTime>> schedParsed = new List<Tuple<string, DateTime, string, string, string, DateTime>>();
            string url = "http://xmltv.s-tv.ru/xchenel.php?xmltv=1&pass=jJoM88wN54&show=2&login=tv4181";
            stitalizator01.CookieAwareWebClient client = new stitalizator01.CookieAwareWebClient();
            Encoding win1251 = Encoding.GetEncoding("windows-1251");
            client.Encoding = win1251;
            string xmlString = client.DownloadString(url);            
            string[] delim = {"<File>"};
            sched = xmlString.Split(delim,System.StringSplitOptions.RemoveEmptyEntries).ToList();

            string name = "";
            string efirWeek = "";
            string channel = "";
            string channelID = "";
            string variant = "";
            DateTime dateTime;
            DateTime efirWeekDT;
            
            foreach(string s in sched)
            {
                if (s.IndexOf("<Name>")>=0)
                {
                    name = s.Substring(s.IndexOf("<Name>") + 6, s.IndexOf("</Name>") - s.IndexOf("<Name>") - 6);
                    efirWeek = s.Substring(s.IndexOf("<EfirWeek>") + 10, s.IndexOf("</EfirWeek>") - s.IndexOf("<EfirWeek>") - 10);
                    efirWeekDT = DateTime.Parse(efirWeek);
                    channel = s.Substring(s.IndexOf("<Channel>") + 9, s.IndexOf("</Channel>") - s.IndexOf("<Channel>") - 9);
                    channelID = s.Substring(s.IndexOf("<ChannelID>") + 11, s.IndexOf("</ChannelID>") - s.IndexOf("<ChannelID>") - 11);
                    variant = s.Substring(s.IndexOf("<Variant>") + 9, s.IndexOf("</Variant>") - s.IndexOf("<Variant>") - 9);
                    dateTime = DateTime.Parse(s.Substring(s.IndexOf("<DateTime>") + 10, s.IndexOf("</DateTime>") - s.IndexOf("<DateTime>") - 10));                    
                    Tuple<string, DateTime, string, string, string, DateTime> curTuple = new Tuple<string, DateTime, string, string, string, DateTime>(name,efirWeekDT,channel,channelID,variant,dateTime);
                    schedParsed.Add(curTuple);
                }
            }
            DateTime curDate = DateTime.Parse(dateStr);
            foreach(Tuple<string,DateTime,string,string,string,DateTime> t in schedParsed)
            {
                if (chCodeStr==t.Item4 & (t.Item2<=curDate & curDate<=t.Item2+TimeSpan.FromDays(6)) & t.Item5=="R")
                {
                    targetUrl = t.Item1;
                    XmlDocument xmlDoc = new XmlDocument();
                    string xmlUrl = targetUrl;
                    string xmlStr;
                    xmlStr = client.DownloadString(xmlUrl);
                    xmlDoc.LoadXml(xmlStr);                    
                    parseXMLdoc(xmlDoc);
                    result = true;
                    break;
                }
            }           
            return result;
        }

        public void parseXMLdoc(XmlDocument xmlDoc)
        {            
            xmlDoc.Save(Path.Combine(HttpContext.ApplicationInstance.Server.MapPath("~/App_Data"),"Temp", "tempDoc.xml"));
            var myXslTrans = new XslCompiledTransform();
            myXslTrans.Load(Path.Combine(HttpContext.ApplicationInstance.Server.MapPath("~/Content"), "tvXml2txt.xslt"));
            XmlWriterSettings xws = myXslTrans.OutputSettings.Clone();
            xws.Encoding = Encoding.UTF8;

            using (XmlWriter xw = XmlWriter.Create(Path.Combine(HttpContext.ApplicationInstance.Server.MapPath("~/App_Data"), "Temp","result.txt"), xws))
            {
                myXslTrans.Transform(Path.Combine(HttpContext.ApplicationInstance.Server.MapPath("~/App_Data"), "Temp", "tempDoc.xml"), xw);
                var x = myXslTrans.OutputSettings.Encoding;
            }

            /*
            myXslTrans.Load(Path.Combine(HttpContext.ApplicationInstance.Server.MapPath("~/Content"), "tvXml2txt.xslt"));
            var x = myXslTrans.OutputSettings.Encoding;
            myXslTrans.Transform(Path.Combine(HttpContext.ApplicationInstance.Server.MapPath("~/App_Data"),"Temp", "tempDoc.xml"), Path.Combine(HttpContext.ApplicationInstance.Server.MapPath("~/App_Data"), "Temp","result.txt"));                   
             */
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
