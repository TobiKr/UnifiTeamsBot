# UnifiTeamsBot
Teams Bot to manage Unifi hotspot vouchers based on Auth Bot. The following features are offered:
- Azure AD Authentication
- Ask user to choose the voucher validity period
- Aks the user to comment the purpose of the voucher
- Create the voucher via Unifi REST API

This bot has been created using [Bot Framework](https://dev.botframework.com) and the TeamsAuthSample. 
> IMPORTANT: This is an early stage development

## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account)
- [.NET Core SDK](https://dotnet.microsoft.com/download) version 2.1
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution
- Forked Version of UniFiSharp: https://github.com/TobiKr/UniFiSharp (hopefully it will be merged :-)

  ```bash
  # determine dotnet version
  dotnet --version
  ```

## To try this bot

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
> the Teams service needs to call into the bot.

1) Clone the repositories

    ```bash
    git clone https://github.com/TobiKr/UniFiSharp.git
    git clone https://github.com/TobiKr/UnifiTeamsBot.git
    ```

1) If you are using Visual Studio
    - Open the solution     
    - Change the appsettings.json to suite your needs
    - The manifest file in this app adds "token.botframework.com" to the list of `validDomains`

1) Run ngrok - point to port 3940

    ```bash
    ngrok http -host-header=rewrite 3940
    ```

1) Create [Bot Framework registration resource](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration) in Azure
    - Use the current `https` URL you were given by running ngrok. Append with the path `/api/messages` used by this sample
    - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
    - __*If you don't have an Azure account*__ you can use this [Bot Framework registration](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/create-a-bot-for-teams#register-your-web-service-with-the-bot-framework)

1) Update the `appsettings.json` configuration for the bot to use the Microsoft App Id and App Password from the Bot Framework registration. (Note the App Password is referred to as the "client secret" in the azure portal and you can always create a new client secret anytime.)

1) __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the  `teamsAppManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Zip** up the contents of the `teamsAppManifest` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")

1) Run your bot, either from Visual Studio with `F5` or using `dotnet run` in the appropriate folder.

## Interacting with the bot in Teams

> Note this `manifest.json` specified that the bot will be installed in a "personal" scope only. Please refer to Teams documentation for more details.

You can interact with this bot by sending it a message. The bot will respond by requesting you to login to AAD, then making a call to the Graph API on your behalf and returning the results.

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions. I will provider a CI/CD setup later.
