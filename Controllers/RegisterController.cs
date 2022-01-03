using Face_rec.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Face_rec.Controllers
{
    public class RegisterController : Controller
    {

        private readonly ILogger<RegisterController> _logger;
        private readonly MyDbContext _context;

        public RegisterController(ILogger<RegisterController> logger, MyDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPersonAsync(IFormCollection collection)
        {
            FaceApiHelper faceApiHelper = new FaceApiHelper();
            AudioApiHelper audioApiHelper = new AudioApiHelper();

            Debug.WriteLine(collection["base64_input_image"].ToString());
            Debug.WriteLine("=======================================================");
            Debug.WriteLine(collection["base64_input_audio"].ToString());



            var json = collection["base64_input_image"].ToString();
            var dataUrlAudio = collection["base64_input_audio"].ToString();

            var data = (JObject)JsonConvert.DeserializeObject(json);

            string name = data["name"].Value<string>();
            string userData = data["userData"].Value<string>();
            var images = data["images"];


            Debug.WriteLine(name);
            Debug.WriteLine(userData);
            Debug.WriteLine(images);

            string[] base64Audio = dataUrlAudio.Split(',');
            byte[] bytesAudio = Convert.FromBase64String(base64Audio[1]);

            string profileIdAudio = audioApiHelper.create_identification_profile();
            string result = audioApiHelper.enroll_voice_profile(profileIdAudio, bytesAudio);

            if (result == "ENROLLED")
            {
                PersonAudio pAudio = new PersonAudio
                {
                    ProfileId = profileIdAudio,
                    Name = name,
                };

                _ = _context.PersonAudios.Add(pAudio);
                _ = await _context.SaveChangesAsync();

                string newPersonId = faceApiHelper.AddPersonToLargePersonGroup(name, userData);

                foreach (string b64 in images)
                {
                    string[] base64Images = b64.Split(',');
                    byte[] bytesImages = Convert.FromBase64String(base64Images[1]);
                    faceApiHelper.AddFace_ToLargePersonGroup_Person_Local(newPersonId, bytesImages);
                }

                faceApiHelper.TrainLargeGroup();


                ViewData["message"] = "New person added";
                return View("../Home/Index");
            }

            ViewData["message"] = "ERROR COULDN'T ADD PERSON";
            return View("../Home/Index");

        }
    }
}
