# Dukebox
An audio library manager and media-player

[Project Webpage](https://djfdyuruiry.github.io/Dukebox/)

### 

### Technical Overview

Dukebox is .NET desktop music jukebox designed to play a wide variety of audio formats and provide simple library management. It uses the [BASS audio library](http://www.un4seen.com/) for playback, SQLite3 & Entity Framework for music library data and WPF as the main GUI framework.

SimpleInjector is used for Dependency Injection and XUnit & FakeItEasy for Unit/Integration Testing.

The app has been designed to run on a machine running Windows Vista SP2 and above, it has not been tested on Mono or any other .NET runtime.

### Build Tools

All that is needed to build and run the software is Visual Studio 2015 Community Edition.

### License

This project uses the [MIT](https://goo.gl/7IaYjt) license