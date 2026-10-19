using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Air.UI
{
    public class UIManager
    {
        public static UIManager Current { get; private set; }

        readonly List<Page> pop = new List<Page>();
        readonly List<Page> normal = new List<Page>();
        readonly List<Page> top = new List<Page>();
        readonly Transform normalRoot;
        readonly Transform topRoot;
        readonly Transform popRoot;
        readonly GameObject popBlocker;
        readonly GameObject touchBlocker;
        UIViewController abandonResumed;
        int sequence;

        public UIManager()
        {
            Current = this;
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            var root = new GameObject("UIRoot", typeof(UIEscapeInput)).transform;
            normalRoot = CreateLayer(root, "Normal", 0);
            topRoot = CreateLayer(root, "Top", 100);
            popRoot = CreateLayer(root, "Pop", 200);
            popBlocker = CreateBlocker(popRoot, "PopBlocker");
            var touchRoot = CreateLayer(root, "TouchBlocker", 300);
            touchBlocker = CreateBlocker(touchRoot, "Blocker");
            UIViewLoader.Load = Load;
        }

        // 不可点时挡住全部层级，包括当前 Pop。
        public void SetUITouchable(bool touchable)
        {
            touchBlocker.SetActive(!touchable);
        }

        public void CloseAll()
        {
            CloseLayer(EPanelLayer.Pop);
            CloseLayer(EPanelLayer.Top);
            CloseLayer(EPanelLayer.Normal);
        }

        public void CloseLayer(EPanelLayer layer)
        {
            var list = ListOf(layer);
            var closing = new UIViewController[list.Count];
            for (int i = 0; i < list.Count; i++)
                closing[i] = list[i].Controller;
            for (int i = closing.Length - 1; i >= 0; i--)
                closing[i].Close();
        }

        // 只登记路径、层级和 Order，不加载 Prefab。
        protected void Register<TView>(UIViewController<TView> controller, string prefabPath, EPanelLayer layer, int order) where TView : UIView
        {
            controller.Setup(prefabPath, layer, order);
        }

        // Controller.Open 的入口。Pop 走栈，Normal 和 Top 按层保留。
        internal void Present(UIViewController controller, IShowParam param)
        {
            if (controller == null || !controller.CanOpen(param)) return;
            if (controller.Layer == EPanelLayer.Pop)
            {
                PresentPop(controller, param);
                return;
            }
            PresentKept(ListOf(controller.Layer), controller, param);
        }

        // Controller.Close 的入口。关掉的是 Pop 栈顶时，下一页用留下的参数 OnResume。
        internal void Dismiss(UIViewController controller)
        {
            if (controller == null || controller.Life != ViewLife.Shown) return;
            var list = ListOf(controller.Layer);
            int index = Find(list, controller);
            if (index < 0) return;
            Page resume = null;
            if (controller.Layer == EPanelLayer.Pop && index == list.Count - 1 && index > 0)
                resume = list[index - 1];
            list.RemoveAt(index);
            controller.ApplyClose();
            if (resume != null && Find(list, resume.Controller) >= 0)
                resume.Controller.ApplyResume(resume.Param);
            Arrange(controller.Layer);
            if (controller.Layer == EPanelLayer.Pop)
                popBlocker.SetActive(pop.Count > 0);
        }

        // 窗口被外部销毁后的记账，不再销毁窗口。只有关掉的是已经 Shown 的栈顶时，下一页才 OnResume。
        internal void Abandon(UIViewController controller, bool resumeCovered)
        {
            if (controller == null) return;
            int index = Find(pop, controller);
            if (index >= 0)
            {
                Page resume = index == pop.Count - 1 && index > 0 ? pop[index - 1] : null;
                pop.RemoveAt(index);
                if (resumeCovered && resume != null && Find(pop, resume.Controller) >= 0 && resume.Controller.Life == ViewLife.Shown)
                {
                    resume.Controller.ApplyResume(resume.Param);
                    abandonResumed = resume.Controller;
                }
                Arrange(EPanelLayer.Pop);
                popBlocker.SetActive(pop.Count > 0);
                return;
            }
            if (Remove(normal, controller)) Arrange(EPanelLayer.Normal);
            if (Remove(top, controller)) Arrange(EPanelLayer.Top);
        }

        // 被盖住时再 Open 不改参数。正是栈顶则换参数并 OnShow。否则先 OnCover 当前栈顶再入栈。
        void PresentPop(UIViewController controller, IShowParam param)
        {
            int index = Find(pop, controller);
            if (index >= 0)
            {
                if (index != pop.Count - 1) return;
                pop[index].Param = param;
                controller.ApplyShow(param);
                return;
            }
            Page covered = pop.Count > 0 ? pop[pop.Count - 1] : null;
            if (covered != null)
                covered.Controller.ApplyCover();
            abandonResumed = null;
            try
            {
                ShowNew(pop, controller, param);
            }
            catch
            {
                RestoreCover(covered, controller);
                popBlocker.SetActive(pop.Count > 0);
                throw;
            }

            RestoreCover(covered, controller);
            popBlocker.SetActive(pop.Count > 0);
        }

        // 新页没有留在栈顶时，把刚盖住的那页露出来。Abandon 已经露过的不再露第二次。
        void RestoreCover(Page covered, UIViewController opened)
        {
            var resumed = abandonResumed;
            abandonResumed = null;
            if (covered == null) return;
            if (Find(pop, opened) >= 0 && opened.Life == ViewLife.Shown) return;
            if (resumed == covered.Controller) return;
            if (Find(pop, covered.Controller) < 0 || covered.Controller.Life != ViewLife.Shown) return;
            covered.Controller.ApplyResume(covered.Param);
        }

        // Normal / Top。已经开着则不关闭，换参数后直接 OnShow。
        void PresentKept(List<Page> list, UIViewController controller, IShowParam param)
        {
            int index = Find(list, controller);
            if (index >= 0)
            {
                list[index].Param = param;
                controller.ApplyShow(param);
                return;
            }
            ShowNew(list, controller, param);
        }

        // 加载失败且没有进入 Shown 时，撤销刚写入的记录。
        void ShowNew(List<Page> list, UIViewController controller, IShowParam param)
        {
            list.Add(new Page(controller, param, sequence++));
            try
            {
                controller.ApplyShow(param);
            }
            catch
            {
                DetachUnshown(list, controller);
                Arrange(controller.Layer);
                throw;
            }

            DetachUnshown(list, controller);
            Arrange(controller.Layer);
        }

        // 没进入 Shown 的记录不能留在层里，否则 Close 永远够不着，View 也会挂在层级下。
        static void DetachUnshown(List<Page> list, UIViewController controller)
        {
            if (controller.Life == ViewLife.Shown && Find(list, controller) >= 0)
                return;
            if (Find(list, controller) >= 0)
                Remove(list, controller);
            if (controller.Life != ViewLife.Shown && controller.Life != ViewLife.Disposed && controller.Life != ViewLife.Hidden)
                controller.DiscardView();
        }

        // 键盘 Esc。栈空时什么都不做。
        internal void CloseTopPop()
        {
            if (pop.Count == 0) return;
            pop[pop.Count - 1].Controller.Close();
        }

        // Pop 的挡板夹在栈顶和下面几页之间。Normal / Top 按 Order 排，相同 Order 后打开的在上。
        void Arrange(EPanelLayer layer)
        {
            int index = 0;
            if (layer == EPanelLayer.Pop)
            {
                int last = pop.Count - 1;
                for (int i = 0; i < last; i++)
                    Place(pop[i], index++);
                popBlocker.transform.SetSiblingIndex(index++);
                if (last >= 0)
                    Place(pop[last], index);
                return;
            }
            var list = ListOf(layer);
            var sorted = new List<Page>(list);
            sorted.Sort(Compare);
            for (int i = 0; i < sorted.Count; i++)
                Place(sorted[i], index++);
        }

        static void Place(Page page, int index)
        {
            var view = page.Controller.ViewTransform;
            if (view != null) view.SetSiblingIndex(index);
        }

        static int Compare(Page a, Page b)
        {
            int order = a.Controller.Order.CompareTo(b.Controller.Order);
            return order != 0 ? order : a.Sequence.CompareTo(b.Sequence);
        }

        List<Page> ListOf(EPanelLayer layer)
        {
            if (layer == EPanelLayer.Pop) return pop;
            if (layer == EPanelLayer.Top) return top;
            return normal;
        }

        static int Find(List<Page> list, UIViewController controller)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i].Controller == controller) return i;
            return -1;
        }

        static bool Remove(List<Page> list, UIViewController controller)
        {
            int index = Find(list, controller);
            if (index < 0) return false;
            list.RemoveAt(index);
            return true;
        }

        UIView Load(string path, EPanelLayer layer)
        {
            var prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
                throw new InvalidOperationException($"找不到界面 Prefab：{path}");
            var parent = layer == EPanelLayer.Pop ? popRoot : layer == EPanelLayer.Top ? topRoot : normalRoot;
            var go = UnityEngine.Object.Instantiate(prefab, parent);
            go.SetActive(true);
            var view = go.GetComponent<UIView>();
            if (view == null)
            {
                UnityEngine.Object.Destroy(go);
                throw new InvalidOperationException($"Prefab {path} 上没有 UIView。");
            }
            return view;
        }

        static Transform CreateLayer(Transform root, string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(root, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return go.transform;
        }

        static GameObject CreateBlocker(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = true;
            go.SetActive(false);
            return go;
        }

        sealed class Page
        {
            public UIViewController Controller { get; }
            public IShowParam Param;
            public int Sequence { get; }

            public Page(UIViewController controller, IShowParam param, int sequence)
            {
                Controller = controller;
                Param = param;
                Sequence = sequence;
            }
        }
    }
}
