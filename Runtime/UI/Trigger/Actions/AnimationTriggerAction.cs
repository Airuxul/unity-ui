using System;
using System.Collections;
using Air.UnityGameCore.Runtime.Coroutine;
using UnityEngine;

namespace Air.UI.Trigger
{
    /// <summary>
    /// 触发动画的动作。支持 Animator.SetTrigger 或 Animation.Play；可选等待动画结束后再触发下一个事件。
    /// </summary>
    [Serializable]
    public class AnimationTriggerAction : TriggerActionBase
    {
        public enum AnimationSourceType
        {
            /// <summary> 使用 Animator 的 Trigger 参数 </summary>
            AnimatorTrigger,

            /// <summary> 使用 Animation 组件播放指定 Clip </summary>
            AnimationClip
        }

        [SerializeField] private GameObject targetObject;
        [SerializeField] private AnimationSourceType sourceType;
        [SerializeField] private string triggerOrClipName;
        [SerializeField] private bool waitForCompletion;

        private Animation _animation;
        private Animator _animator;


        /// <summary> 目标 GameObject，为空时使用挂载 UITriggerCtrl 的对象。 </summary>
        public GameObject TargetObject
        {
            get => targetObject;
            set => targetObject = value;
        }

        public AnimationSourceType SourceType
        {
            get => sourceType;
            set => sourceType = value;
        }

        /// <summary> Animator 的 Trigger 名或 Animation 的 Clip 名。 </summary>
        public string TriggerOrClipName
        {
            get => triggerOrClipName;
            set => triggerOrClipName = value;
        }

        /// <summary> 是否等待动画播放完毕后再触发下一个事件。 </summary>
        public bool WaitForCompletion
        {
            get => waitForCompletion;
            set => waitForCompletion = value;
        }

        public Animator Animator
        {
            get
            {
                if (_animator) return _animator;
                if (TargetObject && !TargetObject.TryGetComponent(out _animator))
                {
                    throw new Exception("Animtor is null but use");
                }

                return _animator;
            }
        }

        public Animation Animation
        {
            get
            {
                if (_animation) return _animation;
                if (TargetObject && !TargetObject.TryGetComponent(out _animation))
                {
                    throw new Exception("Animtor is null but use");
                }

                return _animation;
            }
        }

        public AnimationTriggerAction()
        {
        }

        public AnimationTriggerAction(
            AnimationSourceType sourceType,
            GameObject go,
            string triggerOrClipName,
            bool waitForCompletion
        )
        {
            targetObject = go;
            this.sourceType = sourceType;
            this.triggerOrClipName = triggerOrClipName;
            this.waitForCompletion = waitForCompletion;
        }

        public override void Execute(Action onComplete)
        {
            if (onComplete == null)
            {
                ExecuteInternal(null);
                return;
            }

            if (waitForCompletion)
            {
                MonoBehaviour runner = GetRunner();
                if (runner != null && runner.isActiveAndEnabled)
                {
                    runner.StartCoroutine(ExecuteAndWait(onComplete));
                    return;
                }
            }

            ExecuteInternal(onComplete);
        }

        private MonoBehaviour GetRunner()
        {
            return targetObject == null ? null : targetObject.GetComponent<MonoBehaviour>();
        }

        private IEnumerator ExecuteAndWait(Action onComplete)
        {
            ExecuteInternal(null);

            switch (sourceType)
            {
                case AnimationSourceType.AnimatorTrigger:
                {
                    yield return AnimatorWait();
                    break;
                }
                case AnimationSourceType.AnimationClip:
                {
                    yield return AnimationWait();
                    break;
                }
            }

            onComplete?.Invoke();
        }

        private IEnumerator AnimatorWait()
        {
            if (!Animator || !Animator.runtimeAnimatorController) yield break;
            yield return null;
            var stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            var duration = stateInfo.length;
            yield return WaitFor.Seconds(duration);
        }

        private IEnumerator AnimationWait()
        {
            if (!Animation || !Animation.isPlaying) yield break;
            while (Animation.isPlaying)
            {
                yield return null;
            }
        }

        private void ExecuteInternal(Action onComplete)
        {
            var go = targetObject;
            if (go == null)
            {
                Debug.LogWarning("[AnimationTriggerAction] 目标对象为空，跳过执行");
                onComplete?.Invoke();
                return;
            }

            switch (sourceType)
            {
                case AnimationSourceType.AnimatorTrigger:
                    TriggerByAnimator();
                    break;
                case AnimationSourceType.AnimationClip:
                    TriggerByAnimation();
                    break;
            }

            if (!waitForCompletion)
            {
                onComplete?.Invoke();
            }
        }

        private void TriggerByAnimation()
        {
            Animation?.Play(triggerOrClipName);
        }

        private void TriggerByAnimator()
        {
            Animator?.SetTrigger(triggerOrClipName);
        }

        public override string GetActionTypeName()
        {
            return "播放动画";
        }

        public override bool IsValid()
        {
            if (targetObject == null) return false;
            if (string.IsNullOrEmpty(triggerOrClipName)) return false;
            if (sourceType == AnimationSourceType.AnimatorTrigger)
                return Animator != null;
            return Animation != null;
        }
    }
}
