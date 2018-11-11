using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using System.Web;
using Microsoft.AspNetCore.Http;
using System.IO;

using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;

using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;

namespace teamakari2018mvc.Business
{

    public class Anaryze {

        public List<DangerData> dangerDatas = new List<DangerData>();

        public Anaryze(){

            //危険な言葉を定義する
            dangerDatas.Add(new DangerData("銀行",100));
            dangerDatas.Add(new DangerData("振込",100));
            dangerDatas.Add(new DangerData("弁護士",100));
            dangerDatas.Add(new DangerData("やり方",80));
        }

        // Translation using file input.
        public async Task<String> TranslationWithFileAsync(string uploadfilePath)
        {

            String result = "";
            
            // <TranslationWithFileAsync>
            // Translation source language.
            // Replace with a language of your choice.
//            string fromLanguage = "en-US";
            string fromLanguage = "ja-JP";

            // Creates an instance of a speech translation config with specified subscription key and service region.
            // Replace with your own subscription key and service region (e.g., "westus").
            var config = SpeechTranslationConfig.FromSubscription("a379dbbb3eaf4aad907ac43bbc7558bf", "southeastasia");
            config.SpeechRecognitionLanguage = fromLanguage;
            
            // Translation target language(s).
            // Replace with language(s) of your choice.
            config.AddTargetLanguage("ja");

            var stopTranslation = new TaskCompletionSource<int>();

            // Creates a translation recognizer using file as audio input.
            // Replace with your own audio file name.
            //            using (var audioInput = AudioConfig.FromWavFileInput(@"whatstheweatherlike.wav"))
//            using (var audioInput = AudioConfig.FromWavFileInput(@"hurikome2.wav"))



            // using (var audioInput = AudioConfig.FromWavFileInput(@"hurikome2.wav"))
            using (var audioInput = AudioConfig.FromWavFileInput(uploadfilePath))
            {
                using (var recognizer = new TranslationRecognizer(config, audioInput))
                {
                    // Subscribes to events.
                    recognizer.Recognizing += (s, e) =>
                    {
                        Console.WriteLine($"RECOGNIZING in '{fromLanguage}': Text={e.Result.Text}");
                        foreach (var element in e.Result.Translations)
                        {
                            Console.WriteLine($"    TRANSLATING into '{element.Key}': {element.Value}");
                        }
                    };

                    recognizer.Recognized += (s, e) => {
                        if (e.Result.Reason == ResultReason.TranslatedSpeech)
                        {
                            result += e.Result.Text;
                            Console.WriteLine($"RECOGNIZED in '{fromLanguage}': Text={result}");
                            // Console.WriteLine($"RECOGNIZED in '{fromLanguage}': Text={e.Result.Text}");
                            foreach (var element in e.Result.Translations)
                            {
                                Console.WriteLine($"    TRANSLATED into '{element.Key}': {element.Value}");
                            }
                        }
                        else if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            result += e.Result.Text;
                            Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                            Console.WriteLine($"    Speech not translated.");
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }

                        stopTranslation.TrySetResult(0);
                    };

                    recognizer.SpeechStartDetected += (s, e) => {
                        Console.WriteLine("\nSpeech start detected event.");
                    };

                    recognizer.SpeechEndDetected += (s, e) => {
                        Console.WriteLine("\nSpeech end detected event.");
                    };

                    recognizer.SessionStarted += (s, e) => {
                        Console.WriteLine("\nSession started event.");
                    };

                    recognizer.SessionStopped += (s, e) => {
                        Console.WriteLine("\nSession stopped event.");
                        Console.WriteLine($"\nStop translation.");
                        stopTranslation.TrySetResult(0);
                    };

                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    Console.WriteLine("Start translation...");
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopTranslation.Task });

                    // Stops translation.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
            // </TranslationWithFileAsync>
            return result;
        }



        static string host = "https://japaneast.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases";
        // string host = "https://japaneast.api.cognitive.microsoft.com/text/analytics/v2.0";
        //string path = "/translate?api-version=3.0";
        // Translate to German and Italian.
        //string params_ = "&to=de&to=it";

        //string uri = host + path + params_;

        // NOTE: Replace this example key with a valid subscription key.
        string key = "7e0089d7720341b583c51544ef48263a";

        // string textHello = "こんにちは。本多裕幸です。よろしくお願いします。今日は詐欺のアプリを作成しました。";


        public async Task<Document> Translate(String target)
        {

            Document resultObject = null;
            
            string uri = host;

            var body = new { documents = new List<object>{ new { language = "ja",id ="1",text = target }}} ;

            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient()){
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(uri);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", key);

                    var response = await client.SendAsync(request);
                    var responseBody = await response.Content.ReadAsStringAsync();
                    String result = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(responseBody), Formatting.Indented);
                    resultObject = JsonConvert.DeserializeObject<Document>(result);

                    Console.OutputEncoding = UnicodeEncoding.UTF8;
                    Console.WriteLine(result);
                }
            }

            return resultObject;
        }


        public List<DangerData> JudgeData(List<String> org){
            List<DangerData> result = new List<DangerData>();

            Boolean isfind = false;
            int ritsu = 0;
            
            foreach(String d in org) {
                isfind = false;
                ritsu = 0;

                foreach(DangerData danger in this.dangerDatas){
                    if(d.IndexOf(danger.dangerPhrase) >= 0){
                        isfind = true;
                        ritsu = danger.dangerRitsu;
                        break;
                    }else{
                        
                    }
                }

                if (isfind){
                    result.Add(new DangerData(d,ritsu));
                }else{
                    result.Add(new DangerData(d,0));
                }
            }
            
            return result;
        }

    }


    public class Document {
        public List<Data> documents { get; set; }
    }

    public class Data{
        public String id { get; set; }
        public List<String> keyPhrases { get; set; }
        
    }

    public class DangerData{
        public DangerData(String dangerPhrase,int dangerRitsu){
            this.dangerPhrase = dangerPhrase;
            this.dangerRitsu = dangerRitsu;
        }
        public String dangerPhrase { get; set; }
        public int dangerRitsu{get;set;}
    }


}