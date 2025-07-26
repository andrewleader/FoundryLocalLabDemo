# Foundry Local on Windows Lab Demo

In this lab demo, we are going to be building a financial college support desk app that uses local LLMs to help pre-populate information for our support representatives.

<img width="1378" height="444" alt="image" src="https://github.com/user-attachments/assets/95bfa1a2-a669-4025-ab94-07501ca7f9df" />

## Step 1: Open the solution

Double click the FoundryLocalLabDemo.sln file in the root directory to open the solution.

<img width="193" height="80" alt="image" src="https://github.com/user-attachments/assets/b8826a18-134f-467c-91d8-789853443803" />

## Step 2: Deploy the app

Click the Start Debugging button to deploy the app. We'll keep it open while we edit, and see changes appear live!

<img width="385" height="50" alt="image" src="https://github.com/user-attachments/assets/2d06710f-b7dc-4183-9db4-ea77444a9ca4" />

The app should look like this when it launches.

<img width="400" alt="image" src="https://github.com/user-attachments/assets/11206476-5eaf-440e-b34f-fb55776b2997" />

Notice that no models appear in the middle section. We're going to add the ability to use local LLMs using Foundry Local!

## Step 3: Inspect the NuGet packages

Back in Visual Studio, open the *Solution Explorer* and inspect the dependencies of the project. Notice that the Foundry Local NuGet package is installed, along with some Azure AI (cloud) libraries. If you were starting new, you would install the Foundry Local NuGet package yourself. We have it pre-installed since we leverage some of the types in the sample project.

<img width="328" height="321" alt="image" src="https://github.com/user-attachments/assets/b1aae941-094a-4c32-898d-aa919f1b9d42" />

## Step 4: Open the ExecutionLogic.cs file

Further down in the *Solution Explorer*, find and open the **ExecutionLogic.cs** file. Notice that we have a static `FoundryLocalManager` initialized, but the rest has not been implemented.

<img width="400" alt="image" src="https://github.com/user-attachments/assets/c49f7833-15ec-4208-b26d-13c87405d337" />

## Step 5: Implement getting the catalog models

First, we have to start the Foundry Local service. Update `StartServiceAsync` to call `await manager.StartServiceAsync()`.

```csharp
public static async Task StartServiceAsync()
{
    // Start the Foundry Local service
    await manager.StartServiceAsync();
}
```

Then, we need to implement the method to list all the models available in the catalog. This will return a list of all local models that your device can run, even if they haven't been downloaded.

Within `ListCatalogModelsAsync`, simply call `await manager.ListCatalogModelsAsync()` and return the results.

```csharp
public static async Task<List<ModelInfo>> ListCatalogModelsAsync()
{
    // Return a list of models available in the catalog
    return await manager.ListCatalogModelsAsync();
}
```

And finally, we need to implement the method that gets the **cached** models. These are the models that are **already downloaded** on your device.

Within `ListCachedModelsAsync`, simply call `await manager.ListCachedModelsAsync()` and return the results *(make sure you're calling the **Cached** method, the **Catalog** method looks very similar)*.

```csharp
public static async Task<List<ModelInfo>> ListCachedModelsAsync()
{
    // Return a list of models that are currently cached
    return await manager.ListCachedModelsAsync();
}
```

With those three methods implemented, save your changes (`Ctrl+S`) and then press the **Hot Reload** button (or `Alt+F10`).

<img width="135" height="49" alt="image" src="https://github.com/user-attachments/assets/ff0bb80e-f133-4a23-b899-672e69588351" />

Then, switch back to the app and click the **Refresh Models** button, which will call the APIs we just added! Notice that the models now appear! If you have some models already downloaded, you'll see them in the Downloaded Models section. You'll see the remaining models in the Available for Download section.

<img width="400" alt="image" src="https://github.com/user-attachments/assets/b2a2c6bb-cb7f-4ebc-9328-881496235648" />

Notice that each model includes information about the model, like what hardware it can run on (CPU vs NPU vs GPU).

## Step 6: Optionally implement downloading models

For this lab, if you already have models downloaded, this step is optional since models are often 2 GB or larger and will take a while to download. You can add the code to download models, but we wouldn't recommend clicking one of those in the UI, otherwise you might be stuck for a while.

Notice, however, how easy it is to get a model onto the device! Just call the API with the model ID that you want to be downloaded!

```csharp
public static IAsyncEnumerable<ModelDownloadProgress> DownloadModelAsync(string modelId)
{
    // Download the specified model
    return manager.DownloadModelWithProgressAsync(modelId);
}
```

## Step 7: Implement loading a model

These local LLMs are large, and benefit from a dedicated method to load the model so that it'll be ready for use.

Within `LoadModelAsync`, simply call `await manager.LoadModelAsync(modelId)` to load the specified model into memory!

```csharp
public static async Task LoadModelAsync(string modelId)
{
    // Load the specified model
    await manager.LoadModelAsync(modelId);
}
```

Let's also implement `UnloadModelAsync`... when we're done with a model, it's a good idea to unload it immediately from memory.

```csharp
public static async Task UnloadModelAsync(string modelId)
{
    // Unload the specified model
    await manager.UnloadModelAsync(modelId);
}
```

Now, save your changes (`Ctrl+S`) and press the **Hot Reload** button (or `Alt+F10`).

Switch back to the app and click on the **Phi-3.5-mini-instruct-generic-cpu** model to load the model in memory, which calls the code we just added!

<img width="435" height="337" alt="image" src="https://github.com/user-attachments/assets/a3967c2d-f77d-475c-bd28-908b567ae67a" />

Feel free to click around on some of the messages in the left pane. You'll notice you'll get HTTP 401 errors since we haven't implemented the connection to the models yet.

## Step 8: Implement inferencing a model

Inferencing a model with Foundry Local is actually identical to inferencing a cloud model. We simply need to point our inferencing code to the local model endpoint that Foundry Local exposes. This enables us to easily swap between local and cloud models without changing our inferencing code!

Our inferencing code is already written using the **Azure.AI.OpenAI** library. But we don't have an API key or an endpoint yet.

Update the API key to use Foundry Local's API key: `new ApiKeyCredential(manager.ApiKey)`.

Update the Endpoint to use Foundry Local's endpoint: `Endpoint = manager.Endpoint`.

And that's it! Your method should now look like this...

```csharp
public static async IAsyncEnumerable<StudentProfileUpdate> ParseStudentProfileStreamingAsync(string modelId, string userMessage, [EnumeratorCancellation] CancellationToken cancellationToken)
{
    var chatClient = new ChatClientBuilder(
            new OpenAIClient(new ApiKeyCredential(manager.ApiKey), new OpenAIClientOptions
            {
                Endpoint = manager.Endpoint // Foundry Local endpoint
            })
            .GetChatClient(modelId)
            .AsIChatClient())
        .Build();

    ...
```

Browse through the rest of the code to notice what prompt we're using. We're asking the model to parse the user's message into a JSON object that we'll then use to pre-populate UI fields within the app. We've implemented streaming responses, which adds some complexity to the code, but the newer `await foreach` operator helps you easily handle streaming responses. And finally, we have some logic to clean up the responses which sometimes include non-JSON item like markdown formatters and more.

Save your changes (`Ctrl+S`) and press the **Hot Reload** button (or `Alt+F10`).

Switch back to the app and click on some of the messages in the left panel. Watch the response come in, and watch it populate the fields!

<img width="1378" height="444" alt="image" src="https://github.com/user-attachments/assets/95bfa1a2-a669-4025-ab94-07501ca7f9df" />