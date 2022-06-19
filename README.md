# RSS Reader

## Overview

This project allows you to read and track RSS feeds easily in command line.

In the basic mode it will check all your feeds at the program launch and every 5
minutes after that.

It also provides the ability to display all the items from all the feeds added
in the last hour, day or week.

The project is architectured with a library that does all the heavy lifting of
reading and parsing feeds, configuration and refreshing the data and a main
console program that provides a very basic interface to display received feed items.

## Pre-requirements

You need the [.NET SDK 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed
on your computer to build the project.

## Compatibility

Tested on Windows 11 x64, but should work on [all the OS supported by .NET 6.0](https://github.com/dotnet/core/blob/main/release-notes/6.0/supported-os.md).

## Setup

### Add your RSS feeds to the list

Go to the build output directory and fill the `rss.csv` file with the list of
feeds you want to follow.

### Build the project

You can either:
* Open the `RssReader.sln` file in Visual Studio (tested in VS 2022 but
should work in earlier versions) and press F5 to compile and launch the
program.
* Compile it directly in command line.
```bash
dotnet build --output out
cd out
```

## Program usage

```
Usage: RssReader.exe [day|hour|week]
  to use in watch mode do not input any argument
  to display recent items input the wanted duration
```

For example to use the program as a feed watcher (updated every 5 minutes) just launch the program
```bash
RssReader.exe
```

To display all the new feed items published in the last hour
```bash
RssReader.exe hour
```
