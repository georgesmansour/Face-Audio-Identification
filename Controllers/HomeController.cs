using Face_rec.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace Face_rec.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MyDbContext _context;

        public HomeController(ILogger<HomeController> logger, MyDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            int UpdatedId = _context.Persons.Max(p => p.TableId);
            Person person = _context.Persons.Where(p => p.TableId == UpdatedId).FirstOrDefault();
            person.LoggedOut = DateTime.Now.ToString();
            _context.Update(person);
            await _context.SaveChangesAsync();

            return View("Index");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DashboardAsync(IFormCollection collection)

        {

            FaceApiHelper faceApiHelper = new FaceApiHelper();
            AudioApiHelper audioApiHelper = new AudioApiHelper();

            var dataUrlImage = collection["base64_input_image"].ToString();
            var dataUrlAudio = collection["base64_input_audio"].ToString();

            string[] base64Image = dataUrlImage.Split(',');
            string[] base64Audio = dataUrlAudio.Split(',');




            byte[] bytesImage = Convert.FromBase64String(base64Image[1]);
            byte[] bytesAudio = Convert.FromBase64String(base64Audio[1]);


            string faceId = faceApiHelper.DetectFromLocalFile(bytesImage);

            if (faceId == "FACE NOT DETECTED" || faceId == "MULTIPLE FACES DETECTED")
            {
                Debug.Write($"Cannot continue because: {faceId} ");


                ViewData["message"] = faceId;
                return View("Index");
            }
            else
            {

                string personId = faceApiHelper.IdentifyPerson(faceId);
                Person person = faceApiHelper.getPersonFromId(personId);

                if(person.Name == "PERSON NOT FOUND")
                {
                    ViewData["message"] = "PERSON NOT FOUND";
                    return View("Index");
                }

                List<string> profileIds = _context.PersonAudios.Select(p=>p.ProfileId).ToList();

                string profileIdAudio = audioApiHelper.identify_person_audio(bytesAudio);
                Debug.WriteLine($"profileIdAudio ====> " + profileIdAudio);

                string profileAudioName =_context.PersonAudios.Where(p => p.ProfileId == profileIdAudio).Select(n=>n.Name).FirstOrDefault();

                if (profileAudioName == person.Name)
                {
                    _ = _context.Persons.Add(person);
                    _ = await _context.SaveChangesAsync();

                    ViewData["person"] = person;
                    return View(person);
                }
                ViewData["message"] = "PERSON NOT FOUND";
                return View("Index");
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
