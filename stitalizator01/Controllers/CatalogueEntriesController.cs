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
using System.Text;

namespace stitalizator01.Controllers
{
    public class CatalogueEntriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CatalogueEntries
        public ActionResult Index()
        {
            return View(db.CatalogueEntries.ToList());
        }

        
        public ActionResult advSearch(string Title = "Вожди")//, DateTime? DateMin = null, DateTime? DateMax = null)
        {
            //string title = Title;
            //List<CatalogueEntry> ces = new List<CatalogueEntry>();
            //ces = db.CatalogueEntries.Where(c => c.Title.Contains(title)).ToList();
            return View(db.CatalogueEntries.Where(c => c.Title.Contains(Title)).ToList());
        }

        public ActionResult ClearCat()
        {
            while (db.CatalogueEntries.Count()>0)
            { 
                db.CatalogueEntries.Remove(db.CatalogueEntries.First());
                db.SaveChanges();
            }
            return View("Index");
        }

        public ActionResult ImportCat()
        {
            //db.CatalogueEntries.RemoveRange(db.CatalogueEntries);

            List<CatalogueEntry> ces = new List<CatalogueEntry>();

            int i = 0;
            try
            {
                i=db.CatalogueEntries.Count();
            }
            catch
            {

            }
            using (var reader = new StreamReader(Path.Combine(HttpContext.ApplicationInstance.Server.MapPath("~/Content"), "cat20180130.csv"), Encoding.UTF8))
            {
                reader.ReadLine();
                //List<string> listA = new List<string>();
                //List<string> listB = new List<string>();
                while (!reader.EndOfStream)
                {

                    if (i > 0)
                    {
                        
                        CatalogueEntry curCE = new CatalogueEntry();
                        var line = reader.ReadLine();
                        var values = line.Split('\t');
                        if (values.Length > 10)
                        {
                            try
                            {
                                curCE.CatalogueEntryID = Convert.ToInt32(values[0]);
                            }
                            catch
                            {

                            }
                            try
                            {
                                curCE.Timing = Convert.ToDateTime(values[1]);
                            }
                            catch
                            {
                                curCE.Timing = DateTime.Parse("01-01-2001");
                            }
                            curCE.Title = values[2];
                            try
                            {
                                curCE.TVDate = Convert.ToDateTime(values[3]);
                            }
                            catch { }

                            try { curCE.Dow = Convert.ToInt16(values[4]); } catch { }
                            try
                            {
                                curCE.BegTime = Convert.ToDateTime(values[5]);
                            }
                            catch
                            {
                                curCE.BegTime = DateTime.Parse("01-01-2001");
                            }
                            try
                            {
                                curCE.Sti = Convert.ToSingle(values[6].Replace(".", ","));
                            }
                            catch { }
                            try
                            {
                                curCE.Dm = Convert.ToSingle(values[7].Replace(".", ","));
                            }
                            catch { }
                            try
                            {
                                curCE.Dr = Convert.ToSingle(values[8].Replace(".", ","));
                            }
                            catch { }
                            try
                            {
                                curCE.ProducerCode = Convert.ToInt16(values[9]);
                            }
                            catch { }
                            try
                            { curCE.SellerCode = Convert.ToInt16(values[10]); }
                            catch { }
                            if (i > curCE.CatalogueEntryID)
                            {
                                db.CatalogueEntries.Add(curCE);
                                db.SaveChanges();
                            }
                            //ces.Add(curCE);
                            
                        
                            //listA.Add(values[0]);
                            //listB.Add(values[1]);
                        }
                    }
                    i = i + 1;
                }
            }
            //db.SaveChanges();
            //db.CatalogueEntries.AddRange(ces);

            return View("Index");
        }

        // GET: CatalogueEntries/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CatalogueEntry catalogueEntry = db.CatalogueEntries.Find(id);
            if (catalogueEntry == null)
            {
                return HttpNotFound();
            }
            return View(catalogueEntry);
        }

        // GET: CatalogueEntries/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CatalogueEntries/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CatalogueEntryID,Timing,Title,Repeat,TVDate,Dow,BegTime,Sti,Dm,Dr,ProducerCode,SellerCode")] CatalogueEntry catalogueEntry)
        {
            if (ModelState.IsValid)
            {
                db.CatalogueEntries.Add(catalogueEntry);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(catalogueEntry);
        }

        // GET: CatalogueEntries/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CatalogueEntry catalogueEntry = db.CatalogueEntries.Find(id);
            if (catalogueEntry == null)
            {
                return HttpNotFound();
            }
            return View(catalogueEntry);
        }

        // POST: CatalogueEntries/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CatalogueEntryID,Timing,Title,Repeat,TVDate,Dow,BegTime,Sti,Dm,Dr,ProducerCode,SellerCode")] CatalogueEntry catalogueEntry)
        {
            if (ModelState.IsValid)
            {
                db.Entry(catalogueEntry).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(catalogueEntry);
        }

        // GET: CatalogueEntries/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CatalogueEntry catalogueEntry = db.CatalogueEntries.Find(id);
            if (catalogueEntry == null)
            {
                return HttpNotFound();
            }
            return View(catalogueEntry);
        }

        // POST: CatalogueEntries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CatalogueEntry catalogueEntry = db.CatalogueEntries.Find(id);
            db.CatalogueEntries.Remove(catalogueEntry);
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
