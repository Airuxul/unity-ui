using System.Collections.Generic;
using Air.UI.State;
using Air.UI.Trigger;
using UnityEngine;

namespace Air.UI
{
    /// <summary>
    /// UI组件基类，提供UI生命周期管理
    /// </summary>
    public abstract class UIComponent : MonoBehaviour
    {
        UIComponent parent;

        public UIComponent Parent
        {
            set => parent = value;
            get => parent;
        }

        readonly List<UIComponent> childs = new();

        private List<UIComponent> Childs => childs;

        public IShowParam ShowParam { get; set; }

        protected UITriggerCtrl TriggerCtrl;
        protected UIStateCtrl StateCtrl;

        #region UI生命周期

        public void Init()
        {
            InitOtherComponent();
            childs.Clear();
            Bind();
            foreach (var child in Childs)
            {
                child?.Init();
            }

            OnInit();
        }

        public virtual void Bind()
        {
        }

        protected void AddComponent(UIComponent child)
        {
            if (child == null)
                return;
            child.Parent = this;
            if (!childs.Contains(child))
                childs.Add(child);
        }

        private void InitOtherComponent()
        {
            if (TryGetComponent(out StateCtrl))
            {
                // StateCtrl.Init();
            }

            if (TryGetComponent(out TriggerCtrl))
            {
                // TriggerCtrl.Init();
            }
        }

        public void Show(IShowParam param = null)
        {
            ShowParam = param;
            foreach (var child in Childs)
            {
                child?.Show(param);
            }
            OnShow(param);
            TriggerCtrl?.TriggerUIPanelShow();
        }

        public void ShowAfter()
        {
            foreach (var child in Childs)
            {
                child?.ShowAfter();
            }
            OnShowAfter();
        }

        public void Hide()
        {
            OnHide();
            for (int i = 0; i < childs.Count; i++)
                childs[i]?.Hide();
        }

        public void DestroyView()
        {
            OnDispose();
            foreach (var child in Childs)
            {
                child?.DestroyView();
            }
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnShow(IShowParam param = null)
        {
        }

        protected virtual void OnShowAfter()
        {
        }

        protected virtual void OnHide()
        {
        }

        protected virtual void OnDispose()
        {
        }
        #endregion
    }
}
