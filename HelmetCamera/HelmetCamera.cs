using System;
using System.Collections;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using UnityEngine.SceneManagement;

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
            bool sceneflag = (SceneManager.GetActiveScene().name != "MainMenu") && (SceneManager.GetActiveScene().name != "InitScene") && (SceneManager.GetActiveScene().name != "InitSceneLaunchOptions");
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
            bool isCameraFound = GameObject.Find("Environment/HangarShip/Cameras/ShipCamera") != null;
            if (isCameraFound)
            {
                Debug.Log("[HELMET_CAMERAS] Ship camera founded...");
                if (!isMonitorChanged)
                {
                    GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube")
                        .GetComponent<MeshRenderer>().materials[2].mainTexture = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001")
                        .GetComponent<MeshRenderer>().materials[2].mainTexture;
                    GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[2].mainTexture = renderTexture;

                    helmetCameraNew.AddComponent<Camera>();
                    helmetCameraNew.GetComponent<Camera>().enabled = false;
                    helmetCameraNew.GetComponent<Camera>().targetTexture = renderTexture;
                    helmetCameraNew.GetComponent<Camera>().cullingMask = 20649983;
                    helmetCameraNew.GetComponent<Camera>().farClipPlane = renderDistance;
                    helmetCameraNew.GetComponent<Camera>().nearClipPlane = 0.55f;

                    isMonitorChanged = true;
                    Debug.Log("[HELMET_CAMERAS] Monitors were changed...");

                    Debug.Log("[HELMET_CAMERAS] Turning off vanilla internal ship camera");
                    GameObject.Find("Environment/HangarShip/Cameras/ShipCamera").GetComponent<Camera>().enabled = false;
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
                GameObject cameraMonitorScriptobj = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001/CameraMonitorScript");
                currentTransformIndex = cameraMonitorScriptobj.GetComponent<ManualCameraRenderer>().targetTransformIndex;
                TransformAndName transformAndName = cameraMonitorScriptobj.GetComponent<ManualCameraRenderer>().radarTargets[currentTransformIndex];
                if (!transformAndName.isNonPlayer)
                {
                    try
                    {
                        helmetCameraNew.transform.SetPositionAndRotation(transformAndName.transform.Find("ScavengerModel/metarig/CameraContainer/MainCamera/HelmetLights").position + new Vector3(0f, 0f, 0f),
                            transformAndName.transform.Find("ScavengerModel/metarig/CameraContainer/MainCamera/HelmetLights").rotation * Quaternion.Euler(0f, 0f, 0f));

                        DeadBodyInfo[] deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBodyInfo>();

                        for (int i = 0; i < deadBodies.Length; i++)
                        {
                            if (deadBodies[i].playerScript.playerUsername == transformAndName.name)
                            {
                                helmetCameraNew.transform.SetPositionAndRotation(deadBodies[i].gameObject.transform.Find("spine.001/spine.002/spine.003").position, deadBodies[i].gameObject.transform.Find("spine.001/spine.002/spine.003").rotation * Quaternion.Euler(0f, 0f, 0f));
                            }
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        Debug.Log("[HELMET_CAMERAS] ERROR NULL REFERENCE");
                    }
                }
                else
                {
                    helmetCameraNew.transform.SetPositionAndRotation(transformAndName.transform.position + new Vector3(0f, 1.6f, 0f),
                        transformAndName.transform.rotation * Quaternion.Euler(0f, -90f, 0f));
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
        public static void InitCameras()
        {
            GameObject shipCamera = GameObject.Find("Environment/HangarShip/Cameras/ShipCamera");
            shipCamera.AddComponent<Plugin>();
        }

        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPostfix]
        public static void InitCamera(ref ManualCameraRenderer __instance)
        {
            InitCameras();
        }
    }
}