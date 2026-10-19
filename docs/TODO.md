# TODO — `com.air.unity-ui`

**Last Updated:** 2026-10-18 · **Owner:** package maintainers · **Scope:** UI layer follow-ups on **existing** APIs (English)

> **User doc (Chinese):** [../TODO.zh-CN.md](../TODO.zh-CN.md)

> **Boundary:** `UIView`, `UIViewController`, `UIComponent`, `EPanelLayer`, state/trigger.  
> **Out of scope:** `GameRuntime` and resource loading stay in `com.air.unity-game-core`. Global events and the time wheel stay in `com.air.game-core`.

## Capability baseline

- `UIViewController<TView>` loads a `UIView` on `Open` and destroys it on `Close`
- `EPanelLayer`: Normal / Pop / Top
- Generated `Bind()` registers `comp_` children with `AddComponent`
- Trigger chain: `UITriggerCtrl`, `AnimationTriggerAction`, `EventTriggerAction`
- `CountdownText` formats a TMP label from `ViTimerInstance`

## TODO

当前没有待办。

## Do not assign here

| Topic | Owner package |
|-------|----------------|
| `EventManager` and the time wheel | `com.air.game-core` |
| Resource load refcount / AB policy | `com.air.unity-game-core` |
| `GameRuntime` lifecycle | `com.air.unity-game-core` |
| Entities, scenes, save, input | `com.air.unity-game-core` |
| CLI commands | `com.air.ucp-agent`（本包不依赖；`ui.generate` 仅在安装该包时编译） |
