# 待办 — `com.air.unity-ui`

**最后更新：** 2026-10-18 · **范围：** UI 层现有 API 的后续优化（中文）

> **职责边界：** `UIView`、`UIViewController`、`UIComponent`、`EPanelLayer`、State/Trigger。  
> **不负责：** `GameRuntime` 和资源加载留在 `com.air.unity-game-core`。全局事件和时间轮留在 `com.air.game-core`。  
> Agent 英文条目：[`docs/TODO.md`](docs/TODO.md)

## 现有能力概要

- `UIViewController<TView>` 在 `Open` 时加载 `UIView`，`Close` 时销毁
- `EPanelLayer`：Normal / Pop / Top
- 生成的 `Bind()` 用 `AddComponent` 登记 `comp_` 子组件
- 触发器链：`UITriggerCtrl`、动画/事件动作
- `CountdownText` 用 `ViTimerInstance` 刷新 TMP 倒计时

## 待办列表

当前没有待办。

## 请勿在本包实现

| 主题 | 归属包 |
|------|--------|
| `EventManager` 和时间轮 | `com.air.game-core` |
| 资源加载与 AB 策略 | `com.air.unity-game-core` |
| `GameRuntime` 生命周期 | `com.air.unity-game-core` |
| 实体、场景、存档、输入 | `com.air.unity-game-core` |
| CLI | `com.air.ucp-agent`（本包不依赖；装了才会编译 `ui.generate`） |
