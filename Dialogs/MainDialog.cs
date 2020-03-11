// Copyright (c) Kritten GmbH / Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace azuregeek.UnifiTeamsBotApp
{
    public class MainDialog : LogoutDialog
    {
        protected readonly ILogger Logger;        
        protected readonly BotConfiguration botConfiguration;

        // Define value names for values tracked inside the dialogs.
        private const string constVoucherRequest = "value-voucherRequest";

        // Setup variables, logger and dialogs in constructor
        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog), configuration["AzureADConnectionName"])
        {
            // Setup variables
            Logger = logger;
            botConfiguration = new BotConfiguration
            {
                AllowedTenantId = new Guid(configuration["AllowedTenantID"]),
                AzureADConnectionName = configuration["AzureADConnectionName"],
                UnifiAPIUsername = configuration["UnifiAPIUsername"],
                UnifiAPIPassword = configuration["UnifiAPIPassword"],
                UnifiAPIUri = new Uri(configuration["UnifiAPIUri"]),
                BandwidthDownKbps = Convert.ToInt32(configuration["BandwidthDownKbps"]),
                BandwidthUpKbps = Convert.ToInt32(configuration["BandwidthUpKbps"]),
                HotspotSSID = configuration["HotspotSSID"]
            };

            // Initialize Azure AD Sign In Prompt
            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = botConfiguration.AzureADConnectionName,
                    Text = $"Bitte melde dich mit deiner OdID an, damit ich einen Hotspot-Voucher erzeugen kann.",
                    Title = "OdID-Anmeldung",
                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)                    
                }));

            // Initialize Dialog for Text/Choice prompts
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            // Intialize workflow (Waterfall Dialog)
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                LoginAndWelcomeStepAsync,
                ValidityPeriodStepAsync,
                GenerateAndResponseVoucherStepAsync,
            }));
            
            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        // Show Sign In Prompt
        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        // Welcome user after Sign in, check if Azure AD Tenant has acces and ask for voucher comment
        private async Task<DialogTurnResult> LoginAndWelcomeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse?.Token != null)
            {                
                // Pull in the data from the Microsoft Graph.
                var client = new GraphClient(tokenResponse.Token);
                var me = await client.GetMeAsync();

                // Check if Azure AD Tenant ID matches configured Tenant ID or end Session                
                if(JwtTokenUtil.GetAzureTenantIdFromToken(tokenResponse.Token) != botConfiguration.AllowedTenantId)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Ich darf nicht mit Fremden sprechen - Sorry!"), cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }

                // Instantiate new voucher request and add it to stepContext Values
                var voucherRequest = new VoucherRequest();
                voucherRequest.UserGivenName = me.GivenName;
                voucherRequest.UserPrincipalName = me.UserPrincipalName;
                voucherRequest.BandwidthDownKbps = botConfiguration.BandwidthDownKbps;
                voucherRequest.BandwidthUpKbps = botConfiguration.BandwidthUpKbps;

                stepContext.Values[constVoucherRequest] = voucherRequest;

                // Say Hello :-)
                await stepContext.Context.SendActivityAsync($"Moin { voucherRequest.UserGivenName }! Freut mich, dass ich dir heute bei der Erstellung eines Gäste-WiFi-Voucher helfen darf\U0001F605! Du kannst dich jederzeit mit einer freundlichen Verabschiedung wie \"Ciao\" wieder abmelden.");

                // Ask for voucher comment
                var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Für wen bzw. für was genau benötigst du den Voucher?") };
                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Die Anmeldung an deiner OdID hat leider nicht funktioniert. Bitte schreib' mir in den nächsten Minuten einfach noch einmal. Vielleicht helfe ich dir dann\U0001F607"), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        // Ask for voucher validity
        private async Task<DialogTurnResult> ValidityPeriodStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Confirm message received
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Alles klar, klingt gut."), cancellationToken);

            //Initialize Context Data
            var result = (String)stepContext.Result;
            var voucherRequest = (VoucherRequest)stepContext.Values[constVoucherRequest];

            // store updated voucherRequest object in stepContext Values
            voucherRequest.NewVoucherComment = result;
            stepContext.Values[constVoucherRequest] = voucherRequest;

            // Ask for voucher validity
            // Create the list of validity options to choose from.
            var validityChoices = new List<Choice>();
            validityChoices.Add(new Choice { Value = "4 Stunden", Synonyms = new List<string> { "4 Stunden", "4" } });
            validityChoices.Add(new Choice { Value = "12 Stunden", Synonyms = new List<string> { "12 Stunden", "Tag", "Arbeitstag", "12" } });
            validityChoices.Add(new Choice { Value = "2 Tage", Synonyms = new List<string> { "2 Tage", "48 Stunden", "48" } });
            validityChoices.Add(new Choice { Value = "180 Tage", Synonyms = new List<string> { "180 Tage", "halbes Jahr" } });            
            
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Bitte verrate mir jetzt noch, wie lange der Voucher gültig sein soll. Du kannst die gewünschte Dauer auch als Nachricht senden anstatt auf den Button zu klicken:"),
                RetryPrompt = MessageFactory.Text("Wähle bitte eine Gültigkeitsdauer aus der Liste aus, die ich auch anbiete. Wir sind hier ja nicht bei Wünsch dir was!\U0001F624"),
                Choices = validityChoices,
                Style = ListStyle.HeroCard,
            };
            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> GenerateAndResponseVoucherStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Initialize Context Data
            var validityResponse = (FoundChoice)stepContext.Result;
            var voucherRequest = (VoucherRequest)stepContext.Values[constVoucherRequest];

            // Confirm message received
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Perfekt, ich kümmere mich jetzt um die Erstellung eines Vouchers für {voucherRequest.NewVoucherComment} und eine Dauer von {validityResponse.Value}. Bitte habe einen kleinen Moment Geduld, solange ich in meiner Voucher-Kiste krame."), cancellationToken);

            // Calculate Voucher Validity duration
            switch(validityResponse.Value)
            {
                case "4 Stunden":
                    voucherRequest.NewVoucherValidHours = 4;
                    break;
                case "12 Stunden":
                    voucherRequest.NewVoucherValidHours = 12;
                    break;
                case "2 Tage":
                    voucherRequest.NewVoucherValidHours = 48;
                    break;
                case "180 Tage":
                    voucherRequest.NewVoucherValidHours = 4320;
                    break;
            }

            // Create Voucher
            UnifiClient unifiClient = new UnifiClient(botConfiguration.UnifiAPIUsername, botConfiguration.UnifiAPIPassword, botConfiguration.UnifiAPIUri);
            VoucherResponse voucherResponse = await unifiClient.CreateGuestVoucher(voucherRequest);
            voucherResponse.HotspotSSID = botConfiguration.HotspotSSID;

            // Create Adaptive Hotspot Voucher Card
            var cardAttachment = AdaptiveCardHandler.CreateHotspotVoucherAdaptiveCard(voucherResponse);
            var reply = MessageFactory.Attachment(cardAttachment);
            reply.Text = "So, und hier ist schon dein Voucher. Viel Spaß!";

            // Send Voucher Code to user            
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            

            // End Dialog
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }        
    }
}
