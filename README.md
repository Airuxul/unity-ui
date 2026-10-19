# Unity UI (`com.air.unity-ui`)

[简体中文](README.zh-CN.md)

**Layer L2 UI** — views, view controllers, visual State/Trigger, and editor script codegen. Uses [`com.air.unity-game-core`](../com.air.unity-game-core/README.md) for `ViDelegation`.

## Install

Add to the Unity project `Packages/manifest.json` (adjust the path to your clone):

```json
"com.air.unity-ui": "file:../CustomPackages/packages/com.air.unity-ui"
```

Also add `com.air.unity-game-core` if it is not already present. See the [meta registry](https://github.com/Airuxul/AirUnityPackage/blob/main/config/registry.json) for submodule layout.

## Runtime API

The game creates its own manager and registers `UIViewController<TView>` with a prefab path and `EPanelLayer`. `Open` loads the `UIView`. `Close` destroys that instance.

| Type | Role |
|------|------|
| `UIView` | Page or reusable block on a prefab |
| `UIViewController<TView>` | Session-long controller. `TView` is a `UIView`. `View` holds the current instance |
| `UIComponent` | Shared init / show / hide / dispose hooks |
| `EPanelLayer` | `Normal`, `Pop`, `Top` |
| `UIStateCtrl` / `UITriggerCtrl` | Visual state and trigger actions |

Child `comp_` references are connected in the generated `Bind()` via `AddComponent`. They are not stored as a serialized parent/child list.

## Editor

Select a view component. **Regenerate Designer** rewrites the designer partial. **Bind Fields** assigns the prefixed nodes.

## Dependencies

- `com.air.unity-game-core` 4.0.0+
- `com.unity.textmeshpro` 3.0.6

## Related

- [Unity Game Core](../com.air.unity-game-core/README.md)
- [AirUnityPackage meta docs](https://github.com/Airuxul/AirUnityPackage/blob/main/.cursor/rules/PACKAGE_ARCHITECTURE.md)
