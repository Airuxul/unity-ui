using Air.UnityGameCore.Runtime;
using Air.UnityGameCore.Runtime.Event;
using Air.UnityGameCore.Runtime.Resource;

namespace Air.UI
{
    /// <summary>
    /// UI 瀛愮郴缁熷叆鍙ｃ€傞€氳繃 <see cref="Install"/> 缁戝畾 <see cref="IGameRuntime"/> 鍚庝娇鐢?<see cref="Panels"/>銆?    /// </summary>
    public sealed class UIFramework
    {
        static UIFramework _current;

        public static UIFramework Current =>
            _current ?? throw new System.InvalidOperationException(
                "[UIFramework] 鏈畨瑁呫€傝鍏堣皟鐢?UIFramework.Install(runtime)銆?);

        public static bool IsInstalled => _current != null;

        public IGameRuntime Runtime { get; }
        public UIManager Panels { get; }
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
