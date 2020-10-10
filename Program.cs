using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Intent;

namespace speechtest
{
    public class TopScoringIntent
    {
        public string Intent { get; set; }
        public double Score { get; set; }
    }

    public class Resolution
    {
        string[] Values { get; set; }
    }

    public class EntityType
    {
        public string Entity { get; set; }
        public string Type { get; set; }

        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public Resolution Resolution { get; set; }
    }

    public class Intent
    {

        public string Query { get; set; }
        public TopScoringIntent TopScoringIntent { get; set; }

        public EntityType[] Entities { get; set; }
    }

    class Program
    {
        private static async Task<string> awaitCommand(IntentRecognizer intentRecognizer, SpeechSynthesizer synth)
        {
            await SayAsync(synth, "Yes sir?");
            var r = await intentRecognizer.RecognizeOnceAsync().ConfigureAwait(false);

            switch (r.Reason)
            {
                case ResultReason.RecognizedIntent:
                    return r.Properties.GetProperty(PropertyId.LanguageUnderstandingServiceResponse_JsonResult);
                default:
                    return null;
            }
        }

        public static async Task SayAsync(SpeechSynthesizer synth, string text) => await synth.SpeakSsmlAsync($@"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xmlns:mstts='https://www.w3.org/2001/mstts' xml:lang='en-US'><voice name='en-GB-MiaNeural'>                <prosody pitch='+1st' rate='1.2' >                    {text}                </prosody>            </voice>            </speak>");

        async static Task ParseCommand(SpeechSynthesizer synth, string command)
        {
            Console.WriteLine(command);

            var i = System.Text.Json.JsonSerializer.Deserialize<Intent>(command, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (command == null)
            {
                Console.WriteLine("I beg your pardon");
            }
            else if (command.Equals("go away.", StringComparison.CurrentCultureIgnoreCase))
            {
                await SayAsync(synth, "Going away now sir.");
            }
            else
            {
                Console.WriteLine(command);
                await SayAsync(synth, $"{command}, aye sir.");
            }
        }

        async static Task Main(string[] args)
        {
            const string WAKE_WORD = "hey computer";
            var speechConfig = SpeechConfig.FromSubscription("e073d2855d604ddda74ba6518ab2e6b3", "westeurope");
            var Intentconfig = SpeechConfig.FromSubscription("9051c66d5ba949ac84e32b01c37eb9b4", "westus");
            var audioConfig = AudioConfig.FromDefaultMicrophoneInput();

            var model = LanguageUnderstandingModel.FromAppId("7f7a9344-69b6-4582-a01d-19ffa3c9bed8");

            var continuousRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
            var intentRecognizer = new IntentRecognizer(Intentconfig, audioConfig);
            intentRecognizer.AddAllIntents(model);

            var synthesizer = new SpeechSynthesizer(speechConfig);
            bool _waitingForCommand = false;
            continuousRecognizer.Recognized += async (s, e) =>
            {
                if (!_waitingForCommand)
                {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");

                        if (e.Result.Text.Contains(WAKE_WORD, StringComparison.CurrentCultureIgnoreCase))
                        {
                            Console.WriteLine($"RECOGNIZED: {WAKE_WORD}");
                            _waitingForCommand = true;
                            await ParseCommand(synthesizer, await awaitCommand(intentRecognizer, synthesizer));
                            _waitingForCommand = false;
                            Console.WriteLine("Listening for wake word.");
                        }
                    }
                }
            };

            await continuousRecognizer.StartContinuousRecognitionAsync();
            Console.Write("Press any key!");
            Console.Read();
        }
    }
}
