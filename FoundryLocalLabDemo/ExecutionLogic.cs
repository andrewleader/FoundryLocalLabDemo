using Azure.AI.OpenAI;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Assistants;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundryLocalLabDemo
{
    public static class ExecutionLogic
    {
        private const string ModelName = "deepseek-r1-distill-qwen-7b-qnn-npu";
        private static FoundryLocalManager? manager;
        public static async Task StartModelAsync()
        {
            manager = await FoundryLocalManager.StartModelAsync(ModelName);
        }

        public static IAsyncEnumerable<ChatResponseUpdate> GenerateBotResponseAsync(List<ChatMessage> chatMessages, StudentProfile profile, CancellationToken cancellationToken)
        {
            if (manager == null)
            {
                throw new InvalidOperationException("Model is not started.");
            }

            var chatClient = new ChatClientBuilder(
                    new OpenAIClient(new ApiKeyCredential(manager.ApiKey), new OpenAIClientOptions
                    {
                        Endpoint = manager.Endpoint
                    })
                    .GetChatClient(ModelName)
                    .AsIChatClient())
                .Build();

            return chatClient.GetStreamingResponseAsync(chatMessages, cancellationToken: cancellationToken);
        }
    }
}
