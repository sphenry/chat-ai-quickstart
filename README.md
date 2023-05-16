# Azure Chat Bot Quickstart
This document will guide you on how to use this provided chatbot code, which integrates Azure Communication Services and Azure OpenAI to create a chatbot application.

## Prerequisites
An active Azure subscription.
An active OpenAI subscription.
.NET Core SDK installed on your system.

## Configuration
This code relies on certain configurations to be present in an appsettings.json file. The required settings include:

Azure:OpenAI:Uri: The OpenAI Uri.
Azure:OpenAI:Key: The OpenAI key.
Azure:CommunicationServices:Endpoint: The Azure Communication Services Endpoint URL.
Azure:CommunicationServices:Bot:Token: The token for the bot.
Azure:CommunicationServices:Bot:Id: The id for the bot.
Azure:CommunicationServices:User:Token: The token for the user.
Azure:CommunicationServices:User:Id: The id for the user.

## Code Flow
The code begins by reading the necessary configuration from appsettings.json.
It then initializes an instance of OpenAIClient with the OpenAI Uri and Key.
A ChatClient is created for the bot using its token and the Azure Communication Services Endpoint.
A new chat thread is created with the bot and user as participants.
ChatThreadClient is fetched for both the bot and the user.
An infinite loop is created to keep the chat active.
In each loop, the user's input is taken from the console and sent to the chat thread.
The OpenAI client fetches chat completions based on the history of messages sent in the chat thread and the system message.
The bot's reply is sent to the chat thread.

## Running the Code
Clone the repository or copy the code into a new .NET Core console application.
Make sure to replace the placeholders in appsettings.json with your actual configuration values.
Compile and run the code. You will be able to chat with the bot via the console.

## Note
This code is a basic implementation and does not handle errors or edge cases. In a production setting, you would want to improve this by adding error handling, logging, and possibly a more sophisticated user interface.
