using System;
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
            

        [HttpGet]
        public ActionResult Index(string date = "today")
        {
            DateTime curDay;
            if (date == "today")
            {
                curDay = DateTime.Now.Date;
            }
            else
            {
                curDay = DateTime.Parse(date);
            }
            List<Program> todayProgList = new List<Program>();
            ViewBag.curDate = curDay.ToString("yyyy-MM-dd");
            todayProgList = db.Programs.Where(o => o.TvDate == curDay.Date).ToList();
            return View(todayProgList);
        }


        // GET: Programs
        [HttpGet]
        public ActionResult ProgsByDate(string date = "today",string channelsListStr = "1TV", string filter="9-23")
        {

            DateTime curDay;
            if (date == "today")
            {
                curDay = DateTime.Now.Date;
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
            return RedirectToAction("Index");
        }

        public List<string> getChannelTagsListFromString (string channelListStr)
        {
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
            return View();
        }

        // POST: Programs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProgramID,ChannelCode,ProgTitle,TvDate,TimeStart,TimeEnd,ProgDescr,ShareStiPlus,ShareStiMob,ShareSti,ShareMos18,ShareRus18")] Program program)
        {
            if (ModelState.IsValid)
            {
                db.Programs.Add(program);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

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
        public ActionResult Edit([Bind(Include = "ProgramID,ChannelCode,ProgTitle,TvDate,TimeStart,TimeEnd,ProgDescr,ShareStiPlus,ShareStiMob,ShareSti,ShareMos18,ShareRus18,IsBet")] Program program)
        {
            if (ModelState.IsValid)
            {
                db.Entry(program).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(program);
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

                foreach (ApplicationUser user in db.Users)
                {
                    Bet curBet = new Bet();
                    curBet.ProgramID = curProg.ProgramID;
                    curBet.Program = curProg;
                    curBet.TimeStamp = DateTime.Now;
                    curBet.ApplicationUser = user;
                    db.Bets.Add(curBet);                    
                }
            }
            else
            {
                var x = db.Bets.Where(o => o.ProgramID == curProg.ProgramID);
                db.Bets.RemoveRange(x);                
            }
            db.SaveChanges();
            //return RedirectToAction("Index");
            return PartialView(curProg);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult enterResult([Bind(Include = "ProgramID,ChannelCode,ProgTitle,TvDate,TimeStart,TimeEnd,ProgDescr,ProgCat,ShareStiPlus,ShareStiMob,ShareSti,ShareMos18,ShareRus18")] Program program)
        {
            if (ModelState.IsValid)
            {
                db.Entry(program).State = EntityState.Modified;
                var betsList = db.Bets.Where(b => b.Program.ProgramID == program.ProgramID);
                foreach (Bet bet in betsList)
                {                                        
                    bet.ScoreClassic=calculateScoreClassic(bet.BetSTIplus, (float)bet.Program.ShareStiPlus);
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
                //return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }


        public float calculateScoreClassic(float bet, float result)
        {
            float score = 0;
            float tempResult = 0;

            tempResult = Math.Abs(result - bet);
            if (tempResult <= 2) { score = 1; }
            if (tempResult <= 1) { score = 2; }
            if (tempResult <= 0.5) { score = 3; }
            


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
            db.SaveChanges();
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
                                curProg.TvDate = dt.Date;
                                curProg.ChannelCode = curChannelName;
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
