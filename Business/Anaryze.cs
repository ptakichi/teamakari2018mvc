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

namespace teamakari2018mvc.Business
{

    public class Anaryze {
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
    }
}