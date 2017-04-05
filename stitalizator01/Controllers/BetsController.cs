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
