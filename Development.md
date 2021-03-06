# Development Environment

# Unity Setup

Using [Unity v19.2.11f1](https://unity3d.com/unity/whats-new/2019.2.11).
* Including build support for Linux and Windows.

Install this version of Unity from Unity Hub
* Here are instructions how to install and setup Unity Hub [instructions link](https://docs.unity3d.com/Manual/GettingStartedInstallingHub.html)
* If this link does not work try using the downloads provided on [this forum page](https://forum.unity.com/threads/unity-hub-v-1-3-2-is-now-available.594139/) This will also have a download for Linux.
* This should work for linux, if not here is a supplemental article on setting up unity hub in linux. [article link](https://www.linuxdeveloper.space/install-unity-linux/)

Before you can use Unity hub, you need to setup a license
* If you are a student or using this for a personal project, you can get a personal license.
* If you do not qualify for a personal license use your company license.
* For more information about licenses, please check [Unity's License Page](https://store.unity.com/compare-plans)

Install the proper version of Unity from UnityHub using this unity hub link [unityhub://2019.2.11f1/5f859a4cfee5](unityhub://2019.2.11f1/5f859a4cfee5)
* Note, UnityHub must be registered on your system for it to open the Unity Hub App.
* To do this in linux, just launch the downloaded `UnityHub.AppImage` file.

# Setting Up the Project

Now that the proper version of unity has been installed, open the project with UnityHub.
In order to do this, you must first clone the repo:

```bash
# This can be anywhere you want to store the project
$ cd ~/projects
# Download the git repo
$ git clone git@github.com:nicholas-maltbie/ParallelEnvironment.git
```

# Version Control
Version Control: This project uses a combination of git, git-lfs, and github.

Ensure that you have git installed. If not here is a [guide to install git](https://git-scm.com/book/en/v2/Getting-Started-Installing-Git)
```
git --version
```

Ensure that you also have git lfs installed. It should be setup to auto-track certain types of files as determined in the `.gitattributes` file. If the command to install git-lfs `git lfs install` is giving you trouble, try looking into the [installation guide](https://git-lfs.github.com/)
```bash
# Run this inside the repository after cloning it
# May need to run this on linux
curl -s https://packagecloud.io/install/repositories/github/git-lfs/script.deb.sh | sudo bash
sudo apt-get install git-lfs
# Installing git lfs
git lfs install
git-lfs --version
```

# Opening the Project

After you have downloaded the git repo, launch unity hub and navigate to the `Projects` section on the
menu on the left half of the screen. 

![Unity Hub Projects Screen with highlights around the projects and add buttons](Images/UnityProjects.png)

From this file menu, navigate to the folder where the
project has been downloaded. For example 
`~/projects/ParallelEnvironment`. Then hit the `Open` 
button in the file selector.

![Selecting Project from file selector](Images/UnityLoad.png)

After the environment has a chance to load, the project 
should be listed in the projects area as shown in the image
below.

![Everything setup and good to go](Images/UnityComplete.png)

# Coding Environment

## Languages

Languages: [C#](https://docs.unity3d.com/Manual/CSharpCompiler.html) | [HLSL](https://docs.unity3d.com/Manual/SL-ShadingLanguage.html) | [Compute Shaders](https://docs.unity3d.com/Manual/class-ComputeShader.html)

The compilers for these are included in Unity but having the `.NET Core SDK` is required for the IDE. Instructions to install `.NET Core SDK`: [Installation Instructions](https://dotnet.microsoft.com/download/dotnet-core/sdk-for-vs-code?utm_source=vs-code&utm_medium=referral&utm_campaign=sdk-install). There should be a `.exe` for windows, a set of commands for linux or mac. 
* **Note** If you are installing for linux, ensure that you have the mono libraries installed. Mono Libraries: [https://www.mono-project.com/download/stable](https://www.mono-project.com/download/stable)
* **Additional Note** You may need to reboot the computer after installing the `.NET Core SDK`.

Also make sure to be using .NET 4.x compatibility in Unity. This will allow for full use
of libraries and other features. For more information on how to do this, follow 
Microsoft's guide to switch [Unity to .NET 4.x](https://docs.microsoft.com/en-us/visualstudio/cross-platform/unity-scripting-upgrade?view=vs-2019).

## Enabling the .NET 4.x scripting runtime in Unity

To enable the .NET 4.x scripting runtime, take the following steps:

1. Open PlayerSettings in the Unity Inspector by selecting `Edit > Project Settings > Player > Other Settings`.
2. Under the Configuration heading, click the Scripting Runtime Version dropdown and select .NET 4.x Equivalent. You will be prompted to restart Unity.

![Navigating to menu](Images/SetupDotNet4.x.png)

![Menu to setup changes](Reference/vstu_scripting-runtime-version.png)

From Microsoft's article ([Unity to .NET 4.x](https://docs.microsoft.com/en-us/visualstudio/cross-platform/unity-scripting-upgrade?view=vs-2019))

## Adding new libraries

If you wish to add new .NET libraries to the project, See Microsoft's page on [Using .NET 4.x in Unity](https://docs.microsoft.com/en-us/visualstudio/cross-platform/unity-scripting-upgrade?view=vs-2019#taking-advantage-of-net-compatibility)
and the sub section of "taking advance of .NET compatibility".
You simply have to add the `.dll` file from the 
project package archive. Instructions on how to get this and
implement these changes are in the article linked above. 

# IDE

The IDE we are using is [VSCode](https://code.visualstudio.com/).
* Make sure that you have the `.NET Core SDK` installed. [Installation Instructions](https://dotnet.microsoft.com/download/dotnet-core/sdk-for-vs-code?utm_source=vs-code&utm_medium=referral&utm_campaign=sdk-install). (See earlier section [Languages](#Languages) for more information)

To add VSCode as Unity's default editor, select it under: `Edit > Preferences > External Tools > External Script Editor`. 

# Switching Unity IDE to VSCode

Unity Article on VSCode and Unity for reference [Article](https://code.visualstudio.com/docs/other/unity).

This is written for [Unity v19.2.11f1](https://unity3d.com/unity/whats-new/2019.2.11).

### Steps to switch to VSCode

In the Unity Environment use the following sub-menus

Edit > Preferences > External Tools > External Script Editor

1. Edit (Top left of screen)
2. Preferences (Lower section of the edit menu)

![Edit > Preferences view in Unity](Images/EditPreferences.png)

3. External Tools (Lower section of sub menus for Preferences)
4. Select Visual Studio Code from the External Script Editor

![Preferences > External Tools > Select External](Images/SelectVSCode.png)

# VS Code Extensions

Using the following extensions for VSCode. For information about installing extensions, use [this article](https://code.visualstudio.com/docs/editor/extension-gallery)
* [C#](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp) by Microsoft. 
* [Debugger for Unity](https://marketplace.visualstudio.com/items?itemName=Unity.unity-debug) by Unity Technologies. 
* [Unity Tools](https://marketplace.visualstudio.com/items?itemName=Tobiah.unity-tools) by Tobiah.
* [Unity Code Snippets](https://marketplace.visualstudio.com/items?itemName=kleber-swf.unity-code-snippets) by kleber-swf.
* [Unity Snippets](https://marketplace.visualstudio.com/items?itemName=YclepticStudios.unity-snippets) by YclepticStudios.
* [C# XML Documentation Comments](https://marketplace.visualstudio.com/items?itemName=k--kato.docomment) by Keisuke Kato. 
* [Code Spell Checker](https://marketplace.visualstudio.com/items?itemName=streetsidesoftware.code-spell-checker) by Street Side
    Software (because Nick can't spell and properly spelled comments are great).
* [Shader languages support for VS Code](https://marketplace.visualstudio.com/items?itemName=slevesque.shader) by slevesque 

# Coding Style

As far as coding style, please try to stay consistent with [Csharp Coding Guidelines](https://wiki.unity3d.com/index.php/Csharp_Coding_Guidelines) from Unity's reference guide. 
