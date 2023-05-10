using Azure;
using Azure.Communication;
using Azure.Communication.Chat;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;

namespace ChatQuickstart
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            OpenAIClient client = new OpenAIClient(
                new Uri(configuration["Azure:OpenAI:Uri"]),
                new AzureKeyCredential(configuration["Azure:OpenAI:Key"]));

            Uri endpoint = new Uri(configuration["Azure:CommunicationServices:Endpoint"]);

            var botToken = configuration["Azure:CommunicationServices:Bot:Token"];
            var botId = configuration["Azure:CommunicationServices:Bot:Id"];

            var userToken = configuration["Azure:CommunicationServices:User:Token"];
            var userId = configuration["Azure:CommunicationServices:User:Id"];


            // Create a new ChatClient for the bot using its token.
            ChatClient botChatClient = new ChatClient(endpoint, new CommunicationTokenCredential(botToken));

            // Create a new chat thread with the bot and the user as participants.
            CreateChatThreadResult createChatThreadResult = await botChatClient.CreateChatThreadAsync(
                topic: "Hello GPT Bot!", 
                participants: new[] 
                { 
                    new ChatParticipant(identifier: new CommunicationUserIdentifier(id: botId))
                    {
                        DisplayName = "ChatGPT Bot"
                    },
                    new ChatParticipant(identifier: new CommunicationUserIdentifier(id: userId))
                    {
                        DisplayName = "User"
                    }
                }
            );

            // Get the ChatThreadClient for the bot.
            ChatThreadClient botChatThreadClient = botChatClient.GetChatThreadClient(threadId: createChatThreadResult.ChatThread.Id);

            // Create a new ChatClient for the user using its token.
            ChatClient userChatClient = new ChatClient(
                endpoint, 
                new CommunicationTokenCredential(userToken)
            );

            // Get the ChatThreadClient for the user.
            ChatThreadClient userChatThreadClient = userChatClient.GetChatThreadClient(threadId: createChatThreadResult.ChatThread.Id);

            // Infinite loop to keep the chat going.
            while(true)
            {
                // Get the user's input from the console.
                Console.Write("User: ");
                var userInput = Console.ReadLine();

                // Create the send message options with the user's input.
                SendChatMessageOptions sendChatMessageOptions = new SendChatMessageOptions()
                {
                    Content = userInput,
                    MessageType = ChatMessageType.Text
                };

                // Send the user's message to the chat thread.
                SendChatMessageResult sendChatMessageResult = await userChatThreadClient.SendMessageAsync(sendChatMessageOptions);

                // Set up the options for the chat completion.
                var opt = new ChatCompletionsOptions()
                {
                    Temperature = 0.7f,
                    MaxTokens = 800,
                    NucleusSamplingFactor = 0.95f,
                    FrequencyPenalty = 0,
                    PresencePenalty = 0
                };

                // Create a system message.
                var systemMessage = new Azure.AI.OpenAI.ChatMessage(ChatRole.System, @"You are an AI assistant that helps people find information.");

                // Create a list of messages and add the system message.
                var messages = new List<Azure.AI.OpenAI.ChatMessage>() {systemMessage};

                // Fetch all messages from the chat thread.
                AsyncPageable<Azure.Communication.Chat.ChatMessage> allMessages = botChatThreadClient.GetMessagesAsync();

                // Create a list to store all the fetched messages.
                List<Azure.Communication.Chat.ChatMessage> allMessagesList = new List<Azure.Communication.Chat.ChatMessage>();
                
                // Iterate over the fetched messages and add them to the list.
                await foreach (Azure.Communication.Chat.ChatMessage message in botChatThreadClient.GetMessagesAsync())
                {
                    allMessagesList.Add(message);
                }

                // Sort the list by the CreatedOn property, from oldest to newest.
                allMessagesList.Sort((x, y) => DateTimeOffset.Compare(x.CreatedOn, y.CreatedOn));

                // Iterate over the sorted messages.
                foreach (var message in allMessagesList)
                {
                    // If the message was sent by the bot, add it to the options.
                    if(message.Sender?.RawId == botId)
                    {
                        opt.Messages.Add(new Azure.AI.OpenAI.ChatMessage(ChatRole.Assistant, message.Content.Message));
                    }
                    else if(message.Sender?.RawId == userId) // If the message was sent by the user, add it to the options.
                    {
                        opt.Messages.Add(new Azure.AI.OpenAI.ChatMessage(ChatRole.User, message.Content.Message));
                    }
                }

                // Fetch the chat completions using the OpenAI client.
                Response<ChatCompletions> responseWithoutStream = await client.GetChatCompletionsAsync(
                    "acs-test-chatgpt",
                    opt);

                ChatCompletions completions = responseWithoutStream.Value;
                Console.WriteLine("GPT: " + completions.Choices[0].Message.Content);

                // Send the completion's content as a message to the chat thread.
                SendChatMessageOptions responseChatOptions = new SendChatMessageOptions()
                {
                    Content = completions.Choices[0].Message.Content,
                    MessageType = ChatMessageType.Text
                };

                await userChatThreadClient.SendMessageAsync(responseChatOptions);
            }

        }
    }
}