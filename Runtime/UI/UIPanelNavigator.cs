using System;
using System.Collections.Generic;

namespace Air.UI
{
  /// <summary>Stack-based UI navigation: push hides the previous panel; pop restores it.</summary>
  public sealed class UIPanelNavigator
  {
    readonly UIManager _manager;
    readonly Stack<string> _stack = new();

    public int Count => _stack.Count;

    internal UIPanelNavigator(UIManager manager)
    {
      _manager = manager;
    }

    public void Push(UIPanelConfig config, IUIShowParam showParam = null) =>
      Push(config, showParam, null);

    public void Push(UIPanelConfig config, IUIShowParam showParam, Action<UIPanel> onShown)
    {
      if (config == null)
        throw new ArgumentNullException(nameof(config));

      if (_stack.Count > 0 && _manager.TryGetUIPanel(_stack.Peek(), out var current))
      {
        current.gameObject.SetActive(false);
        current.Hide();
      }

      _stack.Push(config.UIPanelId);
      _manager.ShowPanel(config, showParam, onShown);
    }

    public void Pop()
    {
      if (_stack.Count == 0)
        return;

      var topId = _stack.Pop();
      if (_manager.TryGetUIPanel(topId, out var top))
        _manager.DestoryPanel(top);

      if (_stack.Count > 0 && _manager.TryGetUIPanel(_stack.Peek(), out var previous))
      {
        previous.gameObject.SetActive(true);
        previous.Resume();
      }
    }

    public void Clear()
    {
      while (_stack.Count > 0)
        Pop();
    }
  }
}
