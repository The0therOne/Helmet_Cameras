using System;
using System.Collections;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using UnityEngine.SceneManagement;
using GameNetcodeStuff;

namespace HelmetCamera
{
    // Thanks Solo/CapyCat (BodyCameras) for your help and your permission use your way!
    // Thanks .json and Glitch for testing my updates!
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class PluginInit : BaseUnityPlugin
    {
        public static Harmony _harmony;
        public static ConfigEntry<int> config_isHighQuality;
        public static ConfigEntry<int> config_renderDistance;
        public static ConfigEntry<int> config_cameraFps;

        private void Awake()
        {
            config_isHighQuality = base.Config.Bind<int>("MONITOR QUALITY", "monitorResolution", 0,
                "Low FPS affection. High Quality mode. 0 - vanilla (48x48), 1 - vanilla+ (128x128), 2 - mid quality (256x256), 3 - high quality (512x512), 4 - Very High Quality (1024x1024)");

            config_renderDistance = base.Config.Bind<int>("MONITOR QUALITY", "renderDistance", 20,
                "Low FPS affection. Render distance for helmet camera.");

            config_cameraFps = base.Config.Bind<int>("MONITOR QUALITY", "cameraFps", 30,
                "Very high FPS affection. FPS for helmet camera. To increase YOUR fps, you should low cameraFps value.");

            _harmony = new Harmony("HelmetCamera");
            _harmony.PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded with version {PluginInfo.PLUGIN_VERSION}!");
            Logger.LogInfo("--------Helmet camera patch done.---------");
        }
    }
    public static class PluginInfo
    {
        public const string PLUGIN_GUID = "RickArg.lethalcompany.helmetcameras";
        public const string PLUGIN_NAME = "Helmet_Cameras";
        public const string PLUGIN_VERSION = "2.1.6";
    }

    public class Plugin : MonoBehaviour
    {
        private RenderTexture renderTexture;
        private bool isMonitorChanged = false;
        public static GameObject helmetCameraNew;
        private bool isSceneLoaded = false;
        private bool isCoroutineStarted = false;
        private int currentTransformIndex;
        private int resolution;
        private int renderDistance;
        private float cameraFps;
        private float elapsed;


        private void Awake()
        {
            resolution = PluginInit.config_isHighQuality.Value;
            renderDistance = PluginInit.config_renderDistance.Value;
            cameraFps = PluginInit.config_cameraFps.Value;

            switch (resolution)
            {
                case 0:
                    renderTexture = new RenderTexture(48, 48, 24);
                    break;
                case 1:
                    renderTexture = new RenderTexture(128, 128, 24);
                    break;
                case 2:
                    renderTexture = new RenderTexture(256, 256, 24);
                    break;
                case 3:
                    renderTexture = new RenderTexture(512, 512, 24);
                    break;
                case 4:
                    renderTexture = new RenderTexture(1024, 1024, 24);
                    break;
            }
        }

        public void Start()
        {
            isCoroutineStarted = false;

            while (helmetCameraNew == null)
            {
                helmetCameraNew = new GameObject("HelmetCamera");
            }

            // Checking what scene is loaded. We dont need CameraMod in MainMenu, InitScene, InitSceneLaunchOptions
            bool sceneflag = true;

            switch (SceneManager.GetActiveScene().name)
            {
                case "MainMenu":
                case "InitScene":
                case "InitSceneLaunchOptions":
                    sceneflag = false;
                break;
            }
            
            if (sceneflag)
            {
                isSceneLoaded = true;
                Debug.Log("[HELMET_CAMERAS] Starting coroutine...");
                StartCoroutine(LoadSceneEnter());
            }
            else
            {
                isSceneLoaded = false;
                isMonitorChanged = false;
            }
        }

        private IEnumerator LoadSceneEnter()
        {
            Debug.Log("[HELMET_CAMERAS] 5 seconds for init mode... Please wait...");
            // Waiting ~5 seconds for scene is fully loaded
            yield return new WaitForSeconds(5f);
            isCoroutineStarted = true;

            // Reduce amount of searches by caching
            GameObject foundHangarShip = GameObject.Find("Environment/HangarShip");
            Transform foundMonitorWall = foundHangarShip.transform.Find("ShipModels2b/MonitorWall");
            Transform foundShipCamera = foundHangarShip.transform.Find("Cameras/ShipCamera");

            bool isCameraFound = foundShipCamera != null;
            if (isCameraFound)
            {
                Debug.Log("[HELMET_CAMERAS] Ship camera found...");
                if (!isMonitorChanged)
                {
                    Transform foundCube001 = foundMonitorWall.Find("Cube.001");
                    
                    foundMonitorWall.Find("Cube")
                        .GetComponent<MeshRenderer>().materials[2].mainTexture = foundCube001
                        .GetComponent<MeshRenderer>().materials[2].mainTexture;
                    foundCube001.GetComponent<MeshRenderer>().materials[2].mainTexture = renderTexture;
                    
                    // Reduce get calls by using reference to camera
                    Camera newCamera = helmetCameraNew.AddComponent<Camera>();
                    newCamera.enabled = false;
                    newCamera.targetTexture = renderTexture;
                    newCamera.cullingMask = 20649983;
                    newCamera.farClipPlane = renderDistance;
                    newCamera.nearClipPlane = 0.55f;

                    isMonitorChanged = true;
                    Debug.Log("[HELMET_CAMERAS] Monitors were changed...");

                    Debug.Log("[HELMET_CAMERAS] Turning off vanilla internal ship camera");
                    foundShipCamera.GetComponent<Camera>().enabled = false;
                }
            }
            yield break;
        }

        public void Update()
        {
            bool isConditionsDone = isSceneLoaded && isCoroutineStarted;
            if (isConditionsDone && (StartOfRound.Instance.localPlayerController.isInHangarShipRoom || StartOfRound.Instance.localPlayerController.hasBegunSpectating))
            {
                if (helmetCameraNew.gameObject.activeSelf == false)
                {
                    helmetCameraNew.SetActive(true);
                }
                
                elapsed += Time.deltaTime;
                if (elapsed > 1 / cameraFps)
                {
                    elapsed = 0;
                    helmetCameraNew.GetComponent<Camera>().enabled = true;
                }
                else
                {
                    helmetCameraNew.GetComponent<Camera>().enabled = false;
                }
                
                ManualCameraRenderer cameraMonitorScriptobj = StartOfRound.Instance.mapScreen;
                currentTransformIndex = cameraMonitorScriptobj.targetTransformIndex;
                TransformAndName currentRadarTarget = cameraMonitorScriptobj.radarTargets[currentTransformIndex];

                // Set transform to found position
                helmetCameraNew.transform.SetPositionAndRotation(
                    currentRadarTarget.transform.position + (Vector3.up * 1.6f),
                    currentRadarTarget.transform.rotation * Quaternion.Euler(0f, -90f, 0f)
                );

                // Otherwise set position to targetPlayer, 
                // if dead to deadBody, 
                // -> if spine not found, use already previously set position above
                if (!currentRadarTarget.isNonPlayer && cameraMonitorScriptobj.targetedPlayer != null)
                {
                    // Tagret player if not dead
                    if (!cameraMonitorScriptobj.targetedPlayer.isPlayerDead)
                    {
                        Transform playerVisor = cameraMonitorScriptobj.targetedPlayer.visorCamera.transform;
                        helmetCameraNew.transform.SetPositionAndRotation(playerVisor.position, playerVisor.rotation);
                    }
                    else // Target body if found
                    {
                        DeadBodyInfo deadBody = cameraMonitorScriptobj.targetedPlayer.deadBody;
                        Transform spine = null;

                        // Assuming there are any body parts
                        if (deadBody.bodyParts.Length > 0)
                        {
                            // Check assumption that deadBody.bodyParts[0] is "spine.004"
                            // if true, check if it's parent is "spine.003"
                            if (deadBody.bodyParts[0].name == "spine.004")
                            {
                                if (deadBody.bodyParts[0].transform.parent.name == "spine.003")
                                {
                                    spine = deadBody.bodyParts[0].transform.parent;
                                }
                            }
                            else // otherwise use slower search:
                            {
                                spine = deadBody.transform.Find("spine.001/spine.002/spine.003");
                            }
                        }

                        // If spine found, set camera to it, 
                        // otherwise use previously set position
                        if (spine != null)
                        { 
                            helmetCameraNew.transform.SetPositionAndRotation(spine.position, spine.rotation);
                        }
                    }

                }
            }
            else if (isConditionsDone && !StartOfRound.Instance.localPlayerController.isInHangarShipRoom)
            {
                helmetCameraNew.SetActive(false);
            }
        }

        
    }
}

namespace HelmetCamera.Patches
{
    [HarmonyPatch]
    internal class HelmetCamera
    {
        // Patch the method that's called on every client when they load into new game
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        [HarmonyPostfix]
        public static void OnLocalPlayerFullyLoaded()
        {
            InitCameras();
        }

        public static void InitCameras()
        {
            GameObject shipCamera = GameObject.Find("Environment/HangarShip/Cameras/ShipCamera");
            shipCamera.AddComponent<Plugin>();
        }
    }
}