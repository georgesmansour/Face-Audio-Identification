using Face_rec.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Face_rec
{
    class FaceApiHelper
    {
        public const string SUBSCRIPTION_KEY = "YOUR_FACE_SUBSCRIPTION_KEY";
        public const string LARGE_PERSON_GROUP_ID = "2";

        public FaceApiHelper()
        {
        }

        public string AddPersonToLargePersonGroup(string name, string userdata)
        {
            var client = new RestClient($"https://eastus.api.cognitive.microsoft.com/face/v1.0/largepersongroups/{LARGE_PERSON_GROUP_ID}/persons");

            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Ocp-Apim-Subscription-Key", SUBSCRIPTION_KEY);
            request.AddHeader("Content-Type", "application/json");
            var body = "{" +
                "\"name\" : \"" + name + "\"," +
                "\"userData\" : \"" + userdata + "\"," +
                "}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Debug.WriteLine($"ADD PERSON RESPONSE: {response.Content}");

            var data = (JObject)JsonConvert.DeserializeObject(response.Content);

            string personId = data["personId"].Value<string>();

            return personId;
        }

        public void AddFace_ToLargePersonGroup_Person(string personId, string imageUrl)
        {
            var client = new RestClient($"https://eastus.api.cognitive.microsoft.com/face/v1.0/largepersongroups/{LARGE_PERSON_GROUP_ID}/persons/{personId}/persistedfaces");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Ocp-Apim-Subscription-Key", SUBSCRIPTION_KEY);
            request.AddHeader("Content-Type", "application/json");
            var body = "{" +
                "\"url\" : \"" + imageUrl + "\"" +
                "}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Debug.WriteLine(response.Content);
        }
        public void AddFace_ToLargePersonGroup_Person_Local(string personId, byte[] bytes)
        {
            var client = new RestClient($"https://eastus.api.cognitive.microsoft.com/face/v1.0/largepersongroups/{LARGE_PERSON_GROUP_ID}/persons/{personId}/persistedfaces");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Ocp-Apim-Subscription-Key", SUBSCRIPTION_KEY);
            request.AddHeader("Content-Type", "application/octet-stream");
            request.AddParameter("application/octet-stream", bytes, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }


        public string IdentifyPerson(String faceId)
        {
            var client = new RestClient("https://eastus.api.cognitive.microsoft.com/face/v1.0/identify");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Ocp-Apim-Subscription-Key", SUBSCRIPTION_KEY);
            request.AddHeader("Content-Type", "application/json");
            var body = "{\"largePersonGroupId\":\"" + LARGE_PERSON_GROUP_ID + "\",\"faceIds\":[\"" + faceId + "\"],\"maxNumOfCandidatesReturned\":1,\"confidenceThreshold\":0.5}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Debug.WriteLine($"IDENTIFY RESPONSE:{response.Content}");
            JArray jArray = JArray.Parse(response.Content);

            string candidates = "";
            foreach (JObject item in jArray)
            {
                candidates = item.GetValue("candidates").ToString();
            }

            JArray jArray2 = JArray.Parse(candidates);
            string personId = "";
            foreach (JObject item in jArray2)
            {
                personId = item.GetValue("personId").ToString();
            }

            Debug.WriteLine($"IDENTIFY PERSONID:{personId}");
            return personId;
        }

        public Person getPersonFromId(String personId)
        {

            if (personId == "")
            {
                Debug.WriteLine($"PERSON NOT FOUND");
                Person p = new Person
                {
                    Name = "PERSON NOT FOUND",
                };
                return p;
            }

            var client = new RestClient($"https://eastus.api.cognitive.microsoft.com/face/v1.0/largepersongroups/{LARGE_PERSON_GROUP_ID}/persons/{personId}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Ocp-Apim-Subscription-Key", SUBSCRIPTION_KEY);
            IRestResponse response = client.Execute(request);
            Debug.WriteLine($"GET PERSON RESPONSE: {response.Content}");

            var data = (JObject)JsonConvert.DeserializeObject(response.Content);
            string name = data["name"].Value<string>();
            string userData = data["userData"].Value<string>();

            Debug.WriteLine($"PERSON NAME: {name}");

            Person person = new Person {
                PersonId = personId,
                Name = name,
                UserData = userData,
                LoggedIn = DateTime.Now.ToString(),
                LoggedOut = null,
            };

            return person;
        }

        public string DetectFromLocalFile(byte[] bytes)
        {
            var client = new RestClient("https://eastus.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceId=true&returnFaceLandmarks=true&recognitionModel=recognition_04&returnRecognitionModel=false&detectionModel=detection_03&faceIdTimeToLive=86400");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/octet-stream");
            request.AddHeader("Ocp-Apim-Subscription-Key", SUBSCRIPTION_KEY);
            request.AddParameter("application/octet-stream", bytes, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            Debug.WriteLine($"DETECT RESPONSE:{response.Content}");

            JArray jArray = JArray.Parse(response.Content);
            if (jArray.Count() > 1)
            {
                Debug.WriteLine("MULTIPLE FACES DETECTED");
                return "MULTIPLE FACES DETECTED";
            }
            if (jArray.Count() < 1)
            {
                Debug.WriteLine("FACE NOT DETECTED");
                return "FACE NOT DETECTED";
            }

            string faceid = "";

            foreach (JObject item in jArray)
            {
                faceid = item.GetValue("faceId").ToString();
            }

            Debug.WriteLine($"DETECT FACEID:{faceid}");
            return faceid;
        }

        public void TrainLargeGroup()
        {
            var client = new RestClient($"https://eastus.api.cognitive.microsoft.com/face/v1.0/largepersongroups/{LARGE_PERSON_GROUP_ID}/train");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Ocp-Apim-Subscription-Key", SUBSCRIPTION_KEY);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            Console.WriteLine("GROUP TRAINED");
        }
    }
}
