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

namespace stitalizator01.Controllers
{
    public class BetsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        [HttpPost]        
        public ActionResult Contact()
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
                    return Content("Sent!");
                }
            }
            else
            {
                return Content("Все молодцы! Отправлять некому!");
            }
        }



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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MyBets([Bind(Include = "BetID,PeriodID,ScoreOLS,ScoreClassic,AttemptNo,TimeStamp,IsHorse,BetRus18,BetMos18,BetSTImob,BetSTI,BetSTIplus,ProgramID")] Bet bet)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bet).State = EntityState.Modified;
                //bet.BetSTIplus = (float)(Math.Round(bet.BetSTIplus * 2) / 2);
                isHorse(bet);
                db.SaveChanges();
                return RedirectToAction("MyBets");
            }
            ViewBag.ProgramID = new SelectList(db.Programs, "ProgramID", "ProgTitle", bet.ProgramID);
            return RedirectToAction("MyBets");
        }


        [HttpPost]
        public ActionResult HomePageBets(string betId, string BetSTIplus)
        {
            BetSTIplus = BetSTIplus.Replace(".", ",");
            int id = Convert.ToInt32(betId);
            Bet bet = db.Bets.Find(id);
            db.Entry(bet).State = EntityState.Modified;
            bet.BetSTIplus = float.Parse(BetSTIplus);
            db.SaveChanges();
            isHorse(bet);
            
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
            bets = db.Bets.Where(b => b.ApplicationUser.UserName == User.Identity.Name & b.Program.TvDate >= curDate.Date & !b.IsLocked).ToList();
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
        public ActionResult Edit([Bind(Include = "BetID,PeriodID,ScoreOLS,ScoreClassic,AttemptNo,TimeStamp,IsHorse,BetRus18,BetMos18,BetSTImob,BetSTI,BetSTIplus,UserID,ProgramID")] Bet bet)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bet).State = EntityState.Modified;
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
    }
}
