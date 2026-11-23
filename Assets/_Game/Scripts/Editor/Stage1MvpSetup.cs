using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TD.Core;
using TD.Data;
using TD.Enemies;
using TD.Towers;
using TD.UI;

namespace TD.EditorTools
{
    public static class Stage1MvpSetup
    {
        private const string MaterialsPath = "Assets/_Game/Art/Materials";
        private const string DataPath = "Assets/_Game/Data";
        private const string TowersDataPath = "Assets/_Game/Data/Towers";
        private const string EnemiesDataPath = "Assets/_Game/Data/Enemies";
        private const string TowerPrefabsPath = "Assets/_Game/Prefabs/Towers";
        private const string ProjectilePrefabsPath = "Assets/_Game/Prefabs/Projectiles";
        private const string EnemyPrefabsPath = "Assets/_Game/Prefabs/Enemies";
        private const string ScenePath = "Assets/_Game/Scenes/Gameplay.unity";

        [MenuItem("Tools/TD/Build Stage 1 MVP")]
        public static void BuildStage1Mvp()
        {
            EnsureLayers("BuildSlot", "Enemy");
            TMP_FontAsset font = EnsureTmpEssentialsAndGetFont();

            Material matGround = CreateOrUpdateMaterial("Mat_Ground.mat", new Color(0.18f, 0.22f, 0.20f));
            Material matWall = CreateOrUpdateMaterial("Mat_Wall.mat", new Color(0.36f, 0.38f, 0.42f));
            Material matSlot = CreateOrUpdateMaterial("Mat_Slot.mat", new Color(0.58f, 0.60f, 0.56f));
            Material matHighlight = CreateOrUpdateMaterial("Mat_SlotHighlight.mat", new Color(1.00f, 0.85f, 0.20f));
            Material matBase = CreateOrUpdateMaterial("Mat_Base.mat", new Color(0.20f, 0.55f, 0.95f));
            Material matSpawn = CreateOrUpdateMaterial("Mat_Spawn.mat", new Color(0.75f, 0.18f, 0.22f));
            Material matGunner = CreateOrUpdateMaterial("Mat_Gunner.mat", new Color(0.95f, 0.55f, 0.15f));
            Material matSniper = CreateOrUpdateMaterial("Mat_Sniper.mat", new Color(0.62f, 0.30f, 0.90f));
            Material matSlow = CreateOrUpdateMaterial("Mat_Slow.mat", new Color(0.20f, 0.85f, 0.95f));
            Material matProjBasic = CreateOrUpdateMaterial("Mat_Projectile.mat", new Color(1.00f, 0.92f, 0.25f));
            Material matProjSlow = CreateOrUpdateMaterial("Mat_ProjectileSlow.mat", new Color(0.35f, 0.95f, 1.00f));
            Material matEnemy = CreateOrUpdateMaterial("Mat_EnemyTest.mat", new Color(0.90f, 0.22f, 0.25f));

            GameConfig gameConfig = CreateOrUpdateGameConfig();
            EnemyData enemyData = CreateOrUpdateEnemyData();

            Projectile projBasic = CreateProjectilePrefab("Projectile_Basic.prefab", 0.28f, matProjBasic);
            Projectile projSlow = CreateProjectilePrefab("Projectile_Slow.prefab", 0.30f, matProjSlow);

            Tower gunnerPrefab = CreateTowerPrefab("Tower_Gunner.prefab", matGunner, new Vector3(0.45f, 0.35f, 0.85f));
            Tower sniperPrefab = CreateTowerPrefab("Tower_Sniper.prefab", matSniper, new Vector3(0.30f, 0.30f, 1.30f));
            Tower slowPrefab = CreateTowerPrefab("Tower_Slow.prefab", matSlow, new Vector3(0.65f, 0.50f, 0.65f));

            TowerData gunnerData = CreateOrUpdateTowerData(
                "Tower_Gunner.asset", "Gunner", gunnerPrefab, projBasic,
                new[]
                {
                    new TowerLevelStats { cost = 50, damage = 10f, fireRate = 2.2f, range = 5.5f, projectileSpeed = 16f, slowMultiplier = 1f, slowDuration = 0f, visualScale = 1.00f },
                    new TowerLevelStats { cost = 60, damage = 16f, fireRate = 2.8f, range = 6.0f, projectileSpeed = 18f, slowMultiplier = 1f, slowDuration = 0f, visualScale = 1.15f },
                    new TowerLevelStats { cost = 80, damage = 24f, fireRate = 3.5f, range = 6.5f, projectileSpeed = 20f, slowMultiplier = 1f, slowDuration = 0f, visualScale = 1.30f }
                });

            TowerData sniperData = CreateOrUpdateTowerData(
                "Tower_Sniper.asset", "Sniper", sniperPrefab, projBasic,
                new[]
                {
                    new TowerLevelStats { cost = 80, damage = 35f, fireRate = 0.70f, range = 10f, projectileSpeed = 26f, slowMultiplier = 1f, slowDuration = 0f, visualScale = 1.00f },
                    new TowerLevelStats { cost = 90, damage = 55f, fireRate = 0.85f, range = 11f, projectileSpeed = 28f, slowMultiplier = 1f, slowDuration = 0f, visualScale = 1.15f },
                    new TowerLevelStats { cost = 110, damage = 85f, fireRate = 1.00f, range = 12f, projectileSpeed = 30f, slowMultiplier = 1f, slowDuration = 0f, visualScale = 1.30f }
                });

            TowerData slowData = CreateOrUpdateTowerData(
                "Tower_Slow.asset", "Slow", slowPrefab, projSlow,
                new[]
                {
                    new TowerLevelStats { cost = 65, damage = 6f, fireRate = 1.2f, range = 5.5f, projectileSpeed = 14f, slowMultiplier = 0.55f, slowDuration = 2.0f, visualScale = 1.00f },
                    new TowerLevelStats { cost = 75, damage = 10f, fireRate = 1.4f, range = 6.2f, projectileSpeed = 16f, slowMultiplier = 0.45f, slowDuration = 2.5f, visualScale = 1.15f },
                    new TowerLevelStats { cost = 95, damage = 15f, fireRate = 1.6f, range = 7.0f, projectileSpeed = 18f, slowMultiplier = 0.35f, slowDuration = 3.0f, visualScale = 1.30f }
                });

            EnemyBase enemyPrefab = CreateEnemyPrefab(enemyData, matEnemy);

            BuildGameplayScene(
                font, gameConfig, enemyPrefab,
                new[] { gunnerData, sniperData, slowData },
                matGround, matWall, matSlot, matHighlight, matBase, matSpawn);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Stage 1 MVP setup complete! Press Play to test.");
        }

        private static void EnsureLayers(params string[] requiredLayers)
        {
            Object[] tagAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (tagAssets == null || tagAssets.Length == 0) return;

            SerializedObject tagManager = new SerializedObject(tagAssets[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            foreach (string layerName in requiredLayers)
            {
                bool exists = false;
                int firstEmpty = -1;

                for (int i = 6; i < layersProp.arraySize; i++)
                {
                    string val = layersProp.GetArrayElementAtIndex(i).stringValue;
                    if (val == layerName)
                    {
                        exists = true;
                        break;
                    }
                    if (firstEmpty == -1 && string.IsNullOrEmpty(val))
                    {
                        firstEmpty = i;
                    }
                }

                if (!exists && firstEmpty != -1)
                {
                    layersProp.GetArrayElementAtIndex(firstEmpty).stringValue = layerName;
                }
            }

            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TMP_FontAsset EnsureTmpEssentialsAndGetFont()
        {
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            if (guids.Length == 0)
            {
                TMP_PackageResourceImporter.ImportResources(true, false, false);
                AssetDatabase.Refresh();
                guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            }

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TMP_FontAsset candidate = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (candidate != null && candidate.name.Contains("LiberationSans"))
                {
                    return candidate;
                }
            }

            if (guids.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            return TMP_Settings.defaultFontAsset;
        }

        private static Material CreateOrUpdateMaterial(string fileName, Color color)
        {
            Directory.CreateDirectory(MaterialsPath);
            string fullPath = $"{MaterialsPath}/{fileName}";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);

            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, fullPath);
            }

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            else if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static GameConfig CreateOrUpdateGameConfig()
        {
            Directory.CreateDirectory(DataPath);
            string path = $"{DataPath}/GameConfig.asset";
            GameConfig asset = AssetDatabase.LoadAssetAtPath<GameConfig>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<GameConfig>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.startingGold = 150;
            asset.startingBaseHp = 20;
            asset.sellRefundPercent = 0.7f;
            asset.testEnemyCount = 10;
            asset.testSpawnInterval = 1.5f;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static EnemyData CreateOrUpdateEnemyData()
        {
            Directory.CreateDirectory(EnemiesDataPath);
            string path = $"{EnemiesDataPath}/Enemy_Test.asset";
            EnemyData asset = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EnemyData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.enemyName = "Test Enemy";
            asset.maxHp = 45f;
            asset.moveSpeed = 3f;
            asset.goldReward = 20;
            asset.baseDamage = 2;
            asset.attackRange = 1.6f;
            asset.attackCooldown = 1.2f;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static TowerData CreateOrUpdateTowerData(
            string fileName, string towerName, Tower prefab, Projectile projPrefab, TowerLevelStats[] levels)
        {
            Directory.CreateDirectory(TowersDataPath);
            string path = $"{TowersDataPath}/{fileName}";
            TowerData asset = AssetDatabase.LoadAssetAtPath<TowerData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<TowerData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.towerName = towerName;
            asset.prefab = prefab;
            asset.projectilePrefab = projPrefab;
            asset.levels = levels;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static Projectile CreateProjectilePrefab(string fileName, float scale, Material mat)
        {
            Directory.CreateDirectory(ProjectilePrefabsPath);
            string path = $"{ProjectilePrefabsPath}/{fileName}";

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = Path.GetFileNameWithoutExtension(fileName);
            go.transform.localScale = Vector3.one * scale;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            go.AddComponent<Projectile>();

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return saved.GetComponent<Projectile>();
        }

        private static Tower CreateTowerPrefab(string fileName, Material mat, Vector3 headScale)
        {
            Directory.CreateDirectory(TowerPrefabsPath);
            string path = $"{TowerPrefabsPath}/{fileName}";

            GameObject root = new GameObject(Path.GetFileNameWithoutExtension(fileName));
            Tower tower = root.AddComponent<Tower>();

            GameObject basePedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basePedestal.name = "Base";
            basePedestal.transform.SetParent(root.transform, false);
            basePedestal.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            basePedestal.transform.localScale = new Vector3(0.85f, 0.25f, 0.85f);
            Object.DestroyImmediate(basePedestal.GetComponent<Collider>());
            basePedestal.GetComponent<MeshRenderer>().sharedMaterial = mat;

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 0.70f, 0f);
            head.transform.localScale = headScale;
            Object.DestroyImmediate(head.GetComponent<Collider>());
            head.GetComponent<MeshRenderer>().sharedMaterial = mat;

            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(head.transform, false);
            firePoint.transform.localPosition = new Vector3(0f, 0f, 0.6f);

            SerializedObject so = new SerializedObject(tower);
            so.FindProperty("enemyLayer").intValue = LayerMask.GetMask("Enemy");
            so.FindProperty("turretHead").objectReferenceValue = head.transform;
            so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return saved.GetComponent<Tower>();
        }

        private static EnemyBase CreateEnemyPrefab(EnemyData enemyData, Material mat)
        {
            Directory.CreateDirectory(EnemyPrefabsPath);
            string path = $"{EnemyPrefabsPath}/Enemy_Test.prefab";

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Enemy_Test";
            go.layer = LayerMask.NameToLayer("Enemy");
            go.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;

            NavMeshAgent agent = go.AddComponent<NavMeshAgent>();
            agent.baseOffset = 1.0f;
            agent.radius = 0.4f;
            agent.height = 1.6f;

            EnemyBase enemy = go.AddComponent<EnemyBase>();
            SerializedObject so = new SerializedObject(enemy);
            so.FindProperty("data").objectReferenceValue = enemyData;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return saved.GetComponent<EnemyBase>();
        }

        private static void BuildGameplayScene(
            TMP_FontAsset font,
            GameConfig gameConfig,
            EnemyBase enemyPrefab,
            TowerData[] towerDatas,
            Material matGround,
            Material matWall,
            Material matSlot,
            Material matHighlight,
            Material matBase,
            Material matSpawn)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0f, 17f, -11.5f);
                cam.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
                cam.fieldOfView = 48f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
            }

            Light dirLight = Object.FindFirstObjectByType<Light>();
            if (dirLight != null)
            {
                dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                dirLight.intensity = 1.2f;
            }

            // NavMesh Arena
            GameObject navRoot = new GameObject("NavMeshRoot");
            NavMeshSurface surface = navRoot.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;

            CreateBox("Ground", navRoot.transform, new Vector3(0f, -0.5f, 0f), new Vector3(24f, 1f, 16f), matGround, true);
            CreateBox("Wall_North", navRoot.transform, new Vector3(0f, 0.75f, 7.5f), new Vector3(24f, 1.5f, 1f), matWall, true);
            CreateBox("Wall_South", navRoot.transform, new Vector3(0f, 0.75f, -7.5f), new Vector3(24f, 1.5f, 1f), matWall, true);
            CreateBox("Wall_West", navRoot.transform, new Vector3(-11.5f, 0.75f, 0f), new Vector3(1f, 1.5f, 16f), matWall, true);
            CreateBox("Wall_East", navRoot.transform, new Vector3(11.5f, 0.75f, 0f), new Vector3(1f, 1.5f, 16f), matWall, true);
            CreateBox("Wall_Left", navRoot.transform, new Vector3(-4.0f, 0.75f, 2.0f), new Vector3(1f, 1.5f, 10f), matWall, true);
            CreateBox("Wall_Right", navRoot.transform, new Vector3(4.0f, 0.75f, -2.0f), new Vector3(1f, 1.5f, 10f), matWall, true);

            surface.BuildNavMesh();

            // Spawn & Base
            GameObject spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.position = new Vector3(-8.5f, 0f, 5.0f);
            GameObject spawnVis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spawnVis.name = "Visual";
            spawnVis.transform.SetParent(spawnPoint.transform, false);
            spawnVis.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            spawnVis.transform.localScale = new Vector3(1.5f, 0.05f, 1.5f);
            Object.DestroyImmediate(spawnVis.GetComponent<Collider>());
            spawnVis.GetComponent<MeshRenderer>().sharedMaterial = matSpawn;

            GameObject basePoint = new GameObject("BasePoint");
            basePoint.transform.position = new Vector3(8.5f, 0f, -5.0f);
            GameObject baseVis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseVis.name = "Visual";
            baseVis.transform.SetParent(basePoint.transform, false);
            baseVis.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            baseVis.transform.localScale = new Vector3(1.6f, 2.0f, 1.6f);
            Object.DestroyImmediate(baseVis.GetComponent<Collider>());
            baseVis.GetComponent<MeshRenderer>().sharedMaterial = matBase;

            // 8 Build Slots
            GameObject slotsParent = new GameObject("BuildSlots");
            Vector3[] slotPositions =
            {
                new Vector3(-5.6f, 0f, 4.0f),
                new Vector3(-5.6f, 0f, -0.5f),
                new Vector3(-9.5f, 0f, -1.5f),
                new Vector3(-2.4f, 0f, -1.0f),
                new Vector3(-2.4f, 0f, 3.5f),
                new Vector3(2.4f, 0f, 1.0f),
                new Vector3(2.4f, 0f, -3.5f),
                new Vector3(5.6f, 0f, -1.5f)
            };

            int buildSlotLayer = LayerMask.NameToLayer("BuildSlot");
            for (int i = 0; i < slotPositions.Length; i++)
            {
                CreateBuildSlot($"BuildSlot_{i + 1}", slotsParent.transform, slotPositions[i], buildSlotLayer, matSlot, matHighlight);
            }

            // GameSystems
            GameObject systems = new GameObject("GameSystems");
            GameManager gm = systems.AddComponent<GameManager>();
            SerializedObject gmSo = new SerializedObject(gm);
            gmSo.FindProperty("config").objectReferenceValue = gameConfig;
            gmSo.ApplyModifiedPropertiesWithoutUndo();

            SlotSelector selector = systems.AddComponent<SlotSelector>();
            SerializedObject selSo = new SerializedObject(selector);
            selSo.FindProperty("mainCamera").objectReferenceValue = cam;
            selSo.FindProperty("slotLayer").intValue = LayerMask.GetMask("BuildSlot");
            selSo.ApplyModifiedPropertiesWithoutUndo();

            TestWaveSpawner spawner = systems.AddComponent<TestWaveSpawner>();
            SerializedObject spSo = new SerializedObject(spawner);
            spSo.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab;
            spSo.FindProperty("spawnPoint").objectReferenceValue = spawnPoint.transform;
            spSo.FindProperty("basePoint").objectReferenceValue = basePoint.transform;
            spSo.ApplyModifiedPropertiesWithoutUndo();

            // EventSystem (New Input System)
            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();

            // UI Canvas
            BuildCanvas(font, selector, towerDatas);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);

            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        private static GameObject CreateBox(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, bool keepCollider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static void CreateBuildSlot(
            string name, Transform parent, Vector3 pos, int layer, Material matSlot, Material matHighlight)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = pos;
            root.layer = layer;

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.125f, 0f);
            col.size = new Vector3(1.4f, 0.25f, 1.4f);

            NavMeshObstacle obstacle = root.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;
            obstacle.center = new Vector3(0f, 0.25f, 0f);
            obstacle.size = new Vector3(1.4f, 0.5f, 1.4f);

            GameObject pad = CreateBox("PadVisual", root.transform, new Vector3(0f, 0.1f, 0f), new Vector3(1.4f, 0.2f, 1.4f), matSlot, false);
            pad.layer = layer;

            GameObject highlight = CreateBox("HighlightVisual", root.transform, new Vector3(0f, 0.08f, 0f), new Vector3(1.6f, 0.12f, 1.6f), matHighlight, false);
            highlight.SetActive(false);

            GameObject anchor = new GameObject("TowerAnchor");
            anchor.transform.SetParent(root.transform, false);
            anchor.transform.localPosition = new Vector3(0f, 0.2f, 0f);

            BuildSlot slot = root.AddComponent<BuildSlot>();
            SerializedObject so = new SerializedObject(slot);
            so.FindProperty("towerAnchor").objectReferenceValue = anchor.transform;
            so.FindProperty("highlightVisual").objectReferenceValue = highlight;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildCanvas(TMP_FontAsset font, SlotSelector selector, TowerData[] towerDatas)
        {
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // Top Bar
            RectTransform topBar = CreatePanel("TopBar", canvasGo.transform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -90f), Vector2.zero, new Color(0.08f, 0.10f, 0.14f, 0.78f));

            TMP_Text goldText = CreateTmpText("GoldText", topBar, font, "Gold: 150", 34, TextAlignmentOptions.Left,
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(30f, 0f), new Vector2(320f, 0f));

            TMP_Text baseHpText = CreateTmpText("BaseHpText", topBar, font, "Base HP: 20", 34, TextAlignmentOptions.Left,
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(360f, 0f), new Vector2(680f, 0f));

            TMP_Text enemiesText = CreateTmpText("EnemiesText", topBar, font, "Enemies: 10", 34, TextAlignmentOptions.Left,
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(700f, 0f), new Vector2(1050f, 0f));

            Button pauseBtn = CreateButton("PauseButton", topBar, font, "Pause", 28,
                new Color(0.25f, 0.30f, 0.40f), out _);
            SetRect(pauseBtn.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-110f, 0f), new Vector2(170f, 64f));

            // Bottom Build Panel
            RectTransform buildRoot = CreatePanel("BuildPanelRoot", canvasGo.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-420f, 20f), new Vector2(420f, 160f), new Color(0.08f, 0.10f, 0.14f, 0.88f));

            GameObject buildGroup = CreateFullStretch("BuildGroup", buildRoot);
            Button[] buildButtons = new Button[3];
            TMP_Text[] buildLabels = new TMP_Text[3];
            Color[] btnColors =
            {
                new Color(0.65f, 0.38f, 0.12f),
                new Color(0.42f, 0.22f, 0.65f),
                new Color(0.15f, 0.52f, 0.62f)
            };

            for (int i = 0; i < 3; i++)
            {
                buildButtons[i] = CreateButton($"Btn_Tower_{i}", buildGroup.transform, font, $"Tower {i}", 26, btnColors[i], out buildLabels[i]);
                float x = (i - 1) * 260f;
                SetRect(buildButtons[i].GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(x, 0f), new Vector2(230f, 100f));
            }

            GameObject manageGroup = CreateFullStretch("ManageGroup", buildRoot);
            TMP_Text towerInfoText = CreateTmpText("TowerInfoText", manageGroup.transform, font, "Tower (Lv.1)", 30, TextAlignmentOptions.Left,
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(30f, 0f), new Vector2(340f, 0f));

            Button upgradeBtn = CreateButton("UpgradeButton", manageGroup.transform, font, "Upgrade", 26,
                new Color(0.20f, 0.58f, 0.30f), out TMP_Text upgradeLabel);
            SetRect(upgradeBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(80f, 0f), new Vector2(210f, 96f));

            Button sellBtn = CreateButton("SellButton", manageGroup.transform, font, "Sell", 26,
                new Color(0.68f, 0.22f, 0.22f), out TMP_Text sellLabel);
            SetRect(sellBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(310f, 0f), new Vector2(190f, 96f));

            buildRoot.gameObject.SetActive(false);

            // Overlay Panels (Pause, Win, Lose)
            GameObject pausePanel = CreateOverlayPanel("PausePanel", canvasGo.transform, font, "PAUSED",
                "Resume", out Button resumeBtn, "Restart", out Button pauseRestartBtn);
            GameObject winPanel = CreateOverlayPanel("WinPanel", canvasGo.transform, font, "VICTORY!",
                null, out _, "Restart", out Button winRestartBtn);
            GameObject losePanel = CreateOverlayPanel("LosePanel", canvasGo.transform, font, "DEFEAT",
                null, out _, "Restart", out Button loseRestartBtn);

            // Wire HudUI
            HudUI hud = canvasGo.AddComponent<HudUI>();
            SerializedObject hudSo = new SerializedObject(hud);
            hudSo.FindProperty("goldText").objectReferenceValue = goldText;
            hudSo.FindProperty("baseHpText").objectReferenceValue = baseHpText;
            hudSo.FindProperty("enemiesText").objectReferenceValue = enemiesText;
            hudSo.FindProperty("pauseButton").objectReferenceValue = pauseBtn;
            hudSo.FindProperty("pausePanel").objectReferenceValue = pausePanel;
            hudSo.FindProperty("resumeButton").objectReferenceValue = resumeBtn;
            hudSo.FindProperty("pauseRestartButton").objectReferenceValue = pauseRestartBtn;
            hudSo.FindProperty("winPanel").objectReferenceValue = winPanel;
            hudSo.FindProperty("winRestartButton").objectReferenceValue = winRestartBtn;
            hudSo.FindProperty("losePanel").objectReferenceValue = losePanel;
            hudSo.FindProperty("loseRestartButton").objectReferenceValue = loseRestartBtn;
            hudSo.ApplyModifiedPropertiesWithoutUndo();

            // Wire BuildPanelUI
            BuildPanelUI buildUi = canvasGo.AddComponent<BuildPanelUI>();
            SerializedObject bpSo = new SerializedObject(buildUi);
            bpSo.FindProperty("slotSelector").objectReferenceValue = selector;
            bpSo.FindProperty("panelRoot").objectReferenceValue = buildRoot.gameObject;
            bpSo.FindProperty("buildGroup").objectReferenceValue = buildGroup;
            bpSo.FindProperty("manageGroup").objectReferenceValue = manageGroup;
            SetObjectArray(bpSo.FindProperty("availableTowers"), towerDatas);
            SetObjectArray(bpSo.FindProperty("buildButtons"), buildButtons);
            SetObjectArray(bpSo.FindProperty("buildButtonLabels"), buildLabels);
            bpSo.FindProperty("towerInfoText").objectReferenceValue = towerInfoText;
            bpSo.FindProperty("upgradeButton").objectReferenceValue = upgradeBtn;
            bpSo.FindProperty("upgradeButtonLabel").objectReferenceValue = upgradeLabel;
            bpSo.FindProperty("sellButton").objectReferenceValue = sellBtn;
            bpSo.FindProperty("sellButtonLabel").objectReferenceValue = sellLabel;
            bpSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateOverlayPanel(
            string name, Transform parent, TMP_FontAsset font, string title,
            string primaryLabel, out Button primaryBtn,
            string secondaryLabel, out Button secondaryBtn)
        {
            RectTransform panel = CreatePanel(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.05f, 0.06f, 0.09f, 0.80f));

            RectTransform box = CreatePanel("Box", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-240f, -160f), new Vector2(240f, 160f), new Color(0.14f, 0.17f, 0.23f, 0.96f));

            CreateTmpText("Title", box, font, title, 46, TextAlignmentOptions.Center,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -100f), new Vector2(0f, -20f));

            primaryBtn = null;
            if (!string.IsNullOrEmpty(primaryLabel))
            {
                primaryBtn = CreateButton("PrimaryButton", box, font, primaryLabel, 30, new Color(0.20f, 0.58f, 0.30f), out _);
                SetRect(primaryBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -15f), new Vector2(260f, 70f));
            }

            secondaryBtn = CreateButton("RestartButton", box, font, secondaryLabel, 30, new Color(0.28f, 0.35f, 0.48f), out _);
            float yPos = primaryBtn != null ? -100f : -45f;
            SetRect(secondaryBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, yPos), new Vector2(260f, 70f));

            panel.gameObject.SetActive(false);
            return panel.gameObject;
        }

        private static RectTransform CreatePanel(
            string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            go.GetComponent<Image>().color = color;
            return rt;
        }

        private static GameObject CreateFullStretch(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        private static TMP_Text CreateTmpText(
            string name, Transform parent, TMP_FontAsset font, string text, float fontSize, TextAlignmentOptions align,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            return tmp;
        }

        private static Button CreateButton(
            string name, Transform parent, TMP_FontAsset font, string label, float fontSize, Color bgColor, out TMP_Text tmpOut)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = bgColor;
            Button btn = go.GetComponent<Button>();

            tmpOut = CreateTmpText("Label", go.transform, font, label, fontSize, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(8f, 4f), new Vector2(-8f, -4f));

            return btn;
        }

        private static void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }

        private static void SetObjectArray<T>(SerializedProperty prop, T[] values) where T : Object
        {
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }
    }
}
