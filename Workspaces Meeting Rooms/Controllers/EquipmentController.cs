using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Workspaces_Meeting_Rooms.Models; 
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace Workspaces_Meeting_Rooms.Controllers
{
    public class EquipmentController : Controller
    {
        private WMRDBEntities connection = new WMRDBEntities();

        // GET: Equipment
        public ActionResult Index()
        {
            return View(connection.equipments.ToList());
        }

        // GET: Equipment/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Equipment/Create
        [HttpPost]
        public ActionResult Create(equipment Equipment)
        {
            if (ModelState.IsValid)
            {
                // Agrega el nuevo equipo a la base de datos
                connection.equipments.Add(Equipment);
                connection.SaveChanges();

                // Redirige a la lista de equipos después de crear el nuevo registro
                return RedirectToAction("Index");
            }

            // Si la validación falla, vuelve a mostrar la vista con los datos actuales
            return View(Equipment);
        }


        // GET: Equipment/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var Equipment = connection.equipments.Find(id);
            if (Equipment == null)
            {
                return HttpNotFound();
            }

            return View(Equipment);
        }

        // POST: Equipment/Edit
        [HttpPost]
        public ActionResult Edit(equipment Equipment)
        {
            if (ModelState.IsValid)
            {
                connection.Entry(Equipment).State = EntityState.Modified;
                connection.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(Equipment);
        }


        // GET: Equipment/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var Equipment = connection.equipments.Find(id);
            if (Equipment == null)
            {
                return HttpNotFound();
            }

            return View(Equipment);
        }

        // POST: Equipment/Delete
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var Equipment = connection.equipments.Find(id);
            if (Equipment != null)
            {
                connection.equipments.Remove(Equipment);
                connection.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }

}

