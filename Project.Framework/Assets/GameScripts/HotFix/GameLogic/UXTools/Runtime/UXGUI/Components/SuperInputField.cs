using System;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/SuperInputField", 18)]
    public class SuperInputField : InputField
    {
        [NonSerialized] private EventTrigger _eventTrigger;
        [NonSerialized] private EventTrigger.Entry _onSelectEntry;
        [NonSerialized] private EventTrigger.Entry _onDeSelectEntry;

        public EventTrigger.TriggerEvent onSelect;
        public EventTrigger.TriggerEvent onDeSelect;

        protected override void Awake()
        {
            base.Awake();
            InitEventTrigger();
        }

        public void Refresh()
        {
            UpdateLabel();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            onSelect?.RemoveAllListeners();
            onDeSelect?.RemoveAllListeners();
            onSelect = null;
            onDeSelect = null;

            if (_eventTrigger != null)
            {
                _eventTrigger.triggers.Clear();
                _eventTrigger = null;
            }
        }

        private void InitEventTrigger()
        {
            _eventTrigger = GetComponent<EventTrigger>();
            if (_eventTrigger == null)
            {
                _eventTrigger = gameObject.AddComponent<EventTrigger>();
            }

            for (var i = 0; i < _eventTrigger.triggers.Count; i++)
            {
                var entry = _eventTrigger.triggers[i];
                if (entry.eventID == EventTriggerType.Select)
                {
                    _onSelectEntry = entry;
                }
                else if (entry.eventID == EventTriggerType.Deselect)
                {
                    _onDeSelectEntry = entry;
                }
            }

            _onSelectEntry ??= new EventTrigger.Entry { eventID = EventTriggerType.Select };
            _onDeSelectEntry ??= new EventTrigger.Entry { eventID = EventTriggerType.Deselect };

            if (!_eventTrigger.triggers.Contains(_onSelectEntry))
            {
                _eventTrigger.triggers.Add(_onSelectEntry);
            }

            if (!_eventTrigger.triggers.Contains(_onDeSelectEntry))
            {
                _eventTrigger.triggers.Add(_onDeSelectEntry);
            }

            onSelect = _onSelectEntry.callback;
            onDeSelect = _onDeSelectEntry.callback;
        }
    }
}
