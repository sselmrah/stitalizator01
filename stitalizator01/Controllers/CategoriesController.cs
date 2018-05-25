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
    public class CategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Categories
        public ActionResult Index()
        {
            return View(db.Categories.ToList());
        }

        // GET: Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Categories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CatName,CatNum")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CatName,CatNum")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
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

        public ActionResult ProgsByCat(int catID)
        {
            List<Program> progs = db.Programs.Where(p => p.Cat1.Id == catID || p.Cat2.Id == catID).ToList();
            return View(progs);
        }


        public ActionResult ClearCategories()
        {
            var cats = db.Categories.ToList();
            db.Categories.RemoveRange(cats);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult StripPrograms()
        {
            var progs = db.Programs.ToList();
            foreach (Program prog in progs)
            {
                prog.Cat1 = null;
                prog.Cat2 = null;
            }
            db.SaveChanges();
            return View("Index",db.Categories.ToList());
        }
        public ActionResult NewCategoriesFromPrograms()
        {
            var progs = db.Programs.ToList();
            var cats = db.Categories.ToList();
            //db.Categories.RemoveRange(db.Categories);
            //db.SaveChanges();
            foreach (Program prog in progs)
            {
                if (prog.ProgCat != null)
                {
                    string[] curCatString = prog.ProgCat.Trim().Split(';');
                    if (curCatString.Count()>0)
                    {
                        int num = 0;
                        foreach (string s in curCatString)
                        {
                            num++;
                            if (!cats.Any(c => c.CatName == s.Trim()))
                            {
                                Category newCat = new Category();
                                newCat.CatName = s.Trim();
                                newCat.CatNum = num;
                                cats.Add(newCat);
                                //db.Categories.Add(newCat);
                                //db.SaveChanges();
                            }
                        }
                    }
                }
            }
            db.Categories.AddRange(cats);
            db.SaveChanges();
            return View("Index", db.Categories.ToList());
        }
        public ActionResult UpdateExistingPrograms()
        {
            List<Program> exProgs = db.Programs.Where(p => p.Cat1 == null).ToList();
            List<Program> allProgs = db.Programs.ToList();
            List<Category> allCats = db.Categories.ToList();

            foreach (Program prog in allProgs)
            {
                if (prog.ProgCat !=null)
                {
                    string[] curCatString = prog.ProgCat.Trim().Split(';');
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
                                if (db.Categories.Where(x=>x.CatName==curCatName.Trim()).FirstOrDefault() != null )
                                {
                                    prog.Cat1 = db.Categories.Where(x => x.CatName == curCatName.Trim() && x.CatNum==1).FirstOrDefault();
                                }
                                else
                                {
                                    newCat1.CatName = curCatString[num].Trim();
                                    newCat1.CatNum = 1;
                                    prog.Cat1 = newCat1;
                                }
                                
                            }
                            else
                            {
                                if (db.Categories.Where(x => x.CatName == curCatName.Trim()).FirstOrDefault() != null)
                                {
                                    prog.Cat2 = db.Categories.Where(x => x.CatName == curCatName.Trim() && x.CatNum == 2).FirstOrDefault();
                                }
                                else
                                {
                                    newCat2.CatName = curCatString[num].Trim();
                                    newCat2.CatNum = 2;
                                    prog.Cat2 = newCat2;
                                }
                            }
                            
                            num++;
                        }
                    }

                    db.Entry(prog).State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                    }
                    catch
                    {

                    }
                }

            }
            db.SaveChanges();
            //var newProgs = db.Programs;
            return RedirectToAction("Index",db.Categories);
        }
    }
}
