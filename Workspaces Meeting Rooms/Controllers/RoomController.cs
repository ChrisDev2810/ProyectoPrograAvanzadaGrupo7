using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Workspaces_Meeting_Rooms.Controllers
{
    public class RoomController : Controller
    {
        //Connection to the DB
        private WMRDBEntities connection = new WMRDBEntities();

        // GET: Room
        public ActionResult Index()
        {
            return View(connection.room.ToList());
        }

        // GET: Room/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            room Rooms = connection.room.Find(id);
            if (Rooms == null)
            {
                return HttpNotFound();
            }
            return View(Rooms);
        }

        // GET: Room/Create
        public ActionResult Create()
        {
            // Obtener todos los equipos y enviarlos a la vista
            ViewBag.Equipments = connection.equipment.Select(e => new SelectListItem
            {
                Value = e.equipmentID.ToString(),
                Text = e.equimentDescription
            }).ToList();

            return View();
        }

        // POST: Room/Create
        [HttpPost]
        public ActionResult Create(room room, int[] selectedEquipments)
        {
            if (ModelState.IsValid)
            {
                // Guardar la sala
                connection.room.Add(room);
                connection.SaveChanges();

                // Guardar los equipos seleccionados para esta sala
                if (selectedEquipments != null)
                {
                    foreach (var equipmentId in selectedEquipments)
                    {
                        var roomEquipment = new roomsEquipment
                        {
                            roomID = room.roomID,
                            equipmentID = equipmentId
                        };
                        connection.roomsEquipment.Add(roomEquipment);
                    }
                    connection.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            // Si hay un error en el modelo, recargar la lista de equipos para la vista
            ViewBag.Equipments = connection.equipment.Select(e => new SelectListItem
            {
                Value = e.equipmentID.ToString(),
                Text = e.equimentDescription
            }).ToList();

            return View(room);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var room = connection.room.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }

            // Obtener todos los equipos y marcar los que ya están asignados a esta sala
            var equipments = connection.equipment.ToList();
            var assignedEquipmentIds = connection.roomsEquipment
                .Where(re => re.roomID == room.roomID)
                .Select(re => re.equipmentID)
                .ToList();

            ViewBag.Equipments = equipments.Select(e => new SelectListItem
            {
                Value = e.equipmentID.ToString(),
                Text = e.equimentDescription,
                Selected = assignedEquipmentIds.Contains(e.equipmentID)
            });

            return View(room);
        }

        // POST: Room/Edit/5
        [HttpPost]
        public ActionResult Edit(room room, int[] selectedEquipments)
        {
            if (ModelState.IsValid)
            {
                connection.Entry(room).State = EntityState.Modified;
                connection.SaveChanges();

                // Actualizar los equipos asignados a esta sala
                var existingAssignments = connection.roomsEquipment.Where(re => re.roomID == room.roomID);
                connection.roomsEquipment.RemoveRange(existingAssignments);

                if (selectedEquipments != null)
                {
                    foreach (var equipmentId in selectedEquipments)
                    {
                        connection.roomsEquipment.Add(new roomsEquipment
                        {
                            roomID = room.roomID,
                            equipmentID = equipmentId
                        });
                    }
                }

                connection.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(room);
        }

        // GET: Room/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            room room = connection.room.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        // POST: Room/Delete
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            room room = connection.room.Find(id);
            connection.room.Remove(room);
            connection.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}