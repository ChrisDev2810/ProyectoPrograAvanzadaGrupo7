using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Workspaces_Meeting_Rooms.Controllers
{
    public class MetricsController : Controller
    {
        //Connection to the DB
        private WMRDBEntities connection = new WMRDBEntities();

        // GET: Metrics
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GraphicData()
        {

            var metrics = connection.UsageStatistics.
                Select(metric => new { 
                    metric.roomID,
                    metric.room.name,
                    metric.date,
                    metric.hoursBooked,
                    metric.reservationID
                });


            return Json(new { success = true, graphicData = metrics }, JsonRequestBehavior.AllowGet);
        }

    }
}