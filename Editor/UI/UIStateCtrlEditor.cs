using UnityEngine;
using UnityEditor;
using Air.UI.State;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Air.UI.Editor
{
    /// <summary>
    /// UI状态控制器自定义编辑器
    /// </summary>
    [CustomEditor(typeof(UIStateCtrl))]
    public class UIStateCtrlEditor : UnityEditor.Editor
    {
        private UIStateCtrl _stateCtrl;
        private SerializedProperty _stateGroupsProperty;

        private bool[] _groupFoldouts;
        private bool[] _stateFoldouts;
        private string _newGroupName = "NewGroup";
        private string _newStateName = "NewState";
        private int _selectedGroupIndexForAddState;

        private void OnEnable()
        {
            _stateCtrl = (UIStateCtrl)target;
            _stateGroupsProperty = serializedObject.FindProperty("stateGroups");
            EnsureFoldoutArrays();
        }

        private int GetTotalStateCount()
        {
            int count = 0;
            foreach (var group in _stateCtrl.StateGroups)
            {
                if (group != null)
                {
                    count += group.States.Count;
                }
            }
            return count;
        }

        private void EnsureFoldoutArrays()
        {
            int groupCount = Mathf.Max(
                _stateGroupsProperty?.arraySize ?? 0,
                _stateCtrl.StateGroups?.Count ?? 0);
            int stateCount = GetTotalStateCount();
            if (_groupFoldouts == null || _groupFoldouts.Length != groupCount)
            {
                _groupFoldouts = new bool[Math.Max(1, groupCount)];
            }
            if (_stateFoldouts == null || _stateFoldouts.Length != stateCount)
            {
                _stateFoldouts = new bool[Math.Max(1, stateCount)];
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("UI 状态控制器", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 基础设置
            DrawBasicSettings();

            EditorGUILayout.Space(10);

            // 添加状态组 / 向组内添加状态
            DrawAddGroupAndStateSection();

            EditorGUILayout.Space(10);

            // 显示所有状态组及组内状态
            DrawStateGroupsSection();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 绘制基础设置
        /// </summary>
        private void DrawBasicSettings()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("基础设置", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制添加状态组与向组内添加状态区域
        /// </summary>
        private void DrawAddGroupAndStateSection()
        {
            EditorGUILayout.BeginVertical("box");
            DrawAddGroupRow();
            EditorGUILayout.Space(5);
            DrawAddStateToGroupRow();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制「添加状态组」一行：组名输入 + 添加组按钮
        /// </summary>
        private void DrawAddGroupRow()
        {
            EditorGUILayout.LabelField("添加状态组", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _newGroupName = EditorGUILayout.TextField("组名称", _newGroupName);
            if (GUILayout.Button("添加组", GUILayout.Width(70)) && !string.IsNullOrEmpty(_newGroupName))
            {
                Undo.RecordObject(_stateCtrl, "Add State Group");
                if (_stateCtrl.AddGroup(_newGroupName) != null)
                {
                    _newGroupName = "NewGroup";
                    EnsureFoldoutArrays();
                    EditorUtility.SetDirty(_stateCtrl);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制「向组内添加状态」一行：组选择 + 状态名输入 + 添加状态按钮
        /// </summary>
        private void DrawAddStateToGroupRow()
        {
            EditorGUILayout.LabelField("向组内添加状态", EditorStyles.boldLabel);
            var groupNames = GetGroupNamesForPopup();
            _selectedGroupIndexForAddState = Mathf.Clamp(_selectedGroupIndexForAddState, 0, groupNames.Count - 1);

            EditorGUILayout.BeginHorizontal();
            if (groupNames.Count > 0 && groupNames[0] != "(无组)")
            {
                _selectedGroupIndexForAddState = EditorGUILayout.Popup(_selectedGroupIndexForAddState, groupNames.ToArray(), GUILayout.Width(120));
            }
            _newStateName = EditorGUILayout.TextField("状态名称", _newStateName, GUILayout.MinWidth(180));
            if (GUILayout.Button("添加状态", GUILayout.Width(70)) && TryAddStateToSelectedGroup(groupNames))
            {
                _newStateName = "NewState";
                EnsureFoldoutArrays();
                EditorUtility.SetDirty(_stateCtrl);
            }
            EditorGUILayout.EndHorizontal();
        }

        private List<string> GetGroupNamesForPopup()
        {
            var names = _stateCtrl.StateGroups
                .Where(g => g != null && !string.IsNullOrEmpty(g.GroupName))
                .Select(g => g.GroupName)
                .ToList();
            if (names.Count == 0) names.Add("(无组)");
            return names;
        }

        private bool TryAddStateToSelectedGroup(List<string> groupNames)
        {
            if (groupNames.Count == 0 || groupNames[0] == "(无组)" || string.IsNullOrEmpty(_newStateName))
                return false;
            string groupName = groupNames[_selectedGroupIndexForAddState];
            Undo.RecordObject(_stateCtrl, "Add State");
            return _stateCtrl.GetGroup(groupName)?.AddState(_newStateName) != null;
        }

        /// <summary>
        /// 绘制状态组列表及每组内状态
        /// </summary>
        private void DrawStateGroupsSection()
        {
            EditorGUILayout.LabelField($"状态组 ({_stateCtrl.StateGroups?.Count ?? 0})", EditorStyles.boldLabel);

            if (_stateGroupsProperty == null || _stateCtrl.StateGroups == null || _stateCtrl.StateGroups.Count == 0)
            {
                EditorGUILayout.HelpBox("没有状态组。请先添加一个状态组，再在组内添加状态。", MessageType.Info);
                return;
            }

            EnsureFoldoutArrays();

            int arraySize = _stateGroupsProperty.arraySize;
            if (arraySize <= 0) return;
            int stateLinearIndex = 0;
            for (int g = 0; g < arraySize; g++)
            {
                var groupProperty = _stateGroupsProperty.GetArrayElementAtIndex(g);
                UIStateGroup group = (g < _stateCtrl.StateGroups.Count) ? _stateCtrl.StateGroups[g] : null;
                if (group == null) continue;

                DrawGroupItem(group, g, groupProperty, ref stateLinearIndex);
                EditorGUILayout.Space(5);
            }
        }

        /// <summary>
        /// 绘制单个状态组（含组内当前状态选择与状态列表）
        /// </summary>
        public void DrawGroupItem(UIStateGroup group, int groupIndex, SerializedProperty groupProperty, ref int stateLinearIndex)
        {
            if (groupProperty == null) return;
            var groupNameProperty = groupProperty.FindPropertyRelative("groupName");
            var statesProperty = groupProperty.FindPropertyRelative("states");
            var currentStateIndexProperty = groupProperty.FindPropertyRelative("currentStateIndex");
            if (groupNameProperty == null || statesProperty == null || currentStateIndexProperty == null) return;

            EditorGUILayout.BeginVertical("box");
            if (DrawGroupHeader(group, groupIndex))
            {
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.PropertyField(groupNameProperty, new GUIContent("组名称"));
            DrawGroupCurrentStateSelector(group, currentStateIndexProperty);
            DrawGroupStatesListWhenExpanded(group, groupIndex, ref stateLinearIndex);
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制状态组标题行：折叠 + 删除组按钮。
        /// </summary>
        /// <returns>若用户确认删除组并已执行，返回 true，调用方应结束当前组绘制</returns>
        private bool DrawGroupHeader(UIStateGroup group, int groupIndex)
        {
            EditorGUILayout.BeginHorizontal();
            _groupFoldouts[groupIndex] = EditorGUILayout.Foldout(_groupFoldouts[groupIndex],
                $"组: {group.GroupName} ({group.States.Count} 个状态)", true);
            bool removed = false;
            if (GUILayout.Button("删除组", GUILayout.Width(60)) &&
                EditorUtility.DisplayDialog("确认删除", $"确定要删除状态组 '{group.GroupName}' 吗？", "删除", "取消"))
            {
                Undo.RecordObject(_stateCtrl, "Remove State Group");
                _stateCtrl.RemoveGroup(group.GroupName);
                EnsureFoldoutArrays();
                EditorUtility.SetDirty(_stateCtrl);
                removed = true;
            }
            EditorGUILayout.EndHorizontal();
            return removed;
        }

        /// <summary>
        /// 绘制组内「当前选中状态」下拉与预览按钮
        /// </summary>
        private void DrawGroupCurrentStateSelector(UIStateGroup group, SerializedProperty currentStateIndexProperty)
        {
            List<string> stateNames = BuildStateNamesForPopup(group);
            int currentIndex = GetPopupIndexFromStateIndex(currentStateIndexProperty.intValue, group.States.Count);
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("当前选中状态", currentIndex, stateNames.ToArray());
            if (EditorGUI.EndChangeCheck())
                currentStateIndexProperty.intValue = newIndex == 0 ? -1 : newIndex - 1;
            if (currentStateIndexProperty.intValue >= 0 && GUILayout.Button("预览当前状态"))
                _stateCtrl.PreviewState(group.GroupName, currentStateIndexProperty.intValue);
        }

        private static List<string> BuildStateNamesForPopup(UIStateGroup group)
        {
            var list = new List<string> { "(无)" };
            for (int i = 0; i < group.States.Count; i++)
            {
                var state = group.States[i];
                list.Add(state != null ? $"[{i}] {state.StateName}" : $"[{i}] (空)");
            }
            return list;
        }

        private static int GetPopupIndexFromStateIndex(int stateIndex, int stateCount)
        {
            return (stateIndex >= 0 && stateIndex < stateCount) ? stateIndex + 1 : 0;
        }

        /// <summary>
        /// 组展开时绘制组内状态列表
        /// </summary>
        private void DrawGroupStatesListWhenExpanded(UIStateGroup group, int groupIndex, ref int stateLinearIndex)
        {
            if (!_groupFoldouts[groupIndex])
            {
                stateLinearIndex += group.States.Count;
                return;
            }
            EditorGUI.indentLevel++;
            for (int s = 0; s < group.States.Count; s++)
            {
                var state = group.States[s];
                if (state == null) continue;
                int linearIndex = stateLinearIndex++;
                DrawStateItem(state, groupIndex, s, linearIndex);
                EditorGUILayout.Space(3);
            }
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// 绘制单个状态项
        /// </summary>
        private void DrawStateItem(UIState state, int groupIndex, int stateIndexInGroup, int stateLinearIndex)
        {
            var group = _stateCtrl.StateGroups[groupIndex];
            string groupName = group?.GroupName ?? string.Empty;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            if (stateLinearIndex < _stateFoldouts.Length)
            {
                _stateFoldouts[stateLinearIndex] = EditorGUILayout.Foldout(_stateFoldouts[stateLinearIndex],
                    $"{state.StateName} ({state.Actions.Count} 个动作)", true);
            }

            if (GUILayout.Button("预览", GUILayout.Width(50)))
            {
                _stateCtrl.PreviewState(groupName, stateIndexInGroup);
            }

            if (GUILayout.Button("删除", GUILayout.Width(50)))
            {
                if (EditorUtility.DisplayDialog("确认删除",
                    $"确定要删除状态 '{state.StateName}' 吗？", "删除", "取消"))
                {
                    Undo.RecordObject(_stateCtrl, "Remove State");
                    _stateCtrl.GetGroup(groupName)?.RemoveStateAt(stateIndexInGroup);
                    EnsureFoldoutArrays();
                    EditorUtility.SetDirty(_stateCtrl);
                    return;
                }
            }

            EditorGUILayout.EndHorizontal();

            if (stateLinearIndex < _stateFoldouts.Length && _stateFoldouts[stateLinearIndex])
            {
                EditorGUI.indentLevel++;
                DrawStateActions(state, groupIndex, stateIndexInGroup);
                EditorGUI.indentLevel--;

                EditorGUILayout.Space(5);
                DrawAddActionButtons(state);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制状态动作列表
        /// </summary>
        private void DrawStateActions(UIState state, int groupIndex, int stateIndexInGroup)
        {
            if (state.Actions.Count == 0)
            {
                EditorGUILayout.HelpBox("此状态没有动作。请添加动作。", MessageType.Info);
                return;
            }

            for (int i = 0; i < state.Actions.Count; i++)
            {
                var action = state.Actions[i];
                if (action == null) continue;

                DrawActionItem(state, action, i, groupIndex, stateIndexInGroup);
            }
        }

        /// <summary>
        /// 绘制单个动作项
        /// </summary>
        private void DrawActionItem(UIState state, StateAction action, int actionIndex, int groupIndex, int stateIndexInGroup)
        {
            if (action == null) return;

            EditorGUILayout.BeginVertical("helpbox");

            EditorGUILayout.BeginHorizontal();
            string actionTypeName = action.GetActionTypeName();
            string actionLabel = $"动作 {actionIndex + 1}: {actionTypeName} ({action.GetType().Name})";
            EditorGUILayout.LabelField(actionLabel, EditorStyles.boldLabel);

            if (!action.IsValid())
            {
                GUILayout.Label(new GUIContent("⚠", "此动作配置无效"), GUILayout.Width(20));
            }

            if (GUILayout.Button("删除", GUILayout.Width(50)))
            {
                Undo.RecordObject(_stateCtrl, "Remove Action");
                state.RemoveActionAt(actionIndex);
                EditorUtility.SetDirty(_stateCtrl);
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;

            var groupProperty = _stateGroupsProperty.GetArrayElementAtIndex(groupIndex);
            var statesProperty = groupProperty.FindPropertyRelative("states");
            var stateProperty = statesProperty.GetArrayElementAtIndex(stateIndexInGroup);
            var actionsProperty = stateProperty.FindPropertyRelative("actions");
            var actionProperty = actionsProperty.GetArrayElementAtIndex(actionIndex);

            DrawActionFields(action, actionProperty);

            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }

        /// <summary>
        /// 根据动作类型绘制相应的字段
        /// </summary>
        private void DrawActionFields(StateAction action, SerializedProperty actionProperty)
        {
            // 目标对象（所有动作都有）
            var targetObjectProperty = actionProperty.FindPropertyRelative("targetObject");
            EditorGUILayout.PropertyField(targetObjectProperty, new GUIContent("目标对象"));

            // 根据具体类型绘制特定字段
            switch (action)
            {
                case VisibilityStateAction _:
                    DrawVisibilityActionFields(actionProperty);
                    break;

                case ColorStateAction _:
                    DrawColorActionFields(actionProperty);
                    break;

                case ScaleStateAction _:
                    DrawScaleActionFields(actionProperty);
                    break;

                case PositionStateAction _:
                    DrawPositionActionFields(actionProperty);
                    break;

                case RotationStateAction _:
                    DrawRotationActionFields(actionProperty);
                    break;

                case AlphaStateAction _:
                    DrawAlphaActionFields(actionProperty);
                    break;

                case InteractableStateAction _:
                    DrawInteractableActionFields(actionProperty);
                    break;
            }
        }

        private void DrawVisibilityActionFields(SerializedProperty actionProperty)
        {
            var isVisibleProperty = actionProperty.FindPropertyRelative("isVisible");
            EditorGUILayout.PropertyField(isVisibleProperty, new GUIContent("是否显示"));
        }

        private void DrawColorActionFields(SerializedProperty actionProperty)
        {
            var targetColorProperty = actionProperty.FindPropertyRelative("targetColor");
            EditorGUILayout.PropertyField(targetColorProperty, new GUIContent("目标颜色"));
        }

        private void DrawScaleActionFields(SerializedProperty actionProperty)
        {
            var targetScaleProperty = actionProperty.FindPropertyRelative("targetScale");
            EditorGUILayout.PropertyField(targetScaleProperty, new GUIContent("目标缩放"));
        }

        private void DrawPositionActionFields(SerializedProperty actionProperty)
        {
            var anchoredPositionProperty = actionProperty.FindPropertyRelative("anchoredPosition");
            EditorGUILayout.PropertyField(anchoredPositionProperty, new GUIContent("锚点位置"));
        }

        private void DrawRotationActionFields(SerializedProperty actionProperty)
        {
            var targetRotationProperty = actionProperty.FindPropertyRelative("targetRotation");
            EditorGUILayout.PropertyField(targetRotationProperty, new GUIContent("目标旋转"));
        }

        private void DrawAlphaActionFields(SerializedProperty actionProperty)
        {
            var targetAlphaProperty = actionProperty.FindPropertyRelative("targetAlpha");
            EditorGUILayout.Slider(targetAlphaProperty, 0f, 1f, new GUIContent("目标透明度"));
        }

        private void DrawInteractableActionFields(SerializedProperty actionProperty)
        {
            var interactableProperty = actionProperty.FindPropertyRelative("interactable");
            var blockRaycastsProperty = actionProperty.FindPropertyRelative("blockRaycasts");
            
            EditorGUILayout.PropertyField(interactableProperty, new GUIContent("可交互"));
            EditorGUILayout.PropertyField(blockRaycastsProperty, new GUIContent("阻挡射线"));
        }

        /// <summary>
        /// 绘制添加动作按钮
        /// </summary>
        private void DrawAddActionButtons(UIState state)
        {
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.LabelField("添加动作类型", EditorStyles.boldLabel);
            
            // 第一行：基础动作
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ 显影", GUILayout.Height(25)))
            {
                AddAction<VisibilityStateAction>(state, "Add Visibility Action");
            }
            if (GUILayout.Button("+ 颜色", GUILayout.Height(25)))
            {
                AddAction<ColorStateAction>(state, "Add Color Action");
            }
            if (GUILayout.Button("+ 透明度", GUILayout.Height(25)))
            {
                AddAction<AlphaStateAction>(state, "Add Alpha Action");
            }
            EditorGUILayout.EndHorizontal();

            // 第二行：变换动作
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ 缩放", GUILayout.Height(25)))
            {
                AddAction<ScaleStateAction>(state, "Add Scale Action");
            }
            if (GUILayout.Button("+ 位置", GUILayout.Height(25)))
            {
                AddAction<PositionStateAction>(state, "Add Position Action");
            }
            if (GUILayout.Button("+ 旋转", GUILayout.Height(25)))
            {
                AddAction<RotationStateAction>(state, "Add Rotation Action");
            }
            EditorGUILayout.EndHorizontal();

            // 第三行：交互和动画
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ 交互", GUILayout.Height(25)))
            {
                AddAction<InteractableStateAction>(state, "Add Interactable Action");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 添加指定类型的动作
        /// </summary>
        private void AddAction<T>(UIState state, string undoName) where T : StateAction, new()
        {
            Undo.RecordObject(_stateCtrl, undoName);
            var action = new T();
            state.AddAction(action);
            EditorUtility.SetDirty(_stateCtrl);
        }
    }
}
