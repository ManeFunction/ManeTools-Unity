using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Popup calendar for a UTC date or a <see cref="SerializableDateTimeRange"/>.
    /// </summary>
    public sealed class CalendarPopup : EditorWindow
    {
        private const float WindowWidth = 310f;
        private const float TitleChromeFallback = 28f;
        private const float SizeCeiling = 800f;

        private DateTime _selected;
        private Action<DateTime> _onDateSelected;
        private SerializableDateTimeRange _range;
        private Action<SerializableDateTimeRange> _onRangeSelected;
        private bool _editingStart = true;
        private int _year;
        private int _month;

        private VisualElement _calendar;
        private VisualElement _modeRow;
        private Button _startMode;
        private Button _endMode;
        private Label _title;
        private CalendarGrid _grid;
        private SliderInt _hour;
        private SliderInt _minute;
        private Label _utcValue;
        private Label _localValue;
        private Button _today;
        private Button _ok;

        private bool IsRange => _onRangeSelected != null;

        private DateTime Editing
        {
            get => IsRange ? (_editingStart ? _range.Start : _range.End) : _selected;
            set
            {
                DateTime utc = SerializableDateTime.Normalize(value);
                if (!IsRange)
                {
                    _selected = utc;
                    return;
                }

                if (_editingStart)
                {
                    _range.Start = utc;
                    if (_range.Start > _range.End)
                        _range.End = _range.Start;
                    return;
                }

                DateTime previousStart = _range.Start;
                if (utc < previousStart)
                {
                    _range.Start = utc;
                    _range.End = previousStart;
                    return;
                }

                _range.End = utc;
            }
        }

        /// <summary>
        /// Opens a date picker starting at <paramref name="current"/>.
        /// </summary>
        public static void Show(DateTime current, Action<DateTime> onDateSelected)
        {
            CalendarPopup window = GetWindow<CalendarPopup>(true, "Date", true);
            window.ConfigureDate(current, onDateSelected);
        }

        /// <summary>
        /// Opens a range picker starting at <paramref name="current"/>.
        /// </summary>
        public static void ShowRange(
            SerializableDateTimeRange current,
            Action<SerializableDateTimeRange> onRangeSelected)
        {
            CalendarPopup window = GetWindow<CalendarPopup>(true, "Date range", true);
            window.ConfigureRange(current, onRangeSelected);
        }

        private void CreateGUI()
        {
            VisualTreeAsset tree = UIElementsTools.LoadUXML(typeof(CalendarPopup));
            if (tree == null)
            {
                Debug.LogError("CalendarPopup.uxml was not found next to CalendarPopup.");
                return;
            }

            VisualElement root = rootVisualElement;
            root.Clear();
            tree.CloneTree(root);
            ManeEditorStyles.Apply(root, ManeEditorStyles.Options.Sheet);
            root.style.overflow = Overflow.Hidden;
            root.focusable = true;
            root.tabIndex = 0;
            root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            _calendar = root.Q(className: "mie-calendar");
            _modeRow = root.Q("modeRow");
            _startMode = root.Q<Button>("startMode");
            _endMode = root.Q<Button>("endMode");
            _title = root.Q<Label>("monthTitle");
            _grid = root.Q<CalendarGrid>("grid");
            _hour = root.Q<SliderInt>("hour");
            _minute = root.Q<SliderInt>("minute");
            _utcValue = root.Q<Label>("utcValue");
            _localValue = root.Q<Label>("localValue");
            _ok = root.Q<Button>("ok");
            _today = root.Q<Button>("today");
            Button cancel = root.Q<Button>("cancel");
            Button prevYear = root.Q<Button>("prevYear");
            Button prevMonth = root.Q<Button>("prevMonth");
            Button nextMonth = root.Q<Button>("nextMonth");
            Button nextYear = root.Q<Button>("nextYear");

            if (_calendar == null || _modeRow == null || _startMode == null || _endMode == null || _title == null ||
                _grid == null || _hour == null || _minute == null || _utcValue == null || _localValue == null ||
                _ok == null || _today == null || cancel == null || prevYear == null || prevMonth == null ||
                nextMonth == null || nextYear == null)
            {
                Debug.LogError("CalendarPopup UXML is missing required elements.");
                return;
            }

            _hour.lowValue = 0;
            _hour.highValue = 23;
            _hour.showInputField = true;
            _minute.lowValue = 0;
            _minute.highValue = 59;
            _minute.showInputField = true;

            _startMode.clicked += () => SetMode(true);
            _endMode.clicked += () => SetMode(false);
            prevYear.clicked += () => ShiftYear(-1);
            nextYear.clicked += () => ShiftYear(1);
            prevMonth.clicked += () => ShiftMonth(-1);
            nextMonth.clicked += () => ShiftMonth(1);
            _grid.DayClicked += SelectDay;
            _hour.RegisterValueChangedCallback(evt => SetTime(evt.newValue, Editing.Minute));
            _minute.RegisterValueChangedCallback(evt => SetTime(Editing.Hour, evt.newValue));
            _today.clicked += OnTodayClicked;
            cancel.clicked += Close;
            _ok.clicked += Confirm;
            _calendar.RegisterCallback<GeometryChangedEvent>(OnCalendarGeometry);
            _modeRow.style.display = DisplayStyle.None;

            Refresh();
            FitWindow();
            root.Focus();
        }

        private void ConfigureDate(DateTime current, Action<DateTime> onDateSelected)
        {
            _selected = SerializableDateTime.Normalize(current);
            _onDateSelected = onDateSelected;
            _onRangeSelected = null;
            _range = null;
            _year = _selected.Year;
            _month = _selected.Month;
            UnlockSize();
            Refresh();
            FitWindow();
            rootVisualElement.Focus();
        }

        private void ConfigureRange(
            SerializableDateTimeRange current,
            Action<SerializableDateTimeRange> onRangeSelected)
        {
            _range = current != null
                ? new SerializableDateTimeRange(current.Start, current.End)
                : new SerializableDateTimeRange();
            _onRangeSelected = onRangeSelected;
            _onDateSelected = null;
            _editingStart = true;
            _year = _range.Start.Year;
            _month = _range.Start.Month;
            UnlockSize();
            Refresh();
            FitWindow();
            rootVisualElement.Focus();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Escape)
                return;

            Close();
            evt.StopImmediatePropagation();
        }

        private void UnlockSize()
        {
            minSize = new Vector2(WindowWidth, 8f);
            maxSize = new Vector2(WindowWidth, SizeCeiling);
        }

        private void OnCalendarGeometry(GeometryChangedEvent evt) =>
            ApplyWindowSize(evt.newRect.height);

        private void FitWindow()
        {
            if (_calendar == null)
                return;

            _calendar.schedule.Execute(() => ApplyWindowSize(_calendar.layout.height));
        }

        private void ApplyWindowSize(float contentHeight)
        {
            if (!IsUsableLength(contentHeight))
                return;

            float chrome = position.height - rootVisualElement.layout.height;
            if (chrome is < 0f or > 80f)
                chrome = TitleChromeFallback;

            Vector2 size = new(WindowWidth, contentHeight + chrome);
            if (IsUsableLength(minSize.x) && IsUsableLength(minSize.y) &&
                IsUsableLength(maxSize.x) && IsUsableLength(maxSize.y) &&
                Mathf.Abs(minSize.x - size.x) < .5f && Mathf.Abs(minSize.y - size.y) < .5f &&
                Mathf.Abs(maxSize.x - size.x) < .5f && Mathf.Abs(maxSize.y - size.y) < .5f)
                return;

            maxSize = new Vector2(WindowWidth, SizeCeiling);
            minSize = size;
            maxSize = size;
        }

        private static bool IsUsableLength(float value) => value >= 8f && !float.IsInfinity(value);

        private bool IsCurrentMonth
        {
            get
            {
                DateTime now = DateTime.UtcNow;
                return _year == now.Year && _month == now.Month;
            }
        }

        private void SetMode(bool start)
        {
            _editingStart = start;
            DateTime editing = Editing;
            _year = editing.Year;
            _month = editing.Month;
            Refresh();
        }

        private void ShiftMonth(int delta)
        {
            _month += delta;
            while (_month < 1)
            {
                _month += 12;
                _year--;
            }

            while (_month > 12)
            {
                _month -= 12;
                _year++;
            }

            Refresh();
        }

        private void ShiftYear(int delta)
        {
            _year += delta;
            Refresh();
        }

        private void SelectDay(DateTime day)
        {
            DateTime editing = Editing;
            Editing = new DateTime(day.Year, day.Month, day.Day, editing.Hour, editing.Minute, 0, DateTimeKind.Utc);
            _year = day.Year;
            _month = day.Month;
            if (IsRange && _editingStart)
                _editingStart = false;

            Refresh();
        }

        private void SetTime(int hour, int minute)
        {
            DateTime editing = Editing;
            Editing = new DateTime(editing.Year, editing.Month, editing.Day, hour, minute, 0, DateTimeKind.Utc);
            Refresh();
        }

        private void OnTodayClicked()
        {
            DateTime now = SerializableDateTime.Normalize(DateTime.UtcNow);
            if (IsRange)
            {
                if (IsCurrentMonth)
                    return;

                _year = now.Year;
                _month = now.Month;
                Refresh();
                return;
            }

            Editing = now;
            _year = now.Year;
            _month = now.Month;
            Refresh();
        }

        private void Confirm()
        {
            if (IsRange)
                _onRangeSelected?.Invoke(_range);
            else
                _onDateSelected?.Invoke(_selected);

            Close();
        }

        private void Refresh()
        {
            if (_title == null || _grid == null || _today == null)
                return;
            if (_onDateSelected == null && _onRangeSelected == null)
                return;

            _modeRow.style.display = IsRange ? DisplayStyle.Flex : DisplayStyle.None;
            _startMode.EnableInClassList("mie-calendar-mode__button--active", IsRange && _editingStart);
            _endMode.EnableInClassList("mie-calendar-mode__button--active", IsRange && !_editingStart);
            _title.text = $"{_year:0000}-{_month:00}";
            _grid.Bind(_year, _month, Editing, IsRange ? _range.Start : Editing, IsRange ? _range.End : Editing,
                IsRange, _editingStart);

            _today.text = IsRange ? "Today" : "Set today";
            _today.SetEnabled(!IsRange || !IsCurrentMonth);

            if (_hour.value != Editing.Hour)
                _hour.SetValueWithoutNotify(Editing.Hour);
            if (_minute.value != Editing.Minute)
                _minute.SetValueWithoutNotify(Editing.Minute);

            if (IsRange)
            {
                _utcValue.text =
                    $"{SerializableDateTime.FormatDisplay(_range.Start)} – {SerializableDateTime.FormatDisplay(_range.End)}";
                _localValue.text =
                    $"{SerializableDateTime.FormatDisplay(_range.Start.ToLocalTime())} – {SerializableDateTime.FormatDisplay(_range.End.ToLocalTime())}";
            }
            else
            {
                _utcValue.text = SerializableDateTime.FormatDisplay(_selected);
                _localValue.text = SerializableDateTime.FormatDisplay(_selected.ToLocalTime());
            }
        }
    }
}
