using System;
using Air.UnityGameCore.Runtime.Event;
using UnityEngine;

namespace Air.UI
{
    public enum ViewLife
    {
        None,
        Inited,
        Shown,
        Hidden,
        Disposed
    }

    public interface IViewController
    {
        void Open(IShowParam param);
        void Close();
        bool IsShow { get; }
    }

    public static class UIViewLoader
    {
        public static Func<string, EPanelLayer, UIView> Load;
    }

    public abstract class UIViewController : IViewController
    {
        public string PrefabPath { get; private set; }
        public EPanelLayer Layer { get; private set; }
        public int Order { get; private set; }
        public ViewLife Life { get; protected set; } = ViewLife.None;
        public bool IsShow => Life == ViewLife.Shown;
        bool _eventsBound;

        public void Setup(string prefabPath, EPanelLayer layer, int order)
        {
            PrefabPath = prefabPath;
            Layer = layer;
            Order = order;
        }

        public virtual bool CanOpen(IShowParam param = null) => Life != ViewLife.Disposed;

        // 交给 UIManager 记录并显示。参数保存在 UIManager 的页面记录里。
        public void Open(IShowParam param)
        {
            if (UIManager.Current == null)
                throw new InvalidOperationException("UIManager 尚未创建。");
            UIManager.Current.Present(this, param);
        }

        // 正常关闭：销毁 View，并从 UIManager 的记录里移除。
        public void Close()
        {
            if (UIManager.Current == null) return;
            UIManager.Current.Dismiss(this);
        }

        // View 已经没了。先摘事件，再结束 Controller，然后让 UIManager 删掉记录。
        public void Dispose()
        {
            if (Life == ViewLife.Disposed) return;
            var wasShown = Life == ViewLife.Shown;
            ReleaseEvents();
            Life = ViewLife.Disposed;
            OnDispose();
            DestroyView();
            if (wasShown)
                NotifyClosed();
            if (UIManager.Current != null)
                UIManager.Current.Abandon(this, wasShown);
        }

        // 第一次进入 Shown 时，先登记事件再 OnShow。已经 Shown 的 Open 只再 OnShow。
        internal void ApplyShow(IShowParam param)
        {
            if (!CanOpen(param)) return;
            var opening = Life != ViewLife.Shown;
            if (!HasView) LoadView();
            if (!CanOpen(param)) return;
            if (opening && Life == ViewLife.Shown) return;
            if (Life != ViewLife.Shown)
            {
                ShowView(param);
                if (Life == ViewLife.Disposed) return;
                if (Life == ViewLife.Shown) return;
                Life = ViewLife.Shown;
                BindEvents();
                if (Life != ViewLife.Shown) return;
                OnShow(param);
                if (Life != ViewLife.Shown) return;
                NotifyOpened();
                return;
            }

            ShowView(param);
            OnShow(param);
        }

        // Pop 被上一页盖住。View 保留，只通知它。
        internal void ApplyCover()
        {
            if (Life != ViewLife.Shown || Layer != EPanelLayer.Pop) return;
            CoverView();
        }

        // 上层 Pop 关掉后露出。默认再走一遍 ShowView，参数是入栈时留下的那份。
        internal void ApplyResume(IShowParam param)
        {
            if (Life != ViewLife.Shown || Layer != EPanelLayer.Pop) return;
            ResumeView(param);
        }

        // 先摘掉 Destroyed，OnDestroy 就不会再进 Dispose。
        internal void ApplyClose()
        {
            if (Life != ViewLife.Shown) return;
            ReleaseEvents();
            OnHide();
            if (Life == ViewLife.Disposed) return;
            HideView();
            if (Life == ViewLife.Disposed) return;
            Life = ViewLife.Hidden;
            DestroyView();
            NotifyClosed();
        }

        internal abstract Transform ViewTransform { get; }

        protected abstract bool HasView { get; }
        protected abstract void LoadView();
        protected abstract void ShowView(IShowParam param);
        protected abstract void HideView();
        protected abstract void CoverView();
        protected virtual void ResumeView(IShowParam param) => ShowView(param);
        protected abstract void DestroyView();
        protected virtual void NotifyOpened() { }
        protected virtual void NotifyClosed() { }

        // 打开中途失败时拆掉已经加载的 View，Controller 还可以再次 Open。
        internal void DiscardView()
        {
            if (Life == ViewLife.Disposed || Life == ViewLife.Shown || Life == ViewLife.Hidden) return;
            ReleaseEvents();
            DestroyView();
            if (Life != ViewLife.Disposed)
                Life = ViewLife.None;
        }

        public virtual void OnInit()
        {
        }

        public virtual void OnShow(IShowParam param = null)
        {
        }

        public virtual void OnShowAfter()
        {
        }

        public virtual void OnHide()
        {
        }

        public virtual void OnDispose()
        {
        }

        protected virtual void RegisterEventCallback()
        {
        }

        protected virtual void UnregisterEventCallback()
        {
        }

        void BindEvents()
        {
            if (_eventsBound) return;
            _eventsBound = true;
            RegisterEventCallback();
        }

        void ReleaseEvents()
        {
            if (!_eventsBound) return;
            _eventsBound = false;
            UnregisterEventCallback();
        }
    }

    public abstract class UIViewController<TView> : UIViewController where TView : UIView
    {
        public TView View { get; private set; }
        public ViDelegation LoadedDelegation = new ViDelegation();
        public ViDelegation OpenedDelegation = new ViDelegation();
        public ViDelegation ClosedDelegation = new ViDelegation();

        internal override Transform ViewTransform => View != null ? View.transform : null;
        protected override bool HasView => View != null;

        protected override void LoadView()
        {
            if (UIViewLoader.Load == null)
                throw new InvalidOperationException("UIViewLoader.Load 尚未设置。");
            var view = UIViewLoader.Load(PrefabPath, Layer);
            View = view as TView;
            if (View == null)
            {
                UnityEngine.Object.Destroy(view.gameObject);
                throw new InvalidOperationException($"Prefab {PrefabPath} 上没有 {typeof(TView).Name}。");
            }
            View.Destroyed = Dispose;
            View.Init();
            if (Life == ViewLife.Disposed) return;
            if (Life == ViewLife.None)
                Life = ViewLife.Inited;
            OnInit();
            if (Life == ViewLife.Disposed) return;
            LoadedDelegation.Invoke();
            LoadedDelegation.End();
        }

        protected override void ShowView(IShowParam param)
        {
            View.gameObject.SetActive(true);
            View.Show(param);
        }

        protected override void HideView()
        {
            if (View != null) View.Hide();
        }

        protected override void CoverView()
        {
            if (View != null) View.Cover();
        }

        protected override void DestroyView()
        {
            if (View == null) return;
            var view = View;
            View = null;
            view.Destroyed = null;
            view.DestroyView();
            UnityEngine.Object.Destroy(view.gameObject);
        }

        protected override void NotifyOpened()
        {
            OpenedDelegation.Invoke();
            OpenedDelegation.End();
        }

        protected override void NotifyClosed()
        {
            ClosedDelegation.Invoke();
            ClosedDelegation.End();
        }
    }
}
