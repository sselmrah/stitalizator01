using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using stitalizator01.Models;

namespace stitalizator01.Controllers
{
    public class BetsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();        

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
                    title = "Все ставки на "+curDate.ToString("dd.MM.yyyy");
                    break;
                case "emptybydate":
                    bets = db.Bets.Where(b => b.ApplicationUser.UserName == User.Identity.Name & b.Program.TvDate == curDate.Date & b.BetSTIplus == 0).ToList();
                    title = "Непроставленные ставки на "+curDate.ToString("dd.MM.yyyy");
                    break;
                case "filledbydate":
                    bets = db.Bets.Where(b => b.ApplicationUser.UserName == User.Identity.Name & b.Program.TvDate == curDate.Date & b.BetSTIplus > 0).ToList();
                    title = "Проставленные ставки на " + curDate.ToString("dd.MM.yyyy");
                    break;
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
            return RedirectToAction("MyBets");
        }
        
        public ActionResult HomePageBets()
        {
            DateTime curDate = DateTime.Now;
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
