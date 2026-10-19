using System;
using Air.GameCore;
using TMPro;
using UnityEngine;

namespace Air.UI
{
    /// <summary>
    /// 倒计时文本。挂在已经有 TMP 文本的节点上，按真实时间每秒刷新。
    /// </summary>
    public class CountdownText : MonoBehaviour
    {
        public enum TextType
        {
            Second,
            MinuteSecond,
            HourMinuteSecond,
            Day
        }

        public TextType textType = TextType.Second;

        TMP_Text _label;
        readonly ViTimeNode1 _node = new ViTimeNode1();
        string _format;
        int _left;
        Action _onFinished;
        Action _onTick;
        bool _running;

        void Awake() => _label = GetComponent<TMP_Text>();

        void OnDestroy() => Detch();

        public void SetFormatStr(string format)
        {
            _format = format;
            if (_running)
                Refresh();
        }

        public void SetSeconds(float seconds, Action onFinished = null, Action onTick = null)
        {
            Detch();
            if (_label == null)
                _label = GetComponent<TMP_Text>();
            if (_label == null)
                throw new InvalidOperationException("CountdownText 需要同节点上的 TMP 文本。");

            _onFinished = onFinished;
            _onTick = onTick;
            _left = seconds <= 0f ? 0 : (int)Math.Ceiling(seconds);
            _running = true;
            Refresh();
            if (_left <= 0)
            {
                var finished = _onFinished;
                Detch();
                finished?.Invoke();
                return;
            }

            ViTimerInstance.SetTime(_node, 1f, OnSecond);
        }

        public void Detch()
        {
            _running = false;
            _node.Detach();
            _onFinished = null;
            _onTick = null;
        }

        void OnSecond(ViTimeNodeInterface _)
        {
            if (!_running || this == null)
                return;
            if (_left > 0)
                _left--;
            Refresh();
            var tick = _onTick;
            tick?.Invoke();
            if (!_running || this == null)
                return;
            if (_left <= 0)
            {
                var finished = _onFinished;
                Detch();
                finished?.Invoke();
                return;
            }

            ViTimerInstance.SetTime(_node, 1f, OnSecond);
        }

        void Refresh()
        {
            if (_label == null)
                return;
            var body = FormatBody(_left);
            _label.text = string.IsNullOrEmpty(_format) ? body : string.Format(_format, body);
        }

        string FormatBody(int seconds)
        {
            if (seconds < 0)
                seconds = 0;
            switch (textType)
            {
                case TextType.MinuteSecond:
                    return string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);
                case TextType.HourMinuteSecond:
                    return string.Format("{0:00}:{1:00}:{2:00}", seconds / 3600, (seconds / 60) % 60, seconds % 60);
                case TextType.Day:
                    var days = seconds / 86400;
                    var rest = seconds % 86400;
                    return string.Format("{0}天 {1:00}:{2:00}:{3:00}", days, rest / 3600, (rest / 60) % 60, rest % 60);
                default:
                    return seconds.ToString();
            }
        }
    }
}
