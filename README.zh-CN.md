# Unity UI（`com.air.unity-ui`）

[English](README.md)

**L2 UI 层** — View、ViewController、可视化 State/Trigger 与编辑器脚本生成。通过 [`com.air.unity-game-core`](../com.air.unity-game-core/README.md) 使用 `ViDelegation`。

## 安装

在 Unity 项目 `Packages/manifest.json` 中添加（路径按本地克隆位置调整）：

```json
"com.air.unity-ui": "file:../CustomPackages/packages/com.air.unity-ui"
```

若尚未安装，请同时添加 `com.air.unity-game-core`。子模块目录见 [meta registry](https://github.com/Airuxul/AirUnityPackage/blob/main/config/registry.json)。

## 运行时 API

游戏自己创建管理器，用 Prefab 路径和 `EPanelLayer` 注册 `UIViewController<TView>`。`Open` 加载 `UIView`。`Close` 销毁这一次的实例。

| 类型 | 作用 |
|------|------|
| `UIView` | Prefab 上的页面或可复用块 |
| `UIViewController<TView>` | 整个会话都在的控制器。`TView` 是 `UIView`，`View` 持有当前实例 |
| `UIComponent` | 共用的初始化、显示、隐藏、销毁钩子 |
| `EPanelLayer` | `Normal`、`Pop`、`Top` |
| `UIStateCtrl` / `UITriggerCtrl` | 可视化状态与触发器 |

`comp_` 子组件在生成的 `Bind()` 里用 `AddComponent` 接上，不再序列化父子列表。

## 编辑器

选中 View 组件。**Regenerate Designer** 重写 Designer。**Bind Fields** 按前缀绑定节点。

## ucp-cli（可选）

工程里另外安装了 `com.air.ucp-agent` 时，才会编译 `ui.generate`。本包不依赖命令平台。

```bash
ucp-cli run ui.generate --args "{\"prefab_path\":\"Assets/UI/MyPanel.prefab\",\"output_folder\":\"Assets/Scripts/UI/Panels\",\"class_name\":\"MyPanel\",\"namespace\":\"MyGame.UI\",\"ui_type\":\"panel\"}"
```

命令实现：`Editor/Cli/UiGenerateScriptCommand.cs`（`CliCommandDiscovery` 自动发现）。

## 依赖

- `com.air.unity-game-core` 4.0.0+
- `com.unity.textmeshpro` 3.0.6

## 相关

- [Unity Game Core](../com.air.unity-game-core/README.md)
- [AirUnityPackage 架构说明](https://github.com/Airuxul/AirUnityPackage/blob/main/.cursor/rules/PACKAGE_ARCHITECTURE.md)
