# Unity UI (`unity-ui`)

UI 框架：`UIFramework`、`UIManager`、`UIComponent`、`UIScopedEvents`、状态/触发器与编辑器代码生成。

## 安装

```json
"unity-ui": "file:../CustomPackages/packages/unity-ui"
```

依赖：`com.air.unity-game-core` 2.0.0+、`com.unity.textmeshpro` 3.0.6+

## 快速开始

```csharp
using Air.UI;
using Air.UnityGameCore.Runtime;

var (runtime, ui) = GameEntry.CreateWithUI();
ui.Framework.OpenPanel<MyPanel>();
```

## 约定

- 通过 `UIFramework.Install(runtime)` 绑定 `GameRuntime`，不重复实现 EventBus / 资源加载。
- 生成代码命名空间：`Air.UI.Generated`。

## 相关包

- [Unity Game Core](../com.air.unity-game-core/README.md)
