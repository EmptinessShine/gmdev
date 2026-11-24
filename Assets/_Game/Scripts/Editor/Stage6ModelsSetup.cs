using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using TD.Core;
using TD.Data;
using TD.Enemies;
using TD.Towers;
using TD.VFX;

namespace TD.EditorTools
{
    public static class Stage6ModelsSetup
    {
        private const string ModelsDir = "Assets/_Game/Art/Models";
        private const string ModelMatsDir = "Assets/_Game/Art/Materials/Models";
        private const string HitFlashShaderPath = "Assets/_Game/Art/Shaders/SG_HitFlash.shadergraph";
        private const string TowerPrefabsDir = "Assets/_Game/Prefabs/Towers";
        private const string EnemyPrefabsDir = "Assets/_Game/Prefabs/Enemies";
        private const string TowerDataDir = "Assets/_Game/Data/Towers";
        private const string BuildSlotPrefabPath = "Assets/_Game/Prefabs/BuildSlot.prefab";
        private const string GameplayScenePath = "Assets/_Game/Scenes/Gameplay.unity";

        [MenuItem("Tools/TD/Build Stage 6 (3D Models)")]
        public static void BuildStage6Models()
        {
            AssetDatabase.Refresh();

            Dictionary<string, Material> matMap = BuildMaterialMap();

            UpgradeTowerPrefab("Tower_Gunner.prefab", "Model_TowerBase_Gunner.fbx",
                new[] { "Model_Gunner_Lv1.fbx", "Model_Gunner_Lv2.fbx", "Model_Gunner_Lv3.fbx" },
                new Vector3(0f, 0.48f, 0.72f), matMap);

            UpgradeTowerPrefab("Tower_Sniper.prefab", "Model_TowerBase_Sniper.fbx",
                new[] { "Model_Sniper_Lv1.fbx", "Model_Sniper_Lv2.fbx", "Model_Sniper_Lv3.fbx" },
                new Vector3(0f, 0.48f, 1.05f), matMap);

            UpgradeTowerPrefab("Tower_Slow.prefab", "Model_TowerBase_Slow.fbx",
                new[] { "Model_Slow_Lv1.fbx", "Model_Slow_Lv2.fbx", "Model_Slow_Lv3.fbx" },
                new Vector3(0f, 0.72f, 0.25f), matMap);

            UpdateTowerDataScales("Tower_Gunner.asset");
            UpdateTowerDataScales("Tower_Sniper.asset");
            UpdateTowerDataScales("Tower_Slow.asset");

            UpgradeEnemyPrefab("Enemy_Runner.prefab", "Model_Enemy_Runner.fbx", 0.36f, 1.85f, false, matMap);
            UpgradeEnemyPrefab("Enemy_Tank.prefab", "Model_Enemy_Tank.fbx", 0.56f, 2.15f, true, matMap);
            UpgradeEnemyPrefab("Enemy_FrostMage.prefab", "Model_Enemy_FrostMage.fbx", 0.42f, 2.35f, false, matMap);

            UpgradeBuildSlotPrefab(matMap);
            UpgradeGameplayScene(matMap);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Stage 6 (3D Models) setup complete!");
        }

        private static Dictionary<string, Material> BuildMaterialMap()
        {
            Directory.CreateDirectory(ModelMatsDir);
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Shader hitFlashShader = AssetDatabase.LoadAssetAtPath<Shader>(HitFlashShaderPath) ?? litShader;

            var map = new Dictionary<string, Material>
            {
                ["M_DarkMetal"] = MakeLitMat("M_DarkMetal", litShader, new Color(0.16f, 0.18f, 0.22f), 0.75f, 0.55f, Color.black),
                ["M_LightMetal"] = MakeLitMat("M_LightMetal", litShader, new Color(0.55f, 0.59f, 0.65f), 0.85f, 0.65f, Color.black),
                ["M_Stone"] = MakeLitMat("M_Stone", litShader, new Color(0.32f, 0.34f, 0.39f), 0.10f, 0.25f, Color.black),
                ["M_SlotMetal"] = MakeLitMat("M_SlotMetal", litShader, new Color(0.42f, 0.45f, 0.48f), 0.55f, 0.45f, Color.black),
                ["M_Amber"] = MakeLitMat("M_Amber", litShader, new Color(0.92f, 0.48f, 0.12f), 0.35f, 0.50f, Color.black),
                ["M_Violet"] = MakeLitMat("M_Violet", litShader, new Color(0.54f, 0.24f, 0.88f), 0.40f, 0.55f, Color.black),
                ["M_Cyan"] = MakeLitMat("M_Cyan", litShader, new Color(0.16f, 0.76f, 0.90f), 0.20f, 0.75f, Color.black),
                ["M_AmberGlow"] = MakeLitMat("M_AmberGlow", litShader, new Color(1.00f, 0.75f, 0.20f), 0.10f, 0.80f, new Color(1.8f, 1.0f, 0.2f)),
                ["M_VioletGlow"] = MakeLitMat("M_VioletGlow", litShader, new Color(0.85f, 0.45f, 1.00f), 0.10f, 0.80f, new Color(1.5f, 0.5f, 2.0f)),
                ["M_CyanGlow"] = MakeLitMat("M_CyanGlow", litShader, new Color(0.45f, 0.96f, 1.00f), 0.10f, 0.90f, new Color(0.5f, 1.8f, 2.2f)),
                ["M_BlueGlow"] = MakeLitMat("M_BlueGlow", litShader, new Color(0.28f, 0.68f, 1.00f), 0.10f, 0.90f, new Color(0.4f, 1.2f, 2.4f)),
                ["M_CrimsonGlow"] = MakeLitMat("M_CrimsonGlow", litShader, new Color(1.00f, 0.22f, 0.26f), 0.10f, 0.80f, new Color(2.2f, 0.3f, 0.35f)),
                ["M_RunnerBody"] = MakeHitFlashMat("M_RunnerBody", hitFlashShader, new Color(0.92f, 0.26f, 0.18f)),
                ["M_RunnerAccent"] = MakeHitFlashMat("M_RunnerAccent", hitFlashShader, new Color(0.22f, 0.24f, 0.28f)),
                ["M_TankBody"] = MakeHitFlashMat("M_TankBody", hitFlashShader, new Color(0.30f, 0.44f, 0.28f)),
                ["M_TankMetal"] = MakeHitFlashMat("M_TankMetal", hitFlashShader, new Color(0.20f, 0.23f, 0.26f)),
                ["M_MageRobe"] = MakeHitFlashMat("M_MageRobe", hitFlashShader, new Color(0.18f, 0.36f, 0.90f)),
                ["M_MageGlow"] = MakeHitFlashMat("M_MageGlow", hitFlashShader, new Color(0.45f, 0.95f, 1.00f))
            };
            return map;
        }

        private static Material MakeLitMat(string name, Shader shader, Color color, float metallic, float smoothness, Color emission)
        {
            string path = $"{ModelMatsDir}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else if (mat.shader != shader)
            {
                mat.shader = shader;
            }

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (emission != Color.black && mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission);
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material MakeHitFlashMat(string name, Shader shader, Color baseColor)
        {
            string path = $"{ModelMatsDir}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else if (mat.shader != shader)
            {
                mat.shader = shader;
            }

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            if (mat.HasProperty("_FlashColor")) mat.SetColor("_FlashColor", new Color(2f, 2f, 2f, 1f));
            if (mat.HasProperty("_FlashAmount")) mat.SetFloat("_FlashAmount", 0f);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.35f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static GameObject InstantiateModel(string fbxName, Transform parent, Vector3 localPos, Dictionary<string, Material> matMap)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelsDir}/{fbxName}");
            if (prefab == null)
            {
                Debug.LogError($"Missing model FBX: {ModelsDir}/{fbxName}");
                return null;
            }

            GameObject inst = Object.Instantiate(prefab, parent, false);
            inst.name = Path.GetFileNameWithoutExtension(fbxName);
            inst.transform.localPosition = localPos;
            inst.transform.localRotation = Quaternion.identity;
            inst.transform.localScale = Vector3.one;

            foreach (Collider col in inst.GetComponentsInChildren<Collider>(true))
            {
                Object.DestroyImmediate(col);
            }

            foreach (MeshRenderer mr in inst.GetComponentsInChildren<MeshRenderer>(true))
            {
                Material[] mats = mr.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null) continue;
                    string cleanName = mats[i].name.Replace(" (Instance)", "").Trim();
                    if (matMap.TryGetValue(cleanName, out Material mapped))
                    {
                        mats[i] = mapped;
                    }
                }
                mr.sharedMaterials = mats;
            }

            return inst;
        }

        private static void UpgradeTowerPrefab(
            string prefabName, string baseFbx, string[] tierFbx, Vector3 fireLocalPos, Dictionary<string, Material> matMap)
        {
            string path = $"{TowerPrefabsDir}/{prefabName}";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            if (root == null) return;

            DestroyChildIfExists(root.transform, "Base");
            DestroyChildIfExists(root.transform, "Head");
            DestroyChildIfExists(root.transform, "BaseModel");
            DestroyChildIfExists(root.transform, "TurretHead");

            InstantiateModel(baseFbx, root.transform, Vector3.zero, matMap).name = "BaseModel";

            GameObject turretHead = new GameObject("TurretHead");
            turretHead.transform.SetParent(root.transform, false);
            turretHead.transform.localPosition = new Vector3(0f, 0.56f, 0f);

            GameObject[] tierObjs = new GameObject[tierFbx.Length];
            for (int i = 0; i < tierFbx.Length; i++)
            {
                GameObject tier = InstantiateModel(tierFbx[i], turretHead.transform, Vector3.zero, matMap);
                tier.name = $"LevelVisual_{i + 1}";
                tier.SetActive(i == 0);
                tierObjs[i] = tier;
            }

            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(turretHead.transform, false);
            firePoint.transform.localPosition = fireLocalPos - new Vector3(0f, 0.56f, 0f);

            Transform frozen = root.transform.Find("FrozenVisual");
            if (frozen != null)
            {
                frozen.localPosition = new Vector3(0f, 0.60f, 0f);
                frozen.localScale = new Vector3(1.25f, 1.30f, 1.25f);
            }

            Tower tower = root.GetComponent<Tower>();
            if (tower != null)
            {
                SerializedObject so = new SerializedObject(tower);
                so.FindProperty("turretHead").objectReferenceValue = turretHead.transform;
                so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
                SerializedProperty lvProp = so.FindProperty("levelVisuals");
                lvProp.arraySize = tierObjs.Length;
                for (int i = 0; i < tierObjs.Length; i++)
                {
                    lvProp.GetArrayElementAtIndex(i).objectReferenceValue = tierObjs[i];
                }
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void UpdateTowerDataScales(string assetName)
        {
            TowerData td = AssetDatabase.LoadAssetAtPath<TowerData>($"{TowerDataDir}/{assetName}");
            if (td == null || td.levels == null || td.levels.Length < 3) return;
            td.levels[0].visualScale = 1.00f;
            td.levels[1].visualScale = 1.04f;
            td.levels[2].visualScale = 1.08f;
            EditorUtility.SetDirty(td);
        }

        private static void UpgradeEnemyPrefab(
            string prefabName, string modelFbx, float colliderRadius, float healthBarY, bool isTank, Dictionary<string, Material> matMap)
        {
            string path = $"{EnemyPrefabsDir}/{prefabName}";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            if (root == null) return;

            root.transform.localScale = Vector3.one;

            MeshRenderer rootMr = root.GetComponent<MeshRenderer>();
            if (rootMr != null) Object.DestroyImmediate(rootMr);
            MeshFilter rootMf = root.GetComponent<MeshFilter>();
            if (rootMf != null) Object.DestroyImmediate(rootMf);

            DestroyChildIfExists(root.transform, "MageCrystal");
            DestroyChildIfExists(root.transform, "VisualRoot");

            GameObject visualRoot = InstantiateModel(modelFbx, root.transform, Vector3.zero, matMap);
            visualRoot.name = "VisualRoot";
            visualRoot.layer = root.layer;
            foreach (Transform t in visualRoot.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = root.layer;
            }

            NavMeshAgent agent = root.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.baseOffset = 0f;
                agent.radius = colliderRadius;
                agent.height = 1.6f;
            }

            CapsuleCollider col = root.GetComponent<CapsuleCollider>();
            if (col != null)
            {
                col.center = new Vector3(0f, 0.78f, 0f);
                col.radius = colliderRadius;
                col.height = 1.56f;
            }

            Transform hpCanvas = root.transform.Find("HealthBarCanvas");
            if (hpCanvas != null)
            {
                hpCanvas.localPosition = new Vector3(0f, healthBarY, 0f);
            }

            if (isTank)
            {
                Transform shield = root.transform.Find("ShieldVisual");
                if (shield != null)
                {
                    shield.localPosition = new Vector3(0f, 0.85f, 0f);
                    shield.localScale = new Vector3(1.95f, 1.95f, 1.95f);
                }
            }

            EnemyBase enemy = root.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                SerializedObject eso = new SerializedObject(enemy);
                eso.FindProperty("visualRoot").objectReferenceValue = visualRoot.transform;
                eso.ApplyModifiedPropertiesWithoutUndo();
            }

            HitFlash hf = root.GetComponent<HitFlash>();
            if (hf != null)
            {
                SerializedObject hso = new SerializedObject(hf);
                hso.FindProperty("targetRenderer").objectReferenceValue = visualRoot.GetComponentInChildren<MeshRenderer>();
                hso.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void UpgradeBuildSlotPrefab(Dictionary<string, Material> matMap)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(BuildSlotPrefabPath);
            if (root == null) return;

            DestroyChildIfExists(root.transform, "PadVisual");
            GameObject pad = InstantiateModel("Model_BuildSlotPad.fbx", root.transform, Vector3.zero, matMap);
            pad.name = "PadVisual";
            pad.layer = root.layer;
            foreach (Transform t in pad.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = root.layer;
            }

            Transform anchor = root.transform.Find("TowerAnchor");
            if (anchor != null) anchor.localPosition = new Vector3(0f, 0.21f, 0f);

            PrefabUtility.SaveAsPrefabAsset(root, BuildSlotPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void UpgradeGameplayScene(Dictionary<string, Material> matMap)
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

            GameObject spawnPoint = GameObject.Find("SpawnPoint");
            if (spawnPoint != null)
            {
                DestroyChildIfExists(spawnPoint.transform, "Visual");
                GameObject portal = InstantiateModel("Model_SpawnPortal.fbx", spawnPoint.transform, Vector3.zero, matMap);
                portal.name = "Visual";
                portal.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            }

            GameObject basePoint = GameObject.Find("BasePoint");
            if (basePoint != null)
            {
                DestroyChildIfExists(basePoint.transform, "Visual");
                GameObject citadel = InstantiateModel("Model_BaseCitadel.fbx", basePoint.transform, Vector3.zero, matMap);
                citadel.name = "Visual";
                citadel.transform.localRotation = Quaternion.Euler(0f, -45f, 0f);
            }

            LevelLoader loader = Object.FindAnyObjectByType<LevelLoader>();
            if (loader != null)
            {
                loader.LoadActiveLevel();
            }

            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void DestroyChildIfExists(Transform parent, string childName)
        {
            Transform tf = parent.Find(childName);
            if (tf != null)
            {
                Object.DestroyImmediate(tf.gameObject);
            }
        }
    }
}
