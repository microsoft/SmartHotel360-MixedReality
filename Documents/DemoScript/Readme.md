# Facilities Management Powered by Mixed Reality and Internet of Things (IoT)

## Introduction

SmartHotel360 has implemented a mixed reality solution built on top of Azure Spatial Anchors, a mixed reality service that enables you to create collaborative and spatially aware applications. The goal of the solution is to provide users spatial intelligence that can be used to increase efficiency and ehance occupant experiences in scenarios like facilities management. The example here applies this to a hotel. In this scenario, users can visualize real-time data from IoT senesors in 3D. They can analyze and act upon this data in two ways: **virtually exploring** the hotel facility and data associated with facilities upkeep using a Digital Twin, and **physically visualizing** this data "onsite" and in context using a mixed reality solution with a mobile device like an Android phone or tablet, or Microsoft HoloLens devices. 

## Key Takeaways

The key takeaways of this demo are:

* **Azure Spatial Anchors:** [Azure Spatial Anchors](https://azure.microsoft.com/services/spatial-anchors) is a cloud service that enables you to build a new generation of mixed reality applications that are collaborative, cross-platform, and spatially aware. This solution uses Azure Spatial Anchors to pin data from IoT sensors to precise physical locations, visualize this data as holograms, and do so over large spaces. It also enables you to deliver a shared mixed reality experience across iOS, Android, and HoloLens devices.

* **Azure Digital Twins:** [Azure Digital Twins](https://azure.microsoft.com/services/digital-twins) is a new platform that enables comprehensive digital models and spatially aware solutions that can be applied to any physical environment. Create uniquely relevant experiences by correlating data across the digital and physical worlds.

* **Contextual applications:** IoT and mixed reality can make your connected solutions even stronger by adding spatial context, an understanding of information and data that is specific to a precise location or point of interest. Most IoT projects today start from a things-centric approach, but with Azure Digital Twins, we’ve flipped that around. We’ve found that customers realize huge benefits by first modeling the physical environment and then connecting existing or new devices (“things”) to that model. With [Azure Spatial Anchors](https://azure.microsoft.com/services/spatial-anchors) and [Azure Digital Twins](https://azure.microsoft.com/services/digital-twins), customers gain new spatial intelligence capabilities and new insights into how spaces and infrastructure are really used. By visualizing IoT data onsite and in-context on HoloLens or mobile devices, people can uncover and respond to operational issues before they impact workstreams.

* **End-To-End Solution:** With this demo you can learn how the IoT demo built on top of Azure Digital Twins can be integrated with the new Azure Spatial Anchors mixed reality service.

## Before you begin

You will need to have provisioned a demo environment following the **[setup guide](https://github.com/Microsoft/SmartHotel360-MixedReality#setup)**.

## Walkthrough: Using the Virtual Explorer with Android or HoloLens devices

1. Using the Unity application on Android or HoloLens select the Anchor Set you would like to connect.
2. In the main menu select the Virtual Explorer Admin User.
3. Using the device camera select a place where you would like to place the anchors.
4. Navigate throug the different options of buildings, rooms, floors, etc. Since this solution use both Azure Digital Twins and Spacial Anchor we have deployed two hotels brands with multiple hotels buildings with multiple floors and rooms.
5. Once in the hotel room you selected you can see the two sensors (temperature and light) showing the information of the settings and also the status of that hotel room (occupided and unoccupided).
6. Using the Facility Management website from the IoT solution or the SmartHotel360 Mobile app you can change the values of that specific room and the values will syncronize with the mixed reality application. 

## Walkthrough: Using the Physical Visualizer with MXChip device

> Note: you will need to setup the [MXChip device](https://github.com/Microsoft/SmartHotel360-IoT#mxchip) as part of the IoT Solution for this demo.

1. Using the Unity application on Android or HoloLens let's create a new Anchor Set.
2. Enter a name you would like to use.
3. In the main menu select the Physical Visualizer Admin.
4. Once the device is recognized the application will ask you to select the hotel brand, building, floor and room where this device should be saved.
5. Once saved with the Unity application you will be able to visualize the information from that sensor on top of it.
6. Using the Facility Management website or the SmartHotel360 mobile app you also can change the values for the tempeture or lights of that hotel room and you will be able to see how the values are syncronized using the unity application.
 
## Summary

What we’ve shown here is how Microsoft and SmartHotel360 are working together, using Azure Digital Twins and Azure Spatial Anchors, for a complete IoT and mixed reality Solution to improve operational and energy efficiency, optimize space utilization and enhance occupant experience.
