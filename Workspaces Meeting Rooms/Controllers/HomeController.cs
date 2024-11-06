using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace Workspaces_Meeting_Rooms.Controllers
{
    public class HomeController : Controller
    {
        //Connection to the DB
        private WMRDBEntities connection = new WMRDBEntities();

        //GET: /Home/Index
        [HttpGet]
        public ActionResult Index()
        {
            var rooms = connection.rooms.ToList();

            return View(rooms);
        }

        //GET: /Home/GetRoomDetails
        [HttpGet]
        public JsonResult GetRoomDetails(int id)
        {
            var roomProperties = connection.rooms
            .Where(room => room.roomID == id)
            .Select(room => new
            {
                room.roomID,
                room.name,
                room.capacity,
                room.location,
                room.isActive,
                room.availability_start,
                room.availability_end
            }).FirstOrDefault();

            var roomEquipment = connection.roomsEquipments.Where(room => room.roomID == id).Select(descrip => descrip.equipment.equimentDescription).ToList();


            if (roomProperties == null && roomEquipment == null)
            {
                return Json(new { success = false, message = "Room data not found" }, JsonRequestBehavior.AllowGet);
            }

            var roomDetails = new
            {
                roomEquipment,
                roomProperties = new
                {
                    roomProperties.roomID,
                    roomProperties.name,
                    roomProperties.capacity,
                    roomProperties.location,
                    roomProperties.isActive,
                    availability_start = roomProperties.availability_start.ToString(@"hh\:mm"),
                    availability_end = roomProperties.availability_end.ToString(@"hh\:mm")
                }
            };



            return Json(new { success = true, roomData = roomDetails }, JsonRequestBehavior.AllowGet);
        }

        //GET: /Home/GetReservationsList
        [HttpGet]
        public JsonResult GetReservationsList(int roomID, int userID)
        {
            var reservationData = connection.reservations.Where(reservation => reservation.roomId == roomID && reservation.userID == userID)
                .Select(reservation => new
                {
                    reservation.startTime,
                    reservation.endTime,
                    reservation.status.statusDescription
                }).ToList();

            if (reservationData == null)
            {
                return Json(new { success = false, message = "Reservation data not found" }, JsonRequestBehavior.AllowGet);
            }

            var reservationsList = reservationData.Select(reservation => new
            {
                startTime = reservation.startTime.HasValue ? reservation.startTime.Value.ToString("HH:mm dd-MM-yyyy") : null,
                endTime = reservation.endTime.HasValue ? reservation.endTime.Value.ToString("HH:mm dd-MM-yyyy") : null,
                reservation.statusDescription
            }).ToList();

            return Json(new { success = true, ReservationsListData = reservationsList }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetSessionData()
        {
            var sessionData = new
            {
                userID = Session["userID"],
                userEmail = Session["userEmail"],
            };
            return Json(new { success = true, userData = sessionData }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetRoomAvailability(int roomID, DateTime date)
        {
            var reservations = connection.reservations
                .Where(r => r.roomId == roomID && r.startTime.HasValue)
                .AsEnumerable()
                .Where(r => r.startTime.Value.Date == date.Date)
                .Select(r => new
                {
                    startTime = r.startTime.HasValue ? r.startTime.Value.ToString("yyyy-MM-dd HH:mm") : null,
                    endTime = r.endTime.HasValue ? r.endTime.Value.ToString("yyyy-MM-dd HH:mm") : null
                }).ToList();

            return Json(new { success = true, reservationsData = reservations }, JsonRequestBehavior.AllowGet);
        }


        //POST: /Home/CreateReservation
        [HttpPost]
        public JsonResult CreateReservation(reservation reservation)
        {

            if (ModelState.IsValid)
            {
                connection.reservations.Add(reservation);
                connection.SaveChanges();

                return Json(new { success = true, message = "Reservacion realizada con exito!" });
            }

            return Json(new { success = false, message = "Hubo un fallo al realizar la reserva" });
        }

    }
}