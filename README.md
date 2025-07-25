# Foundry Local on Windows Lab Demo

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

First, we have to start the Foundry Local service. Update `StartServiceAsync` to return the result from `manager.StartServiceAsync()`.

```csharp
public static Task StartServiceAsync()
{
    // Start the Foundry Local service
    return manager.StartServiceAsync();
}
```

Then, we need to implement the method to list all the models available in the catalog. This will return a list of all local models that your device can run, even if they haven't been downloaded.

Within `ListCatalogModelsAsync`, simply call 
