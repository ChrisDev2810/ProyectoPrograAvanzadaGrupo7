using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Workspaces_Meeting_Rooms.Controllers
{
    public class RoomController : Controller
    {
        // GET: Rooms
        public ActionResult Index()
        {
            return View();
        }
    }
}