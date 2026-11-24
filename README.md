# Tower Defense RK2 (`gmdev`)

3D Tower Defense для Android (Landscape) на **Unity 6000.6.7f1** (Universal 3D / URP).

## Стек и требования
- **Движок:** Unity `6000.6.7f1` (URP) — рекомендуется ставить одинаковую версию на Windows и Mac.
- **Платформа:** Android (APK), ориентация Landscape, цель 30+ FPS.
- **Ввод:** Unity New Input System.
- **Навигация:** AI Navigation (`NavMeshSurface` + `NavMeshAgent`).
- **Архитектура и правила:** [PROJECT_SPEC.md](PROJECT_SPEC.md), [SCRIPT_MAP.md](SCRIPT_MAP.md), [unity-architect.md](unity-architect.md), [DECISIONS.md](DECISIONS.md).

## Синхронизация Windows ↔ Mac (Git)
- В корне настроены `.gitignore` (исключены `Library/`, `UserSettings/`, `Temp/`, `Logs/`, `.utmp/`, сборки) и `.gitattributes` (единый `eol=lf` для текстовых файлов и ассетов Unity).
- На втором ПК (Mac): `git pull` → в Unity Hub выбрать **Add → Add project from disk** → указать папку `gmdev` (Unity сама пересоберет `Library/`).

## Структура `Assets/_Game`
- `Art/` (`Materials`, `Models`, `Shaders`)
- `Audio/` (`Mixer`, `Music`, `SFX`)
- `Data/` (`Towers`, `Enemies`, `Levels`) — `ScriptableObject`
- `Prefabs/` (`Towers`, `Enemies`, `Projectiles`, `VFX`, `UI`)
- `Scenes/`
- `Scripts/` (`Core`, `Towers`, `Enemies`, `Data`, `Save`, `Audio`, `VFX`, `UI`)

## Audio Sources & Licenses (A12)
Все 16 аудиофайлов (2 музыкальных трека + 14 SFX) сгенерированы процедурным PCM-синтезом специально для данного проекта (`22050 Hz, 16-bit mono WAV`).

| Файл | Назначение | Источник / Формат | Лицензия |
|---|---|---|---|
| `Assets/_Game/Audio/Music/Music_Menu.wav` | Фоновая музыка главного меню (8 с луп) | Авторский синтез (Python PCM WAV) | CC0 1.0 |
| `Assets/_Game/Audio/Music/Music_Battle.wav` | Фоновая музыка боя (8 с луп) | Авторский синтез (Python PCM WAV) | CC0 1.0 |
| `Assets/_Game/Audio/SFX/SFX_Build.wav` | Постройка башни | Авторский синтез (Python PCM WAV) | CC0 1.0 |
| `Assets/_Game/Audio/SFX/SFX_Upgrade.wav` | Улучшение башни | Авторский синтез (Python PCM WAV) | CC0 1.0 |
| `Assets/_Game/Audio/SFX/SFX_Sell.wav` | Продажа башни | Авторский синтез (Python PCM WAV) | CC0 1.0 |
| `Assets/_Game/Audio/SFX/SFX_Shoot_Gunner.wav` | Выстрел Gunner | Авторский синтез (Python PCM WAV) | CC0 1.0 |
| `Assets/_Game/Audio/SFX/SFX_Shoot_Sniper.wav` | Выстрел Sniper | Авторский синтез (Python PCM WAV) | CC0 1.0 |
| `Assets/_Game/Audio/SFX/SFX_Shoot_Slow.wav` | Выстрел Slow | Авторский синтез (Python PCM WAV) | CC0 1.0 |
| `Assets/_Game/Audio/SFX/SFX_Hit.wav` | Попадание по врагу | Авторский синтез (Python PCM WAV) | CC0 1.0 |
| `Assets/_Game/Audio/SFX/SFX_Pickup.wav` | Начисление золота за убийство | Авторский синтез (Python PCM WAV) | CC0 1.0 |
| `Assets/_Game/Audio/SFX/SFX_WindUp.wav` | Wind-up врагов перед атакой/способностью | Авторский синтез (Python PCM WAV) | CC0 1.0 |
| `Assets/_Game/Audio/SFX/SFX_Death.wav` | Смерть врага | Авторский синтез (Python PCM WAV) | CC0 1.0 |
| `Assets/_Game/Audio/SFX/SFX_Freeze.wav` | Заморозка башни Магом | Авторский синтез (Python PCM WAV) | CC0 1.0 |
| `Assets/_Game/Audio/SFX/SFX_UIClick.wav` | Клик по UI-кнопкам | Авторский синтез (Python PCM WAV) | CC0 1.0 |
| `Assets/_Game/Audio/SFX/SFX_Win.wav` | Победа на уровне | Авторский синтез (Python PCM WAV) | CC0 1.0 |
| `Assets/_Game/Audio/SFX/SFX_Lose.wav` | Поражение на уровне | Авторский синтез (Python PCM WAV) | CC0 1.0 |
