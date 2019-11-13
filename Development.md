# Development Environment

## Unity Version

Using [Unity v19.2.11f1](https://unity3d.com/unity/whats-new/2019.2.11).
* Including build support for Linux and Windows.

Install this version of Unity from Unity Hub
* Here are instructions how to install and setup Unity Hub [instructions link](https://docs.unity3d.com/Manual/GettingStartedInstallingHub.html)
* This should work for linux, if not here is a supplemental article on setting up unity hub in linux. [article link](https://www.linuxdeveloper.space/install-unity-linux/)

## Coding Environment

### Languages

Languages: [C#](https://docs.unity3d.com/Manual/CSharpCompiler.html) | [HLSL](https://docs.unity3d.com/Manual/SL-ShadingLanguage.html) | [Compute Shaders](https://docs.unity3d.com/Manual/class-ComputeShader.html)

The compilers for these are included in Unity but having the `.Net Core SDK` is required for the IDE. Instructions to install `.Net Core SDK`: [Installation Instructions](https://dotnet.microsoft.com/download/dotnet-core/sdk-for-vs-code?utm_source=vs-code&utm_medium=referral&utm_campaign=sdk-install). There should be a `.exe` for windows, a set of commands for linux or mac. 
* **Note** If you are installing for linux, ensure that you have the mono libraries installed. Mono Libraries: [https://www.mono-project.com/download/stable](https://www.mono-project.com/download/stable)
* **Additional Note** You may need to reboot the computer after installing the `.Net Core SDK`.


### Version Control
Version Control: This project uses a combination of git, git-lfs, and github.

Ensure that you have git installed. If not here is a [guide to install git](https://git-scm.com/book/en/v2/Getting-Started-Installing-Git)
```
git --version
```

Ensure that you also have git lfs installed. It should be setup to auto-track certain types of files as determined in the `.gitattributes` file. If the command to install git-lfs `git lfs install` is giving you trouble, try looking into the [installation guide](https://git-lfs.github.com/)
```
# Run this inside the repository after cloning it
git lfs install
git-lfs --version
```

### IDE

The IDE we are using is [VSCode](https://code.visualstudio.com/).
* Make sure that you have the `.Net Core SDK` installed. [Installation Instructions](https://dotnet.microsoft.com/download/dotnet-core/sdk-for-vs-code?utm_source=vs-code&utm_medium=referral&utm_campaign=sdk-install). (See earlier section [Languages](#Languages) for more information)

To add VSCode as Unity's default editor, select it under: `Edit > Preferences > External Tools > External Script Editor`. Here is a guide to switching unity to use VSCode as default [guide link](SwitchIDE.md).

Using the following extensions for VSCode. For information about installing extensions, use [this article](https://code.visualstudio.com/docs/editor/extension-gallery)
* [C#](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp) by Microsoft. 
* [Debugger for Unity](https://marketplace.visualstudio.com/items?itemName=Unity.unity-debug) by Unity Technologies. 
* [Unity Tools](https://marketplace.visualstudio.com/items?itemName=Tobiah.unity-tools) by Tobiah.
* [Unity Code Snippets](https://marketplace.visualstudio.com/items?itemName=kleber-swf.unity-code-snippets) by kleber-swf.
* [Unity Snippets](https://marketplace.visualstudio.com/items?itemName=YclepticStudios.unity-snippets) by YclepticStudios.
* [C# XML Documentation Comments](https://marketplace.visualstudio.com/items?itemName=k--kato.docomment) by Keisuke Kato. 
* [Code Spell Checker](https://marketplace.visualstudio.com/items?itemName=streetsidesoftware.code-spell-checker) by Street Side
    Software (because Nick can't spell and properly spelled comments are great).

### Style

As far as coding style, please try to stay consistent with [Csharp Coding Guidelines](https://wiki.unity3d.com/index.php/Csharp_Coding_Guidelines) from Unity's reference guide. 
