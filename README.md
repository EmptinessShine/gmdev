# Tower Defense RK2 (`gmdev`)

3D Tower Defense для Android (Landscape) на **Unity 6000.6.7f1** (Universal 3D / URP).

## Стек и требования
- **Движок:** Unity `6000.6.7f1` (URP) — рекомендуется ставить одинаковую версию на Windows и Mac.
- **Платформа:** Android (APK), ориентация Landscape, цель 30+ FPS.
- **Ввод:** Unity New Input System.
- **Навигация:** AI Navigation (`NavMeshSurface` + `NavMeshAgent`).
- **Архитектура и правила:** [PROJECT_SPEC.md](PROJECT_SPEC.md), [unity-architect.md](unity-architect.md), [DECISIONS.md](DECISIONS.md).

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
*(Будет заполнено на этапе A12 при добавлении музыкальных треков и SFX.)*
