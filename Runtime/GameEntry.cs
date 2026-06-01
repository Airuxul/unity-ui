using Air.UnityGameCore.Runtime;

namespace Air.UI
{
    public readonly struct GameEntry
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
    }
}
