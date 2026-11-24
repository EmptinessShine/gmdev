# Script Map — Tower Defense RK2 (`gmdev`)

Краткая карта архитектуры для защиты проекта (Assignments 10–13).

## Архитектурная схема

```mermaid
flowchart TD
    subgraph Persistent["DontDestroyOnLoad (MainMenu → Gameplay)"]
        SM["SaveManager (save.json)"]
        AM["AudioManager (AudioMixer + Crossfade)"]
    end

    subgraph Data["ScriptableObjects (0 магических чисел)"]
        GC["GameConfig"]
        TD["TowerData (3 уровня)"]
        ED["EnemyData"]
        WS["WaveSetData"]
        LD["LevelData (3 уровня)"]
    end

    subgraph Core["Gameplay Core"]
        LL["LevelLoader (стены, слоты, NavMesh)"]
        GM["GameManager (Gold, BaseHP, GameState)"]
        SS["SlotSelector (New Input System)"]
        BS["BuildSlot (Build / Upgrade / Sell)"]
    end

    subgraph Combat["Towers & Enemies (FSM)"]
        TW["Tower (First-target, LevelVisuals 1..3)"]
        PR["Projectile (урон + замедление)"]
        RI["RangeIndicator (круг радиуса)"]
        WSP["WaveSpawner"]
        EB["EnemyBase (NavMeshAgent + HP + Slow)"]
        RE["RunnerEnemy"]
        TE["TankEnemy"]
        FM["FrostMageEnemy (Raycast + стена)"]
    end

    subgraph JuiceUI["Juice (A13) & UI"]
        JM["JuiceManager (6 VFX + Volume)"]
        HF["HitFlash (Shader Graph + MPB)"]
        CS["CameraShake"]
        HS["HitStop (safe timeScale)"]
        HUD["HudUI + BuildPanelUI + FpsCounterUI"]
    end

    SM --> LL
    SM --> AM
    LD --> LL
    WS --> WSP
    GC --> GM
    TD --> TW
    ED --> EB

    LL --> GM
    SS --> BS
    BS --> TW
    TW --> PR
    TW --> RI
    WSP --> EB
    EB <|-- RE
    EB <|-- TE
    EB <|-- FM
    FM -->|Freeze| TW
    PR -->|Damage/Slow| EB

    EB --> HF
    EB --> JM
    GM --> JM
    BS --> JM
    JM --> CS
    JM --> HS
    GM --> HUD
    BS --> HUD
```

## Таблица Runtime-скриптов (`26` скриптов)

| Папка | Скрипт | Роль |
|---|---|---|
| `Data` | `GameConfig` | Глобальный баланс (процент возврата при продаже `0.7`, дефолты) |
| `Data` | `TowerData` | Уровни башен `1→2→3` (стоимость, урон, скорость, радиус, slow, масштаб) |
| `Data` | `EnemyData` | Параметры врагов (HP, скорость, награда, wind-up, щит, конус обзора Мага) |
| `Data` | `WaveSetData` | Набор волн уровня (`WaveDefinition[]` и `WaveEntry[]`) |
| `Data` | `LevelData` | Описание уровня (стены, слоты, спавн, база, цвет пола, волны) |
| `Save` | `SaveData` | Сериализуемая структура `save.json` (уровни, Best HP, громкость, Juice) |
| `Save` | `SaveManager` | Чтение/запись `save.json` через `JsonUtility` + `try/catch`, сброс прогресса |
| `Audio` | `AudioManager` | `AudioMixer` (dB `Log10*20`), снапшоты `Normal/Paused`, кроссфейд, пул SFX |
| `Core` | `GameManager` | Золото, HP базы, пауза, победа/поражение, переходы между сценами |
| `Core` | `LevelLoader` | Динамическая сборка геометрии уровня и пересчет `NavMeshSurface` |
| `Core` | `BuildSlot` | Слот строительства: `TryBuild`, `TryUpgrade`, `TrySell` (70%), подсветка |
| `Core` | `SlotSelector` | Клик/тач через `Pointer.current` + `EventSystem.RaycastAll` |
| `Towers` | `Tower` | Поиск лидера (`First`), поворот башни, стрельба, `levelVisuals`, заморозка |
| `Towers` | `Projectile` | Полет к цели, нанесение урона и замедления |
| `Towers` | `RangeIndicator` | Отрисовка точного мирового радиуса атаки через `LineRenderer` |
| `Enemies` | `EnemyBase` | Базовый класс на `NavMeshAgent`, HP, замедление, `DistanceTraveled`, события |
| `Enemies` | `RunnerEnemy` | FSM 4 состояния: `Move → WindUp (0.5с) → Attack → Cooldown` |
| `Enemies` | `TankEnemy` | FSM 6 состояний: `ShieldWindUp (0.5с) → ShieldedMove (3с, -70% урона)` |
| `Enemies` | `FrostMageEnemy` | FSM 6 состояний: радиус + угол `±60°` + `Raycast` (стены) + экран → Freeze `0.8с` |
| `Enemies` | `WaveSpawner` | Поочередный спавн волн с паузами между волнами |
| `VFX` | `JuiceManager` | Управление 6 префабами частиц, `Volume`, `CameraShake`, `HitStop`, тумблером |
| `VFX` | `HitFlash` | Белая вспышка при уроне через `MaterialPropertyBlock` (`SG_HitFlash`) |
| `VFX` | `CameraShake` | Затухающая тряска камеры на `unscaledDeltaTime` с восстановлением в `OnDisable` |
| `VFX` | `HitStop` | Микро-пауза `0.05 с` (`Time.timeScale = 0.05`) с безопасным сбросом и учетом паузы |
| `UI` | `MainMenuUI`, `AudioSettingsUI` | Выбор уровня (открыто/закрыто), подтверждение сброса, слайдеры громкости |
| `UI` | `HudUI`, `BuildPanelUI`, `DamagePopup`, `EnemyHealthBar`, `FpsCounterUI` | HUD, статы башни при клике, всплывающий урон, HP-бары, тумблер Juice, FPS |

## Соответствие требованиям курса (A10–A13)
- **A10:** `EnemyBase` + 3 наследника (`RunnerEnemy`, `TankEnemy`, `FrostMageEnemy`), FSM 4–6 состояний, `Physics.Raycast` по слою `Wall` + угол + `IsOnScreen()`, `NavMeshAgent`, видимый и слышимый wind-up `0.5–0.7 с`.
- **A11:** 0 магических чисел баланса, 3 `LevelData` в одной сцене `Gameplay.unity`, `save.json` через `JsonUtility` + `try/catch` в `Application.persistentDataPath`, автосейв после уровня/настроек/`OnApplicationPause(true)`, `Reset Progress` с подтверждением.
- **A12:** `MainAudioMixer` (`Master`, `Music`, `SFX`, снапшоты `Normal` и `Paused` с Lowpass), 14 SFX с pitch `0.92–1.08`, 2 трека музыки с кроссфейдом (`DontDestroyOnLoad`), слайдеры в дБ (`Log10 * 20`), лицензии в `README.md`.
- **A13:** Частицы на 6 событиях, URP Shader Graph `SG_HitFlash` через `MaterialPropertyBlock`, `GlobalVolume` (`Bloom`, `ColorAdjustments`, `Vignette`), `CameraShake`, `HitStop` `0.05 с`, переключатель `Juice: ON/OFF`, счетчик `FPS`.
