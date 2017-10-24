using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using stitalizator01.Models;

using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace stitalizator01.Controllers
{
    public class BetsController : CustomController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private static readonly log4net.ILog logB = log4net.LogManager.GetLogger("BetsController.cs");

        [HttpGet]
        public ActionResult MyBets(string filter = "allbydate", string date = "01.01.1900")
        {            
            List<Bet> bets = new List<Bet>();
            DateTime curDate;
            string title = "";
            if (date == "01.01.1900")
            {
                curDate = DateTime.Today;
            }
            else
            {
                curDate = DateTime.Parse(date.ToString());
            }

            if (User.IsInRole("Admin"))
            {
                switch (filter)
                {
                    case "all":
                        bets = db.Bets.ToList();
                        title = "Все ставки";
                        break;
                    case "empty":
                        bets = db.Bets.Where(b => b.BetSTIplus == 0).ToList();
                        title = "Непроставленные ставки";
                        break;
                    case "filled":
                        bets = db.Bets.Where(b => b.BetSTIplus > 0).ToList();
                        title = "Проставленные ставки";
                        break;
                    case "allbydate":
                        bets = db.Bets.Where(b => b.Program.TvDate == curDate.Date).ToList();
                        title = "Все ставки на " + curDate.ToString("dd.MM.yyyy");
                        break;
                    case "emptybydate":
                        bets = db.Bets.Where(b => b.Program.TvDate == curDate.Date & b.BetSTIplus == 0).ToList();
                        title = "Непроставленные ставки на " + curDate.ToString("dd.MM.yyyy");
                        break;
                    case "filledbydate":
                        bets = db.Bets.Where(b => b.Program.TvDate == curDate.Date & b.BetSTIplus > 0).ToList();
                        title = "Проставленные ставки на " + curDate.ToString("dd.MM.yyyy");
                        break;
                }
            }
            else
            {
                switch (filter)
                {
                    case "all":
                        bets = db.Bets.Where(b => b.ApplicationUser.UserName == User.Identity.Name).ToList();
                        title = "Все ставки";
                        break;
                    case "empty":
                        bets = db.Bets.Where(b => b.ApplicationUser.UserName == User.Identity.Name & b.BetSTIplus == 0).ToList();
                        title = "Непроставленные ставки";
                        break;
                    case "filled":
                        bets = db.Bets.Where(b => b.ApplicationUser.UserName == User.Identity.Name & b.BetSTIplus > 0).ToList();
                        title = "Проставленные ставки";
                        break;
                    case "allbydate":
                        bets = db.Bets.Where(b => b.ApplicationUser.UserName == User.Identity.Name & b.Program.TvDate == curDate.Date).ToList();
                        title = "Все ставки на " + curDate.ToString("dd.MM.yyyy");
                        break;
                    case "emptybydate":
                        bets = db.Bets.Where(b => b.ApplicationUser.UserName == User.Identity.Name & b.Program.TvDate == curDate.Date & b.BetSTIplus == 0).ToList();
                        title = "Непроставленные ставки на " + curDate.ToString("dd.MM.yyyy");
                        break;
                    case "filledbydate":
                        bets = db.Bets.Where(b => b.ApplicationUser.UserName == User.Identity.Name & b.Program.TvDate == curDate.Date & b.BetSTIplus > 0).ToList();
                        title = "Проставленные ставки на " + curDate.ToString("dd.MM.yyyy");
                        break;
                }
            }
            
            ViewData["curDate"] = curDate.ToString("yyyy-MM-dd");
            ViewData["tTitle"] = title;
            return View(bets);
        }



        public ActionResult WhiteBoard(int periodId = 0)
        {
            Period curPeriod = new Period();
            if (periodId == 0)
            {
                DateTime tempDt = DateTime.UtcNow;
                if (tempDt.DayOfWeek == DayOfWeek.Monday) { tempDt = tempDt - TimeSpan.FromDays(1); }
                curPeriod = getPeriodByDate(tempDt,false);
            }
            else
            {
                curPeriod = db.Periods.Where(p => p.PeriodID == periodId).FirstOrDefault();
            }
            List<Bet> bets = db.Bets.Where(b => (b.Program.TvDate >= curPeriod.BegDate) & (b.Program.TvDate <= curPeriod.EndDate)).OrderBy(b => b.ApplicationUser.UserName).ThenBy(b => b.Program.TvDate).ThenBy(b=> b.Program.ChannelCode.Length).ThenBy(b => b.Program.TimeStart).ToList();
            

            return View(bets);
        }

        public ActionResult WhiteBoardHomepage(int periodId = 0)
        {
            Period curPeriod = new Period();
            List<Period> periods = db.Periods.Where(p => !p.IsMetaPeriod).OrderBy(p => p.BegDate).ToList();
            if (periodId == 0)
            {
                DateTime tempDt = DateTime.UtcNow;
                if (tempDt.DayOfWeek == DayOfWeek.Monday) { tempDt = tempDt - TimeSpan.FromDays(1); }
                curPeriod = getPeriodByDate(tempDt, false);
            }
            else
            {
                curPeriod = db.Periods.Where(p => p.PeriodID == periodId).FirstOrDefault();
            }
            List<Bet> bets = db.Bets.Where(b => (b.ApplicationUser.UserName!="Admin")&(b.Program.TvDate >= curPeriod.BegDate) & (b.Program.TvDate <= curPeriod.EndDate)).OrderBy(b => b.ApplicationUser.UserName).ThenBy(b => b.Program.TvDate).ThenByDescending(b => b.Program.ChannelCode.Length).ThenBy(b => b.Program.TimeStart).ToList();


            bool last = false;
            bool first = false;
            if (periods.LastOrDefault() == curPeriod)
            {
                last = true;
            }
            if (periods.FirstOrDefault() == curPeriod)
            {
                first = true;
            }
            
            ViewBag.last = last;
            ViewBag.first = first;
            ViewBag.periodId = curPeriod.PeriodID;
            ViewBag.periodDescription = curPeriod.PeriodDescription;
            return PartialView(bets);
        }
        public ActionResult WhiteBoardPdf(int periodId = 0)
        {
            Period curPeriod = new Period();
            List<Period> periods = db.Periods.Where(p => !p.IsMetaPeriod).OrderBy(p => p.BegDate).ToList();
            if (periodId == 0)
            {
                DateTime tempDt = DateTime.UtcNow;
                if (tempDt.DayOfWeek == DayOfWeek.Monday) { tempDt = tempDt - TimeSpan.FromDays(1); }
                curPeriod = getPeriodByDate(tempDt, false);
            }
            else
            {
                curPeriod = db.Periods.Where(p => p.PeriodID == periodId).FirstOrDefault();
            }
            List<Bet> bets = db.Bets.Where(b => (b.ApplicationUser.UserName != "Admin") & (b.Program.TvDate >= curPeriod.BegDate) & (b.Program.TvDate <= curPeriod.EndDate)).OrderBy(b => b.ApplicationUser.UserName).ThenBy(b => b.Program.TvDate).ThenByDescending(b => b.Program.ChannelCode.Length).ThenBy(b => b.Program.TimeStart).ToList();


            bool last = false;
            bool first = false;
            if (periods.LastOrDefault() == curPeriod)
            {
                last = true;
            }
            if (periods.FirstOrDefault() == curPeriod)
            {
                first = true;
            }

            ViewBag.last = last;
            ViewBag.first = first;
            ViewBag.periodId = curPeriod.PeriodID;
            ViewBag.periodDescription = curPeriod.PeriodDescription;
            return Pdf(bets);
        }
        public ActionResult SwitchWhiteBoardHomepage(int periodId, string direction)
        {
            Period curPeriod = new Period();

            List<Period> periods = db.Periods.Where(p => !p.IsMetaPeriod).OrderBy(p => p.BegDate).ToList();
            Period oldPeriod = periods.Find(p => p.PeriodID == periodId);
            int index = periods.IndexOf(oldPeriod);

            if (direction == "next")
            {
                index++;
                if (index < periods.Count())
                {
                    curPeriod = periods[index];
                }
            }
            else
            {
                index--;
                if (index >= 0)
                {
                    curPeriod = periods[index];
                }
            }

            /*
            if (periodId == 0)
            {
                DateTime tempDt = DateTime.UtcNow;
                if (tempDt.DayOfWeek == DayOfWeek.Monday) { tempDt = tempDt - TimeSpan.FromDays(1); }
                curPeriod = getPeriodByDate(tempDt, false);
            }
            else
            {
                curPeriod = db.Periods.Where(p => p.PeriodID == periodId).FirstOrDefault();
            }
            */
            List<Bet> bets = db.Bets.Where(b => (b.ApplicationUser.UserName != "Admin") & (b.Program.TvDate >= curPeriod.BegDate) & (b.Program.TvDate <= curPeriod.EndDate)).OrderBy(b => b.ApplicationUser.UserName).ThenBy(b => b.Program.TvDate).ThenByDescending(b => b.Program.ChannelCode.Length).ThenBy(b => b.Program.TimeStart).ToList();


            bool last = false;
            bool first = false;
            if (periods.LastOrDefault() == curPeriod)
            {
                last = true;
            }
            if (periods.FirstOrDefault() == curPeriod)
            {
                first = true;
            }

            ViewBag.last = last;
            ViewBag.first = first;
            ViewBag.periodId = curPeriod.PeriodID;
            ViewBag.periodDescription = curPeriod.PeriodDescription;
            return PartialView(bets);
        }

        [HttpGet]
        public ActionResult manualTeleSend(string userName)
        {
            DateTime now = DateTime.UtcNow+TimeSpan.FromHours(3);
            List<Bet> allbets = db.Bets.Where(b => b.ApplicationUser.TelegramUserName == userName & b.Program.TvDate==now.Date).ToList();
            string allbetsstr = allbets.Count().ToString()+"; ";
            List<ConversationStarter> css = db.CSs.ToList();
            
            if (css.Count() > 0)
            {
                foreach (ConversationStarter cs in css)
                {
                    if (cs.ChannelId == "telegram" & cs.ApplicationUser.TelegramUserName == userName)
                    {
                        ApplicationUser curUser = cs.ApplicationUser;


                        var userAccount = new ChannelAccount(cs.ToId, cs.ToName);
                        var botAccount = new ChannelAccount(cs.FromId, cs.FromName);
                        var connector = new ConnectorClient(new Uri(cs.ServiceUrl));

                        Activity activity = new Activity();
                        activity.From = botAccount;
                        activity.Recipient = userAccount;
                        activity.Conversation = new ConversationAccount(id: cs.ConversationId);
                        activity.Id = "1";
                        
                        string text = "Нужно сделать ставки!";
                        activity.ChannelData = new TelegramChannelData()
                        {
                            method = "sendMessage",
                            parameters = new TelegramParameters()
                            {
                                text = text
                            }
                        };
                        allbetsstr += activity.Recipient.Name.ToString()+"-"+activity.Recipient.Id.ToString() + ". ";
                        try
                        {
                            //connector.Conversations.ReplyToActivity(activity);
                            connector.Conversations.SendToConversation(activity);
                        }
                        catch (Exception ex)
                        {
                            allbetsstr = ex.Message;
                        }
                    }
                }
            }
            
            return Content(allbetsstr);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MyBets([Bind(Include = "BetID,PeriodID,ScoreOLS,ScoreClassic,AttemptNo,TimeStamp,IsHorse,IsLocked,BetRus18,BetMos18,BetSTImob,BetSTI,BetSTIplus,ProgramID")] Bet bet)        
        {
            if (ModelState.IsValid)
            {
                db.Entry(bet).State = EntityState.Modified;
                //bet.BetSTIplus = (float)(Math.Round(bet.BetSTIplus * 2) / 2);
                isHorse(bet);
                db.SaveChanges();
                //Какая-то фигня с лошадьми... Нужно проверить...
                var betsList = db.Bets.Where(b => b.Program.ProgramID == bet.Program.ProgramID);
                foreach (Bet b in betsList)
                {
                    b.ScoreClassic = calculateScoreClassic(b);
                    b.ScoreOLS = calculateScoreOLS(b.BetSTIplus, (float)b.Program.ShareStiPlus);
                    db.Entry(bet).State = EntityState.Modified;
                }
                db.SaveChanges();
                //log.Info(String.Format("User {0} has placed a bet {1} for program {3} using mybets page", bet.ApplicationUser.UserName, bet.BetSTIplus, bet.Program.ProgTitle));
                string msg = "";
                msg = "User " + bet.ApplicationUser.UserName + " has placed a bet " + bet.BetSTIplus + " for program " + bet.Program.ProgTitle + " using mybets page";
                logB.Info(msg);  
                return Content("bet updated");
                //return RedirectToAction("MyBets");
            }
            ViewBag.ProgramID = new SelectList(db.Programs, "ProgramID", "ProgTitle", bet.ProgramID);
            //return RedirectToAction("MyBets");
            return Content("bet NOT updated");
        }


        [HttpPost]
        public ActionResult HomePageBets(string betId, string BetSTIplus)
        {
            BetSTIplus = BetSTIplus.Replace(".", ",");
            int id = Convert.ToInt32(betId);
            Bet bet = db.Bets.Find(id);
            db.Entry(bet).State = EntityState.Modified;
            bet.BetSTIplus = float.Parse(BetSTIplus);
            bet.TimeStamp = DateTime.UtcNow + MvcApplication.utcMoscowShift;
            db.SaveChanges();
            isHorse(bet);
            if (bet.Program.ShareStiPlus>0)
            {
                bet.ScoreClassic = calculateScoreClassic(bet);
                bet.ScoreOLS = calculateScoreOLS(bet.BetSTIplus, (float)bet.Program.ShareStiPlus);
                Program program = db.Programs.Where(p => p.ProgramID == bet.Program.ProgramID).FirstOrDefault();
                var betsList = db.Bets.Where(b => b.Program.ProgramID == program.ProgramID);
                foreach (Bet b in betsList)
                {

                    b.ScoreClassic = calculateScoreClassic(b);
                    b.ScoreOLS = calculateScoreOLS(b.BetSTIplus, (float)b.Program.ShareStiPlus);
                    db.Entry(b).State = EntityState.Modified;
                }
                db.SaveChanges();
                //log.Info(String.Format("User {0} has placed a bet {1} for program {3} using homepage form", bet.ApplicationUser.UserName, bet.BetSTIplus, bet.Program.ProgTitle));
            }
            string msg = "";
            msg = "User " + bet.ApplicationUser.UserName + " has placed a bet " + bet.BetSTIplus + " for program " + bet.Program.ProgTitle + " using homepage form";
            logB.Info(msg);
            //Program program = db.Programs.Find(bet.ProgramID);
            //return RedirectToAction("MyBets");
            return Content(bet.Program.IsHorse.ToString());
        }                


        private void isHorse(Bet bet)
        {
            var bets = db.Bets.Where(b => b.ProgramID == bet.ProgramID).FirstOrDefault();
            
            if (bets != null)                
            {
                var tempRes = db.Bets.Where(b => (b.ProgramID == bet.ProgramID) & (b.BetSTIplus > 0));
                double maxBet = 0;
                double minBet = 0;
                if (tempRes.Count() > 0)
                {
                    //double maxBet = db.Bets.Where(b => (b.Program.ProgramID == bet.Program.ProgramID) & (b.BetSTIplus > 0)).Max(b => b.BetSTIplus);
                    //double minBet = db.Bets.Where(b => (b.Program.ProgramID == bet.Program.ProgramID) & (b.BetSTIplus > 0)).Min(b => b.BetSTIplus);
                    minBet = tempRes.Min(b => b.BetSTIplus);
                    maxBet = tempRes.Max(b => b.BetSTIplus);
                }
                    Program program = db.Programs.Find(bet.ProgramID);
                    Period curPeriod = getPeriodByDate(program.TvDate, false);
                if (Math.Abs(maxBet-minBet)>=5)
                {            
                    //Увеличиваем количество очков в розыгрыше при добавлении лошадки
                    if (!program.IsHorse)
                    {
                        curPeriod.ScoresGambled += 3;
                        db.Entry(curPeriod).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    program.IsHorse = true;
                    db.Entry(program).State = EntityState.Modified;                    
                    db.SaveChanges();
                    //var programs = db.Bets.Select(b => {b.IsHorse})
                    //collection.Select(c => { c.PropertyToSet = value; return c; }).ToList();
                }                
                else
                {
                    //Уменьшаем количество очков в розыгрыше при удалении лошадки
                    if (program.IsHorse)
                    {
                        curPeriod.ScoresGambled -= 3;
                        db.Entry(curPeriod).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    program.IsHorse = false;
                    db.Entry(program).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        private Period getPeriodByDate(DateTime dt, bool metaPeriod = false)
        {
            Period period = new Period();
            period = db.Periods.Where(p => (p.BegDate <= dt.Date) & (p.EndDate >= dt.Date) & (p.IsMetaPeriod == metaPeriod)).FirstOrDefault();
            if (period == null)
            {
                period = db.Periods.Where(p => (p.IsMetaPeriod == metaPeriod)).FirstOrDefault();
            }
            return period;
        }



        public ActionResult HomePageBets()
        {
            DateTime curDate = DateTime.UtcNow+MvcApplication.utcMoscowShift;
            List<Bet> bets = new List<Bet>();
            bets = db.Bets.Where(b => b.ApplicationUser.UserName == User.Identity.Name & b.Program.TvDate >= curDate.Date & !b.IsLocked).OrderBy(b=>b.Program.TvDate).ThenByDescending(b => b.Program.ChannelCode.Length).ThenBy(b => b.Program.TimeStart).ToList();
            if (bets.Count>0)
            {
                return PartialView(bets);
            }
            else
            {
                return PartialView("StavkiSdelany");
            }            
        }


        // GET: Bets
        public ActionResult Index()
        {
            var bets = db.Bets.Include(b => b.Program);
            return View(bets.ToList());
        }

        // GET: Bets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bet bet = db.Bets.Find(id);
            if (bet == null)
            {
                return HttpNotFound();
            }
            return View(bet);
        }

        
        // GET: Bets/Create
        public ActionResult Create()
        {
            ViewBag.ProgramID = new SelectList(db.Programs, "ProgramID", "ProgTitle");
            return View();
        }

        // POST: Bets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BetID,PeriodID,ScoreOLS,ScoreClassic,AttemptNo,TimeStamp,IsHorse,BetRus18,BetMos18,BetSTImob,BetSTI,BetSTIplus,UserID,ProgramID")] Bet bet)
        {
            if (ModelState.IsValid)
            {
                db.Bets.Add(bet);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProgramID = new SelectList(db.Programs, "ProgramID", "ProgTitle", bet.ProgramID);
            return View(bet);
        }

        
        // GET: Bets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bet bet = db.Bets.Find(id);
            if (bet == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProgramID = new SelectList(db.Programs, "ProgramID", "ProgTitle", bet.ProgramID);
            return View(bet);
        }

        // POST: Bets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BetID,PeriodID,ScoreOLS,ScoreClassic,AttemptNo,TimeStamp,IsHorse,IsLocked,BetRus18,BetMos18,BetSTImob,BetSTI,BetSTIplus,UserID,ProgramID")] Bet bet)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bet).State = EntityState.Modified;
                isHorse(bet);
                db.SaveChanges();
                var betsList = db.Bets.Where(b => b.Program.ProgramID == bet.Program.ProgramID);
                foreach (Bet b in betsList)
                {
                    b.ScoreClassic = calculateScoreClassic(b);
                    b.ScoreOLS = calculateScoreOLS(b.BetSTIplus, (float)b.Program.ShareStiPlus);
                    db.Entry(bet).State = EntityState.Modified;
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            isHorse(bet);
            ViewBag.ProgramID = new SelectList(db.Programs, "ProgramID", "ProgTitle", bet.ProgramID);
            return View(bet);
        }

        
        // GET: Bets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bet bet = db.Bets.Find(id);
            if (bet == null)
            {
                return HttpNotFound();
            }
            return View(bet);
        }

        // POST: Bets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Bet bet = db.Bets.Find(id);
            db.Bets.Remove(bet);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }



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

        public float calculateScoreOLS(float bet, float result)
        {
            float score = 0;
            score = (float)Math.Pow((result - bet), 2);

            return score;
        }
    }
}
