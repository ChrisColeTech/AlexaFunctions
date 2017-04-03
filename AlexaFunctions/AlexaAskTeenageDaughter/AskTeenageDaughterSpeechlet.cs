﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using AlexaSkillsKit.Speechlet;
using AlexaSkillsKit.UI;
using AlexaSkillsKit.Slu;


namespace AlexaFunctions
{
    public class AskTeenageDaughterSpeechlet : SpeechletAsync
    {
        #region Constants and Private Members
        const string PROTECTEDWORDS = "mom, mother, mommy, dad, father, daddy";
        #endregion

        #region Properties
        public TraceWriter Logger { get; set; }
        public IAsyncCollector<string> AskTeenageQueue { get; set; }
        #endregion

        #region Constructor
        public AskTeenageDaughterSpeechlet(TraceWriter log, IAsyncCollector<string> alexaAskTeenageRequestQueue)
        {
            Logger = log;
            AskTeenageQueue = alexaAskTeenageRequestQueue;
        }
        #endregion

        #region Public Overrides
        public override async Task OnSessionStartedAsync(SessionStartedRequest request, Session session)
        {
            AskTeenageQueue.AddAsync(JsonConvert.SerializeObject(request));
            Logger.Info($"OnSessionStarted requestId={request.RequestId}, sessionId={session.SessionId}");
        }

        public override async Task OnSessionEndedAsync(SessionEndedRequest request, Session session)
        {
            AskTeenageQueue.AddAsync(JsonConvert.SerializeObject(request));
            Logger.Info($"OnSessionStarted requestId={request.RequestId}, sessionId={session.SessionId}");
        }

        public override async Task<SpeechletResponse> OnLaunchAsync(LaunchRequest request, Session session)
        {
            try
            {
                AskTeenageQueue.AddAsync(JsonConvert.SerializeObject(request));
            }
            catch (Exception ex)
            {
                Logger.Error($"Exception: {ex.ToString()}");
            }
            Logger.Info($"OnSessionStarted requestId={request.RequestId}, sessionId={session.SessionId}");
            return await GetWelcomeResponseAsync();
        }

        public override async Task<SpeechletResponse> OnIntentAsync(IntentRequest request, Session session)
        {
            // Get intent from the request object.
            Intent intent = request.Intent;
            string intentName = (intent != null) ? intent.Name : null;

            Logger.Info($"OnIntent intentName={intentName} requestId={request.RequestId}, sessionId={session.SessionId}");
            AskTeenageQueue.AddAsync(JsonConvert.SerializeObject(request));
            AskTeenageQueue.AddAsync(JsonConvert.SerializeObject(session));
            

            // Note: If the session is started with an intent, no welcome message will be rendered;
            // rather, the intent specific response will be returned.

            switch (intentName)
            {
                case "AskTeenageDaughterOpinion":
                    return await BuildAskTeenageDaughterOpinionResponseAsync(intent, session);
                case "AskTeenageDaughterParticipation":
                    return await BuildAskTeenageDaughterParticipationResponseAsync(intent, session);
                case "AskTeenageDaughterStatus":
                    return await BuildAskTeenageDaughterStatusResponseAsync(intent, session);
                default:
                    throw new SpeechletException("Invalid Intent");
            }
        }
        #endregion

        #region Private Methods
        private async Task<SpeechletResponse> GetWelcomeResponseAsync()
        {
            // Create the welcome message.
            string speechOutput =
                "Say something like\nGood Morning.\nDo you want to go to soccer?\n What do you think of jogging?";

            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input
            return await BuildSpeechletResponseAsync("Welcome", speechOutput, false);
        }
        private async Task<SpeechletResponse> BuildSpeechletResponseAsync(string title, string output, bool shouldEndSession)
        {
            // Create the Simple card content.
            SimpleCard card = new SimpleCard();
            card.Title = String.Format("SessionSpeechlet - {0}", title);            
            card.Content = String.Format("SessionSpeechlet - {0}", output);

            // Create the plain text output.
            PlainTextOutputSpeech speech = new PlainTextOutputSpeech();
            speech.Text = output;

            // Create the speechlet response.
            SpeechletResponse response = new SpeechletResponse();
            response.ShouldEndSession = shouldEndSession;
            response.OutputSpeech = speech;
            response.Card = card;
            return response;
        }

        private async Task<SpeechletResponse> BuildAskTeenageDaughterOpinionResponseAsync(Intent intent, Session session)
        {
            string subject = intent.Slots["Subject"].Value;
            string speechOutput = (PROTECTEDWORDS.Contains(subject)) ?
                $"{subject} rules." :
                $"{subject} sucks.";
            return await BuildSpeechletResponseAsync(intent.Name, speechOutput, false);

        }
        private async Task<SpeechletResponse> BuildAskTeenageDaughterParticipationResponseAsync(Intent intent, Session session)
        {
            string activity = intent.Slots["Activity"].Value;
            string speechOutput = (PROTECTEDWORDS.Contains(activity)) ?
                $"{activity} rules." :
                $"{activity} sucks.";
            return await BuildSpeechletResponseAsync(intent.Name, speechOutput, false);
        }
        private async Task<SpeechletResponse> BuildAskTeenageDaughterStatusResponseAsync(Intent intent, Session session)
        {
            string speechOutput = "Growl";
            return await BuildSpeechletResponseAsync(intent.Name, speechOutput, false);

        }
    }
#endregion
}