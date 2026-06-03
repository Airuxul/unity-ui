using System;
using Air.UnityGameCore.Runtime;

namespace Air.UI
{
    public readonly struct GameEntry : IDisposable
    {
        public IGameRuntime Runtime { get; }
        public UIFramework UI { get; }

        GameEntry(IGameRuntime runtime, UIFramework ui)
        {
            Runtime = runtime;
            UI = ui;
        }

        public static GameEntry CreateWithUI(IGameRuntime runtime = null)
        {
            runtime ??= GameRuntime.CreateDefault();
            var ui = UIFramework.Install(runtime);
            return new GameEntry(runtime, ui);
        }

        public void Dispose()
        {
            if (UI != null)
                UIFramework.Uninstall();
            if (Runtime is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
