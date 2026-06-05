# AGENTS — `com.air.unity-ui`

**Last Updated:** 2026-06-02 · **Scope:** canonical agent entry (this repository)

## Package facts

| Field | Value |
|-------|--------|
| **UPM id** | `com.air.unity-ui` |
| **Layer** | L2 UI |
| **Depends on** | `com.air.unity-game-core` (runtime: `GameRuntime`, `EventBus`, `IResManager`) |
| **Must not** | Duplicate `EventBus` or resource loading; no CLI protocol |

## User documentation

| File | Language |
|------|----------|
| [README.md](../README.md) | English |
| [README.zh-CN.md](../README.zh-CN.md) | Chinese |
| [TODO.zh-CN.md](../TODO.zh-CN.md) | Chinese backlog — IDs sync with [TODO.md](TODO.md) |

## Agent documentation

| File | Purpose |
|------|---------|
| [AGENTS.md](AGENTS.md) | This file |
| [DOC_GOVERNANCE.md](DOC_GOVERNANCE.md) | Doc workflow for this repo |
| [CHANGELOG_AGENT.md](CHANGELOG_AGENT.md) | Agent change log |
| [TODO.md](TODO.md) | English optimization backlog |
| [TODO.md](TODO.md) | Optimization backlog (existing features; meta [TODO_ROADMAP](https://github.com/Airuxul/AirUnityPackage/blob/main/docs/TODO_ROADMAP.md)) |

## Code layout

| Path | Contents |
|------|----------|
| `Runtime/` | `GameEntry`, `UIFramework`, `UIManager`, `UIPanel`, `UIScopedEvents`, `UI/State/`, `UI/Trigger/` |
| `Editor/UI/` | Script generator (`UIGeneratorWindow`), serializers, state/trigger inspectors |

Namespace: `Air.UI` (runtime), `Air.UI.Editor` (editor).

## Required reads before doc updates

1. `docs/AGENTS.md`
2. `docs/DOC_GOVERNANCE.md`
3. `README.md`, `README.zh-CN.md`

## Meta repository standards

This package is indexed in [AirUnityPackage](https://github.com/Airuxul/AirUnityPackage) (`config/registry.json`, layer **L2 UI**). When editing C# or layers, also follow:

- [C_SHARP_STANDARDS](https://github.com/Airuxul/AirUnityPackage/blob/main/.cursor/rules/C_SHARP_STANDARDS.md)
- [ARCHITECTURE](https://github.com/Airuxul/AirUnityPackage/blob/main/.cursor/rules/PACKAGE_ARCHITECTURE.md)
- [CONSTRAINTS](https://github.com/Airuxul/AirUnityPackage/blob/main/.cursor/rules/PACKAGE_CONSTRAINTS.md)

Doc skills (`doc-read-index`, `doc-generate-update`) live **only** in the meta repo `.cursor/skills/` — do not add skills under this package.
