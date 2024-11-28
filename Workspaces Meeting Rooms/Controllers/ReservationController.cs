using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Workspaces_Meeting_Rooms.Models;

namespace Workspaces_Meeting_Rooms.Controllers
{
    public class ReservationController : Controller
    {
        // Conexión a la base de datos
        private WMRDBEntities connection = new WMRDBEntities();

        // GET: Reservation
        public ActionResult Index(DateTime? startDate, int? roomId, int? userId)
        {
            // Obtener las reservas de la base de datos
            var reservations = connection.reservation.AsQueryable();

            // Filtrar por fecha si se proporciona
            if (startDate.HasValue)
            {
                DateTime startDateOnly = startDate.Value.Date; // Obtiene solo la fecha (sin horas)

                // Filtrar reservas que tengan la fecha de inicio igual a la fecha seleccionada
                reservations = reservations.Where(r => DbFunctions.TruncateTime(r.startTime) == startDateOnly);
            }

            // Filtrar por sala si se proporciona
            if (roomId.HasValue)
            {
                reservations = reservations.Where(r => r.roomId == roomId.Value);
            }

            // Filtrar por usuario si se proporciona
            if (userId.HasValue)
            {
                reservations = reservations.Where(r => r.userID == userId.Value);
            }

            // Cargar las salas y usuarios para el dropdown
            ViewBag.Rooms = new SelectList(connection.room, "roomID", "name");
            ViewBag.Users = new SelectList(connection.users, "userID", "email");

            // Devolver la vista con las reservas filtradas
            return View(reservations.ToList());
        }

        //************** Listas para Editar **************//
        // GET: Reservation/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Buscar la reserva en la base de datos
            var reservation = connection.reservation.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }

            // Obtener los detalles de la sala asociada con la reserva
            var room = connection.room.Find(reservation.roomId);
            if (room == null)
            {
                return HttpNotFound();
            }

            // Obtener las salas disponibles (rooms)
            var rooms = connection.room.ToList();
            ViewBag.Rooms = rooms.Select(r => new SelectListItem
            {
                Value = r.roomID.ToString(),
                Text = r.name
            }).ToList();

            // Obtener los usuarios disponibles
            ViewBag.Users = new SelectList(connection.users, "userID", "email", reservation.userID);

            // Obtener los estados disponibles
            ViewBag.Statuses = new SelectList(connection.status, "statusID", "statusDescription", reservation.statusID);

            // Convertir a DateTime las horas de disponibilidad de la sala (para validación en la vista)
            DateTime roomAvailabilityStart = DateTime.Today.Add(room.availability_start); // Sumar la hora de disponibilidad
            DateTime roomAvailabilityEnd = DateTime.Today.Add(room.availability_end); // Sumar la hora de finalización

            // Pasar los valores a la vista en el formato adecuado
            ViewBag.RoomAvailabilityStart = roomAvailabilityStart.ToString("yyyy-MM-ddTHH:mm");
            ViewBag.RoomAvailabilityEnd = roomAvailabilityEnd.ToString("yyyy-MM-ddTHH:mm");

            return View(reservation);
        }



        [HttpPost]
        public ActionResult Edit(reservation reservation, int? selectedRoom, int? selectedUser, int? selectedStatus)
        {
            if (ModelState.IsValid)
            {
                var existingReservation = connection.reservation
                    .Where(r => r.reservationID == reservation.reservationID)
                    .FirstOrDefault();

                if (existingReservation == null)
                {
                    ModelState.AddModelError("", "La reserva ya no existe.");
                    return View(reservation);
                }

                // Obtener los detalles de la sala asociada con la reserva (para validación)
                var room = connection.room.Find(selectedRoom);
                if (room == null)
                {
                    ModelState.AddModelError("", "La sala seleccionada no existe.");
                    return View(reservation);
                }

                // Convertir TimeSpan a DateTime para la validación
                DateTime roomAvailabilityStart = DateTime.Today.Add(room.availability_start); // Hora de inicio de la sala
                DateTime roomAvailabilityEnd = DateTime.Today.Add(room.availability_end); // Hora de finalización de la sala

                // Validación de hora de inicio y finalización
                if (reservation.startTime < roomAvailabilityStart || reservation.startTime >= roomAvailabilityEnd)
                {
                    ModelState.AddModelError("", "La hora de inicio está fuera del rango de disponibilidad de la sala.");
                    return View(reservation);
                }

                if (reservation.endTime > roomAvailabilityEnd || reservation.endTime <= roomAvailabilityStart)
                {
                    ModelState.AddModelError("", "La hora de finalización está fuera del rango de disponibilidad de la sala.");
                    return View(reservation);
                }

                // Actualizar la reserva con los valores seleccionados
                existingReservation.roomId = selectedRoom ?? existingReservation.roomId;
                existingReservation.userID = selectedUser ?? existingReservation.userID;
                existingReservation.statusID = selectedStatus ?? existingReservation.statusID;
                existingReservation.startTime = reservation.startTime;
                existingReservation.endTime = reservation.endTime;

                connection.Entry(existingReservation).State = EntityState.Modified;
                connection.SaveChanges();

                return RedirectToAction("Index");
            }

            // Si el modelo no es válido, recargar las listas
            ViewBag.Rooms = new SelectList(connection.room, "roomID", "roomID", reservation.roomId);
            ViewBag.Users = new SelectList(connection.users, "userID", "userID", reservation.userID);
            ViewBag.Statuses = new SelectList(connection.status, "statusID", "statusID", reservation.statusID);

            return View(reservation);
        }




        // GET: Reservation/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var reservation = connection.reservation.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }

            return View(reservation);
        }

        // POST: Reservation/Delete/5
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var reservation = connection.reservation.Find(id);
            connection.reservation.Remove(reservation);
            connection.SaveChanges();
            return RedirectToAction("Index");
        }

        // Método para redondear a la media hora más cercana
        private DateTime RoundToNearestHalfHour(DateTime dateTime)
        {
            int minutes = dateTime.Minute;
            int roundedMinutes = (minutes < 30) ? 0 : 30;
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, roundedMinutes, 0);
        }
    }
}