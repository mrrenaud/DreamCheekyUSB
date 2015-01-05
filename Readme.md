# DreamCheekyUSB for Big Red Button

*This repo is forked from [DreamCheekyUSB on CodePlex](https://dreamcheekyusb.codeplex.com/) which was originaly working with the Dream Cheeky Iron Man USB Stress Button.*

This library uses the [HidLibrary](https://github.com/mikeobrien/HidLibrary/) delivered by Nuget. However, if you notice the device isn't responding anymore after pushing the button a few times, you should consider to use the updated version of the [HidLibrary for DreamCheekyUSB](https://github.com/MrRenaud/HidLibrary) that implements a timeout and closes the connection with the device to reopen it at the next read/write attempt. This solves the hanging problem.

-------------

**DreamCheekyUSB** provides .NET drivers for the **Dream Cheeky Webmail Notifier** (http://www.dreamcheeky.com/webmail-notifier) and the **Dream Cheeky Big Red Button** (http://dreamcheeky.com/big-red-button).

It was created using the https://github.com/mikeobrien/HidLibrary/ and is released under the Apache License V2.0.

You can control the LED using either DreamCheekyLED.exe with command line arguments or via C#/VB/Powershell using 
the DreamCheekyUSB.DreamCheekyLED object. The code supports multiple devices and has options for blinking and fading.
See DreamCheekyLED.exe for command line examples or the DreamCheekyLED*.ps1 files for powershell examples.

The DreamCheekyBTN.exe has command line arguments that will let you run a program whenever the button is pressed or
convert the button press events into a keyboard macro that can perform an action or be picked up by other programs
like AutoHotKey. The code should also support multiple devices, and can be run multiple times for additional triggers.
See DreamCheekyBTN.exe for command line examples or the AutoHotKey.ahk file for a sample AutoHotKey script.
