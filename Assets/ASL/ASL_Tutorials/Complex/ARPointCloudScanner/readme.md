## ASL ARPointCloudScanner Tutorial Solution Readme

This document covers usage and extension of the ASL ARPointCloudScanner tutorial solution.  This tutorial was created on 3/10/2022 for usage in the UWB ASL project.

### About

The ASL ARPointCloud Scanner enables real-world object and environment topology to be converted into point cloud representations and shared over Unity game instances using the Augmented Space Library framework in real-time.  
This ASL template solves existing persistence limitations on Unity SfM point cloud capturing using the out-of-box ARPointCoudManager, 
while enabling distributed ParticleSystem objects to be synchronized over network instances with shared initial particle coordinates and colors.  

The ASL ARPointCloud solution utilizes simple camera SfM capturing found in common ARCore enabled Android mobile devices, providing a low barrier to entry.  
Additional device motion speed, confidence, and position filtering is utilized to increase scan accuracy and enable the ability to isolate single objects from a complex environment.  
The distributed point cloud data can be further extended to build object and environment meshes and game objects for real-time usage within shared Unity ASL game instances or enable photogrammetry functionality

### Unity Setup

The unity system version must be updated to at least 2020.3.19f1 to utilize particle additions to the ParticleSystem storage.  
The current 2020.3.0f1 will throw "index out of range" exceptions and will not persist point cloud data due to an internal Unity ParticleSystem bug.

It is highly recommended to update the ARCore XR Plugin to at least 4.2.0 in order to increase point cloud scanning accuracy.  
If you are getting inaccurate z depth with point captures, check the AR version.  
Unity -> Window -> Package Manager -> ARCore XR Plugin -> Update to latest 

Do not check in the updated system files to source control as this could break other consumers without full testing, notification, and due dilligence. 

Enable Android build capability within the Unity hub for device builds.

You will get Android JDK errors when building for Android on the first time within a Unity session.  To mitigate, uncheck and check the Android JDK installed with Unity checkbox:
Unity -> Edit -> Preferences -> External Tools -> JDK Installed with Unity

### Scene Setup

Make sure the following scenes are added to the Build Settings -> Scenes In Build in order:

ASL_LobbyScene
ASL_SceneLoader
ASL_ARCorePointCloud

Add "ASL_ARCorePointCloud" to the AvailableScenes Dropdown object for the ASL_LobbyScene scene:

(Hierarchy Panel) ASL_LobbyScene -> LobbyManager -> AvailableScenes -> (Inspector panel) Dropdown -> Options

After performing an Android build, the ASL_LobbyScene, ASL_SceneLoader, and ASLARCorePointCloud scenes will be automatically added to the current scene due to Unity behavior.  
Delete the unnecessary scenes to avoid functional issues before the next debugging/play session or build.

Scene to load: ASL_Tutorials -> Complex -> ARPointCloudScanner -> Scenes -> ARCorePointCloud

### Usage Notes

For PC controls: WASD and mouse controls are enabled for camera fly-around movement. 

For Android devices, the filter sphere is controlled by touch input:
*X | Y translation: one finger movement
*Scale: two finger pinch zoom
*Z translation: three finger up/down movement

For best scanning, use a repeated rocking motion along with a left/right positional shift to capture position and rotational data movement necessary for accurate SfM.  
Do not move too fast or too slow.  If you see the recording button turn yellow, you are either moving too fast or too slow to capture points accurately. 

For multiple mobile device world origin and position synchronization:
1. Choose a device as an anchor manager.  Only place cloud anchors with the chosen device.
2. Briefly scan the room to establish the key ARPlane objects.
3. Place the world anchor point.  This will initialize a blue cube to the first position and rotation that the device was in upon scene entry.
4. Wait a bit for the anchor point to resolve across devices.  This may take 2 to 120 seconds.  If the anchor does not resolve and the origin cubes do not sync, you may have an unresolvable anchor and need to restart and scan more thoroughly.
5. Add additonal normal cloud anchor points to increase accuracy. (Red cylinders)

The points will be sampled using the camera background texture position representation of the 3d point.  
If toggling the custom color button, the alternate choice will use a default green color for every point.  
The button will change to green to denote the green point sampling.

### Known Bugs

Occasionally, the ARCore native functionality will lose the world map, when this happens, the ARCore world origin will shift.  The origin normally returns to the original point once more device scanning and movement is performed. 
Recommend further hardening to listen for Map Loss exceptions in Unity core.

Accuracy of the World Origin cloud anchor is often up to 2in off.  If more percision is needed, recommend restarting application and adding a new world origin cloud anchor to chance a better anchor point resolution result.

Points close to but not inside the filter sphere may be captured.  This is by design due to the bounds check in the Collider.  Can switch to radius check if greater accuracy is required.

Cloud anchor points will not resolve and sync across devices when a PC instance is added to the session.  