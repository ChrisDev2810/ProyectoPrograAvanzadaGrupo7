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
            return View(connection.rooms.ToList());
        }

        // GET: Room/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            room Rooms = connection.rooms.Find(id);
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
            ViewBag.Equipments = connection.equipments.Select(e => new SelectListItem
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
                connection.rooms.Add(room);
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
                        connection.roomsEquipments.Add(roomEquipment);
                    }
                    connection.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            // Si hay un error en el modelo, recargar la lista de equipos para la vista
            ViewBag.Equipments = connection.equipments.Select(e => new SelectListItem
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

            var room = connection.rooms.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }

            // Obtener todos los equipos y marcar los que ya están asignados a esta sala
            var equipments = connection.equipments.ToList();
            var assignedEquipmentIds = connection.roomsEquipments
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
                var existingAssignments = connection.roomsEquipments.Where(re => re.roomID == room.roomID);
                connection.roomsEquipments.RemoveRange(existingAssignments);

                if (selectedEquipments != null)
                {
                    foreach (var equipmentId in selectedEquipments)
                    {
                        connection.roomsEquipments.Add(new roomsEquipment
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
            room room = connection.rooms.Find(id);
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
            room room = connection.rooms.Find(id);
            connection.rooms.Remove(room);
            connection.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}