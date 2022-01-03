using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VisioForge.Libs.NAudio.Wave;

namespace Face_rec
{
    public class AudioApiHelper
    {

        public const string SUBSCRIPTION_KEY = "YOUR_AUDIO_SUBSCRIPTION_KEY";


        public string create_identification_profile()
        {

            var client = new RestClient("https://westus.api.cognitive.microsoft.com/speaker/identification/v2.0/text-independent/profiles");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Ocp-Apim-Subscription-Key", SUBSCRIPTION_KEY);
            request.AddHeader("Content-Type", "application/json");
            var body = @"{
                            " + "\n" +
                            @"    ""locale"":""en-us""
                            " + "\n" +
                             @"}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Debug.WriteLine(response.Content);

            var data = (JObject)JsonConvert.DeserializeObject(response.Content);
            string profileId = data["profileId"].Value<string>();
            Debug.WriteLine($"create Identification profile ====>" + profileId);
            return profileId;

        }

        public string enroll_voice_profile(string profileId, byte[] bytes)
        {
            File.WriteAllBytes("inputSignUp.wav", bytes);

            FileStream fileStream = new FileStream("inputSignUp.wav", FileMode.Open);
            var waveFormat = new NAudio.Wave.WaveFormat(48000, 16, 1);
            var reader = new NAudio.Wave.RawSourceWaveStream(fileStream, waveFormat);
            using (NAudio.Wave.WaveStream convertedStream = WaveFormatConversionStream.CreatePcmStream(reader))
            {
                NAudio.Wave.WaveFileWriter.CreateWaveFile("outputSignUp.wav", convertedStream);
            }
            fileStream.Close();

            byte[] signupBytes = File.ReadAllBytes("outputSignUp.wav");


            var client = new RestClient("https://westus.api.cognitive.microsoft.com/speaker/identification/v2.0/text-independent/profiles/{profileId}/enrollments");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Ocp-Apim-Subscription-Key", SUBSCRIPTION_KEY);
            request.AddHeader("Content-Type", "application/octet-stream");
            request.AddParameter("application/octet-stream", signupBytes, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Debug.WriteLine(response.Content);
            var data = (JObject)JsonConvert.DeserializeObject(response.Content);
            if (data["enrollmentStatus"].Value<string>().Equals("Enrolled") && data["remainingEnrollmentsSpeechLength"].Value<float>() == 0.0)
            {

                Debug.WriteLine("==================ENROLLED==================");
                return "ENROLLED";

            }
            return "ERROR";

        }

        public string identify_person_audio(byte[] bytes)
        {
            File.WriteAllBytes("inputLogin.wav", bytes);

            FileStream fileStream = new FileStream("inputLogin.wav", FileMode.Open);
            var waveFormat = new NAudio.Wave.WaveFormat(48000,16, 1);
            var reader = new NAudio.Wave.RawSourceWaveStream(fileStream, waveFormat);
            using (NAudio.Wave.WaveStream convertedStream = WaveFormatConversionStream.CreatePcmStream(reader))
            {
                NAudio.Wave.WaveFileWriter.CreateWaveFile("outputLogin.wav", convertedStream);
            }
            fileStream.Close();

            byte[] loginBytes = File.ReadAllBytes("outputLogin.wav");

            var client = new RestClient("https://westus.api.cognitive.microsoft.com/speaker/identification/v2.0/text-independent/profiles/identifySingleSpeaker?profileIds=2d467162-2ec8-4420-bb1f-4b261a6eb978");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Ocp-Apim-Subscription-Key", SUBSCRIPTION_KEY);
            request.AddHeader("Content-Type", "application/octet-stream");
            request.AddParameter("application/octet-stream", loginBytes, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Debug.WriteLine($"===================================================================================================");
            Debug.WriteLine(response.Content);

            var data = (JObject)JsonConvert.DeserializeObject(response.Content);
            if (data["identifiedProfile"] != null)
            {
                string profileId = data["identifiedProfile"]["profileId"].Value<string>();
                Debug.WriteLine($"identification  ====>" + profileId);
                return profileId;
            }
            
            return "ERROR";


        }

            

    }
}
