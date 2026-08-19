using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiplayerActivity.Editor
{
    public static class MultiplayerActivityProjectBuilder
    {
        private const string ScenePath = "Assets/Scenes/NetworkBulletDemo.unity";
        private const string PrefabPath = "Assets/Prefabs/NetworkBullet.prefab";
        private const string PrefabListPath = "Assets/NetworkPrefabsList.asset";
        private const string MaterialPath = "Assets/Materials/BulletMaterial.mat";

        [MenuItem("Tools/Multiplayer Activity/Rebuild Simple Demo")]
        public static void Build()
        {
            EnsureFolders();
            DeleteOldAssets();

            Material bulletMaterial = CreateBulletMaterial();
            NetworkObject bulletPrefab = CreateBulletPrefab(bulletMaterial);
            NetworkPrefabsList prefabList = CreatePrefabList(bulletPrefab.gameObject);
            CreateScene(bulletPrefab, prefabList);
            ConfigureProject();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Simple network bullet demo created successfully.");
        }

        [MenuItem("Tools/Multiplayer Activity/Build Windows Player")]
        public static void BuildWindowsPlayer()
        {
            Build();
            Directory.CreateDirectory("Builds/SimpleWindows");

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/SimpleWindows/NetworkBulletDemo.exe",
                target = BuildTarget.StandaloneWindows64
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new System.Exception("Windows build failed.");
        }

        private static void EnsureFolders()
        {
            CreateFolder("Assets", "Scenes");
            CreateFolder("Assets", "Prefabs");
            CreateFolder("Assets", "Materials");
        }

        private static void CreateFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static void DeleteOldAssets()
        {
            AssetDatabase.DeleteAsset("Assets/Scenes/NetworkSpawnDemo.unity");
            AssetDatabase.DeleteAsset("Assets/Prefabs/NetworkCube.prefab");
            AssetDatabase.DeleteAsset("Assets/Materials/NetworkCubeBase.mat");
            AssetDatabase.DeleteAsset(ScenePath);
            AssetDatabase.DeleteAsset(PrefabPath);
            AssetDatabase.DeleteAsset(MaterialPath);
            AssetDatabase.DeleteAsset(PrefabListPath);
        }

        private static Material CreateBulletMaterial()
        {
            Material material = new(Shader.Find("Standard"));
            material.color = Color.yellow;
            AssetDatabase.CreateAsset(material, MaterialPath);
            return material;
        }

        private static NetworkObject CreateBulletPrefab(Material material)
        {
            GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.name = "NetworkBullet";
            bullet.transform.localScale = Vector3.one * 0.3f;
            bullet.GetComponent<MeshRenderer>().sharedMaterial = material;
            bullet.AddComponent<NetworkObject>();
            bullet.AddComponent<NetworkTransform>();
            bullet.AddComponent<NetworkBullet>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(bullet, PrefabPath);
            Object.DestroyImmediate(bullet);
            return prefab.GetComponent<NetworkObject>();
        }

        private static NetworkPrefabsList CreatePrefabList(GameObject prefab)
        {
            NetworkPrefabsList list = ScriptableObject.CreateInstance<NetworkPrefabsList>();
            list.Add(new NetworkPrefab { Prefab = prefab });
            AssetDatabase.CreateAsset(list, PrefabListPath);
            return list;
        }

        private static void CreateScene(NetworkObject bulletPrefab, NetworkPrefabsList prefabList)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateLight();
            CreateFloor();

            GameObject managerObject = new("NetworkManager");
            NetworkManager manager = managerObject.AddComponent<NetworkManager>();
            UnityTransport transport = managerObject.AddComponent<UnityTransport>();
            managerObject.AddComponent<NetworkMenu>();

            manager.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport,
                EnableSceneManagement = true
            };
            manager.NetworkConfig.Prefabs.NetworkPrefabsLists =
                new List<NetworkPrefabsList> { prefabList };

            CreateGun(bulletPrefab);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void CreateGun(NetworkObject bulletPrefab)
        {
            GameObject gun = new("Network Gun");
            gun.transform.position = new Vector3(0f, 0.65f, 0f);
            gun.AddComponent<NetworkObject>();
            GunShooter shooter = gun.AddComponent<GunShooter>();

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Gun Body";
            body.transform.SetParent(gun.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.8f, 0.5f, 1.2f);

            GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrel.name = "Barrel";
            barrel.transform.SetParent(gun.transform);
            barrel.transform.localPosition = new Vector3(0f, 0.1f, 0.9f);
            barrel.transform.localScale = new Vector3(0.25f, 0.25f, 1.2f);

            GameObject muzzleObject = new("Muzzle");
            muzzleObject.transform.SetParent(gun.transform);
            muzzleObject.transform.localPosition = new Vector3(0f, 0.1f, 1.55f);

            SerializedObject serializedShooter = new(shooter);
            serializedShooter.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;
            serializedShooter.FindProperty("muzzle").objectReferenceValue = muzzleObject.transform;
            serializedShooter.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = new Vector3(0f, 4f, -8f);
            cameraObject.transform.rotation = Quaternion.Euler(17f, 0f, 0f);
            camera.backgroundColor = new Color(0.12f, 0.15f, 0.22f);
        }

        private static void CreateLight()
        {
            GameObject lightObject = new("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        }

        private static void CreateFloor()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position = new Vector3(0f, 0f, 10f);
            floor.transform.localScale = new Vector3(3f, 1f, 5f);
        }

        private static void ConfigureProject()
        {
            PlayerSettings.productName = "Simple Network Bullet Demo";
            PlayerSettings.defaultScreenWidth = 800;
            PlayerSettings.defaultScreenHeight = 500;
            PlayerSettings.runInBackground = true;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }
    }
}
