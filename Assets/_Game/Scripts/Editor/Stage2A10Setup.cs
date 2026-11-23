using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using TD.Core;
using TD.Data;
using TD.Enemies;
using TD.Towers;
using TD.UI;

namespace TD.EditorTools
{
    public static class Stage2A10Setup
    {
        private const string MaterialsPath = "Assets/_Game/Art/Materials";
        private const string EnemiesDataPath = "Assets/_Game/Data/Enemies";
        private const string LevelsDataPath = "Assets/_Game/Data/Levels";
        private const string EnemyPrefabsPath = "Assets/_Game/Prefabs/Enemies";
        private const string TowerPrefabsPath = "Assets/_Game/Prefabs/Towers";
        private const string ScenePath = "Assets/_Game/Scenes/Gameplay.unity";

        [MenuItem("Tools/TD/Build Stage 2 (A10)")]
        public static void BuildStage2A10()
        {
            EnsureLayers("BuildSlot", "Enemy", "Tower", "Wall");

            Material matRunner = CreateOrUpdateMaterial("Mat_Runner.mat", new Color(0.92f, 0.30f, 0.20f), false, false);
            Material matTank = CreateOrUpdateMaterial("Mat_Tank.mat", new Color(0.32f, 0.42f, 0.30f), false, false);
            Material matTankShield = CreateOrUpdateMaterial("Mat_TankShield.mat", new Color(0.25f, 0.72f, 1.00f, 0.38f), true, false);
            Material matFrostMage = CreateOrUpdateMaterial("Mat_FrostMage.mat", new Color(0.20f, 0.38f, 0.92f), false, false);
            Material matMageBeam = CreateOrUpdateMaterial("Mat_MageBeam.mat", new Color(0.35f, 0.92f, 1.00f, 1.00f), false, true);
            Material matFrozenOverlay = CreateOrUpdateMaterial("Mat_FrozenOverlay.mat", new Color(0.45f, 0.90f, 1.00f, 0.45f), true, false);
            Material matRangeRing = CreateOrUpdateMaterial("Mat_RangeRing.mat", new Color(0.30f, 0.92f, 1.00f, 0.90f), false, true);

            UpdateTowerPrefabs(matFrozenOverlay, matRangeRing);
            UpdateSniperRange();
            UpdateGameConfig();

            EnemyData runnerData = CreateOrUpdateEnemyData("Enemy_Runner.asset", "Runner",
                55f, 3.8f, 10, 2, 1.6f, 1.1f, 0.5f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

            EnemyData tankData = CreateOrUpdateEnemyData("Enemy_Tank.asset", "Tank",
                220f, 2.1f, 22, 4, 1.8f, 1.6f, 0.7f, 0.5f, 6.0f, 3.0f, 0.7f, 0f, 0f, 0f);

            EnemyData mageData = CreateOrUpdateEnemyData("Enemy_FrostMage.asset", "Frost Mage",
                105f, 2.6f, 18, 2, 1.7f, 1.4f, 0.5f, 0.6f, 3.2f, 0f, 0f, 6.5f, 60f, 0.8f);

            RunnerEnemy runnerPrefab = CreateRunnerPrefab(runnerData, matRunner);
            TankEnemy tankPrefab = CreateTankPrefab(tankData, matTank, matTankShield);
            FrostMageEnemy magePrefab = CreateFrostMagePrefab(mageData, matFrostMage, matMageBeam);

            WaveSetData waveSet = CreateOrUpdateWaveSet(runnerPrefab, tankPrefab, magePrefab);

            UpdateGameplayScene(waveSet);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Stage 2 (A10) setup complete! Press Play to test.");
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

        private static Material CreateOrUpdateMaterial(string fileName, Color color, bool transparent, bool unlit)
        {
            Directory.CreateDirectory(MaterialsPath);
            string fullPath = $"{MaterialsPath}/{fileName}";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);

            Shader shader = unlit
                ? Shader.Find("Universal Render Pipeline/Unlit")
                : Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, fullPath);
            }
            else if (mat.shader != shader)
            {
                mat.shader = shader;
            }

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            else if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

            if (transparent)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 3000;
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void UpdateTowerPrefabs(Material matFrozenOverlay, Material matRangeRing)
        {
            string[] prefabNames = { "Tower_Gunner.prefab", "Tower_Sniper.prefab", "Tower_Slow.prefab" };
            int towerLayer = LayerMask.NameToLayer("Tower");

            foreach (string prefabName in prefabNames)
            {
                string path = $"{TowerPrefabsPath}/{prefabName}";
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                if (root == null) continue;

                root.layer = towerLayer;

                BoxCollider col = root.GetComponent<BoxCollider>();
                if (col == null) col = root.AddComponent<BoxCollider>();
                col.center = new Vector3(0f, 0.5f, 0f);
                col.size = new Vector3(0.9f, 1.0f, 0.9f);

                Transform frozenTf = root.transform.Find("FrozenVisual");
                GameObject frozenGo;
                if (frozenTf == null)
                {
                    frozenGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    frozenGo.name = "FrozenVisual";
                    frozenGo.transform.SetParent(root.transform, false);
                    Object.DestroyImmediate(frozenGo.GetComponent<Collider>());
                }
                else
                {
                    frozenGo = frozenTf.gameObject;
                }

                frozenGo.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                frozenGo.transform.localScale = new Vector3(1.1f, 1.2f, 1.1f);
                frozenGo.GetComponent<MeshRenderer>().sharedMaterial = matFrozenOverlay;
                frozenGo.SetActive(false);

                Transform rangeTf = root.transform.Find("RangeIndicator");
                GameObject rangeGo = rangeTf != null ? rangeTf.gameObject : new GameObject("RangeIndicator");
                if (rangeTf == null)
                {
                    rangeGo.transform.SetParent(root.transform, false);
                }

                LineRenderer lr = rangeGo.GetComponent<LineRenderer>();
                if (lr == null) lr = rangeGo.AddComponent<LineRenderer>();
                lr.startWidth = 0.09f;
                lr.endWidth = 0.09f;
                lr.useWorldSpace = true;
                lr.loop = true;
                lr.sharedMaterial = matRangeRing;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.enabled = false;

                RangeIndicator rangeComp = rangeGo.GetComponent<RangeIndicator>();
                if (rangeComp == null) rangeComp = rangeGo.AddComponent<RangeIndicator>();

                Tower tower = root.GetComponent<Tower>();
                if (tower != null)
                {
                    SerializedObject so = new SerializedObject(tower);
                    so.FindProperty("frozenVisual").objectReferenceValue = frozenGo;
                    so.FindProperty("rangeIndicator").objectReferenceValue = rangeComp;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void UpdateSniperRange()
        {
            TowerData sniper = AssetDatabase.LoadAssetAtPath<TowerData>("Assets/_Game/Data/Towers/Tower_Sniper.asset");
            if (sniper == null || sniper.levels == null || sniper.levels.Length < 3) return;
            sniper.levels[0].range = 8.5f;
            sniper.levels[1].range = 9.5f;
            sniper.levels[2].range = 10.5f;
            EditorUtility.SetDirty(sniper);
        }

        private static void UpdateGameConfig()
        {
            GameConfig cfg = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/_Game/Data/GameConfig.asset");
            if (cfg == null) return;
            cfg.startingGold = 110;
            cfg.startingBaseHp = 20;
            EditorUtility.SetDirty(cfg);
        }

        private static EnemyData CreateOrUpdateEnemyData(
            string fileName, string enemyName,
            float maxHp, float moveSpeed, int goldReward, int baseDamage,
            float attackRange, float attackCooldown, float attackWindUp,
            float abilityWindUp, float abilityCooldown,
            float shieldDuration, float shieldReduction,
            float detectRange, float detectHalfAngle, float freezeDuration)
        {
            Directory.CreateDirectory(EnemiesDataPath);
            string path = $"{EnemiesDataPath}/{fileName}";
            EnemyData asset = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EnemyData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.enemyName = enemyName;
            asset.maxHp = maxHp;
            asset.moveSpeed = moveSpeed;
            asset.goldReward = goldReward;
            asset.baseDamage = baseDamage;
            asset.attackRange = attackRange;
            asset.attackCooldown = attackCooldown;
            asset.attackWindUpDuration = attackWindUp;
            asset.abilityWindUpDuration = abilityWindUp;
            asset.abilityCooldown = abilityCooldown;
            asset.shieldDuration = shieldDuration;
            asset.shieldDamageReduction = shieldReduction;
            asset.detectRange = detectRange;
            asset.detectHalfAngle = detectHalfAngle;
            asset.freezeDuration = freezeDuration;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static RunnerEnemy CreateRunnerPrefab(EnemyData data, Material mat)
        {
            Directory.CreateDirectory(EnemyPrefabsPath);
            string path = $"{EnemyPrefabsPath}/Enemy_Runner.prefab";

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Enemy_Runner";
            go.layer = LayerMask.NameToLayer("Enemy");
            go.transform.localScale = new Vector3(0.6f, 0.7f, 0.6f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;

            NavMeshAgent agent = go.AddComponent<NavMeshAgent>();
            agent.baseOffset = 1.0f;
            agent.radius = 0.35f;
            agent.height = 1.5f;

            AudioSource audio = go.AddComponent<AudioSource>();
            audio.playOnAwake = false;

            RunnerEnemy enemy = go.AddComponent<RunnerEnemy>();
            SerializedObject so = new SerializedObject(enemy);
            so.FindProperty("data").objectReferenceValue = data;
            so.FindProperty("audioSource").objectReferenceValue = audio;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return saved.GetComponent<RunnerEnemy>();
        }

        private static TankEnemy CreateTankPrefab(EnemyData data, Material mat, Material shieldMat)
        {
            Directory.CreateDirectory(EnemyPrefabsPath);
            string path = $"{EnemyPrefabsPath}/Enemy_Tank.prefab";

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Enemy_Tank";
            go.layer = LayerMask.NameToLayer("Enemy");
            go.transform.localScale = new Vector3(1.0f, 0.85f, 1.0f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;

            NavMeshAgent agent = go.AddComponent<NavMeshAgent>();
            agent.baseOffset = 1.0f;
            agent.radius = 0.5f;
            agent.height = 1.8f;

            GameObject shield = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shield.name = "ShieldVisual";
            shield.transform.SetParent(go.transform, false);
            shield.transform.localPosition = Vector3.zero;
            shield.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
            Object.DestroyImmediate(shield.GetComponent<Collider>());
            shield.GetComponent<MeshRenderer>().sharedMaterial = shieldMat;
            shield.SetActive(false);

            AudioSource audio = go.AddComponent<AudioSource>();
            audio.playOnAwake = false;

            TankEnemy enemy = go.AddComponent<TankEnemy>();
            SerializedObject so = new SerializedObject(enemy);
            so.FindProperty("data").objectReferenceValue = data;
            so.FindProperty("audioSource").objectReferenceValue = audio;
            so.FindProperty("shieldVisual").objectReferenceValue = shield;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return saved.GetComponent<TankEnemy>();
        }

        private static FrostMageEnemy CreateFrostMagePrefab(EnemyData data, Material mat, Material beamMat)
        {
            Directory.CreateDirectory(EnemyPrefabsPath);
            string path = $"{EnemyPrefabsPath}/Enemy_FrostMage.prefab";

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Enemy_FrostMage";
            go.layer = LayerMask.NameToLayer("Enemy");
            go.transform.localScale = new Vector3(0.68f, 0.9f, 0.68f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;

            GameObject hat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hat.name = "MageCrystal";
            hat.transform.SetParent(go.transform, false);
            hat.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            hat.transform.localScale = new Vector3(0.55f, 0.45f, 0.55f);
            Object.DestroyImmediate(hat.GetComponent<Collider>());
            hat.GetComponent<MeshRenderer>().sharedMaterial = beamMat;

            GameObject castOrigin = new GameObject("CastOrigin");
            castOrigin.transform.SetParent(go.transform, false);
            castOrigin.transform.localPosition = new Vector3(0f, 0.9f, 0.35f);

            LineRenderer beam = go.AddComponent<LineRenderer>();
            beam.positionCount = 2;
            beam.startWidth = 0.14f;
            beam.endWidth = 0.08f;
            beam.useWorldSpace = true;
            beam.sharedMaterial = beamMat;
            beam.enabled = false;

            NavMeshAgent agent = go.AddComponent<NavMeshAgent>();
            agent.baseOffset = 1.0f;
            agent.radius = 0.38f;
            agent.height = 1.8f;

            AudioSource audio = go.AddComponent<AudioSource>();
            audio.playOnAwake = false;

            FrostMageEnemy enemy = go.AddComponent<FrostMageEnemy>();
            SerializedObject so = new SerializedObject(enemy);
            so.FindProperty("data").objectReferenceValue = data;
            so.FindProperty("audioSource").objectReferenceValue = audio;
            so.FindProperty("towerLayer").intValue = LayerMask.GetMask("Tower");
            so.FindProperty("wallLayer").intValue = LayerMask.GetMask("Wall");
            so.FindProperty("telegraphBeam").objectReferenceValue = beam;
            so.FindProperty("castOrigin").objectReferenceValue = castOrigin.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return saved.GetComponent<FrostMageEnemy>();
        }

        private static WaveSetData CreateOrUpdateWaveSet(RunnerEnemy runner, TankEnemy tank, FrostMageEnemy mage)
        {
            Directory.CreateDirectory(LevelsDataPath);
            string path = $"{LevelsDataPath}/WaveSet_Level1.asset";
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
                    waveName = "Wave 1: Runners",
                    delayBeforeNextWave = 4.0f,
                    entries = new[]
                    {
                        new WaveEntry { enemyPrefab = runner, count = 5, spawnInterval = 1.35f }
                    }
                },
                new WaveDefinition
                {
                    waveName = "Wave 2: Runners & Tanks",
                    delayBeforeNextWave = 4.5f,
                    entries = new[]
                    {
                        new WaveEntry { enemyPrefab = runner, count = 6, spawnInterval = 1.05f },
                        new WaveEntry { enemyPrefab = tank, count = 2, spawnInterval = 2.0f }
                    }
                },
                new WaveDefinition
                {
                    waveName = "Wave 3: Runners & Frost Mages",
                    delayBeforeNextWave = 4.5f,
                    entries = new[]
                    {
                        new WaveEntry { enemyPrefab = runner, count = 7, spawnInterval = 0.95f },
                        new WaveEntry { enemyPrefab = mage, count = 3, spawnInterval = 1.8f }
                    }
                },
                new WaveDefinition
                {
                    waveName = "Wave 4: Heavy Assault",
                    delayBeforeNextWave = 4.5f,
                    entries = new[]
                    {
                        new WaveEntry { enemyPrefab = tank, count = 4, spawnInterval = 1.8f },
                        new WaveEntry { enemyPrefab = mage, count = 3, spawnInterval = 1.6f }
                    }
                },
                new WaveDefinition
                {
                    waveName = "Wave 5: Final Swarm",
                    delayBeforeNextWave = 0f,
                    entries = new[]
                    {
                        new WaveEntry { enemyPrefab = runner, count = 10, spawnInterval = 0.75f },
                        new WaveEntry { enemyPrefab = tank, count = 4, spawnInterval = 1.6f },
                        new WaveEntry { enemyPrefab = mage, count = 4, spawnInterval = 1.4f }
                    }
                }
            };

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void UpdateGameplayScene(WaveSetData waveSet)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            int wallLayer = LayerMask.NameToLayer("Wall");
            GameObject navRoot = GameObject.Find("NavMeshRoot");
            if (navRoot != null)
            {
                foreach (Transform child in navRoot.transform)
                {
                    if (child.name.StartsWith("Wall_"))
                    {
                        child.gameObject.layer = wallLayer;
                    }
                }
            }

            GameObject systems = GameObject.Find("GameSystems");
            if (systems != null)
            {
                TestWaveSpawner oldSpawner = systems.GetComponent<TestWaveSpawner>();
                if (oldSpawner != null) Object.DestroyImmediate(oldSpawner);

                WaveSpawner spawner = systems.GetComponent<WaveSpawner>();
                if (spawner == null) spawner = systems.AddComponent<WaveSpawner>();

                GameObject spawnPoint = GameObject.Find("SpawnPoint");
                GameObject basePoint = GameObject.Find("BasePoint");

                SerializedObject spSo = new SerializedObject(spawner);
                spSo.FindProperty("waveSet").objectReferenceValue = waveSet;
                if (spawnPoint != null) spSo.FindProperty("spawnPoint").objectReferenceValue = spawnPoint.transform;
                if (basePoint != null) spSo.FindProperty("basePoint").objectReferenceValue = basePoint.transform;
                spSo.ApplyModifiedPropertiesWithoutUndo();
            }

            GameObject canvasGo = GameObject.Find("Canvas");
            if (canvasGo != null)
            {
                Transform topBar = canvasGo.transform.Find("TopBar");
                HudUI hud = canvasGo.GetComponent<HudUI>();
                if (topBar != null && hud != null)
                {
                    Transform waveTf = topBar.Find("WaveText");
                    TMP_Text waveTmp;
                    if (waveTf == null)
                    {
                        GameObject go = new GameObject("WaveText", typeof(RectTransform), typeof(TextMeshProUGUI));
                        go.transform.SetParent(topBar, false);
                        RectTransform rt = go.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(0f, 0f);
                        rt.anchorMax = new Vector2(0f, 1f);
                        rt.offsetMin = new Vector2(1070f, 0f);
                        rt.offsetMax = new Vector2(1380f, 0f);

                        waveTmp = go.GetComponent<TextMeshProUGUI>();
                        TMP_Text refText = topBar.Find("GoldText")?.GetComponent<TMP_Text>();
                        if (refText != null) waveTmp.font = refText.font;
                        waveTmp.text = "Wave: 1/5";
                        waveTmp.fontSize = 34;
                        waveTmp.alignment = TextAlignmentOptions.Left;
                        waveTmp.color = Color.white;
                    }
                    else
                    {
                        waveTmp = waveTf.GetComponent<TMP_Text>();
                    }

                    SerializedObject hudSo = new SerializedObject(hud);
                    hudSo.FindProperty("waveText").objectReferenceValue = waveTmp;
                    hudSo.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
        }
    }
}
