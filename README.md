DualityScripting
================

A scripting plugin for Duality that lets you write and edit C# and F# code on the fly, while the editor or game is running, without the need for a full editor restart.

Compiling
================
This plugin uses NuGet to pull in references to our fork of Duality. The references are publicly available on MyGet at the following URL:

https://www.myget.org/F/6416d9912a7c4d46bc983870fb440d25/

In Visual Studio, go to Tools -> Package Manager -> Package Sources, and add that URL as a package source and the references will be pulled down when you build.
