using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundryLocal.Core.Services;

//// TODO: Could better use Dependency Injection to provide this service where needed
public static class ChatService
{
    public static async IAsyncEnumerable<StudentProfileUpdate> ParseStudentProfileStreamingAsync(ModelManager modelManager, string userMessage, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var chatClient = new ChatClientBuilder(
                new OpenAIClient(new ApiKeyCredential(modelManager.ApiKey), new OpenAIClientOptions
                {
                    Endpoint = modelManager.Endpoint
                })
                .GetChatClient(modelManager.SelectedModel.Name)
                .AsIChatClient())
            .Build();

        // Create a custom schema with string enums instead of using JSchemaGenerator
        var schema = StudentProfile.GetJSONSchema();

        string prompt = "Parse the provided user text into a JSON object that matches the provided JSON schema. If a property isn't found from the user text, leave it null. Be sure to set the HighSchoolStatus and CitizenshipStatus fields exactly as their enum string values are specified in the schema, do not add spaces or punctuation. Respond ONLY with the JSON object.";
        prompt += "\n\n";
        prompt += "JSON SCHEMA:\n\n";
        prompt += schema;
        prompt += "\n\nUSER TEXT:\n\n";
        prompt += userMessage;

        var streamingResp = chatClient.GetStreamingResponseAsync(prompt, cancellationToken: cancellationToken);

        var respText = "";
        await foreach (var streamResp in streamingResp)
        {
            respText += streamResp.Text;

            if (streamResp.FinishReason == null)
            {
                yield return new StudentProfileUpdate
                {
                    Text = streamResp.Text,
                    StudentProfile = null // Not finalized yet
                };
            }
        }

        StudentProfile? parsedProfile = null;

        try
        {
            // Configure JsonSerializerOptions to handle string enums during deserialization
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            parsedProfile = JsonSerializer.Deserialize<StudentProfile>(ExtractJson(respText), options);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to parse response:\n\n" + respText);
        }

        if (parsedProfile != null)
        {
            yield return new StudentProfileUpdate
            {
                Text = "",
                StudentProfile = parsedProfile
            };
        }
    }

    /// <summary>
    /// Helper method to ensure clean ouput of JSON from the response text before trying to deserialize it.
    /// </summary>
    /// <param name="respText">Input text from the response.</param>
    /// <returns>Extracted JSON object.</returns>
    private static string ExtractJson(string respText)
    {
        // Handle cleaning up the response to get to the JSON
        respText = respText.Trim();
        if (respText.StartsWith("<think>"))
        {
            respText = respText.Substring("<think>".Length);
            respText = respText.Substring(respText.IndexOf("</think>") + "</think>".Length);
            respText = respText.Trim();
        }
        if (respText.StartsWith("```json"))
        {
            respText = respText.Substring("```json".Length);
            respText = respText.Substring(0, respText.Length - 3);
            respText = respText.Trim();
        }
        if (respText.LastIndexOf("{") != 0)
        {
            // Handle when it returns a nested object
            respText = respText.Substring(1, respText.Length - 2);
            respText = respText.Trim();
        }

        return respText;
    }
}
