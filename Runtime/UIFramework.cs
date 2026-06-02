using Air.UnityGameCore.Runtime;
using Air.UnityGameCore.Runtime.Event;
using Air.UnityGameCore.Runtime.Resource;

namespace Air.UI
{
    /// <summary>
    /// UI 子系统入口。通过 <see cref="Install"/> 绑定 <see cref="IGameRuntime"/> 后使用 <see cref="Panels"/>。
    /// </summary>
    public sealed class UIFramework
    {
        static UIFramework _current;

        public static UIFramework Current =>
            _current ?? throw new System.InvalidOperationException(
                "[UIFramework] 未安装。请先调用 UIFramework.Install(runtime)。");

        public static bool IsInstalled => _current != null;

        public IGameRuntime Runtime { get; }
        public UIManager Panels { get; }
        public UIPanelNavigator Navigator => Panels.Navigator;
        public EventBus Events => Runtime.Events;

        UIFramework(IGameRuntime runtime)
        {
            Runtime = runtime;
            Panels = new UIManager(runtime.Resources, runtime.Events);
        }

        public static UIFramework Install(IGameRuntime runtime)
        {
            _current = new UIFramework(runtime);
            return _current;
        }

        public static void Uninstall() => _current = null;
    }
}