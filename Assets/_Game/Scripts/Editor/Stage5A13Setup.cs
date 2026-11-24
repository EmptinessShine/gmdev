using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TD.UI;
using TD.VFX;

namespace TD.EditorTools
{
    public static class Stage5A13Setup
    {
        private const string SoftDotPath = "Assets/_Game/Art/VFX_SoftDot.png";
        private const string ParticleMatPath = "Assets/_Game/Art/Materials/Mat_Particle.mat";
        private const string HitFlashShaderPath = "Assets/_Game/Art/Shaders/SG_HitFlash.shadergraph";
        private const string VolumeProfilePath = "Assets/_Game/Art/VFX_VolumeProfile.asset";
        private const string VfxPrefabsDir = "Assets/_Game/Prefabs/VFX";
        private const string EnemyPrefabsDir = "Assets/_Game/Prefabs/Enemies";
        private const string MaterialsDir = "Assets/_Game/Art/Materials";
        private const string GameplayScenePath = "Assets/_Game/Scenes/Gameplay.unity";

        [MenuItem("Tools/TD/Build Stage 5 (A13)")]
        public static void BuildStage5A13()
        {
            AssetDatabase.Refresh();

            Texture2D softDot = EnsureSoftDotTexture();
            Material particleMat = CreateOrUpdateParticleMaterial(softDot);

            ParticleSystem hitFx = CreateParticlePrefab("VFX_Hit.prefab", particleMat, new Color(1.0f, 0.78f, 0.22f, 1f), 8, 0.25f, 0.18f, 4.0f, 0f, 0.12f);
            ParticleSystem pickupFx = CreateParticlePrefab("VFX_Pickup.prefab", particleMat, new Color(1.0f, 0.90f, 0.20f, 1f), 10, 0.55f, 0.22f, 2.2f, -2.5f, 0.20f);
            ParticleSystem deathFx = CreateParticlePrefab("VFX_Death.prefab", particleMat, new Color(1.0f, 0.30f, 0.22f, 1f), 18, 0.45f, 0.30f, 5.5f, 2.0f, 0.25f);
            ParticleSystem buildFx = CreateParticlePrefab("VFX_Build.prefab", particleMat, new Color(0.35f, 0.95f, 1.0f, 1f), 14, 0.40f, 0.26f, 2.8f, -1.0f, 0.60f);
            ParticleSystem freezeFx = CreateParticlePrefab("VFX_Freeze.prefab", particleMat, new Color(0.55f, 0.92f, 1.0f, 1f), 12, 0.45f, 0.26f, 3.5f, 0.5f, 0.35f);
            ParticleSystem winFx = CreateParticlePrefab("VFX_Win.prefab", particleMat, new Color(0.45f, 1.0f, 0.55f, 1f), 45, 1.20f, 0.35f, 7.0f, 4.0f, 0.50f);

            ApplyHitFlashShaderToEnemyMaterials();
            UpdateEnemyPrefabsWithHitFlash();

            VolumeProfile profile = CreateOrUpdateVolumeProfile();

            UpdateGameplayScene(profile, hitFx, pickupFx, deathFx, buildFx, freezeFx, winFx);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Stage 5 (A13) setup complete!");
        }

        private static Texture2D EnsureSoftDotTexture()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SoftDotPath));
            if (!File.Exists(SoftDotPath))
            {
                const int size = 32;
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                float center = (size - 1) * 0.5f;
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = (x - center) / center;
                        float dy = (y - center) / center;
                        float d = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                        float alpha = d * d;
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                }
                tex.Apply();
                File.WriteAllBytes(SoftDotPath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(SoftDotPath, ImportAssetOptions.ForceUpdate);
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(SoftDotPath);
        }

        private static Material CreateOrUpdateParticleMaterial(Texture2D softDot)
        {
            Directory.CreateDirectory(MaterialsDir);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(ParticleMatPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, ParticleMatPath);
            }
            else if (shader != null && mat.shader != shader)
            {
                mat.shader = shader;
            }

            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", softDot);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 2f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static ParticleSystem CreateParticlePrefab(
            string fileName, Material mat, Color startColor,
            int burstCount, float lifetime, float startSize, float startSpeed, float gravity, float radius)
        {
            Directory.CreateDirectory(VfxPrefabsDir);
            string path = $"{VfxPrefabsDir}/{fileName}";

            GameObject go = new GameObject(Path.GetFileNameWithoutExtension(fileName));
            ParticleSystem ps = go.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = ps.main;
            main.duration = lifetime;
            main.loop = false;
            main.startLifetime = lifetime;
            main.startSpeed = startSpeed;
            main.startSize = startSize;
            main.startColor = startColor;
            main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.maxParticles = burstCount + 10;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;

            ParticleSystem.SizeOverLifetimeModule sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

            ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
            psr.sharedMaterial = mat;
            psr.shadowCastingMode = ShadowCastingMode.Off;
            psr.receiveShadows = false;

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return saved.GetComponent<ParticleSystem>();
        }

        private static void ApplyHitFlashShaderToEnemyMaterials()
        {
            Shader hitFlashShader = AssetDatabase.LoadAssetAtPath<Shader>(HitFlashShaderPath);
            if (hitFlashShader == null)
            {
                Debug.LogWarning($"SG_HitFlash shader not found at {HitFlashShaderPath}");
                return;
            }

            string[] matNames = { "Mat_Runner.mat", "Mat_Tank.mat", "Mat_FrostMage.mat" };
            foreach (string matName in matNames)
            {
                string path = $"{MaterialsDir}/{matName}";
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) continue;

                Color baseCol = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;
                mat.shader = hitFlashShader;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseCol);
                if (mat.HasProperty("_FlashColor")) mat.SetColor("_FlashColor", new Color(2f, 2f, 2f, 1f));
                if (mat.HasProperty("_FlashAmount")) mat.SetFloat("_FlashAmount", 0f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.35f);
                EditorUtility.SetDirty(mat);
            }
        }

        private static void UpdateEnemyPrefabsWithHitFlash()
        {
            string[] prefabs = { "Enemy_Runner.prefab", "Enemy_Tank.prefab", "Enemy_FrostMage.prefab" };
            foreach (string file in prefabs)
            {
                string path = $"{EnemyPrefabsDir}/{file}";
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                if (root == null) continue;

                HitFlash hf = root.GetComponent<HitFlash>();
                if (hf == null) hf = root.AddComponent<HitFlash>();

                MeshRenderer mr = root.GetComponent<MeshRenderer>();
                SerializedObject so = new SerializedObject(hf);
                so.FindProperty("targetRenderer").objectReferenceValue = mr;
                so.FindProperty("flashDuration").floatValue = 0.10f;
                so.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static VolumeProfile CreateOrUpdateVolumeProfile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(VolumeProfilePath));
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, VolumeProfilePath);
            }

            if (!profile.TryGet(out Bloom bloom)) bloom = profile.Add<Bloom>(true);
            bloom.active = true;
            bloom.threshold.Override(0.90f);
            bloom.intensity.Override(0.38f);
            bloom.scatter.Override(0.60f);

            if (!profile.TryGet(out ColorAdjustments colorAdj)) colorAdj = profile.Add<ColorAdjustments>(true);
            colorAdj.active = true;
            colorAdj.contrast.Override(10f);
            colorAdj.saturation.Override(14f);

            if (!profile.TryGet(out Vignette vignette)) vignette = profile.Add<Vignette>(true);
            vignette.active = true;
            vignette.intensity.Override(0.22f);
            vignette.smoothness.Override(0.35f);

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void UpdateGameplayScene(
            VolumeProfile profile,
            ParticleSystem hitFx,
            ParticleSystem pickupFx,
            ParticleSystem deathFx,
            ParticleSystem buildFx,
            ParticleSystem freezeFx,
            ParticleSystem winFx)
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            TMP_FontAsset font = GetDefaultFont();

            Camera cam = Camera.main;
            CameraShake shake = null;
            if (cam != null)
            {
                UniversalAdditionalCameraData camData = cam.GetComponent<UniversalAdditionalCameraData>();
                if (camData != null) camData.renderPostProcessing = true;

                shake = cam.GetComponent<CameraShake>();
                if (shake == null) shake = cam.gameObject.AddComponent<CameraShake>();
            }

            GameObject volGo = GameObject.Find("GlobalVolume");
            if (volGo == null) volGo = new GameObject("GlobalVolume");
            Volume volume = volGo.GetComponent<Volume>();
            if (volume == null) volume = volGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;

            GameObject systems = GameObject.Find("GameSystems");
            if (systems != null)
            {
                HitStop hitStop = systems.GetComponent<HitStop>();
                if (hitStop == null) hitStop = systems.AddComponent<HitStop>();

                JuiceManager jm = systems.GetComponent<JuiceManager>();
                if (jm == null) jm = systems.AddComponent<JuiceManager>();

                SerializedObject jso = new SerializedObject(jm);
                jso.FindProperty("postProcessVolume").objectReferenceValue = volume;
                jso.FindProperty("cameraShake").objectReferenceValue = shake;
                jso.FindProperty("hitStop").objectReferenceValue = hitStop;
                jso.FindProperty("hitFxPrefab").objectReferenceValue = hitFx;
                jso.FindProperty("pickupFxPrefab").objectReferenceValue = pickupFx;
                jso.FindProperty("deathFxPrefab").objectReferenceValue = deathFx;
                jso.FindProperty("buildFxPrefab").objectReferenceValue = buildFx;
                jso.FindProperty("freezeFxPrefab").objectReferenceValue = freezeFx;
                jso.FindProperty("winFxPrefab").objectReferenceValue = winFx;
                jso.ApplyModifiedPropertiesWithoutUndo();
            }

            UpdateGameplayHud(font);
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void UpdateGameplayHud(TMP_FontAsset font)
        {
            GameObject canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null) return;

            Transform topBar = canvasGo.transform.Find("TopBar");
            HudUI hud = canvasGo.GetComponent<HudUI>();

            if (topBar != null && hud != null)
            {
                Transform levelTf = topBar.Find("LevelText");
                if (levelTf != null)
                {
                    RectTransform lrt = levelTf.GetComponent<RectTransform>();
                    lrt.offsetMin = new Vector2(1090f, 0f);
                    lrt.offsetMax = new Vector2(1500f, 0f);
                }

                Transform oldJuiceBtn = topBar.Find("JuiceToggleButton");
                if (oldJuiceBtn != null) Object.DestroyImmediate(oldJuiceBtn.gameObject);

                GameObject btnGo = new GameObject("JuiceToggleButton", typeof(RectTransform), typeof(Image), typeof(Button));
                btnGo.transform.SetParent(topBar, false);
                btnGo.GetComponent<Image>().color = new Color(0.18f, 0.52f, 0.32f);
                RectTransform brt = btnGo.GetComponent<RectTransform>();
                brt.pivot = new Vector2(1f, 0.5f);
                brt.anchorMin = new Vector2(1f, 0.5f);
                brt.anchorMax = new Vector2(1f, 0.5f);
                brt.anchoredPosition = new Vector2(-195f, 0f);
                brt.sizeDelta = new Vector2(175f, 62f);

                GameObject lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                lblGo.transform.SetParent(btnGo.transform, false);
                RectTransform lblRt = lblGo.GetComponent<RectTransform>();
                lblRt.anchorMin = Vector2.zero;
                lblRt.anchorMax = Vector2.one;
                lblRt.offsetMin = new Vector2(6f, 4f);
                lblRt.offsetMax = new Vector2(-6f, -4f);
                TextMeshProUGUI lblTmp = lblGo.GetComponent<TextMeshProUGUI>();
                if (font != null) lblTmp.font = font;
                lblTmp.text = "Juice: ON";
                lblTmp.fontSize = 26f;
                lblTmp.alignment = TextAlignmentOptions.Center;
                lblTmp.color = Color.white;

                SerializedObject hso = new SerializedObject(hud);
                hso.FindProperty("juiceToggleButton").objectReferenceValue = btnGo.GetComponent<Button>();
                hso.FindProperty("juiceToggleLabel").objectReferenceValue = lblTmp;
                hso.ApplyModifiedPropertiesWithoutUndo();
            }

            Transform oldFps = canvasGo.transform.Find("FpsCounter");
            if (oldFps != null) Object.DestroyImmediate(oldFps.gameObject);

            GameObject fpsGo = new GameObject("FpsCounter", typeof(RectTransform), typeof(TextMeshProUGUI));
            fpsGo.transform.SetParent(canvasGo.transform, false);
            RectTransform frt = fpsGo.GetComponent<RectTransform>();
            frt.pivot = new Vector2(1f, 0f);
            frt.anchorMin = new Vector2(1f, 0f);
            frt.anchorMax = new Vector2(1f, 0f);
            frt.anchoredPosition = new Vector2(-20f, 16f);
            frt.sizeDelta = new Vector2(170f, 44f);

            TextMeshProUGUI fpsTmp = fpsGo.GetComponent<TextMeshProUGUI>();
            if (font != null) fpsTmp.font = font;
            fpsTmp.text = "FPS: 60";
            fpsTmp.fontSize = 28f;
            fpsTmp.alignment = TextAlignmentOptions.BottomRight;
            fpsTmp.color = new Color(0.45f, 0.95f, 0.50f);

            FpsCounterUI fpsUi = fpsGo.AddComponent<FpsCounterUI>();
            SerializedObject fso = new SerializedObject(fpsUi);
            fso.FindProperty("fpsText").objectReferenceValue = fpsTmp;
            fso.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TMP_FontAsset GetDefaultFont()
        {
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            foreach (string guid in guids)
            {
                TMP_FontAsset candidate = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (candidate != null && candidate.name.Contains("LiberationSans")) return candidate;
            }
            return guids.Length > 0
                ? AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]))
                : TMP_Settings.defaultFontAsset;
        }
    }
}
