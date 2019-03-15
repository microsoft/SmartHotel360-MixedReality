# SmartHotel360
During **Connect(); 2017** event this year we presented beautiful app demos using Xamarin and many features of Azure. This repository contains a mixed reality demo and reference application to show how to integrate the new mixed reality service **Azure Spatial Anchors** in a Unity project using the existing [SmartHotel360 IoT](https://github.com/Microsoft/SmartHotel360-IoT) demo.

# SmartHotel360 Repos

For this reference app scenario, we rely on several apps and services from the SmartHotel360 solution. You can find all SmartHotel360 repos in the following locations:

- [SmartHotel360](https://github.com/Microsoft/SmartHotel360)
- [IoT](https://github.com/Microsoft/SmartHotel360-IoT)
- [Mixed Reality](https://github.com/Microsoft/SmartHotel360-MixedReality)
- [Backend](https://github.com/Microsoft/SmartHotel360-Backend)
- [Website](https://github.com/Microsoft/SmartHotel360-Website)
- [Mobile](https://github.com/Microsoft/SmartHotel360-Mobile)
- [Sentiment Analysis](https://github.com/Microsoft/SmartHotel360-SentimentAnalysis)
- [Registration](https://github.com/Microsoft/SmartHotel360-Registration)

# SmartHotel360 - Mixed Reality Demo

## Getting Started

SmartHotel360 deployed a new mixed Reality solution built on top of **Azure Spatial Anchors** that is compatible with Android and HoloLens devices through Unity to visualize the information of the hotel room sensors used in the Internet of Things (IoT) solutions mentioned below. 

## Demo Scripts

You can find a **[demo script](Documents/DemoScript)** with walkthroughs once you have finished the setup of this demo.

## Setup

Prior to following these steps, you should have already completed the steps and deployed the SmartHotel360 IoT solution found in this repository: https://github.com/Microsoft/SmartHotel360-IoT. These steps rely on resources deployed from that solution and this API will not function without those resources.

### 1. Set up a Service Principal and register an Azure Active Directory application

 Follow these instructions to [create a service principal and register an Azure Active Directory (AD) application](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-create-service-principal-portal?view=azure-cli-latest).

During the creation process you will need to take note of the following information:

- Tenant Id
- App Id
- App Key

> Note: you can use the same app you created for the IoT demo.

### 2. Setup Azure Spatial Anchors Account

Use the Azure Portal to create an Azure Spatial Anchors account resource. Either create a new resource group or use an existing one and click the "Add" button. Search for "Spatial Anchors" and select the first result. Assign a name to your new Spatial Anchors account. Once the Spatial Anchors account is created, make a note of the Account Id found on the Overview page of the newly created account.

### 3. Assign permissions to the Azure AD Application Registration

After creating the application registration, you need to assign that application permissions to use the Azure Spatial Anchors account. Navigate to the newly created Spatial Anchors account in the Azure Portal and select Access Control (IAM) tab. Add a role assignment to this resource and select the Application Registration that you created in the first step. Assign this application registration to the role of Spatial Anchors Account Owner and click save.

### 4. Deploy the API project using Visual Studio

Using Visual Studio 2017, open and build the API solution. Right click on the SmartHotel.MixedReality.API project and click Publish. Create a new App Service or choose an existing App Service if you already have created one. Click Publish and wait for the operation to finish. A new browser window should open and you should see the Swagger UI for the API endpoints.

### 5. Configure the API Application Settings

In the Azure Portal, find the newly created App Service and navigate to the Application Settings blade. The following settings can be set in the Application Settings:

1. DatabaseSettings__MongoDBConnectionString - This is the connection string for the SmartHotel 360 Cosmsos DB
2. SpatialServices__TenantId - Tenant Id of your Active Directory which the Application Registration was created
3. SpatialServices__AccountId - Account Id of the Azure Spatial Anchors Account
4. SpatialServices__applicationId - Application Id of the Application Registration created in previous steps
5. SpatialServices__applicationKey - Secret of the Application Registration created in previous steps
6. authorizationSettings__ApiKey - Secret api key that is used to protect the Mixed Reality API
7. DigitalTwins__ManagementApiUrl - Management API Url of the Smart Hotel Digital Twins instance created in previous steps
8. DigitalTwins__ClientId - Application Id of the Application Registration created from the SmartHotel 360 readme
9. DigitalTwins__ClientSecret - Application Secreate of the Application Registration created from the SmartHotel 360 readme
10. DigitalTwins__TenantId - Tenant Id of the Azure Active Directory that the SmartHotel 360 solution was deployed in.

## Services

This folder contains the SmartHotel360 Mixed Reality API.

## Unity

This is a cross-platform Unity project that shares as much code and assets as possible between platforms.  The SmartHotelMR subfolder contains assets and scripts specific to this project. For building for a specific platform, please refer to the platform sections below.

### Prerequisites

Prior to building the Unity project for any platform, you'll need to update the values in Unity/SmartHotelMR/Assets/SmartHotelMR/Scripts/Globals.cs.  Fill in the values for your deployed SmartHotelMR Service API URL and API Key as well as your Spatial Anchors Account ID.

### Android

The Android version of this project uses ARCore for its Mixed Reality needs. Download the `unitypackage` file from the [ARCore SDK for Unity releases](https://github.com/google-ar/arcore-unity-sdk/releases/tag/v1.5.0) and import the custom asset package (it needs to be the version 1.5).  To build, switch to the Android platform and then select the Mobile/Android specific scenes in the Build Settings window.  Should look similar to this:

![AndroidBuildSettings](Documents/Images/AndroidBuildSettings.png)
After you build and Export the project, open it in Android Studio.  Once loaded and synced, build the solution.

### HoloLens

The HoloLens version of this project uses the Mixed Reality Toolkit for AR needs. To build for the HoloLens platform, first switch platforms to Universal Windows Platform.  Then mark the HoloLens specific scenes as active and uncheck the Android scenes.  Your Build Settings should look like the screenshot below:

![HoloLensBuildSettings](Documents/Images/HoloLensBuildSettings.png)

After you Build the project, open the solution in Visual Studio.  Select x86 as the target configuration and build as usual.  If building in Release mode, make sure to edit the SmartHotelMR project settings and select "Compile with .NET Native tool chain" in the Build settings.

### Usage

The HoloLens version of this project uses voice commands for certain actions.  The following is a list of commands and actions:

#### In any mode (Admin or User) and both modules (Physical Visualizer and Virtual Explorer):

**"Show Menu"**: Take the user back to the module selection scene

#### In Admin mode and any module

**"Exit Admin"**: Exit Admin mode and place user in User mode (without reloading scene)

#### In Physical Visualizer Admin mode

**"Placement Mode"**: Switch to placing anchor(s)

**"Selection Mode"**: Switch to selecting anchor(s) and allow for deleting of them

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
