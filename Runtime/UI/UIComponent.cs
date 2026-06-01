using System.Collections.Generic;
using Air.UI.State;
using Air.UI.Trigger;
using UnityEngine;

namespace Air.UI
{
    /// <summary>
    /// UI组件基类，提供UI生命周期管理
    /// </summary>
    public abstract class UIComponent : MonoBehaviour, IUIScopedEvents
    {
        [SerializeField] 
        private UIComponent parent;

        public UIComponent Parent
        {
            set => parent = value;
            get => parent;
        }

        [SerializeField]
        protected List<UIComponent> childs = new();

        private List<UIComponent> Childs => childs;

        private bool IsInit { get; set; }
        public bool IsDestoryed { get; set; }

        public IUIShowParam UIShowParam { get; set; }

        protected UITriggerCtrl TriggerCtrl;
        protected UIStateCtrl StateCtrl;

        #region UI生命周期
        
        protected void Init()
        {
            if (IsInit) return;
            IsInit = true;
            IsDestoryed = false;
            InitOtherComponent();
            foreach (var child in Childs)
            {
                child?.Init();
            }
            OnUIInit();
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

        /// <summary>
        /// 页面加载完实例化逻辑
        /// </summary>
        /// <param name="param"></param>
        public void Show(IUIShowParam param = null)
        {
            UIShowParam = param;
            foreach (var child in Childs)
            {
                child?.Show(param);
            }
            OnUIShow(param);
            // 播放显示动画
            TriggerCtrl?.TriggerUIPanelShow();
        }

        /// <summary>
        /// 页面展示动画结束时的回调逻辑
        /// </summary>
        public void ShowAfter()
        {
            foreach (var child in Childs)
            {
                child?.ShowAfter();
            }
            OnUIShowAfter();
        }

        /// <summary>
        /// 页面重新回到最顶上
        /// </summary>
        public void Resume()
        {
            foreach (var child in Childs)
            {
                child?.Show(UIShowParam);
            }
            OnUIShow(UIShowParam);
        }

        /// <summary>
        /// 页面隐藏逻辑
        /// </summary>
        public void Hide()
        {
            OnUIHide();
            foreach (var child in Childs)
            {
                child?.Destory();
            }
            // todo hide动画
        }

        /// <summary>
        /// 销毁页面
        /// </summary>
        public void Destory()
        {
            OnUIDestory();
            this.ClearEvents();
            IsInit = false;
            IsDestoryed = true;
            foreach (var child in Childs)
            {
                child?.Destory();
            }
        }

        /// <summary>
        /// 初始化UI组件
        /// </summary>
        protected virtual void OnUIInit()
        {
            // 子类可以重写此方法进行初始化逻辑
        }

        /// <summary>
        /// 显示UI组件
        /// </summary>
        protected virtual void OnUIShow(IUIShowParam param = null)
        {
            // 子类可以重写此方法进行显示逻辑
        }

        
        /// <summary>
        /// 显示完UI组件后回调
        /// </summary>
        protected virtual void OnUIShowAfter()
        {
            // 子类可以重写此方法进行动画展示完后的逻辑
        }

        /// <summary>
        /// 隐藏UI组件
        /// </summary>
        protected virtual void OnUIHide()
        {
            // 子类可以重写此方法进行隐藏逻辑
        }

        /// <summary>
        /// 销毁UI组件
        /// </summary>
        protected virtual void OnUIDestory()
        {
            
        }
        #endregion

        #region 事件
        UIScopedEvents _scopedEvents;

        public UIScopedEvents ScopedEvents =>
            _scopedEvents ??= new UIScopedEvents(UIFramework.Current.Events);
        #endregion
    }
}
