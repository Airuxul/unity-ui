# Unity UI (`com.air.unity-ui`) v2

UI panels, component lifecycle, State/Trigger, and editor codegen. Depends on `com.air.unity-game-core` for `GameRuntime` / `EventBus` / resources.

## Quick start

```csharp
using Air.UI;
using Air.UnityGameCore.Runtime;

var entry = GameEntry.CreateWithUI();
entry.Runtime.Events.On("game.ready", () => { });
entry.UI.Panels.ShowPanel(config, showParam);

// Or step by step:
var runtime = GameRuntime.CreateDefault();
var ui = UIFramework.Install(runtime);
```

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

- `UIEvents.PanelShown`
- `UIEvents.PanelClosed`

## Dependencies

- `com.air.unity-game-core` 2.0.0+
- `com.unity.textmeshpro` 3.0.6

## Related

- [Unity Game Core](../com.air.unity-game-core/README.md)
