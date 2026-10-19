using System;
using UnityEngine;

namespace Air.UI
{
    public class UIView : UIComponent
    {
        // 窗口被外部销毁时通知 Controller。正常 Close 会先清空，避免再走一遍 Dispose。
        internal Action Destroyed;

        void OnDestroy()
        {
            var callback = Destroyed;
            Destroyed = null;
            callback?.Invoke();
        }

        // 只有 Pop 页面会被盖住。不传给子组件。露出默认再走 Show。
        public void Cover()
        {
            OnCover();
        }

        protected virtual void OnCover()
        {
        }
    }
}
