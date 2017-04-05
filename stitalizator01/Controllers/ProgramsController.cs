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

namespace stitalizator01.Controllers
{
    public class ProgramsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Programs
        public ActionResult Index()
        {
            getScheduleByDateChannel();
            return View(db.Programs.ToList());
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
        public ActionResult Edit([Bind(Include = "ProgramID,ChannelCode,ProgTitle,TvDate,TimeStart,TimeEnd,ProgDescr,ShareStiPlus,ShareStiMob,ShareSti,ShareMos18,ShareRus18")] Program program)
        {
            if (ModelState.IsValid)
            {
                db.Entry(program).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(program);
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


        public List<Tuple<string,string,string,string,string,DateTime>> getScheduleByDateChannel (string dateStr = "05.04.2017", string chCodeStr = "10")
        {
            List<string> sched = new List<string>();
            List<Tuple<string, string, string, string, string, DateTime>> schedParsed = new List<Tuple<string, string, string, string, string, DateTime>>();
            string url = "http://xmltv.s-tv.ru/xchenel.php?xmltv=1&pass=jJoM88wN54&show=2&login=tv4181";
            WebClient client = new WebClient();            

            string xmlString = client.DownloadString(url);            
            string[] delim = {"<File>"};
            sched = xmlString.Split(delim,System.StringSplitOptions.RemoveEmptyEntries).ToList();

            string name = "";
            string efirWeek = "";
            string channel = "";
            string channelID = "";
            string variant = "";
            DateTime dateTime;
            
            foreach(string s in sched)
            {
                if (s.IndexOf("<Name>")>=0)
                {
                    name = s.Substring(s.IndexOf("<Name>") + 6, s.IndexOf("</Name>") - s.IndexOf("<Name>") - 6);
                    efirWeek = s.Substring(s.IndexOf("<EfirWeek>") + 10, s.IndexOf("</EfirWeek>") - s.IndexOf("<EfirWeek>") - 10);                    
                    channel = s.Substring(s.IndexOf("<Channel>") + 9, s.IndexOf("</Channel>") - s.IndexOf("<Channel>") - 9);
                    channelID = s.Substring(s.IndexOf("<ChannelID>") + 11, s.IndexOf("</ChannelID>") - s.IndexOf("<ChannelID>") - 11);
                    variant = s.Substring(s.IndexOf("<Variant>") + 9, s.IndexOf("</Variant>") - s.IndexOf("<Variant>") - 9);
                    dateTime = DateTime.Parse(s.Substring(s.IndexOf("<DateTime>") + 10, s.IndexOf("</DateTime>") - s.IndexOf("<DateTime>") - 10));                    
                    Tuple<string, string, string, string, string, DateTime> curTuple = new Tuple<string, string, string, string, string, DateTime>(name,efirWeek,channel,channelID,variant,dateTime);
                    schedParsed.Add(curTuple);
                }
            }

            return schedParsed;
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
