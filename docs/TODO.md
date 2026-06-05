# TODO — `com.air.unity-ui`

**Last Updated:** 2026-06-03 · **Owner:** package maintainers · **Scope:** UI layer follow-ups on **existing** APIs (English)

> **User doc (Chinese):** [../TODO.zh-CN.md](../TODO.zh-CN.md)

> **Boundary:** `UIFramework`, `UIManager`, `UIPanelNavigator`, `UIScopedEvents`, `GameEntry`.  
> **Out of scope:** Duplicate `EventBus` / `IResManager` / `GameRuntime` implementation.  
> **Meta rollup:** [AirUnityPackage `docs/TODO_ROADMAP.md`](https://github.com/Airuxul/AirUnityPackage/blob/main/docs/TODO_ROADMAP.md)

## Capability baseline

- `GameEntry.CreateWithUI` composes `IGameRuntime` + `UIFramework.Install`
- Panel show/hide via `IResManager.LoadInstanceAsync` / `UnloadRes`
- Layer roots (Normal/Pop/Top), `UIPanelNavigator` stack
- `UIScopedEvents` on shared `Runtime.Events`
- Trigger chain: `UITriggerCtrl`, `AnimationTriggerAction`, `EventTriggerAction`

## TODO

| ID | Pri | Title | Description |
|----|-----|-------|-------------|
| UI-01 | P0 | `Uninstall` teardown | Destroy `UIRoot`, clear `_uiMap`, unload open panels on `Dispose` (today leaks `DontDestroyOnLoad`). |
| UI-02 | P0 | `Hide()` vs destroy | `UIComponent.Hide()` calls `Destory()` on children; breaks navigator hide/resume. |
| UI-03 | P1 | Deferred `DestoryPanel` | Implement `isImmediate: false` close-animation path (empty branch today). |
| UI-04 | P1 | HideUI trigger path | Add `TriggerUIPanelHide`; fix default HideUI wiring (currently mirrors ShowUI / `ShowAfter`). |
| UI-05 | P1 | `ShowPanel` in-flight guard | Prevent parallel async loads racing on same `UIPanelId`. |
| UI-06 | P2 | Navigator vs direct show | Document or enforce stack when `ShowPanel` bypasses `Navigator`. |
| UI-07 | P2 | `UIScopedEvents` multi-listener | Allow multiple handlers per name or document single-handler contract. |
| UI-08 | P2 | Dead config surface | Use or remove `UIPanelConfig.Order` and unused `PresetResolutions`. |
| UI-09 | P3 | Split resolution helpers | Move screen/resolution APIs out of core `UIManager`. |
| UI-10 | P3 | `Destory` → `Destroy` aliases | Breaking rename with obsolete aliases + README/template sync. |

## Do not assign here

| Topic | Owner package |
|-------|----------------|
| `EventBus` implementation | `com.air.unity-game-core` |
| Resource load refcount / AB policy | `com.air.unity-game-core` |
| `GameRuntime` lifecycle | `com.air.unity-game-core` |
| Entities, scenes, save, input | `com.air.unity-game-core` |
| CLI / connector | `com.air.unity-connector` |
