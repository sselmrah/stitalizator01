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
    public class ChannelsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();



        public ActionResult ChannelCheckBoxes()
        {            
            return PartialView(db.Channels.ToList());
        }

        // GET: /Channels/
        public ActionResult Index()
        {
            return View(db.Channels.ToList());
        }

        // GET: /Channels/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Channel channel = db.Channels.Find(id);
            if (channel == null)
            {
                return HttpNotFound();
            }
            return View(channel);
        }

        // GET: /Channels/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Channels/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="ChannelID,ChannelCode,ChannelTag,ChannelName,IsDefault")] Channel channel)
        {
            if (ModelState.IsValid)
            {
                db.Channels.Add(channel);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(channel);
        }

        // GET: /Channels/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Channel channel = db.Channels.Find(id);
            if (channel == null)
            {
                return HttpNotFound();
            }
            return View(channel);
        }

        // POST: /Channels/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="ChannelID,ChannelCode,ChannelTag,ChannelName,IsDefault")] Channel channel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(channel).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(channel);
        }

        // GET: /Channels/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Channel channel = db.Channels.Find(id);
            if (channel == null)
            {
                return HttpNotFound();
            }
            return View(channel);
        }

        // POST: /Channels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Channel channel = db.Channels.Find(id);
            db.Channels.Remove(channel);
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
