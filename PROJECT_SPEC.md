# Tower Defense RK2 — Project Spec

## Tech Stack
- **Engine:** Unity `6000.6.7f1` (Universal 3D / URP, Windows & Mac sync)
- **Platform:** Android (APK, Landscape, 30+ FPS)
- **Input:** Unity New Input System
- **Navigation:** AI Navigation (`NavMeshSurface` + `NavMeshAgent`)
- **Art:** Unity primitives first → upgraded/generated models in Stage 6

## Core Gameplay
- **Genre:** 3D Tower Defense, top-down angled static camera (~55°)
- **Slots:** Fixed build pads on the map
- **Towers (3 types, 3 upgrade levels each):**
  1. Gunner (fast, cheap)
  2. Sniper (long range, high damage)
  3. Slow (slows enemies)
- **Tower Actions:** Build, Upgrade (Lvl 1→2→3), Sell (no moving)
- **Economy:** Gold granted automatically on kill (+ pickup particle & SFX)
- **Win / Lose:** Clear all waves with Base HP > 0 / Base HP reaches 0

## Three Enemy Types (A10)
All inherit from lightweight `EnemyBase`, separate behavior scripts, 3+ FSM states each, visible + audible wind-up (0.3–0.8 s), cooldown, no off-screen attacks:
1. **Runner:** `Move` → `WindUp` (0.5 s at base) → `Attack` → `Cooldown`
2. **Tank:** `Move` → `ShieldWindUp` (0.5 s on hit) → `ShieldedMove` (3 s, ~70% damage reduction) → `AttackWindUp` (0.7 s) → `Attack`
3. **Frost Mage:** `Move` (detects towers via distance + angle + `Physics.Raycast`, walls block LoS, on-screen check) → `CastWindUp` (0.6 s beam + sound) → `CastFreeze` (freezes tower 0.8 s + cooldown) → `AttackBase`

## Course Checklists
- **A10:** 3 unique enemy scripts, 3+ FSM states each, Raycast sight blocked by walls, NavMesh, fair wind-up & cooldown, 2–4 tries balance.
- **A11:** All balance in `ScriptableObject`, 3 levels from `LevelData` in 1 scene, `save.json` (`JsonUtility` in `persistentDataPath`), crash-safe defaults, save on level end / settings / `OnApplicationPause(true)`, Level Select, Reset Progress with confirm panel.
- **A12:** `AudioMixer` (`Master`, `Music`, `SFX`, `Paused` snapshot with lowpass), 8+ SFX with pitch/clip variation, 2 music tracks with crossfade (`DontDestroyOnLoad`), dB sliders (`Log10 * 20`), audio licenses in `README.md`.
- **A13:** Particles on 5+ events, 1 Shader Graph (`MaterialPropertyBlock` hit flash), URP Volume (Bloom, Color Adjustments, Vignette), Screen Shake, Hit Stop (~0.05 s, safe `Time.timeScale` restore), before/after toggle + FPS counter (30+ FPS on Android).

## Roadmap
- [x] **Stage 0:** Unity 6 URP project, Android Landscape, New Input System, AI Navigation, folders, Git, `DECISIONS.md`, `README.md`
- [x] **Stage 1 (MVP):** Arena, NavMesh, Base HP, slots, 3 towers (build/upgrade/sell), HUD, Pause
- [ ] **Stage 2 (A10):** Runner, Tank, Frost Mage, waves, balance
- [ ] **Stage 3 (A11):** ScriptableObjects, 3 `LevelData`, JSON saves, Level Select, Reset
- [ ] **Stage 4 (A12):** AudioMixer, 8+ SFX, 2 music tracks, settings dB sliders, licenses
- [ ] **Stage 5 (A13):** Particles, Shader Graph, URP Volume, Shake, Hit Stop, FPS & profiling
- [ ] **Stage 6:** Replace primitives with upgraded models
