using System;
using System.Collections.Generic;
using UnityEngine;

namespace Air.UI.State
{
    /// <summary>
    /// UI状态数据
    /// </summary>
    [Serializable]
    public class UIState
    {
        [SerializeField] private string stateName;
        
        // 使用SerializeReference支持多态序列化
        [SerializeReference] private List<StateAction> actions = new();

        public string StateName => stateName;
        public List<StateAction> Actions => actions;

        public UIState(string name)
        {
            stateName = name;
        }

        /// <summary>
        /// 应用当前状态
        /// </summary>
        public void Apply()
        {
            foreach (var action in actions)
            {
                if (action != null && action.IsValid())
                {
                    try
                    {
                        action.Apply();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[UIState] 应用动作失败: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 添加动作
        /// </summary>
        public void AddAction(StateAction action)
        {
            if (action != null && !actions.Contains(action))
            {
                actions.Add(action);
            }
        }

        /// <summary>
        /// 移除动作
        /// </summary>
        public void RemoveAction(StateAction action)
        {
            actions.Remove(action);
        }

        /// <summary>
        /// 移除指定索引的动作
        /// </summary>
        public void RemoveActionAt(int index)
        {
            if (index >= 0 && index < actions.Count)
            {
                actions.RemoveAt(index);
            }
        }

        /// <summary>
        /// 获取所有有效的动作
        /// </summary>
        public List<StateAction> GetValidActions()
        {
            return actions.FindAll(a => a != null && a.IsValid());
        }
    }

    /// <summary>
    /// UI 状态组：组内包含多个状态，有且只能有一个状态被选中。
    /// </summary>
    [Serializable]
    public class UIStateGroup
    {
        [SerializeField] private string groupName;
        [SerializeField] private List<UIState> states = new();
        [SerializeField] private int currentStateIndex = -1;

        private UIState _currentState;

        /// <summary>
        /// 组名称
        /// </summary>
        public string GroupName => groupName;

        /// <summary>
        /// 组内所有状态
        /// </summary>
        public List<UIState> States => states;

        /// <summary>
        /// 当前选中的状态下标（-1 表示未选中）
        /// </summary>
        public int CurrentStateIndex => currentStateIndex;

        /// <summary>
        /// 当前选中的状态名称（未选中时为空）
        /// </summary>
        public string CurrentStateName => _currentState != null ? _currentState.StateName : string.Empty;

        /// <summary>
        /// 当前选中的状态
        /// </summary>
        public UIState CurrentState => _currentState;

        /// <summary>
        /// 无参构造函数供 Unity 序列化使用
        /// </summary>
        public UIStateGroup()
        {
            groupName = string.Empty;
        }

        public UIStateGroup(string name)
        {
            groupName = name ?? string.Empty;
        }

        /// <summary>
        /// 根据当前下标同步 _currentState
        /// </summary>
        internal void Initialize()
        {
            if (currentStateIndex >= 0 && currentStateIndex < states.Count && states[currentStateIndex] != null)
            {
                _currentState = states[currentStateIndex];
            }
            else
            {
                _currentState = null;
                if (currentStateIndex >= states.Count || currentStateIndex < -1)
                {
                    currentStateIndex = -1;
                }
            }
        }

        /// <summary>
        /// 通过下标获取组内状态
        /// </summary>
        /// <param name="index">状态下标</param>
        /// <returns>状态对象，越界或为 null 时返回 null</returns>
        public UIState GetState(int index)
        {
            if (index < 0 || index >= states.Count) return null;
            return states[index];
        }

        /// <summary>
        /// 设置组内当前状态（组内有且只能有一个状态被选中）。
        /// 切换时先清除当前选中，再设置并应用新状态。
        /// </summary>
        /// <param name="index">状态下标</param>
        /// <returns>是否设置成功</returns>
        public bool SetState(int index)
        {
            if (index < 0 || index >= states.Count)
            {
                Debug.LogWarning($"[UIStateGroup] 组 '{groupName}' 状态下标越界: {index}, 总数: {states.Count}");
                return false;
            }

            var state = states[index];
            if (state == null)
            {
                Debug.LogWarning($"[UIStateGroup] 组 '{groupName}' 下标 {index} 处状态为空");
                return false;
            }

            // 先将当前选中的状态去除
            currentStateIndex = -1;
            _currentState = null;

            // 再设置并应用新的选中状态
            currentStateIndex = index;
            _currentState = state;
            state.Apply();
            return true;
        }

        /// <summary>
        /// 添加状态到组内
        /// </summary>
        public UIState AddState(string stateName)
        {
            var newState = new UIState(stateName ?? string.Empty);
            states.Add(newState);
            return newState;
        }

        /// <summary>
        /// 通过下标从组内移除状态
        /// </summary>
        public void RemoveStateAt(int index)
        {
            if (index < 0 || index >= states.Count) return;
            states.RemoveAt(index);
            if (currentStateIndex == index)
            {
                currentStateIndex = -1;
                _currentState = null;
            }
            else if (currentStateIndex > index)
            {
                currentStateIndex--;
            }
        }

        /// <summary>
        /// 刷新组内当前选中的状态
        /// </summary>
        public void RefreshCurrentState()
        {
            if (_currentState != null)
            {
                _currentState.Apply();
            }
        }
    }

    /// <summary>
    /// UI状态控制器
    /// 支持多个状态组，每组内有多个状态且仅能有一个被选中；在编辑器中预设并在运行时切换。
    /// </summary>
    [RequireComponent(typeof(UIComponent))]
    public class UIStateCtrl : MonoBehaviour
    {
        [SerializeField] private List<UIStateGroup> stateGroups = new();

        #if UNITY_EDITOR
        public List<UIStateGroup> StateGroups => stateGroups;
        #endif
        
        private Dictionary<string, UIStateGroup> _groupDict;

        private Dictionary<string, UIStateGroup> GroupDict
        {
            get {
                if (_groupDict == null)
                {
                    InitializeGroups();
                }

                return _groupDict;
            }
        }

        /// <summary>
        /// 初始化状态组字典及各组内状态
        /// </summary>
        private void InitializeGroups()
        {
            _groupDict = new Dictionary<string, UIStateGroup>();
            foreach (var group in stateGroups)
            {
                if (group == null || string.IsNullOrEmpty(group.GroupName)) continue;
                if (_groupDict.ContainsKey(group.GroupName))
                {
                    Debug.LogError($"[UIStateCtrl] 状态组名称重复: {group.GroupName}");
                    continue;
                }
                group.Initialize();
                _groupDict[group.GroupName] = group;
            }
        }

        /// <summary>
        /// 设置指定组内的当前状态（该组内有且只能有一个状态被选中）
        /// </summary>
        /// <param name="groupName">组名称</param>
        /// <param name="stateIndex">状态下标</param>
        /// <returns>是否设置成功</returns>
        public bool SetState(string groupName, int stateIndex)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                Debug.LogError("[UIStateCtrl] 组名称不能为空");
                return false;
            }

            if (GroupDict.TryGetValue(groupName, out var group))
            {
                return group.SetState(stateIndex);
            }

            Debug.LogError($"[UIStateCtrl] 未找到状态组: {groupName}");
            return false;
        }

        /// <summary>
        /// 获取指定组内当前选中的状态下标（-1 表示未选中）
        /// </summary>
        public int GetCurrentStateIndex(string groupName)
        {
            return GroupDict.TryGetValue(groupName, out var group) ? group.CurrentStateIndex : -1;
        }

        /// <summary>
        /// 获取指定组内当前选中的状态名称（未选中时为空）
        /// </summary>
        public string GetCurrentStateName(string groupName)
        {
            return GroupDict.TryGetValue(groupName, out var group) ? group.CurrentStateName : null;
        }

        /// <summary>
        /// 获取指定组
        /// </summary>
        public UIStateGroup GetGroup(string groupName)
        {
            GroupDict.TryGetValue(groupName, out var group);
            return group;
        }

        /// <summary>
        /// 通过组名与状态下标获取指定组内的状态
        /// </summary>
        /// <param name="groupName">组名称</param>
        /// <param name="stateIndex">状态下标</param>
        /// <returns>状态对象</returns>
        public UIState GetState(string groupName, int stateIndex)
        {
            var group = GetGroup(groupName);
            return group?.GetState(stateIndex);
        }

        /// <summary>
        /// 添加状态组
        /// </summary>
        /// <param name="groupName">组名称</param>
        /// <returns>新创建的状态组</returns>
        public UIStateGroup AddGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                Debug.LogError("[UIStateCtrl] 组名称不能为空");
                return null;
            }

            if (GroupDict.TryGetValue(groupName, out var group))
            {
                Debug.LogError($"[UIStateCtrl] 状态组已存在: {groupName}");
                return group;
            }

            var newGroup = new UIStateGroup(groupName);
            newGroup.Initialize();
            stateGroups.Add(newGroup);
            GroupDict[groupName] = newGroup;
            return newGroup;
        }

        /// <summary>
        /// 移除状态组
        /// </summary>
        /// <param name="groupName">组名称</param>
        public void RemoveGroup(string groupName)
        {
            if (!GroupDict.TryGetValue(groupName, out var group)) return;
            stateGroups.Remove(group);
            _groupDict.Remove(groupName);
        }

        /// <summary>
        /// 刷新所有组内当前选中的状态
        /// </summary>
        public void RefreshCurrentState()
        {
            foreach (var (_, group) in GroupDict)
            {
                group?.RefreshCurrentState();
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器预览指定组内某下标的状态
        /// </summary>
        public void PreviewState(string groupName, int stateIndex)
        {
            var state = GetState(groupName, stateIndex);
            state?.Apply();
        }

        /// <summary>
        /// 当仅有一个状态组时，编辑器预览该组内某下标的状态
        /// </summary>
        public void PreviewState(int stateIndex)
        {
            foreach (var group in GroupDict.Values)
            {
                var state = group.GetState(stateIndex);
                state?.Apply();
                return;
            }
        }
#endif
    }
}
