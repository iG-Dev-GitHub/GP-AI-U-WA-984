using System;
using UnityEngine.UIElements;
using WorkoutDrop.Core;

namespace WorkoutDrop.UI.Components
{
    /// <summary>
    /// Bottom-anchored rest countdown. Mirrors <c>src/components/RestTimer.tsx</c>:
    /// counts down once per second, fires onDone at zero, and offers a Skip button.
    /// </summary>
    public class RestTimerElement : VisualElement
    {
        private int _remaining;
        private bool _finished;
        private readonly Label _time;
        private readonly Action _onDone;
        private readonly Action _onSkip;
        private IVisualElementScheduledItem _tick;

        public RestTimerElement(int seconds, Action onDone, Action onSkip)
        {
            _remaining = seconds;
            _onDone = onDone;
            _onSkip = onSkip;

            AddToClassList("rest-timer");

            var left = new VisualElement();
            left.AddToClassList("rest-timer__row");
            left.Add(Icons.Create("hourglass", 20, Palette.PrimaryCyan));
            var label = new Label("REST");
            label.AddToClassList("rest-timer__label");
            left.Add(label);
            Add(left);

            _time = new Label(UIUtils.FormatClock(_remaining));
            _time.AddToClassList("rest-timer__time");
            Add(_time);

            var skip = UIUtils.TactileButton("Skip Rest", "play-skip-forward", UIUtils.ButtonVariant.Secondary, Skip);
            skip.AddToClassList("rest-timer__skip");
            Add(skip);

            _tick = schedule.Execute(Tick).Every(1000);
        }

        private void Tick()
        {
            if (_finished) return;
            _remaining -= 1;
            if (_remaining <= 0)
            {
                _remaining = 0;
                _time.text = UIUtils.FormatClock(0);
                _finished = true;
                _tick?.Pause();
                _onDone?.Invoke();
                return;
            }
            _time.text = UIUtils.FormatClock(_remaining);
        }

        private void Skip()
        {
            if (_finished) return;
            _finished = true;
            _tick?.Pause();
            _onSkip?.Invoke();
        }

        public void Stop() { _finished = true; _tick?.Pause(); }
    }
}
