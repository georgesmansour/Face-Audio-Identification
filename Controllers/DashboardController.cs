using Face_rec.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Face_rec.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            Person person = new Person
            {
                PersonId = "id",
                Name = "name",
                UserData = "Userdata",
                LoggedIn="LoggedIn",
                LoggedOut = "LoggedOut",
            };

            ViewData["person"] = person;
            return View(person);
        }
    }
}
