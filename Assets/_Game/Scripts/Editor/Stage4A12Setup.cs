using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TD.Audio;
using TD.Enemies;
using TD.UI;

namespace TD.EditorTools
{
    public static class Stage4A12Setup
    {
        private const string MixerPath = "Assets/_Game/Audio/Mixer/MainAudioMixer.mixer";
        private const string MusicDir = "Assets/_Game/Audio/Music";
        private const string SfxDir = "Assets/_Game/Audio/SFX";
        private const string EnemyPrefabsDir = "Assets/_Game/Prefabs/Enemies";
        private const string MainMenuScenePath = "Assets/_Game/Scenes/MainMenu.unity";
        private const string GameplayScenePath = "Assets/_Game/Scenes/Gameplay.unity";
        private const string WhiteSpritePath = "Assets/_Game/Art/UI_White.png";

        [MenuItem("Tools/TD/Build Stage 4 (A12)")]
        public static void BuildStage4A12()
        {
            AssetDatabase.Refresh();
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (mixer == null)
            {
                Debug.LogError($"MainAudioMixer not found at {MixerPath}");
                return;
            }

            AudioMixerGroup musicGroup = FindGroup(mixer, "Music");
            AudioMixerGroup sfxGroup = FindGroup(mixer, "SFX");
            AudioMixerSnapshot normalSnap = mixer.FindSnapshot("Normal");
            AudioMixerSnapshot pausedSnap = mixer.FindSnapshot("Paused");

            bool hasMasterParam = mixer.GetFloat("MasterVolume", out _);
            bool hasMusicParam = mixer.GetFloat("MusicVolume", out _);
            bool hasSfxParam = mixer.GetFloat("SFXVolume", out _);

            Debug.Log($"[Stage 4 AudioMixer Check] MusicGroup={musicGroup != null}, SfxGroup={sfxGroup != null}, NormalSnap={normalSnap != null}, PausedSnap={pausedSnap != null}, Params=({hasMasterParam}/{hasMusicParam}/{hasSfxParam})");

            AudioClip windUpClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/SFX_WindUp.wav");
            RouteEnemyPrefabs(sfxGroup, windUpClip);

            UpdateMainMenuScene(mixer, musicGroup, sfxGroup, normalSnap, pausedSnap);
            UpdateGameplayScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Stage 4 (A12) setup complete! Open MainMenu.unity and press Play to test audio.");
        }

        private static AudioMixerGroup FindGroup(AudioMixer mixer, string groupName)
        {
            AudioMixerGroup[] groups = mixer.FindMatchingGroups(groupName);
            return groups != null && groups.Length > 0 ? groups[0] : null;
        }

        private static void RouteEnemyPrefabs(AudioMixerGroup sfxGroup, AudioClip windUpClip)
        {
            string[] prefabs = { "Enemy_Runner.prefab", "Enemy_Tank.prefab", "Enemy_FrostMage.prefab" };
            foreach (string file in prefabs)
            {
                string path = $"{EnemyPrefabsDir}/{file}";
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                if (root == null) continue;

                AudioSource src = root.GetComponent<AudioSource>();
                if (src != null)
                {
                    src.outputAudioMixerGroup = sfxGroup;
                    src.playOnAwake = false;
                }

                EnemyBase enemy = root.GetComponent<EnemyBase>();
                if (enemy != null && windUpClip != null)
                {
                    SerializedObject eso = new SerializedObject(enemy);
                    eso.FindProperty("windUpClip").objectReferenceValue = windUpClip;
                    eso.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void UpdateMainMenuScene(
            AudioMixer mixer, AudioMixerGroup musicGroup, AudioMixerGroup sfxGroup,
            AudioMixerSnapshot normalSnap, AudioMixerSnapshot pausedSnap)
        {
            var scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            TMP_FontAsset font = GetDefaultFont();
            Sprite whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(WhiteSpritePath);

            GameObject audioGo = GameObject.Find("AudioManager");
            if (audioGo == null) audioGo = new GameObject("AudioManager");
            AudioManager am = audioGo.GetComponent<AudioManager>();
            if (am == null) am = audioGo.AddComponent<AudioManager>();

            SerializedObject amSo = new SerializedObject(am);
            amSo.FindProperty("mainMixer").objectReferenceValue = mixer;
            amSo.FindProperty("musicGroup").objectReferenceValue = musicGroup;
            amSo.FindProperty("sfxGroup").objectReferenceValue = sfxGroup;
            amSo.FindProperty("normalSnapshot").objectReferenceValue = normalSnap;
            amSo.FindProperty("pausedSnapshot").objectReferenceValue = pausedSnap;
            amSo.FindProperty("menuMusic").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{MusicDir}/Music_Menu.wav");
            amSo.FindProperty("battleMusic").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{MusicDir}/Music_Battle.wav");
            amSo.FindProperty("sfxBuild").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/SFX_Build.wav");
            amSo.FindProperty("sfxUpgrade").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/SFX_Upgrade.wav");
            amSo.FindProperty("sfxSell").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/SFX_Sell.wav");
            amSo.FindProperty("sfxShootGunner").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/SFX_Shoot_Gunner.wav");
            amSo.FindProperty("sfxShootSniper").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/SFX_Shoot_Sniper.wav");
            amSo.FindProperty("sfxShootSlow").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/SFX_Shoot_Slow.wav");
            amSo.FindProperty("sfxHit").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/SFX_Hit.wav");
            amSo.FindProperty("sfxPickup").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/SFX_Pickup.wav");
            amSo.FindProperty("sfxWindUp").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/SFX_WindUp.wav");
            amSo.FindProperty("sfxDeath").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/SFX_Death.wav");
            amSo.FindProperty("sfxFreeze").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/SFX_Freeze.wav");
            amSo.FindProperty("sfxUIClick").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/SFX_UIClick.wav");
            amSo.FindProperty("sfxWin").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/SFX_Win.wav");
            amSo.FindProperty("sfxLose").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/SFX_Lose.wav");
            amSo.ApplyModifiedPropertiesWithoutUndo();

            GameObject canvasGo = GameObject.Find("Canvas");
            if (canvasGo != null)
            {
                MainMenuUI menuUi = canvasGo.GetComponent<MainMenuUI>();

                Transform resetTf = canvasGo.transform.Find("ResetProgressButton");
                if (resetTf != null)
                {
                    resetTf.GetComponent<RectTransform>().anchoredPosition = new Vector2(-180f, 110f);
                }

                Transform settingsBtnTf = canvasGo.transform.Find("SettingsButton");
                Button settingsBtn = settingsBtnTf != null
                    ? settingsBtnTf.GetComponent<Button>()
                    : CreateButton("SettingsButton", canvasGo.transform, font, "Settings", 28, new Color(0.24f, 0.34f, 0.48f), out _);
                RectTransform sbrt = settingsBtn.GetComponent<RectTransform>();
                sbrt.anchorMin = new Vector2(0.5f, 0f);
                sbrt.anchorMax = new Vector2(0.5f, 0f);
                sbrt.anchoredPosition = new Vector2(180f, 110f);
                sbrt.sizeDelta = new Vector2(300f, 76f);

                Transform oldSettingsPanel = canvasGo.transform.Find("SettingsPanel");
                if (oldSettingsPanel != null) Object.DestroyImmediate(oldSettingsPanel.gameObject);

                GameObject settingsPanel = CreateOverlayModal("SettingsPanel", canvasGo.transform, new Vector2(680f, 480f), out Transform box);
                CreateTmp("Title", box, font, "AUDIO SETTINGS", 40, TextAlignmentOptions.Center,
                    new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -90f), new Vector2(0f, -15f));

                BuildSlidersBlock(box, font, whiteSprite, 85f, 95f, out _);

                Button closeBtn = CreateButton("CloseButton", box, font, "Close", 28, new Color(0.28f, 0.35f, 0.46f), out _);
                RectTransform crt = closeBtn.GetComponent<RectTransform>();
                crt.anchorMin = new Vector2(0.5f, 0f);
                crt.anchorMax = new Vector2(0.5f, 0f);
                crt.anchoredPosition = new Vector2(0f, 55f);
                crt.sizeDelta = new Vector2(220f, 66f);

                settingsPanel.SetActive(false);

                if (menuUi != null)
                {
                    SerializedObject mso = new SerializedObject(menuUi);
                    mso.FindProperty("settingsButton").objectReferenceValue = settingsBtn;
                    mso.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
                    mso.FindProperty("closeSettingsButton").objectReferenceValue = closeBtn;
                    mso.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        private static void UpdateGameplayScene()
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            TMP_FontAsset font = GetDefaultFont();
            Sprite whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(WhiteSpritePath);

            GameObject canvasGo = GameObject.Find("Canvas");
            if (canvasGo != null)
            {
                Transform pauseBox = canvasGo.transform.Find("PausePanel/Box");
                if (pauseBox != null)
                {
                    RectTransform bxrt = pauseBox.GetComponent<RectTransform>();
                    bxrt.offsetMin = new Vector2(-320f, -310f);
                    bxrt.offsetMax = new Vector2(320f, 310f);

                    Transform title = pauseBox.Find("Title");
                    if (title != null)
                    {
                        RectTransform trt = title.GetComponent<RectTransform>();
                        trt.offsetMin = new Vector2(0f, -95f);
                        trt.offsetMax = new Vector2(0f, -15f);
                    }

                    Transform oldAudioBlock = pauseBox.Find("AudioSliders");
                    if (oldAudioBlock != null) Object.DestroyImmediate(oldAudioBlock.gameObject);

                    BuildSlidersBlock(pauseBox, font, whiteSprite, 150f, 70f, out _);

                    SetBoxBtnPos(pauseBox.Find("PrimaryButton"), new Vector2(0f, -110f));
                    SetBoxBtnPos(pauseBox.Find("RestartButton"), new Vector2(0f, -185f));
                    SetBoxBtnPos(pauseBox.Find("MenuButton"), new Vector2(0f, -260f));
                }
            }

            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void SetBoxBtnPos(Transform btnTf, Vector2 pos)
        {
            if (btnTf == null) return;
            RectTransform rt = btnTf.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(260f, 60f);
        }

        private static void BuildSlidersBlock(
            Transform parent, TMP_FontAsset font, Sprite whiteSprite,
            float topY, float rowStep, out AudioSettingsUI settingsUi)
        {
            GameObject container = new GameObject("AudioSliders", typeof(RectTransform));
            container.transform.SetParent(parent, false);
            RectTransform crt = container.GetComponent<RectTransform>();
            crt.anchorMin = Vector2.zero;
            crt.anchorMax = Vector2.one;
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;

            Slider master = CreateSliderRow("Master", container.transform, font, whiteSprite, topY, out TMP_Text masterVal);
            Slider music = CreateSliderRow("Music", container.transform, font, whiteSprite, topY - rowStep, out TMP_Text musicVal);
            Slider sfx = CreateSliderRow("SFX", container.transform, font, whiteSprite, topY - rowStep * 2f, out TMP_Text sfxVal);

            settingsUi = container.AddComponent<AudioSettingsUI>();
            SerializedObject so = new SerializedObject(settingsUi);
            so.FindProperty("masterSlider").objectReferenceValue = master;
            so.FindProperty("musicSlider").objectReferenceValue = music;
            so.FindProperty("sfxSlider").objectReferenceValue = sfx;
            so.FindProperty("masterValueText").objectReferenceValue = masterVal;
            so.FindProperty("musicValueText").objectReferenceValue = musicVal;
            so.FindProperty("sfxValueText").objectReferenceValue = sfxVal;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Slider CreateSliderRow(
            string label, Transform parent, TMP_FontAsset font, Sprite whiteSprite, float yPos, out TMP_Text valText)
        {
            CreateTmp($"{label}Label", parent, font, label, 26, TextAlignmentOptions.Left,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-260f, yPos - 20f), new Vector2(-130f, yPos + 20f));

            GameObject sliderGo = new GameObject($"{label}Slider", typeof(RectTransform), typeof(Slider));
            sliderGo.transform.SetParent(parent, false);
            RectTransform srt = sliderGo.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0.5f);
            srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(30f, yPos);
            srt.sizeDelta = new Vector2(280f, 32f);

            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(sliderGo.transform, false);
            RectTransform bgrt = bg.GetComponent<RectTransform>();
            bgrt.anchorMin = new Vector2(0f, 0.3f);
            bgrt.anchorMax = new Vector2(1f, 0.7f);
            bgrt.offsetMin = Vector2.zero;
            bgrt.offsetMax = Vector2.zero;
            Image bgImg = bg.GetComponent<Image>();
            bgImg.sprite = whiteSprite;
            bgImg.color = new Color(0.22f, 0.26f, 0.34f);

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            RectTransform fart = fillArea.GetComponent<RectTransform>();
            fart.anchorMin = new Vector2(0f, 0.3f);
            fart.anchorMax = new Vector2(1f, 0.7f);
            fart.offsetMin = Vector2.zero;
            fart.offsetMax = Vector2.zero;

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform frt = fill.GetComponent<RectTransform>();
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero;
            frt.offsetMax = Vector2.zero;
            Image fillImg = fill.GetComponent<Image>();
            fillImg.sprite = whiteSprite;
            fillImg.color = new Color(0.25f, 0.68f, 0.95f);

            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderGo.transform, false);
            RectTransform hart = handleArea.GetComponent<RectTransform>();
            hart.anchorMin = Vector2.zero;
            hart.anchorMax = Vector2.one;
            hart.offsetMin = Vector2.zero;
            hart.offsetMax = Vector2.zero;

            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            RectTransform hrt = handle.GetComponent<RectTransform>();
            hrt.sizeDelta = new Vector2(22f, 34f);
            Image hImg = handle.GetComponent<Image>();
            hImg.sprite = whiteSprite;
            hImg.color = Color.white;

            Slider slider = sliderGo.GetComponent<Slider>();
            slider.minValue = 0.0001f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.fillRect = frt;
            slider.handleRect = hrt;
            slider.targetGraphic = hImg;

            valText = CreateTmp($"{label}Value", parent, font, "100%", 24, TextAlignmentOptions.Right,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(180f, yPos - 20f), new Vector2(265f, yPos + 20f));

            return slider;
        }

        private static GameObject CreateOverlayModal(string name, Transform parent, Vector2 boxSize, out Transform boxOut)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = Vector2.zero;
            prt.anchorMax = Vector2.one;
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.82f);

            GameObject box = new GameObject("Box", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(panel.transform, false);
            RectTransform bxrt = box.GetComponent<RectTransform>();
            bxrt.anchorMin = new Vector2(0.5f, 0.5f);
            bxrt.anchorMax = new Vector2(0.5f, 0.5f);
            bxrt.sizeDelta = boxSize;
            box.GetComponent<Image>().color = new Color(0.14f, 0.17f, 0.23f, 0.98f);
            boxOut = box.transform;
            return panel;
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

        private static TMP_Text CreateTmp(
            string name, Transform parent, TMP_FontAsset font, string text, float size, TextAlignmentOptions align,
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
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = Color.white;
            return tmp;
        }

        private static Button CreateButton(
            string name, Transform parent, TMP_FontAsset font, string label, float size, Color bgColor, out TMP_Text outLabel)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = bgColor;
            Button btn = go.GetComponent<Button>();

            outLabel = CreateTmp("Label", go.transform, font, label, size, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(8f, 6f), new Vector2(-8f, -6f));
            return btn;
        }
    }
}
