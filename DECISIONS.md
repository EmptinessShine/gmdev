# Decisions Log

## Stage 0 — Project & Git Setup
- **Просили:** Подготовить пустой Unity URP-проект под Android Landscape, структуру `Assets/_Game`, Git-синхронизацию между Windows и Mac, `README.md` и `DECISIONS.md`.
- **Проблема / уточнение:** Проект был создан во вложенной папке `twrdfns`, а на текущем ПК установлена версия `6000.6.7f1` вместо `6000.6.0f1`.
- **Что изменили:** Перенесли проект в корень репозитория, зафиксировали `6000.6.7f1` в `PROJECT_SPEC.md`, добавили `.gitignore` и `.gitattributes` (LF) для чистой синхронизации Windows ↔ Mac, утвердили Замедляющую башню (Slow) третьей башней.
- **Почему:** Корень репозитория упрощает открытие проекта на обеих ОС, единые окончания строк предотвращают лишние диффы в Git, а выбор третьей башни закрывает дизайн перед Этапом 1.

## Stage 1 — MVP (Arena, Slots, 3 Towers, HUD, Pause)
- **Просили:** Арена с `NavMesh`, база с HP, фиксированные слоты, 3 башни (`Gunner`, `Sniper`, `Slow`) с 3 уровнями и продажей (70%), HUD, пауза, `New Input System`.
- **Проблема / решение:** Unity уже был открыт на проекте без импортированных `TMP Essentials`, поэтому сделали идемпотентный Editor-скрипт `Stage1MvpSetup.cs`, который в один клик импортировал TMP, настроил слои, создал ассеты/префабы/сцену и запек `NavMesh`.
- **Архитектура:** Все числа баланса сразу вынесены в `ScriptableObject` (`GameConfig`, `TowerData`, `EnemyData`), тач/клик работает через `Pointer.current` + `EventSystem.RaycastAll`, в `Tower` и `EnemyBase` заложены хуки под Stage 2 (`ApplyFreeze`, `virtual UpdateBehavior`).
- **Проверка:** Сцена скомпилировалась и прошла ручной тест в Play Mode без ошибок (строительство, апгрейд, продажа, замедление, золото, урон базе, пауза, победа/поражение).

## Stage 2 — A10 (Three Enemy Types, Waves & Balance)
- **Просили:** 3 типа врагов (`RunnerEnemy`, `TankEnemy`, `FrostMageEnemy`) от `EnemyBase`, FSM 3+ состояния, видимый+слышимый wind-up, Raycast-обзор Мага со стенами, 5 волн и фикс резкости.
- **Проблемы плейтеста:** (1) `Mobile_RPAsset` рендерил в `0.8x` (мыло); (2) стартовый баланс позволял пройти 5 волн 4 башнями в центре; (3) выбор ближайшей цели заставлял Gunner'ов переключаться с лидера и пропускать 1-ю волну.
- **Что изменили:** Поставили `RenderScale = 1.0` + `MSAA 2x`; добавили видимый круг радиуса `RangeIndicator` при выборе башни; переключили таргетинг на `First` по `EnemyBase.DistanceTraveled`; сбалансировали золото (`110g`, награды `10/22/18g`), HP (`55/220/105`), дальность Sniper (`8.5/9.5/10.5`) и 1-ю волну (`5` Runner, `1.35с`).
- **Проверка:** Все 3 поведения врагов, радиус башен, фокус первого юнита и волны проверены в Play Mode без ошибок.

## Stage 3 — A11 (Data, 3 Levels, JSON Saves & UX Polish)
- **Просили:** 3 уровня `LevelData` в одной сцене `Gameplay`, `MainMenu` с выбором уровня и подтверждением сброса, `save.json` (`JsonUtility` + `try/catch`), 0 магических чисел, а также HP-бары врагов, цифры урона и характеристики башни при клике.
- **Проблемы / баги:** (1) `EditorSceneManager.OpenScene/NewScene` выгружал `LevelData` до сериализации `SaveManager.levels`; (2) `Destroy(child)` откладывался до конца кадра, поэтому `BuildNavMesh()` запекал невидимые стены Level 1 в уровни 2–3; (3) взаимное избегание `NavMeshAgent` создавало пробки в проходах.
- **Что изменили:** Перезагружаем ассеты после открытия/создания сцен в `Stage3A11Setup.cs`; сразу отключаем и отцепляем старые стены/слоты + вызываем `Physics.SyncTransforms()` перед `BuildNavMesh()` в `LevelLoader.cs`; переключили врагов на `NoObstacleAvoidance`, поставили слоты вплотную к стенам и добавили автопроверку `NavMeshPath` для всех 3 уровней.
- **Проверка:** Все 3 уровня пройдены от `MainMenu` до `VICTORY!` без застреваний, работают HP-бары, цифры урона, статы башен, разблокировка уровней и сброс прогресса.

## Stage 4 — A12 (Sound Pass: AudioMixer, Music Crossfade, 14 SFX)
- **Просили:** `AudioMixer` (`Master`, `Music`, `SFX`, снапшот `Paused` с Lowpass), 8+ SFX с pitch-вариацией (`0.92–1.08`), 2 музыкальных трека с кроссфейдом (`DontDestroyOnLoad` без перезапуска на рестарте), слайдеры в дБ (`Log10 * 20`) с сохранением и лицензии в `README.md`.
- **Проблемы / баги:** (1) ручная настройка микшера и конфликт `System.Object` в `Stage4A12Setup.cs`; (2) `?.` по уничтоженному `AudioManager.Instance` вызывал `MissingReferenceException` при повторном Play Mode.
- **Что изменили:** Сгенерировали 16 CC0 `.wav`-файлов (2 лупа + 14 SFX), настроили `MainAudioMixer.mixer` (`Music/SFX`, `Lowpass Simple`, `Normal/Paused`, exposed-параметры), добавили слайдеры в `MainMenu` и `PausePanel`, маршрутизировали `AudioSource` врагов в `SFX`, добавили сброс статики в `OnDestroy` и `SubsystemRegistration` у всех синглтонов.
- **Проверка:** Все `.meta`-файлы сгенерированы, проект чисто закоммичен для переноса на Mac и перехода к Этапу 5 (A13 — Juice Pass).

## Stage 5 — A13 (Juice Pass: Particles, Shader Graph, URP Volume, Shake, Hit Stop, FPS)
- **Просили:** Частицы на 5+ событиях, 1 URP Shader Graph (`Hit Flash` через `MaterialPropertyBlock`), URP Volume (`Bloom`, `Color Adjustments`, `Vignette`), Screen Shake, Hit Stop (`~0.05 с` с безопасным восстановлением `Time.timeScale`), переключатель «до/после» (`Juice: ON/OFF`) и счетчик FPS.
- **Подход и инструменты:** Сгенерировали нативный URP 17.6 `SG_HitFlash.shadergraph` на базе шаблона Unity 6, собрали 6 префабов частиц (`Hit`, `Pickup`, `Death`, `Build`, `Freeze`, `Win`), а сборку и проверку сцены выполнили прямо в редакторе через `unity` MCP (`0` ошибок компиляции).
- **Безопасность и оптимизация:** `HitFlash` работает через `MaterialPropertyBlock` (без копирования материалов); `HitStop` срабатывает только на тяжелых событиях (смерть Танка, урон базе), уважает `GameState.Paused` и гарантированно сбрасывает `Time.timeScale = 1f` в `OnDisable`/`OnDestroy`.
- **Проверка:** Все эффекты, переключатель `Juice: ON/OFF` (сохраняется в `save.json`), пауза и счетчик FPS протестированы в Play Mode без ошибок.
