# Unity UI (`com.air.unity-ui`)

[简体中文](README.zh-CN.md)

**Layer L2 UI** — panels, component lifecycle, visual State/Trigger, and editor script codegen. Uses [`com.air.unity-game-core`](../com.air.unity-game-core/README.md) for `GameRuntime`, `EventBus`, and resource loading (do not duplicate those systems here).

## Install

Add to the Unity project `Packages/manifest.json` (adjust the path to your clone):

```json
"com.air.unity-ui": "file:../CustomPackages/packages/com.air.unity-ui"
```

Also add `com.air.unity-game-core` if it is not already present. See the [meta registry](https://github.com/Airuxul/AirUnityPackage/blob/main/config/registry.json) for submodule layout.

## Quick start (`GameEntry`)

```csharp
using Air.UI;
using Air.UnityGameCore.Runtime;

using var entry = GameEntry.CreateWithUI();
entry.Runtime.Events.On("game.ready", () => { });
entry.UI.Panels.ShowPanel(config, showParam);

// Stack navigation (Normal / Pop / Top layers)
entry.UI.Navigator.Push(settingsConfig, null, panel => { /* loaded */ });
entry.UI.Navigator.Pop();
```

`GameEntry` creates a default `GameRuntime`, calls `UIFramework.Install(runtime)`, and disposes both on `Dispose()`.

## Manual setup (`UIFramework`)

```csharp
var runtime = GameRuntime.CreateDefault();
var ui = UIFramework.Install(runtime);
ui.Panels.ShowPanel(config, showParam);
// ...
UIFramework.Uninstall();
runtime.Dispose();
```

## Runtime API

| Type | Role |
|------|------|
| `GameEntry` | One-shot `GameRuntime` + `UIFramework` |
| `UIFramework` | Subsystem entry: `Panels`, `Navigator`, `Events` (runtime bus) |
| `UIManager` | Root canvas, Normal/Pop/Top layers, show/hide/destroy panels |
| `UIPanel` / `UIComponent` | Panel and widget lifecycle |
| `UIPanelNavigator` | Stack push/pop (hides previous panel) |
| `UIScopedEvents` | Per-component `EventBus` subscriptions with auto cleanup |
| `UIStateCtrl` / `UITriggerCtrl` | Visual state and trigger actions |
| `UIEvents` | Global panel shown/closed event names |

## Scoped UI events

```csharp
public partial class MyPanel : UIPanel
{
    protected override void OnUIInit()
    {
        this.On("custom.event", OnCustom);
    }

    void OnCustom() { }

    protected override void OnUIDestory() => this.ClearEvents();
}
```

## Global UI event names

- `UIEvents.PanelShown` — `ui.panel.shown`
- `UIEvents.PanelClosed` — `ui.panel.closed`

## Editor

- **Window → AirUI → UI Script Generator** — generates panel/component logic from hierarchy
- Context menu shortcuts on UI components (see `UIGeneratorContextMenu`)

## Dependencies

- `com.air.unity-game-core` 4.0.0+
- `com.unity.textmeshpro` 3.0.6

## Related

- [Unity Game Core](../com.air.unity-game-core/README.md)
- [AirUnityPackage meta docs](https://github.com/Airuxul/AirUnityPackage/blob/main/.cursor/rules/PACKAGE_ARCHITECTURE.md)
