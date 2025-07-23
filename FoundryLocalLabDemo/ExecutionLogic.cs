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
        private static FoundryLocalManager manager = new FoundryLocalManager();

        public static async Task StartServiceAsync()
        {
            await manager.StartServiceAsync();
        }

        public static Task<List<ModelInfo>> ListCatalogModelsAsync()
        {
            return manager.ListCatalogModelsAsync();
        }

        public static Task<List<ModelInfo>> ListCachedModelsAsync()
        {
            return manager.ListCachedModelsAsync();
        }

        public static Task LoadModelAsync(string modelName)
        {
            return manager.LoadModelAsync(modelName);
        }

        public static IAsyncEnumerable<ChatResponseUpdate> GenerateBotResponseAsync(string modelName, List<ChatMessage> chatMessages, StudentProfile profile, CancellationToken cancellationToken)
        {
            var chatClient = new ChatClientBuilder(
                    new OpenAIClient(new ApiKeyCredential(manager.ApiKey), new OpenAIClientOptions
                    {
                        Endpoint = manager.Endpoint
                    })
                    .GetChatClient(modelName)
                    .AsIChatClient())
                .Build();

            return chatClient.GetStreamingResponseAsync(chatMessages, cancellationToken: cancellationToken);
        }
    }
}
