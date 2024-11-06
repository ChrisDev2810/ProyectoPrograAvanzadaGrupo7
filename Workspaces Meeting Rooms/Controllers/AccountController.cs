using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Workspaces_Meeting_Rooms.Models;

namespace Workspaces_Meeting_Rooms.Controllers

{
    public class AccountController : Controller
    {
        //Connection to the DB
        private WMRDBEntities connection = new WMRDBEntities();

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }


        // POST: Login
        [HttpPost]
        public ActionResult Login(Login model) //Utilice un modelo separado donde la informacion enviada desde el form se mantendra en memoria para hacer la validacion con la DB
        {
            //Usando un modelo separado del de la DB mantengo la solucion mas limpia con solo los parametros que ocupo y se compara con los datos en memoria con lo que esta en la DB
            var user = connection.users.SingleOrDefault(found => found.email == model.Email && found.password == model.Password);

            //Si el correo y el password son correctos con los ingresados en la DB se autentitica el usuario
            if (user != null)
            {
                //Datos almacenados en session para ser utilizados luego en la ejecucion
                Session["userID"] = user.userID;
                Session["userEmail"] = user.email;
                Session["isAdmin"] = user.isAdmin;
                FormsAuthentication.SetAuthCookie(user.email, model.RememberMe); //Se genera una cookie de autenticacion
                return RedirectToAction("Index", "Home");
            }
            else 
            {
                ModelState.AddModelError("", "Direccion de correo o contraseña incorrectos");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //Investigue y esta propiedad evitaria ataques SCRF
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }
    }
}