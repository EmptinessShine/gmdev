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
using TD.Save;
using TD.UI;

namespace TD.EditorTools
{
    public static class Stage3A11Setup
    {
        private const string LevelsDataPath = "Assets/_Game/Data/Levels";
        private const string UiPrefabsPath = "Assets/_Game/Prefabs/UI";
        private const string EnemyPrefabsPath = "Assets/_Game/Prefabs/Enemies";
        private const string GameplayScenePath = "Assets/_Game/Scenes/Gameplay.unity";
        private const string MainMenuScenePath = "Assets/_Game/Scenes/MainMenu.unity";
        private const string WhiteSpritePath = "Assets/_Game/Art/UI_White.png";

        [MenuItem("Tools/TD/Build Stage 3 (A11)")]
        public static void BuildStage3A11()
        {
            TMP_FontAsset font = GetDefaultFont();
            Sprite whiteSprite = EnsureWhiteSprite();

            BuildSlot buildSlotPrefab = CreateOrUpdateBuildSlotPrefab();
            DamagePopup damagePopupPrefab = CreateOrUpdateDamagePopupPrefab(font);
            UpdateEnemyPrefabs(damagePopupPrefab, whiteSprite);

            RunnerEnemy runner = AssetDatabase.LoadAssetAtPath<RunnerEnemy>($"{EnemyPrefabsPath}/Enemy_Runner.prefab");
            TankEnemy tank = AssetDatabase.LoadAssetAtPath<TankEnemy>($"{EnemyPrefabsPath}/Enemy_Tank.prefab");
            FrostMageEnemy mage = AssetDatabase.LoadAssetAtPath<FrostMageEnemy>($"{EnemyPrefabsPath}/Enemy_FrostMage.prefab");

            WaveSetData waveSet1 = AssetDatabase.LoadAssetAtPath<WaveSetData>($"{LevelsDataPath}/WaveSet_Level1.asset");
            WaveSetData waveSet2 = CreateOrUpdateWaveSet2(runner, tank, mage);
            WaveSetData waveSet3 = CreateOrUpdateWaveSet3(runner, tank, mage);

            LevelData level1 = CreateOrUpdateLevel1(waveSet1);
            LevelData level2 = CreateOrUpdateLevel2(waveSet2);
            LevelData level3 = CreateOrUpdateLevel3(waveSet3);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UpdateGameplayScene();
            BuildMainMenuScene();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(GameplayScenePath, true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Stage 3 (A11) setup complete! Open MainMenu.unity and press Play to test.");
        }

        private static TMP_FontAsset GetDefaultFont()
        {
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            foreach (string guid in guids)
            {
                TMP_FontAsset candidate = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
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

        private static Sprite EnsureWhiteSprite()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(WhiteSpritePath));
            if (!File.Exists(WhiteSpritePath))
            {
                Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                Color[] pixels = new Color[16];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
                tex.SetPixels(pixels);
                tex.Apply();
                File.WriteAllBytes(WhiteSpritePath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(WhiteSpritePath, ImportAssetOptions.ForceUpdate);
            }

            TextureImporter importer = AssetImporter.GetAtPath(WhiteSpritePath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(WhiteSpritePath);
        }

        private static BuildSlot CreateOrUpdateBuildSlotPrefab()
        {
            string path = "Assets/_Game/Prefabs/BuildSlot.prefab";
            Material matSlot = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Materials/Mat_Slot.mat");
            Material matHighlight = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Materials/Mat_SlotHighlight.mat");
            int buildSlotLayer = LayerMask.NameToLayer("BuildSlot");

            GameObject root = new GameObject("BuildSlot");
            root.layer = buildSlotLayer;

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.125f, 0f);
            col.size = new Vector3(1.4f, 0.25f, 1.4f);

            NavMeshObstacle obstacle = root.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;
            obstacle.carvingMoveThreshold = 0.1f;
            obstacle.center = new Vector3(0f, 0.25f, 0f);
            obstacle.size = new Vector3(1.25f, 0.5f, 1.25f);

            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = "PadVisual";
            pad.layer = buildSlotLayer;
            pad.transform.SetParent(root.transform, false);
            pad.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            pad.transform.localScale = new Vector3(1.4f, 0.2f, 1.4f);
            Object.DestroyImmediate(pad.GetComponent<Collider>());
            if (matSlot != null) pad.GetComponent<MeshRenderer>().sharedMaterial = matSlot;

            GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            highlight.name = "HighlightVisual";
            highlight.transform.SetParent(root.transform, false);
            highlight.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            highlight.transform.localScale = new Vector3(1.6f, 0.12f, 1.6f);
            Object.DestroyImmediate(highlight.GetComponent<Collider>());
            if (matHighlight != null) highlight.GetComponent<MeshRenderer>().sharedMaterial = matHighlight;
            highlight.SetActive(false);

            GameObject anchor = new GameObject("TowerAnchor");
            anchor.transform.SetParent(root.transform, false);
            anchor.transform.localPosition = new Vector3(0f, 0.2f, 0f);

            BuildSlot slot = root.AddComponent<BuildSlot>();
            SerializedObject so = new SerializedObject(slot);
            so.FindProperty("towerAnchor").objectReferenceValue = anchor.transform;
            so.FindProperty("highlightVisual").objectReferenceValue = highlight;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return saved.GetComponent<BuildSlot>();
        }

        private static DamagePopup CreateOrUpdateDamagePopupPrefab(TMP_FontAsset font)
        {
            Directory.CreateDirectory(UiPrefabsPath);
            string path = $"{UiPrefabsPath}/DamagePopup.prefab";

            GameObject go = new GameObject("DamagePopup");
            TextMeshPro tmp = go.AddComponent<TextMeshPro>();
            if (font != null) tmp.font = font;
            tmp.fontSize = 5f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.text = "10";
            tmp.rectTransform.sizeDelta = new Vector2(3f, 1.5f);

            go.AddComponent<DamagePopup>();

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return saved.GetComponent<DamagePopup>();
        }

        private static void UpdateEnemyPrefabs(DamagePopup popupPrefab, Sprite whiteSprite)
        {
            string[] names = { "Enemy_Runner.prefab", "Enemy_Tank.prefab", "Enemy_FrostMage.prefab" };
            foreach (string fileName in names)
            {
                string path = $"{EnemyPrefabsPath}/{fileName}";
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                if (root == null) continue;

                EnemyBase enemy = root.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    SerializedObject eso = new SerializedObject(enemy);
                    eso.FindProperty("damagePopupPrefab").objectReferenceValue = popupPrefab;
                    eso.ApplyModifiedPropertiesWithoutUndo();
                }

                NavMeshAgent agent = root.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
                    agent.autoBraking = false;
                    agent.angularSpeed = 360f;
                    agent.acceleration = 20f;
                    agent.radius = fileName.Contains("Tank") ? 0.38f : 0.30f;
                }

                Transform oldCanvas = root.transform.Find("HealthBarCanvas");
                if (oldCanvas != null) Object.DestroyImmediate(oldCanvas.gameObject);

                GameObject canvasGo = new GameObject("HealthBarCanvas", typeof(RectTransform), typeof(Canvas));
                canvasGo.transform.SetParent(root.transform, false);
                canvasGo.transform.localPosition = new Vector3(0f, 1.55f, 0f);
                canvasGo.transform.localScale = Vector3.one * 0.01f;

                Canvas c = canvasGo.GetComponent<Canvas>();
                c.renderMode = RenderMode.WorldSpace;
                RectTransform crt = canvasGo.GetComponent<RectTransform>();
                crt.sizeDelta = new Vector2(120f, 18f);

                GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
                bg.transform.SetParent(canvasGo.transform, false);
                RectTransform bgrt = bg.GetComponent<RectTransform>();
                bgrt.anchorMin = Vector2.zero;
                bgrt.anchorMax = Vector2.one;
                bgrt.offsetMin = Vector2.zero;
                bgrt.offsetMax = Vector2.zero;
                Image bgImg = bg.GetComponent<Image>();
                bgImg.sprite = whiteSprite;
                bgImg.color = new Color(0.08f, 0.08f, 0.10f, 0.85f);

                GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fill.transform.SetParent(bg.transform, false);
                RectTransform frt = fill.GetComponent<RectTransform>();
                frt.anchorMin = Vector2.zero;
                frt.anchorMax = Vector2.one;
                frt.offsetMin = new Vector2(2f, 2f);
                frt.offsetMax = new Vector2(-2f, -2f);
                Image fillImg = fill.GetComponent<Image>();
                fillImg.sprite = whiteSprite;
                fillImg.type = Image.Type.Filled;
                fillImg.fillMethod = Image.FillMethod.Horizontal;
                fillImg.fillOrigin = 0;
                fillImg.fillAmount = 1f;
                fillImg.color = new Color(0.25f, 0.85f, 0.35f);

                EnemyHealthBar bar = canvasGo.AddComponent<EnemyHealthBar>();
                SerializedObject bso = new SerializedObject(bar);
                bso.FindProperty("enemy").objectReferenceValue = enemy;
                bso.FindProperty("fillImage").objectReferenceValue = fillImg;
                bso.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static WaveSetData CreateOrUpdateWaveSet2(RunnerEnemy runner, TankEnemy tank, FrostMageEnemy mage)
        {
            Directory.CreateDirectory(LevelsDataPath);
            string path = $"{LevelsDataPath}/WaveSet_Level2.asset";
            WaveSetData asset = AssetDatabase.LoadAssetAtPath<WaveSetData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<WaveSetData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.waves = new[]
            {
                new WaveDefinition
                {
                    waveName = "Wave 1",
                    delayBeforeNextWave = 4.0f,
                    entries = new[] { new WaveEntry { enemyPrefab = runner, count = 7, spawnInterval = 1.15f } }
                },
                new WaveDefinition
                {
                    waveName = "Wave 2",
                    delayBeforeNextWave = 4.0f,
                    entries = new[]
                    {
                        new WaveEntry { enemyPrefab = runner, count = 6, spawnInterval = 0.95f },
                        new WaveEntry { enemyPrefab = tank, count = 3, spawnInterval = 1.8f }
                    }
                },
                new WaveDefinition
                {
                    waveName = "Wave 3",
                    delayBeforeNextWave = 4.0f,
                    entries = new[]
                    {
                        new WaveEntry { enemyPrefab = runner, count = 8, spawnInterval = 0.85f },
                        new WaveEntry { enemyPrefab = mage, count = 3, spawnInterval = 1.7f }
                    }
                },
                new WaveDefinition
                {
                    waveName = "Wave 4",
                    delayBeforeNextWave = 4.5f,
                    entries = new[]
                    {
                        new WaveEntry { enemyPrefab = tank, count = 4, spawnInterval = 1.6f },
                        new WaveEntry { enemyPrefab = mage, count = 4, spawnInterval = 1.5f }
                    }
                },
                new WaveDefinition
                {
                    waveName = "Wave 5",
                    delayBeforeNextWave = 0f,
                    entries = new[]
                    {
                        new WaveEntry { enemyPrefab = runner, count = 12, spawnInterval = 0.7f },
                        new WaveEntry { enemyPrefab = tank, count = 5, spawnInterval = 1.5f },
                        new WaveEntry { enemyPrefab = mage, count = 4, spawnInterval = 1.4f }
                    }
                }
            };

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static WaveSetData CreateOrUpdateWaveSet3(RunnerEnemy runner, TankEnemy tank, FrostMageEnemy mage)
        {
            Directory.CreateDirectory(LevelsDataPath);
            string path = $"{LevelsDataPath}/WaveSet_Level3.asset";
            WaveSetData asset = AssetDatabase.LoadAssetAtPath<WaveSetData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<WaveSetData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.waves = new[]
            {
                new WaveDefinition
                {
                    waveName = "Wave 1",
                    delayBeforeNextWave = 3.5f,
                    entries = new[]
                    {
                        new WaveEntry { enemyPrefab = runner, count = 8, spawnInterval = 1.0f },
                        new WaveEntry { enemyPrefab = tank, count = 1, spawnInterval = 2.0f }
                    }
                },
                new WaveDefinition
                {
                    waveName = "Wave 2",
                    delayBeforeNextWave = 4.0f,
                    entries = new[]
                    {
                        new WaveEntry { enemyPrefab = runner, count = 8, spawnInterval = 0.9f },
                        new WaveEntry { enemyPrefab = mage, count = 3, spawnInterval = 1.6f }
                    }
                },
                new WaveDefinition
                {
                    waveName = "Wave 3",
                    delayBeforeNextWave = 4.0f,
                    entries = new[]
                    {
                        new WaveEntry { enemyPrefab = tank, count = 4, spawnInterval = 1.7f },
                        new WaveEntry { enemyPrefab = runner, count = 8, spawnInterval = 0.8f }
                    }
                },
                new WaveDefinition
                {
                    waveName = "Wave 4",
                    delayBeforeNextWave = 4.0f,
                    entries = new[]
                    {
                        new WaveEntry { enemyPrefab = tank, count = 5, spawnInterval = 1.5f },
                        new WaveEntry { enemyPrefab = mage, count = 4, spawnInterval = 1.4f }
                    }
                },
                new WaveDefinition
                {
                    waveName = "Wave 5",
                    delayBeforeNextWave = 4.5f,
                    entries = new[]
                    {
                        new WaveEntry { enemyPrefab = runner, count = 12, spawnInterval = 0.65f },
                        new WaveEntry { enemyPrefab = tank, count = 5, spawnInterval = 1.4f },
                        new WaveEntry { enemyPrefab = mage, count = 5, spawnInterval = 1.3f }
                    }
                },
                new WaveDefinition
                {
                    waveName = "Wave 6",
                    delayBeforeNextWave = 0f,
                    entries = new[]
                    {
                        new WaveEntry { enemyPrefab = tank, count = 6, spawnInterval = 1.3f },
                        new WaveEntry { enemyPrefab = mage, count = 6, spawnInterval = 1.2f },
                        new WaveEntry { enemyPrefab = runner, count = 14, spawnInterval = 0.55f }
                    }
                }
            };

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static LevelData CreateOrUpdateLevel1(WaveSetData waveSet)
        {
            return CreateOrUpdateLevel("Level_1.asset", 0, "Level 1: S-Curve", 110, 20,
                new Color(0.18f, 0.22f, 0.20f),
                new Vector3(-8.5f, 0f, 5.0f),
                new Vector3(8.5f, 0f, -5.0f),
                new[]
                {
                    new WallBlock { position = new Vector3(-4.0f, 0.75f, 2.0f), scale = new Vector3(1f, 1.5f, 10f) },
                    new WallBlock { position = new Vector3(4.0f, 0.75f, -2.0f), scale = new Vector3(1f, 1.5f, 10f) }
                },
                new[]
                {
                    new Vector3(-5.6f, 0f, 4.0f),
                    new Vector3(-5.6f, 0f, -0.5f),
                    new Vector3(-10.1f, 0f, -1.5f),
                    new Vector3(-2.4f, 0f, -1.0f),
                    new Vector3(-2.4f, 0f, 3.5f),
                    new Vector3(2.4f, 0f, 1.0f),
                    new Vector3(2.4f, 0f, -3.5f),
                    new Vector3(5.6f, 0f, -1.5f)
                },
                waveSet);
        }

        private static LevelData CreateOrUpdateLevel2(WaveSetData waveSet)
        {
            return CreateOrUpdateLevel("Level_2.asset", 1, "Level 2: Twin Gates", 125, 20,
                new Color(0.17f, 0.20f, 0.26f),
                new Vector3(-9.2f, 0f, 0f),
                new Vector3(9.2f, 0f, 0f),
                new[]
                {
                    new WallBlock { position = new Vector3(-3.5f, 0.75f, 4.5f), scale = new Vector3(1f, 1.5f, 5.0f) },
                    new WallBlock { position = new Vector3(-3.5f, 0.75f, -4.5f), scale = new Vector3(1f, 1.5f, 5.0f) },
                    new WallBlock { position = new Vector3(3.5f, 0.75f, 0f), scale = new Vector3(1f, 1.5f, 6.4f) }
                },
                new[]
                {
                    new Vector3(-4.8f, 0f, 3.6f),
                    new Vector3(-4.8f, 0f, -3.6f),
                    new Vector3(-2.2f, 0f, 3.6f),
                    new Vector3(-2.2f, 0f, -3.6f),
                    new Vector3(2.2f, 0f, 1.8f),
                    new Vector3(2.2f, 0f, -1.8f),
                    new Vector3(4.8f, 0f, 1.8f),
                    new Vector3(4.8f, 0f, -1.8f)
                },
                waveSet);
        }

        private static LevelData CreateOrUpdateLevel3(WaveSetData waveSet)
        {
            return CreateOrUpdateLevel("Level_3.asset", 2, "Level 3: Citadel", 145, 20,
                new Color(0.24f, 0.18f, 0.18f),
                new Vector3(-9.0f, 0f, -5.0f),
                new Vector3(9.0f, 0f, 5.0f),
                new[]
                {
                    new WallBlock { position = new Vector3(-5.2f, 0.75f, -2.25f), scale = new Vector3(1f, 1.5f, 9.5f) },
                    new WallBlock { position = new Vector3(0.0f, 0.75f, 2.25f), scale = new Vector3(1f, 1.5f, 9.5f) },
                    new WallBlock { position = new Vector3(5.2f, 0.75f, -2.25f), scale = new Vector3(1f, 1.5f, 9.5f) }
                },
                new[]
                {
                    new Vector3(-6.5f, 0f, -1.8f),
                    new Vector3(-6.5f, 0f, 1.4f),
                    new Vector3(-3.9f, 0f, -2.0f),
                    new Vector3(-1.3f, 0f, 1.8f),
                    new Vector3(1.3f, 0f, 1.8f),
                    new Vector3(1.3f, 0f, -1.6f),
                    new Vector3(3.9f, 0f, -2.0f),
                    new Vector3(6.5f, 0f, 1.0f)
                },
                waveSet);
        }

        private static LevelData CreateOrUpdateLevel(
            string fileName, int index, string levelName, int gold, int hp, Color groundColor,
            Vector3 spawnPos, Vector3 basePos, WallBlock[] walls, Vector3[] slots, WaveSetData waveSet)
        {
            Directory.CreateDirectory(LevelsDataPath);
            string path = $"{LevelsDataPath}/{fileName}";
            LevelData asset = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.levelIndex = index;
            asset.levelName = levelName;
            asset.startingGold = gold;
            asset.startingBaseHp = hp;
            asset.groundColor = groundColor;
            asset.spawnPosition = spawnPos;
            asset.basePosition = basePos;
            asset.interiorWalls = walls;
            asset.slotPositions = slots;
            asset.waveSet = waveSet;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void UpdateGameplayScene()
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            LevelData fallbackLevel = AssetDatabase.LoadAssetAtPath<LevelData>($"{LevelsDataPath}/Level_1.asset");
            BuildSlot buildSlotPrefab = AssetDatabase.LoadAssetAtPath<BuildSlot>("Assets/_Game/Prefabs/BuildSlot.prefab");
            TMP_FontAsset font = GetDefaultFont();

            GameObject navRoot = GameObject.Find("NavMeshRoot");
            NavMeshSurface surface = navRoot != null ? navRoot.GetComponent<NavMeshSurface>() : null;
            MeshRenderer groundRenderer = null;

            if (navRoot != null)
            {
                for (int i = navRoot.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = navRoot.transform.GetChild(i);
                    if (child.name == "Wall_Left" || child.name == "Wall_Right")
                    {
                        Object.DestroyImmediate(child.gameObject);
                    }
                    else if (child.name == "Ground")
                    {
                        groundRenderer = child.GetComponent<MeshRenderer>();
                    }
                }
            }

            Transform dynWalls = navRoot != null ? navRoot.transform.Find("DynamicWalls") : null;
            if (dynWalls == null && navRoot != null)
            {
                GameObject dw = new GameObject("DynamicWalls");
                dw.transform.SetParent(navRoot.transform, false);
                dynWalls = dw.transform;
            }

            GameObject slotsParent = GameObject.Find("BuildSlots");
            GameObject spawnPoint = GameObject.Find("SpawnPoint");
            GameObject basePoint = GameObject.Find("BasePoint");
            GameObject systems = GameObject.Find("GameSystems");
            Material matWall = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/Materials/Mat_Wall.mat");

            if (systems != null)
            {
                GameManager gm = systems.GetComponent<GameManager>();
                WaveSpawner spawner = systems.GetComponent<WaveSpawner>();
                LevelLoader loader = systems.GetComponent<LevelLoader>();
                if (loader == null) loader = systems.AddComponent<LevelLoader>();

                SerializedObject lso = new SerializedObject(loader);
                lso.FindProperty("fallbackLevel").objectReferenceValue = fallbackLevel;
                lso.FindProperty("navMeshSurface").objectReferenceValue = surface;
                lso.FindProperty("dynamicWallsParent").objectReferenceValue = dynWalls;
                if (slotsParent != null) lso.FindProperty("slotsParent").objectReferenceValue = slotsParent.transform;
                if (spawnPoint != null) lso.FindProperty("spawnPoint").objectReferenceValue = spawnPoint.transform;
                if (basePoint != null) lso.FindProperty("basePoint").objectReferenceValue = basePoint.transform;
                lso.FindProperty("groundRenderer").objectReferenceValue = groundRenderer;
                lso.FindProperty("buildSlotPrefab").objectReferenceValue = buildSlotPrefab;
                lso.FindProperty("wallMaterial").objectReferenceValue = matWall;
                lso.FindProperty("waveSpawner").objectReferenceValue = spawner;
                lso.ApplyModifiedPropertiesWithoutUndo();

                if (gm != null)
                {
                    SerializedObject gso = new SerializedObject(gm);
                    gso.FindProperty("levelLoader").objectReferenceValue = loader;
                    gso.ApplyModifiedPropertiesWithoutUndo();
                }

                ValidateAllLevelsPathing(loader, new[]
                {
                    fallbackLevel,
                    AssetDatabase.LoadAssetAtPath<LevelData>($"{LevelsDataPath}/Level_2.asset"),
                    AssetDatabase.LoadAssetAtPath<LevelData>($"{LevelsDataPath}/Level_3.asset")
                });
            }

            UpdateGameplayUi(font);
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void ValidateAllLevelsPathing(LevelLoader loader, LevelData[] levels)
        {
            if (loader == null || levels == null) return;

            SerializedObject lso = new SerializedObject(loader);
            SerializedProperty fallbackProp = lso.FindProperty("fallbackLevel");
            LevelData originalFallback = fallbackProp.objectReferenceValue as LevelData;

            for (int i = 0; i < levels.Length; i++)
            {
                LevelData lvl = levels[i];
                if (lvl == null) continue;

                fallbackProp.objectReferenceValue = lvl;
                lso.ApplyModifiedPropertiesWithoutUndo();
                loader.LoadActiveLevel();

                NavMeshPath path = new NavMeshPath();
                bool ok = NavMesh.CalculatePath(lvl.spawnPosition, lvl.basePosition, NavMesh.AllAreas, path)
                          && path.status == NavMeshPathStatus.PathComplete;
                if (ok)
                {
                    Debug.Log($"[NavMesh Check] {lvl.levelName}: PathComplete ({path.corners.Length} corners)");
                }
                else
                {
                    Debug.LogError($"[NavMesh Check] {lvl.levelName}: PATH BLOCKED ({path.status})!");
                }
            }

            fallbackProp.objectReferenceValue = originalFallback;
            lso.ApplyModifiedPropertiesWithoutUndo();
            loader.LoadActiveLevel();
        }

        private static void UpdateGameplayUi(TMP_FontAsset font)
        {
            GameObject canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null) return;

            Transform topBar = canvasGo.transform.Find("TopBar");
            HudUI hud = canvasGo.GetComponent<HudUI>();

            if (topBar != null && hud != null)
            {
                SetTopLabelRect(topBar.Find("GoldText"), 20f, 240f);
                SetTopLabelRect(topBar.Find("BaseHpText"), 250f, 510f);
                SetTopLabelRect(topBar.Find("EnemiesText"), 520f, 800f);
                SetTopLabelRect(topBar.Find("WaveText"), 810f, 1080f);

                Transform levelTf = topBar.Find("LevelText");
                TMP_Text levelTmp;
                if (levelTf == null)
                {
                    levelTmp = CreateTmp("LevelText", topBar, font, "Level 1", 30, TextAlignmentOptions.Left);
                }
                else
                {
                    levelTmp = levelTf.GetComponent<TMP_Text>();
                }
                SetTopLabelRect(levelTmp.transform, 1090f, 1560f);

                Transform pauseTf = topBar.Find("PauseButton");
                if (pauseTf != null)
                {
                    RectTransform prt = pauseTf.GetComponent<RectTransform>();
                    prt.pivot = new Vector2(1f, 0.5f);
                    prt.anchorMin = new Vector2(1f, 0.5f);
                    prt.anchorMax = new Vector2(1f, 0.5f);
                    prt.anchoredPosition = new Vector2(-20f, 0f);
                    prt.sizeDelta = new Vector2(160f, 62f);
                }

                Button pauseMenuBtn = EnsureBoxButton(canvasGo.transform.Find("PausePanel/Box"), "MenuButton", font, "Menu", new Vector2(0f, -185f), new Color(0.35f, 0.25f, 0.28f));
                ResizeBox(canvasGo.transform.Find("PausePanel/Box"), 480f, 440f, 120f);

                Button nextLevelBtn = EnsureBoxButton(canvasGo.transform.Find("WinPanel/Box"), "NextLevelButton", font, "Next Level", new Vector2(0f, -15f), new Color(0.20f, 0.58f, 0.30f));
                Button winMenuBtn = EnsureBoxButton(canvasGo.transform.Find("WinPanel/Box"), "MenuButton", font, "Menu", new Vector2(0f, -185f), new Color(0.35f, 0.25f, 0.28f));
                Transform winRestart = canvasGo.transform.Find("WinPanel/Box/RestartButton");
                if (winRestart != null) winRestart.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -100f);
                ResizeBox(canvasGo.transform.Find("WinPanel/Box"), 480f, 440f, 120f);

                Button loseMenuBtn = EnsureBoxButton(canvasGo.transform.Find("LosePanel/Box"), "MenuButton", font, "Menu", new Vector2(0f, -130f), new Color(0.35f, 0.25f, 0.28f));
                ResizeBox(canvasGo.transform.Find("LosePanel/Box"), 480f, 380f, 95f);

                SerializedObject hso = new SerializedObject(hud);
                hso.FindProperty("levelText").objectReferenceValue = levelTmp;
                hso.FindProperty("nextLevelButton").objectReferenceValue = nextLevelBtn;
                hso.FindProperty("pauseMenuButton").objectReferenceValue = pauseMenuBtn;
                hso.FindProperty("winMenuButton").objectReferenceValue = winMenuBtn;
                hso.FindProperty("loseMenuButton").objectReferenceValue = loseMenuBtn;
                hso.ApplyModifiedPropertiesWithoutUndo();
            }

            BuildPanelUI buildPanel = canvasGo.GetComponent<BuildPanelUI>();
            Transform buildRoot = canvasGo.transform.Find("BuildPanelRoot");
            if (buildPanel != null && buildRoot != null)
            {
                RectTransform brt = buildRoot.GetComponent<RectTransform>();
                brt.offsetMin = new Vector2(-470f, 20f);
                brt.offsetMax = new Vector2(470f, 165f);

                Transform manageGroup = buildRoot.Find("ManageGroup");
                if (manageGroup != null)
                {
                    Transform infoTf = manageGroup.Find("TowerInfoText");
                    if (infoTf != null)
                    {
                        RectTransform irt = infoTf.GetComponent<RectTransform>();
                        irt.anchorMin = new Vector2(0f, 0.5f);
                        irt.anchorMax = new Vector2(0f, 1f);
                        irt.offsetMin = new Vector2(28f, 0f);
                        irt.offsetMax = new Vector2(440f, -10f);
                    }

                    Transform statsTf = manageGroup.Find("TowerStatsText");
                    TMP_Text statsTmp = statsTf != null
                        ? statsTf.GetComponent<TMP_Text>()
                        : CreateTmp("TowerStatsText", manageGroup, font, "DMG: 10 | SPD: 2.2/s | RNG: 5.5", 22, TextAlignmentOptions.TopLeft);
                    RectTransform srt = statsTmp.GetComponent<RectTransform>();
                    srt.anchorMin = new Vector2(0f, 0f);
                    srt.anchorMax = new Vector2(0f, 0.52f);
                    srt.offsetMin = new Vector2(28f, 8f);
                    srt.offsetMax = new Vector2(450f, 0f);
                    statsTmp.color = new Color(0.82f, 0.90f, 1f);

                    SerializedObject bpso = new SerializedObject(buildPanel);
                    bpso.FindProperty("towerStatsText").objectReferenceValue = statsTmp;
                    bpso.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        private static void SetTopLabelRect(Transform tf, float left, float right)
        {
            if (tf == null) return;
            RectTransform rt = tf.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.offsetMin = new Vector2(left, 0f);
            rt.offsetMax = new Vector2(right, 0f);
        }

        private static void ResizeBox(Transform boxTf, float width, float height, float titleTopOffset)
        {
            if (boxTf == null) return;
            RectTransform rt = boxTf.GetComponent<RectTransform>();
            rt.offsetMin = new Vector2(-width * 0.5f, -height * 0.5f);
            rt.offsetMax = new Vector2(width * 0.5f, height * 0.5f);
            Transform title = boxTf.Find("Title");
            if (title != null)
            {
                RectTransform trt = title.GetComponent<RectTransform>();
                trt.offsetMin = new Vector2(0f, -titleTopOffset);
                trt.offsetMax = new Vector2(0f, -16f);
            }
        }

        private static Button EnsureBoxButton(Transform boxTf, string btnName, TMP_FontAsset font, string label, Vector2 anchoredPos, Color color)
        {
            if (boxTf == null) return null;
            Transform existing = boxTf.Find(btnName);
            Button btn = existing != null
                ? existing.GetComponent<Button>()
                : CreateButton(btnName, boxTf, font, label, 28, color, out _);

            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(260f, 66f);
            return btn;
        }

        private static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            TMP_FontAsset font = GetDefaultFont();
            LevelData[] levels =
            {
                AssetDatabase.LoadAssetAtPath<LevelData>($"{LevelsDataPath}/Level_1.asset"),
                AssetDatabase.LoadAssetAtPath<LevelData>($"{LevelsDataPath}/Level_2.asset"),
                AssetDatabase.LoadAssetAtPath<LevelData>($"{LevelsDataPath}/Level_3.asset")
            };
            if (levels[0] == null || levels[1] == null || levels[2] == null)
            {
                Debug.LogWarning("One or more LevelData assets failed to load in BuildMainMenuScene!");
            }

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.10f, 0.12f, 0.17f);
            }

            GameObject saveGo = new GameObject("SaveManager");
            SaveManager sm = saveGo.AddComponent<SaveManager>();
            SerializedObject smSo = new SerializedObject(sm);
            SerializedProperty levelsProp = smSo.FindProperty("levels");
            levelsProp.arraySize = levels.Length;
            for (int i = 0; i < levels.Length; i++)
            {
                levelsProp.GetArrayElementAtIndex(i).objectReferenceValue = levels[i];
            }
            smSo.ApplyModifiedPropertiesWithoutUndo();

            GameObject esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();

            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            TMP_Text title = CreateTmp("TitleText", canvasGo.transform, font, "TOWER DEFENSE", 64, TextAlignmentOptions.Center);
            title.fontStyle = FontStyles.Bold;
            RectTransform trt = title.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.5f, 1f);
            trt.anchorMax = new Vector2(0.5f, 1f);
            trt.anchoredPosition = new Vector2(0f, -130f);
            trt.sizeDelta = new Vector2(900f, 100f);

            TMP_Text saveInfo = CreateTmp("SaveInfoText", canvasGo.transform, font, "Unlocked Levels: 1/3", 30, TextAlignmentOptions.Center);
            saveInfo.color = new Color(0.75f, 0.85f, 1f);
            RectTransform srt = saveInfo.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 1f);
            srt.anchorMax = new Vector2(0.5f, 1f);
            srt.anchoredPosition = new Vector2(0f, -210f);
            srt.sizeDelta = new Vector2(700f, 60f);

            Button[] lvlButtons = new Button[levels.Length];
            TMP_Text[] lvlLabels = new TMP_Text[levels.Length];
            for (int i = 0; i < levels.Length; i++)
            {
                lvlButtons[i] = CreateButton($"Btn_Level_{i + 1}", canvasGo.transform, font, $"Level {i + 1}", 30,
                    new Color(0.18f, 0.42f, 0.68f), out lvlLabels[i]);
                RectTransform lrt = lvlButtons[i].GetComponent<RectTransform>();
                lrt.anchorMin = new Vector2(0.5f, 0.5f);
                lrt.anchorMax = new Vector2(0.5f, 0.5f);
                lrt.anchoredPosition = new Vector2((i - 1) * 420f, 10f);
                lrt.sizeDelta = new Vector2(360f, 180f);
            }

            Button resetBtn = CreateButton("ResetProgressButton", canvasGo.transform, font, "Reset Progress", 28,
                new Color(0.65f, 0.22f, 0.24f), out _);
            RectTransform rrt = resetBtn.GetComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0.5f, 0f);
            rrt.anchorMax = new Vector2(0.5f, 0f);
            rrt.anchoredPosition = new Vector2(0f, 110f);
            rrt.sizeDelta = new Vector2(300f, 76f);

            // Confirm Reset Modal
            GameObject confirmPanel = new GameObject("ConfirmResetPanel", typeof(RectTransform), typeof(Image));
            confirmPanel.transform.SetParent(canvasGo.transform, false);
            RectTransform cprt = confirmPanel.GetComponent<RectTransform>();
            cprt.anchorMin = Vector2.zero;
            cprt.anchorMax = Vector2.one;
            cprt.offsetMin = Vector2.zero;
            cprt.offsetMax = Vector2.zero;
            confirmPanel.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.82f);

            GameObject box = new GameObject("Box", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(confirmPanel.transform, false);
            RectTransform bxrt = box.GetComponent<RectTransform>();
            bxrt.anchorMin = new Vector2(0.5f, 0.5f);
            bxrt.anchorMax = new Vector2(0.5f, 0.5f);
            bxrt.sizeDelta = new Vector2(620f, 320f);
            box.GetComponent<Image>().color = new Color(0.14f, 0.17f, 0.23f, 0.98f);

            TMP_Text confirmTitle = CreateTmp("ConfirmText", box.transform, font, "Reset all saved progress?", 36, TextAlignmentOptions.Center);
            RectTransform ctrt = confirmTitle.GetComponent<RectTransform>();
            ctrt.anchorMin = new Vector2(0f, 0.5f);
            ctrt.anchorMax = new Vector2(1f, 1f);
            ctrt.offsetMin = new Vector2(20f, 0f);
            ctrt.offsetMax = new Vector2(-20f, -20f);

            Button yesBtn = CreateButton("ConfirmYesButton", box.transform, font, "Yes, Reset", 28, new Color(0.70f, 0.22f, 0.22f), out _);
            RectTransform yrt = yesBtn.GetComponent<RectTransform>();
            yrt.anchorMin = new Vector2(0.5f, 0.5f);
            yrt.anchorMax = new Vector2(0.5f, 0.5f);
            yrt.anchoredPosition = new Vector2(-130f, -70f);
            yrt.sizeDelta = new Vector2(220f, 72f);

            Button cancelBtn = CreateButton("ConfirmCancelButton", box.transform, font, "Cancel", 28, new Color(0.28f, 0.35f, 0.45f), out _);
            RectTransform crt2 = cancelBtn.GetComponent<RectTransform>();
            crt2.anchorMin = new Vector2(0.5f, 0.5f);
            crt2.anchorMax = new Vector2(0.5f, 0.5f);
            crt2.anchoredPosition = new Vector2(130f, -70f);
            crt2.sizeDelta = new Vector2(220f, 72f);

            confirmPanel.SetActive(false);

            MainMenuUI menuUi = canvasGo.AddComponent<MainMenuUI>();
            SerializedObject mso = new SerializedObject(menuUi);
            SetObjArray(mso.FindProperty("levelButtons"), lvlButtons);
            SetObjArray(mso.FindProperty("levelButtonLabels"), lvlLabels);
            mso.FindProperty("saveInfoText").objectReferenceValue = saveInfo;
            mso.FindProperty("resetProgressButton").objectReferenceValue = resetBtn;
            mso.FindProperty("confirmResetPanel").objectReferenceValue = confirmPanel;
            mso.FindProperty("confirmResetYesButton").objectReferenceValue = yesBtn;
            mso.FindProperty("confirmResetCancelButton").objectReferenceValue = cancelBtn;
            mso.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(MainMenuScenePath));
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        private static TMP_Text CreateTmp(string name, Transform parent, TMP_FontAsset font, string text, float size, TextAlignmentOptions align)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = Color.white;
            return tmp;
        }

        private static Button CreateButton(string name, Transform parent, TMP_FontAsset font, string label, float size, Color bgColor, out TMP_Text outLabel)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = bgColor;
            Button btn = go.GetComponent<Button>();

            outLabel = CreateTmp("Label", go.transform, font, label, size, TextAlignmentOptions.Center);
            RectTransform rt = outLabel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(8f, 6f);
            rt.offsetMax = new Vector2(-8f, -6f);
            return btn;
        }

        private static void SetObjArray<T>(SerializedProperty prop, T[] values) where T : Object
        {
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }
    }
}
