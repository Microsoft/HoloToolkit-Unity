# MRTK Examples Hub #

![MRTK Examples Hub](../Documentation/Images/ExamplesHub/MRTK_ExamplesHub.png)

MRTK Examples Hub is a Unity scene that makes it easy to experience multiple scenes. It uses MRTK's Scene System to load & unload the scenes. 

**MRTKExamplesHub.unity** is the container scene that has shared components including ``MixedRealityToolkit`` and ``MixedRealityPlayspace``. **MRTKExamplesHubMainMenu.unity** scene has the cube buttons.

## MRTKExamplesHub Scene and the Scene System ##
Open **MRTKExamplesHub.unity** which is located at ``MixedRealityToolkit.Examples/Demos/ExamplesHub/Scenes/`` It is an empty scene with MixedRealityToolkit, MixedRealityPlayspace and LoadHubOnStartup. This scene is configured to use MRTK's Scene System. Click ``MixedRealitySceneSystem`` under MixedRealityToolkit. It will display the Scene System's information in the Inspector panel.
<br/><br/><img src="../Documentation/Images/ExamplesHub/MRTK_ExamplesHub_Hierarchy.png" width="300">
<br/><br/><img src="../Documentation/Images/ExamplesHub/MRTK_ExamplesHub_Inspector1.png" width="450">

On the bottom of the Inspector, it displays the list of the scenes defined in the Scene System Profile. You can click the scene names to load/unload them. 
<br/><br/><img src="../Documentation/Images/ExamplesHub/MRTK_ExamplesHub_Inspector2.png" width="550">
<br/><br/><img src="../Documentation/Images/ExamplesHub/MRTK_ExamplesHub_SceneSystem3.png">Example of loading _MRTKExamplesHub_ scene by clicking the scene name in the list.
<br/><br/><img src="../Documentation/Images/ExamplesHub/MRTK_ExamplesHub_SceneSystem4.png">Example of loading _HandInteractionExamples_ scene.
<br/><br/><img src="../Documentation/Images/ExamplesHub/MRTK_ExamplesHub_SceneSystem5.png">
Example of loading multiple scenes.

## Running the scene ##
The scene works on both Unity's game mode and the device. To build and deploy, simply build **DefaultManagerScene** with other scenes that are included in the Scene System's list. The inspector also makes it easy to add scenes to the Build Settings.

## How MRTKExamplesHub loads a scene ##
In the **MRTKExamplesHub** scene, you can find the ``ExamplesHubButton`` prefab. 
There is a **FrontPlate** object in the prefab which contains ``Interactable``. 
Using the Interactable's ``OnClick()`` and ``OnTouch()`` event, it triggers the **LoadContentScene** script's **LoadContent()** function. 
In the **LoadContentScene** script's Inspector, you can define the scene name to load.
<br/><br/><img src="../Documentation/Images/ExamplesHub/MRTK_ExamplesHub_SceneSystem6.png">
<br/><br/><img src="../Documentation/Images/ExamplesHub/MRTK_ExamplesHub_SceneSystem8.png" width="450">
<br/><br/><img src="../Documentation/Images/ExamplesHub/MRTK_ExamplesHub_SceneSystem7.png" width="450">

The script uses the Scene System's LoadContent() function to load the scene. 
Please refer to the [Scene System](SceneSystem/SceneSystemGettingStarted.md) page for more details.
```csharp
MixedRealityToolkit.SceneSystem.LoadContent(contentName, loadSceneMode);
```
 
## Returning to the main menu scene ##
To return to the main menu scene (MRTKExamplesHubMainMenu scene), you can use the exact same method. **ToggleFeaturesPanelExamplesHub.prefab** provides the 'Home' button which contains the **LoadContentScene** script. Use this prefab or provide your custom home button in each scene to allow the user to return to the main scene.

<img src="../Documentation/Images/ExamplesHub/MRTK_ExamplesHubToggleFeaturesPanel.png" width="450">

<img src="../Documentation/Images/ExamplesHub/MRTK_ExamplesHubHomeButton.png" width="450">



## Adding additional buttons ##
In the **CubeCollection** object, duplicate (or add) _ExampleHubButton_ prefabs and click **Update Collection** in the ``GridObjectCollection``. 
This will update the cylinder layout based on the new total number of buttons. 
Please refer to the [Object Collection](README_ObjectCollection.md) page for more details.
<br/><br/><img src="../Documentation/Images/ExamplesHub/MRTK_ExamplesHub_SceneSystem9.png">
<br/><br/><img src="../Documentation/Images/ExamplesHub/MRTK_ExamplesHub_SceneSystem10.png">

After adding the buttons, update the scene name in the **LoadContentScene** script(explained above). 
Add additional scenes to the Scene System's profile.
