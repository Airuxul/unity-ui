# Unity UI（`com.air.unity-ui`）

[English](README.md)

**L2 UI 层** — UI 面板、组件生命周期、可视化 State/Trigger 与编辑器脚本生成。通过 [`com.air.unity-game-core`](../com.air.unity-game-core/README.md) 使用 `GameRuntime`、`EventBus` 与资源加载（请勿在本包重复实现）。

## 安装

在 Unity 项目 `Packages/manifest.json` 中添加（路径按本地克隆位置调整）：

```json
"com.air.unity-ui": "file:../CustomPackages/packages/com.air.unity-ui"
```

若尚未安装，请同时添加 `com.air.unity-game-core`。子模块目录见 [meta registry](https://github.com/Airuxul/AirUnityPackage/blob/main/config/registry.json)。

## 快速开始（`GameEntry`）

```csharp
using Air.UI;
using Air.UnityGameCore.Runtime;

using var entry = GameEntry.CreateWithUI();
entry.Runtime.Events.On("game.ready", () => { });
entry.UI.Panels.ShowPanel(config, showParam);

// 栈式导航（Normal / Pop / Top 层）
entry.UI.Navigator.Push(settingsConfig, null, panel => { /* 已加载 */ });
entry.UI.Navigator.Pop();
```

`GameEntry` 会创建默认 `GameRuntime`、调用 `UIFramework.Install(runtime)`，并在 `Dispose()` 时一并释放。

## 手动安装（`UIFramework`）

```csharp
var runtime = GameRuntime.CreateDefault();
var ui = UIFramework.Install(runtime);
ui.Panels.ShowPanel(config, showParam);
// ...
UIFramework.Uninstall();
runtime.Dispose();
```

## 运行时 API

| 类型 | 作用 |
|------|------|
| `GameEntry` | 一次性组合 `GameRuntime` + `UIFramework` |
| `UIFramework` | 子系统入口：`Panels`、`Navigator`、`Events`（运行时总线） |
| `UIManager` | UI 根节点、Normal/Pop/Top 层、显示/隐藏/销毁面板 |
| `UIPanel` / `UIComponent` | 面板与控件生命周期 |
| `UIPanelNavigator` | 栈式 Push/Pop（会隐藏上一层） |
| `UIScopedEvents` | 组件级 `EventBus` 订阅与自动清理 |
| `UIStateCtrl` / `UITriggerCtrl` | 可视化状态与触发器 |
| `UIEvents` | 全局面板显示/关闭事件名 |

## 作用域 UI 事件

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

## 全局 UI 事件名

- `UIEvents.PanelShown` — `ui.panel.shown`
- `UIEvents.PanelClosed` — `ui.panel.closed`

## 编辑器

- **Window → AirUI → UI Script Generator** — 从层级生成面板/组件逻辑脚本
- UI 组件右键菜单快捷入口（见 `UIGeneratorContextMenu`）

## 依赖

- `com.air.unity-game-core` 4.0.0+
- `com.unity.textmeshpro` 3.0.6

## 相关

- [Unity Game Core](../com.air.unity-game-core/README.md)
- [AirUnityPackage 架构说明](https://github.com/Airuxul/AirUnityPackage/blob/main/.cursor/rules/PACKAGE_ARCHITECTURE.md)
