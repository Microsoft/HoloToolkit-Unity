# Getting started with Holographic Remoting

Holographic remoting streams holographic content from a PC to your Microsoft HoloLens in real-time, using a Wi-Fi connection. This feature can significantly increase developer productivity when developing mixed reality applications.

## Initial setup

To enable remoting to a HoloLens, it is important to ensure that the project is using the latest remoting components.

1. Open **Window > Package Manager**
    - Verify that latest version of the **Windows Mixed Reality** package is installed.
1. Ensure the latest Holographic Remoting application is installed, on the HoloLens, via the Microsoft Store.

### HoloLens 2

When using a HoloLens 2, support for remoting articulated hand and eye tracking data has been added to MRTK. To enable these features,
configure the project using the following steps.

1. Run the MRTK Configurator Utility (**Mixed Reality Toolkit > Utilities > Configure Unity Project**)
1. Expand **Modify Configurations**

    ![MRTK Configurator](../Images/Tools/Remoting/EnableMSBuildForUnity.png)

1. Ensure that **Enable MSBuild for Unity** is selected
1. Click **Apply**

### Debugging HoloLens 2 remoting via Unity package import

If HoloLens 2 hand joints and eye tracking aren't working over remoting, there are three common points of potential issues. They're listed below in the order they should be checked.

#### MSBuildForUnity package import via writing into the package.manifest

For the first point, the best way to check is to open Window -> Package Manager and make sure MSBuild for Unity shows up in the packages list. If it's there, assume this step succeeded. If it's not there, try running Mixed Reality Toolkit -> Utilities -> Configure Unity and repeat the steps above for running the MRTK Configurator.

![MSB4U Package Manager](../Images/Tools/Remoting/MSB4UPackageManager.png)

#### DotNetWinRT NuGet package resolution

For the second point, the best way to check is to search the Assets folder for DotNetWinRT.dll. If this doesn't exist, navigate to MixedRealityToolkit.Providers / WindowsMixedReality / Shared / DotNetAdapter in the Project view and select DotNetAdapter.csproj. Assuming part 1 did succeed, there should be a custom inspector with Build, Rebuild, and Clean buttons. Try clicking Build or Rebuild, and then re-search for DotNetWinRT.dll. If that DLL now exists, this step succeeded.

![DotNetAdapter Inspector](../Images/Tools/Remoting/DotNetAdapterInspector.png)

#### `DOTNETWINRT_PRESENT` #define written into player settings

For the third point, navigate to the Unity Player Settings. From there, under the UWP tab, check under Other Settings for the Scripting Define Symbols. Make sure DOTNETWINRT_PRESENT is properly written in that list. If that's there, this step succeeded.

![DotNetWinRT Present](../Images/Tools/Remoting/DotNetWinRTPresent.png)

After all three of those, hand joints over remoting should be working! If not, there might be something misconfigured in the profiles for general hand joints on-device. In that case, please [reach out on one of our help resources](../GettingStartedWithTheMRTK.md#getting-help).

### Removing HoloLens 2-specific remoting support

If you're running into conflicts or other issues due to the presence of the DotNetWinRT adapter, please [reach out on one of our help resources](../GettingStartedWithTheMRTK.md#getting-help).

You can also temporarily remove the adapter to workaround your issue via the following steps:

1. In Unity, go to Window -> Package Manager and uninstall MSBuild for Unity
1. Search for DotNetWinRT.dll in your assets list in Unity and either delete the DLL or delete the Plugins (MRTK 2.2 or earlier) or Dependencies (MRTK 2.3 or later) folder that contains it a few levels up. That should remove these conflicting namespaces, while keeping MRTK around
1. If you run the MRTK Configurator again, make sure you don't re-enable MSBuild for Unity

## Connecting to the HoloLens

Once the project has been configured, a connection can be established to the HoloLens.

1. In **File > Build Settings**, ensure that the project build type is set to **Universal Windows Platform**
1. On the HoloLens, launch the **Holographic Remoting** application.
1. In Unity, select **Window > XR > Holographic Emulation**.

    ![Start Holographic Emulation](../Images/Tools/Remoting/StartHolographicEmulation.png)

1. Set **Emulation Mode** to **Remote to Device**.

    ![Set Emulation Mode](../Images/Tools/Remoting/SelectEmulationMode.png)

1. Select the **Device Version**.

    ![Select Device Version](../Images/Tools/Remoting/SelectDeviceVersion.png)

1. Using the IP Address displayed by the Holographic Remoting application, set the **Remote Machine** field.

    ![Enter IP Address](../Images/Tools/Remoting/EnterIPAddress.png)

1. Click **Connect**.

> [!NOTE]
> If you cannot connect, make sure your HoloLens 2 is not plugged in to your PC and restart Unity.

## Starting a remoting session

With Unity connected to the HoloLens, enter play mode in the editor.

When the session is complete, exit play mode.

> [!NOTE]
> There is a known issue with some versions of Unity where the editor may hang upon entering play mode during a remoting session. This issue may manifest if the Holographic window is open when the project is loaded. To ensure this issue does not occur, always close the Holographic dialog prior to exiting Unity.

## See also

- [Holographic Remoting troubleshooting and limitations](https://docs.microsoft.com/windows/mixed-reality/holographic-remoting-troubleshooting)
- [Microsoft Holographic Remoting software license terms](https://docs.microsoft.com/legal/mixed-reality/microsoft-holographic-remoting-software-license-terms)
