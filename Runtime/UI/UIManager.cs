using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Air.UnityGameCore.Runtime.Event;
using Air.UI.Extensions;
using Air.UnityGameCore.Runtime.Resource;
using Object = UnityEngine.Object;

namespace Air.UI
{
    /// <summary>
    /// 分辨率信息
    /// </summary>
    public struct ResolutionInfo
    {
        public readonly string Name;
        public readonly int Width;
        public readonly int Height;
        public readonly bool Fullscreen;

        public ResolutionInfo(string name, int width, int height, bool fullscreen = false)
        {
            Name = name;
            Width = width;
            Height = height;
            Fullscreen = fullscreen;
        }
    }

    public class UIManager
    {
        // UI 配置常量
        private const float ReferenceResolutionWidth = 1920f;
        private const float ReferenceResolutionHeight = 1080f;
        private const float MatchWidthOrHeight = 0.5f;
        
        // 层级排序常量
        private const int PopLayerSortingOrder = 100;
        private const int TopLayerSortingOrder = 200;
        
        // 预设分辨率选项
        private static readonly ResolutionInfo[] PresetResolutions = {
            new("1920x1080 (Full HD)", 1920, 1080),
            new("1280x720 (HD)", 1280, 720),
            new("1600x900", 1600, 900),
            new("2560x1440 (2K)", 2560, 1440),
            new("3840x2160 (4K)", 3840, 2160),
            new("800x600", 800, 600),
            new("1024x768", 1024, 768),
        };
        
        private readonly IResManager _resManager;
        private readonly EventBus _events;
        private readonly Dictionary<string, UIPanel> _uiMap;

        private GameObject _uiRoot;
        private GameObject _popRoot;
        private GameObject _topRoot;
        private CanvasScaler _canvasScaler;
        
        public UIManager(IResManager resManager, EventBus events)
        {
            _resManager = resManager;
            _events = events;
            _uiMap = new Dictionary<string, UIPanel>();

            CreateRoot();
        }

        /// <summary>
        /// 创建 UI 根节点和各层级节点
        /// </summary>
        private void CreateRoot()
        {
            // 创建 UI 根节点
            _uiRoot = new GameObject("UIRoot");
            SetupCanvas(_uiRoot);
            SetupCanvasScaler(_uiRoot);
            _uiRoot.AddComponent<GraphicRaycaster>();
            Object.DontDestroyOnLoad(_uiRoot);
            
            // 创建各层级节点
            _popRoot = CreateChildRoot("PopRoot", _uiRoot.transform, sortingOrder: PopLayerSortingOrder);
            _topRoot = CreateChildRoot("TopRoot", _uiRoot.transform, sortingOrder: TopLayerSortingOrder);
        }

        /// <summary>
        /// 配置 Canvas 组件
        /// </summary>
        private void SetupCanvas(GameObject root)
        {
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
        }

        /// <summary>
        /// 配置 CanvasScaler 组件
        /// </summary>
        private void SetupCanvasScaler(GameObject root)
        {
            _canvasScaler = root.AddComponent<CanvasScaler>();
            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvasScaler.referenceResolution = new Vector2(ReferenceResolutionWidth, ReferenceResolutionHeight);
            _canvasScaler.matchWidthOrHeight = MatchWidthOrHeight;
        }

        /// <summary>
        /// 创建子层级节点
        /// </summary>
        /// <param name="name">节点名称</param>
        /// <param name="parent">父节点</param>
        /// <param name="sortingOrder">渲染排序</param>
        /// <returns>创建的子节点</returns>
        private GameObject CreateChildRoot(string name, Transform parent, int sortingOrder = 0)
        {
            var childRoot = new GameObject(name);
            var rectTransform = childRoot.AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            
            // 设置 RectTransform 充满父节点
            rectTransform.FillParent();

            return childRoot;
        }

        public UIPanel GetUIPanel(string uiPanelId)
        {
            return _uiMap[uiPanelId];
        }
        
        public void ShowPanel(UIPanelConfig uiPanelConfig, IUIShowParam showParam)
        {
            var prefabPath = uiPanelConfig.PrefabPath;
            _resManager.LoadInstanceAsync<GameObject>(prefabPath, go =>
            {
                if (!go.TryGetComponent(out UIPanel uiPanel))
                {
                    Debug.LogError($"cant find UIPanel component, path: {prefabPath}");
                    return;
                }
                SetPanelRoot(uiPanelConfig.UILayer, go);
                var rectTransform = go.GetComponent<RectTransform>();
                rectTransform.FillParent();
                
                uiPanel.Init(uiPanelConfig);
                uiPanel.Show(showParam);
                _uiMap.Add(uiPanelConfig.UIPanelId, uiPanel);
                
                _events.Emit(UIEvents.PanelShown, uiPanelConfig);
            });
        }

        private void SetPanelRoot(EPanelLayer panelLayer, GameObject uiPanelGo)
        {
            GameObject rootGo;
            switch (panelLayer)
            {
                case EPanelLayer.Pop:
                    rootGo = _popRoot;
                    break;
                case EPanelLayer.Top:
                    rootGo = _topRoot;
                    break;
                default:
                    throw new Exception("UILayer is wrong");
            }
            uiPanelGo.transform.SetParent(rootGo.transform);
        }

        public void DestoryPanel(UIPanel uiPanel, bool isImmediate = true)
        {
            var uiPanelConfig = uiPanel.UIPanelConfig;
            _uiMap.Remove(uiPanelConfig.UIPanelId);
            // todo: 这个设计目前还没有太大用处，等待后续有关闭动画的需求再继续实现
            if (isImmediate)
            {
                uiPanel.Destory();
                Object.Destroy(uiPanel.gameObject);
                _resManager.UnloadRes(uiPanelConfig.PrefabPath);
            
                _events.Emit(UIEvents.PanelClosed, uiPanelConfig);
            }
        }

        #region 分辨率调整功能

        /// <summary>
        /// 设置屏幕分辨率
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="fullscreen">是否全屏</param>
        private void SetScreenResolution(int width, int height, bool fullscreen = false)
        {
            Screen.SetResolution(width, height, fullscreen);
            Debug.Log($"分辨率已设置为: {width}x{height}, 全屏: {fullscreen}");
        }

        /// <summary>
        /// 设置屏幕分辨率（使用预设配置）
        /// </summary>
        public void SetScreenResolution(ResolutionInfo resolutionInfo)
        {
            SetScreenResolution(resolutionInfo.Width, resolutionInfo.Height, resolutionInfo.Fullscreen);
        }

        /// <summary>
        /// 设置 UI 参考分辨率（不改变屏幕分辨率）
        /// </summary>
        /// <param name="width">参考宽度</param>
        /// <param name="height">参考高度</param>
        public void SetUIReferenceResolution(float width, float height)
        {
            if (_canvasScaler != null)
            {
                _canvasScaler.referenceResolution = new Vector2(width, height);
                Debug.Log($"UI 参考分辨率已设置为: {width}x{height}");
            }
        }

        /// <summary>
        /// 设置 CanvasScaler 的匹配模式
        /// </summary>
        /// <param name="matchWidthOrHeight">0 为匹配宽度，1 为匹配高度，0.5 为平衡</param>
        public void SetMatchWidthOrHeight(float matchWidthOrHeight)
        {
            if (_canvasScaler != null)
            {
                _canvasScaler.matchWidthOrHeight = Mathf.Clamp01(matchWidthOrHeight);
                Debug.Log($"MatchWidthOrHeight 已设置为: {matchWidthOrHeight}");
            }
        }

        /// <summary>
        /// 获取当前屏幕分辨率信息
        /// </summary>
        public string GetCurrentResolutionInfo()
        {
            return $"当前分辨率: {Screen.width}x{Screen.height}, 全屏: {Screen.fullScreen}";
        }

        /// <summary>
        /// 获取 UI 参考分辨率信息
        /// </summary>
        public string GetUIReferenceResolutionInfo()
        {
            if (_canvasScaler != null)
            {
                var refRes = _canvasScaler.referenceResolution;
                return $"UI 参考分辨率: {refRes.x}x{refRes.y}, Match: {_canvasScaler.matchWidthOrHeight:F2}";
            }
            return "CanvasScaler 未初始化";
        }

        #endregion
    }
}
