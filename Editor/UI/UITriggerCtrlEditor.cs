using UnityEngine;
using UnityEditor;
using Air.UI.Trigger;

namespace Air.UI.Editor
{
    /// <summary>
    /// UI 触发控制器自定义编辑器。优化绑定与动作列表的渲染，支持添加/删除绑定与动作。
    /// </summary>
    [CustomEditor(typeof(UITriggerCtrl))]
    public class UITriggerCtrlEditor : UnityEditor.Editor
    {
        private UITriggerCtrl _triggerCtrl;
        private SerializedProperty _bindingsProperty;
        private bool[] _bindingFoldouts;
        private string _newBindingName = "NewBinding";

        private void OnEnable()
        {
            _triggerCtrl = (UITriggerCtrl)target;
            _triggerCtrl.InitDefaultBindings();
            _bindingsProperty = serializedObject.FindProperty("bindings");
            EnsureBindingFoldouts();
        }

        private void EnsureBindingFoldouts()
        {
            int count = _bindingsProperty?.arraySize ?? 0;
            if (_bindingFoldouts == null || _bindingFoldouts.Length != count)
            {
                _bindingFoldouts = new bool[Mathf.Max(1, count)];
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("UI 触发控制器", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("通过代码调用 Trigger(绑定名) 或 Trigger(索引) 触发；绑定内动作按顺序执行，可设置执行完后继续触发的下一绑定。", MessageType.None);
            EditorGUILayout.Space(5);

            DrawAddBindingSection();
            EditorGUILayout.Space(8);
            DrawBindingsSection();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 绘制「添加绑定」区域。
        /// </summary>
        private void DrawAddBindingSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("添加绑定", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _newBindingName = EditorGUILayout.TextField("绑定名称", _newBindingName, GUILayout.MinWidth(180));
            if (GUILayout.Button("添加", GUILayout.Width(60)) && !string.IsNullOrWhiteSpace(_newBindingName))
            {
                _bindingsProperty.arraySize++;
                var newBinding = _bindingsProperty.GetArrayElementAtIndex(_bindingsProperty.arraySize - 1);
                newBinding.FindPropertyRelative("bindingName").stringValue = _newBindingName.Trim();
                EnsureBindingFoldouts();
                _newBindingName = "NewBinding";
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制绑定列表。
        /// </summary>
        private void DrawBindingsSection()
        {
            EditorGUILayout.LabelField($"绑定列表 ({_bindingsProperty.arraySize})", EditorStyles.boldLabel);

            if (_bindingsProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("暂无绑定。请在上方输入绑定名称并点击「添加」。", MessageType.Info);
                return;
            }

            EnsureBindingFoldouts();

            for (int i = 0; i < _bindingsProperty.arraySize; i++)
            {
                var bindingProperty = _bindingsProperty.GetArrayElementAtIndex(i);
                TriggerBinding binding = (i < _triggerCtrl.Bindings.Count) ? _triggerCtrl.Bindings[i] : null;
                DrawBindingItem(binding, i, bindingProperty);
                EditorGUILayout.Space(5);
            }
        }

        /// <summary>
        /// 绘制单个绑定项。
        /// </summary>
        private void DrawBindingItem(TriggerBinding binding, int bindingIndex, SerializedProperty bindingProperty)
        {
            var bindingNameProperty = bindingProperty.FindPropertyRelative("bindingName");
            var actionsProperty = bindingProperty.FindPropertyRelative("actions");
            var nextBindingNameProperty = bindingProperty.FindPropertyRelative("nextBindingName");

            if (bindingNameProperty == null || actionsProperty == null || nextBindingNameProperty == null)
                return;

            EditorGUILayout.BeginVertical("box");

            string headerLabel = binding != null
                ? $"绑定: {binding.BindingName} ({binding.Actions.Count} 个动作)"
                : $"绑定 [{bindingIndex}]";
            EditorGUILayout.BeginHorizontal();
            _bindingFoldouts[bindingIndex] = EditorGUILayout.Foldout(_bindingFoldouts[bindingIndex], headerLabel, true);
            if (GUILayout.Button("删除绑定", GUILayout.Width(70)))
            {
                if (EditorUtility.DisplayDialog("确认删除", "确定要删除该绑定吗？", "删除", "取消"))
                {
                    _bindingsProperty.DeleteArrayElementAtIndex(bindingIndex);
                    EnsureBindingFoldouts();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (_bindingFoldouts[bindingIndex])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(bindingNameProperty, new GUIContent("绑定名称"));
                EditorGUILayout.PropertyField(nextBindingNameProperty, new GUIContent("完成后触发下一绑定"));

                if (binding != null)
                {
                    EditorGUILayout.Space(5);
                    DrawActionsSection(binding, bindingIndex, bindingProperty, actionsProperty);
                    EditorGUILayout.Space(5);
                    DrawAddActionButtons(binding, bindingIndex);
                }
                else
                {
                    EditorGUILayout.PropertyField(actionsProperty, new GUIContent("动作列表"), true);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制动作列表。
        /// </summary>
        private void DrawActionsSection(TriggerBinding binding, int bindingIndex, SerializedProperty bindingProperty, SerializedProperty actionsProperty)
        {
            EditorGUILayout.LabelField("动作列表", EditorStyles.boldLabel);
            if (binding.Actions.Count == 0)
            {
                EditorGUILayout.HelpBox("此绑定暂无动作。请点击下方「+ 触发事件」或「+ 播放动画」添加。", MessageType.Info);
                return;
            }

            for (int j = 0; j < binding.Actions.Count; j++)
            {
                var action = binding.Actions[j];
                if (action == null) continue;

                var actionProperty = actionsProperty.GetArrayElementAtIndex(j);
                DrawActionItem(binding, action, j, bindingIndex, actionProperty);
            }
        }

        /// <summary>
        /// 绘制单个动作项。
        /// </summary>
        private void DrawActionItem(TriggerBinding binding, TriggerActionBase action, int actionIndex, int bindingIndex, SerializedProperty actionProperty)
        {
            EditorGUILayout.BeginVertical("helpbox");

            EditorGUILayout.BeginHorizontal();
            string actionTypeName = action.GetActionTypeName();
            EditorGUILayout.LabelField($"动作 {actionIndex + 1}: {actionTypeName}", EditorStyles.boldLabel);
            if (!action.IsValid())
            {
                GUILayout.Label(new GUIContent("⚠", "此动作配置无效"), GUILayout.Width(20));
            }
            if (GUILayout.Button("删除", GUILayout.Width(50)))
            {
                Undo.RecordObject(_triggerCtrl, "Remove Trigger Action");
                binding.Actions.RemoveAt(actionIndex);
                serializedObject.Update();
                EditorUtility.SetDirty(_triggerCtrl);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            DrawTriggerActionFields(action, actionProperty);
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }

        /// <summary>
        /// 根据动作类型绘制对应字段。
        /// </summary>
        private void DrawTriggerActionFields(TriggerActionBase action, SerializedProperty actionProperty)
        {
            switch (action)
            {
                case EventTriggerAction _:
                    var onTriggerProperty = actionProperty.FindPropertyRelative("onTrigger");
                    if (onTriggerProperty != null)
                        EditorGUILayout.PropertyField(onTriggerProperty, new GUIContent("触发事件"), true);
                    break;

                case AnimationTriggerAction _:
                    var targetObjectProperty = actionProperty.FindPropertyRelative("targetObject");
                    var sourceTypeProperty = actionProperty.FindPropertyRelative("sourceType");
                    var triggerOrClipNameProperty = actionProperty.FindPropertyRelative("triggerOrClipName");
                    var waitForCompletionProperty = actionProperty.FindPropertyRelative("waitForCompletion");
                    if (targetObjectProperty != null)
                        EditorGUILayout.PropertyField(targetObjectProperty, new GUIContent("目标对象"));
                    if (sourceTypeProperty != null)
                        EditorGUILayout.PropertyField(sourceTypeProperty, new GUIContent("动画来源"));
                    if (triggerOrClipNameProperty != null)
                        EditorGUILayout.PropertyField(triggerOrClipNameProperty, new GUIContent("Trigger/Clip 名称"));
                    if (waitForCompletionProperty != null)
                        EditorGUILayout.PropertyField(waitForCompletionProperty, new GUIContent("等待播放完成"));
                    break;
            }
        }

        /// <summary>
        /// 绘制添加动作按钮。
        /// </summary>
        private void DrawAddActionButtons(TriggerBinding binding, int bindingIndex)
        {
            EditorGUILayout.LabelField("添加动作", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ 触发事件", GUILayout.Height(22)))
            {
                Undo.RecordObject(_triggerCtrl, "Add Event Trigger Action");
                binding.Actions.Add(new EventTriggerAction());
                serializedObject.Update();
                EditorUtility.SetDirty(_triggerCtrl);
            }
            if (GUILayout.Button("+ 播放动画", GUILayout.Height(22)))
            {
                Undo.RecordObject(_triggerCtrl, "Add Animation Trigger Action");
                binding.Actions.Add(new AnimationTriggerAction());
                serializedObject.Update();
                EditorUtility.SetDirty(_triggerCtrl);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
