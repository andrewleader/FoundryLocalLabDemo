using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Assistants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundryLocalLabDemo
{
    public static class ExecutionLogic
    {
        public static IAsyncEnumerable<ChatResponseUpdate> GenerateBotResponseAsync(List<ChatMessage> chatMessages, StudentProfile profile)
        {
            var chatClient = new ChatClientBuilder(
                    new OpenAIClient("key")
                    .GetChatClient("deepseek")
                    .AsIChatClient())
                .Build();

            return chatClient.GetStreamingResponseAsync(chatMessages);
        }
    }
}
