# 待办 — `com.air.unity-ui`

**最后更新：** 2026-06-03 · **范围：** UI 层现有 API 的后续优化（中文）

> **职责边界：** `UIFramework`、`UIManager`、`UIPanelNavigator`、`UIScopedEvents`、`GameEntry`。  
> **不负责：** 重复实现 `EventBus` / `IResManager` / `GameRuntime`。  
> Agent 英文条目：[`docs/TODO.md`](docs/TODO.md)

## 现有能力概要

- `GameEntry.CreateWithUI` 组合运行时与 `UIFramework.Install`
- 面板经 `LoadInstanceAsync` / `UnloadRes` 加载卸载
- Normal/Pop/Top 三层与导航栈
- `UIScopedEvents` 复用 `Runtime.Events`
- 触发器链：`UITriggerCtrl`、动画/事件动作

## 待办列表

| ID | 优先级 | 标题 | 说明 |
|----|--------|------|------|
| UI-01 | P0 | `Uninstall` 完整清理 | 销毁 `UIRoot`、清空面板表、卸载资源（当前 `DontDestroyOnLoad` 泄漏）。 |
| UI-02 | P0 | `Hide()` 与销毁 | `Hide()` 误调子节点 `Destory()`，破坏导航栈隐藏/恢复。 |
| UI-03 | P1 | 延迟关闭面板 | 实现 `DestoryPanel(..., isImmediate: false)` 关播动画分支。 |
| UI-04 | P1 | HideUI 触发路径 | 增加隐藏触发；修正 HideUI 默认绑定（勿与 ShowUI 相同）。 |
| UI-05 | P1 | `ShowPanel` 并发防护 | 同一 `UIPanelId` 禁止并行异步加载竞态。 |
| UI-06 | P2 | 导航栈与直显 | 绕过 `Navigator` 直显时的栈策略文档或约束。 |
| UI-07 | P2 | 作用域事件多监听 | 支持同名多处理器或明确单监听契约。 |
| UI-08 | P2 | 无效配置项 | 使用或移除 `Order`、未使用的预设分辨率表。 |
| UI-09 | P3 | 分辨率 API 拆分 | 将屏幕/分辨率调试 API 移出核心 `UIManager`。 |
| UI-10 | P3 | `Destory` 拼写修正 | 提供 `Destroy` 别名与废弃标记，同步 README/模板。 |

## 请勿在本包实现

| 主题 | 归属包 |
|------|--------|
| `EventBus` 实现 | `com.air.unity-game-core` |
| 资源加载与 AB 策略 | `com.air.unity-game-core` |
| `GameRuntime` 生命周期 | `com.air.unity-game-core` |
| 实体、场景、存档、输入 | `com.air.unity-game-core` |
| CLI | `com.air.unity-connector` |
