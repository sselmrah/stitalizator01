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
    public class PeriodsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Periods
        public ActionResult Index()
        {
            return View(db.Periods.ToList());
        }

        // GET: Periods/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Period period = db.Periods.Find(id);
            if (period == null)
            {
                return HttpNotFound();
            }
            return View(period);
        }

        // GET: Periods/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Periods/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PeriodID,PeriodDescription,EndDate,BegDate,ApplicationUser,IsMetaPeriod")] Period period)
        {
            if (ModelState.IsValid)
            {
                db.Periods.Add(period);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(period);
        }

        public ActionResult HomePageStats(bool metaPeriod)
        {
            Period period = new Period();
            period = getCurrentPeriod(metaPeriod);
            float score;
            if (User.Identity.IsAuthenticated)
            {
                score = db.Bets.Where(b => (b.ApplicationUser.UserName == User.Identity.Name) & (b.Program.TvDate >= period.BegDate) & (b.Program.TvDate <= period.EndDate)).Select(b=>b.ScoreClassic).DefaultIfEmpty(0).Sum();                
            }
            else
            {
                score = 0;
            }
            float totalScore = period.ScoresGambled;
            float percentage = score / totalScore;

            ViewBag.periodDescr = period.PeriodDescription;
            ViewBag.periodId = period.PeriodID;
            ViewBag.score= score;
            ViewBag.totalScore = totalScore;
            ViewBag.percentage = percentage;
            return PartialView();
        }

        public ActionResult HomePageLeaderboard(bool metaPeriod)
        {
            Period period = new Period();
            //DateTime curDay = DateTime.Now;
            //DateTime prevDay = curDay - TimeSpan.FromDays(1);
            List<LeaderboardEntry> userResults = new List<LeaderboardEntry>();
            
            /*if (!metaPeriod)
            {
                period = db.Periods.Where(p => (p.BegDate < curDay.Date) & (p.EndDate >= prevDay.Date) & !p.IsMetaPeriod).FirstOrDefault();
                if (period == null)
                {
                    period = db.Periods.Where(p => !p.IsMetaPeriod).FirstOrDefault();
                }
                userResults = getScoresByPeriod(period);
            }
            else
            {
                period = db.Periods.Where(p => (p.BegDate < curDay.Date) & (p.EndDate >= prevDay.Date) & p.IsMetaPeriod).FirstOrDefault();
                if (period == null)
                {
                    period = db.Periods.Where(p => p.IsMetaPeriod).FirstOrDefault();
                }
                userResults = getScoresByPeriod(period);
            }
            */

            period = getCurrentPeriod(metaPeriod);
            userResults = getScoresByPeriod(period);

            ViewBag.periodDescr = period.PeriodDescription;
            ViewBag.periodId = period.PeriodID;

            return PartialView(userResults);
        }


        private Period getCurrentPeriod(bool metaPeriod)
        {
            Period period = new Period();
            DateTime curDay = DateTime.Now;
            DateTime prevDay = curDay - TimeSpan.FromDays(1);
            
            period = db.Periods.Where(p => (p.BegDate < curDay.Date) & (p.EndDate >= prevDay.Date) & (p.IsMetaPeriod == metaPeriod)).FirstOrDefault();
            if (period == null)
            {
                period = db.Periods.Where(p => (p.IsMetaPeriod == metaPeriod)).FirstOrDefault();
            }
            return period;
        }



        private List<LeaderboardEntry> getScoresByPeriod(Period period)
        {
            List<LeaderboardEntry> userResults = db.Bets.Where(bet => (bet.Program.TvDate >= period.BegDate & bet.Program.TvDate <= period.EndDate))
                                             .GroupBy(b => b.ApplicationUser,
                                                      b => b.ScoreClassic,
                                                      (key, g) => new LeaderboardEntry
                                                      {
                                                          ApplicationUser = key,
                                                          Score = g.Sum()
                                                      })
                                             .OrderByDescending(p => p.Score).ToList()
                                             ;
            return userResults;
        }


        public ActionResult Leaderboard(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Period period = db.Periods.Find(id);
            if (period == null)
            {
                return HttpNotFound();
            }

            List<LeaderboardEntry> userResults = getScoresByPeriod(period);

            return View(userResults);
        }


        // GET: Periods/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Period period = db.Periods.Find(id);
            if (period == null)
            {
                return HttpNotFound();
            }
            return View(period);
        }

        // POST: Periods/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PeriodID,UserID,PeriodDescription,EndDate,BegDate,IsMetaPeriod")] Period period)
        {
            if (ModelState.IsValid)
            {
                db.Entry(period).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(period);
        }

        // GET: Periods/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Period period = db.Periods.Find(id);
            if (period == null)
            {
                return HttpNotFound();
            }
            return View(period);
        }

        // POST: Periods/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Period period = db.Periods.Find(id);
            db.Periods.Remove(period);
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
