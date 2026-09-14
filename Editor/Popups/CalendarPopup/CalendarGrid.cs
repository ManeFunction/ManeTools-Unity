using System;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [UxmlElement]
    public sealed partial class CalendarGrid : VisualElement
    {
        public const string UssClassName = "mie-calendar-grid-root";
        public const int Cells = 42;
        public const int Columns = 7;

        private static readonly string[] DayNames = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

        private readonly Button[] _days = new Button[Cells];
        private readonly DateTime[] _dates = new DateTime[Cells];

        private bool _rangeMode;
        private bool _editingStart;
        private DateTime _rangeStart;
        private DateTime _rangeEnd;
        private DateTime _selected;
        private int _month;
        private DateTime? _hover;

        public event Action<DateTime> DayClicked;

        public CalendarGrid()
        {
            AddToClassList(UssClassName);

            VisualElement weekdays = new();
            weekdays.AddToClassList("mie-calendar-weekdays");
            for (int i = 0; i < DayNames.Length; i++)
            {
                Label day = new(DayNames[i]);
                day.AddToClassList("mie-calendar-weekday");
                weekdays.Add(day);
            }

            Add(weekdays);

            VisualElement grid = new();
            grid.AddToClassList("mie-calendar-grid");
            for (int i = 0; i < Cells; i++)
            {
                int index = i;
                Button day = new();
                day.AddToClassList("mie-button");
                day.AddToClassList("mie-calendar-day");
                day.clicked += () => DayClicked?.Invoke(_dates[index]);
                day.RegisterCallback<PointerEnterEvent>(_ => OnDayEnter(index));
                _days[i] = day;
                grid.Add(day);
            }

            grid.RegisterCallback<PointerLeaveEvent>(_ => OnGridLeave());
            Add(grid);
        }

        public void Bind(int year, int month, DateTime selected, DateTime rangeStart, DateTime rangeEnd,
            bool rangeMode, bool editingStart)
        {
            DateTime first = new(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            int offset = ((int)first.DayOfWeek + 6) % Columns;
            int trailing = Cells - offset - DateTime.DaysInMonth(year, month);
            if (offset == 0 && trailing > Columns)
                offset = Columns;

            DateTime gridStart = first.AddDays(-offset);
            DateTime? hover = _hover;
            _rangeMode = rangeMode;
            _editingStart = editingStart;
            _rangeStart = rangeStart.Date;
            _rangeEnd = rangeEnd.Date;
            _selected = selected.Date;
            _month = month;
            _hover = null;

            for (int i = 0; i < Cells; i++)
            {
                DateTime cell = gridStart.AddDays(i);
                _dates[i] = cell;
                _days[i].text = cell.Day.ToString();
                if (hover == cell.Date)
                    _hover = hover;
            }

            ApplyHighlight();
        }

        private void OnDayEnter(int index)
        {
            if (!_rangeMode)
                return;

            DateTime date = _dates[index].Date;
            if (_hover == date)
                return;

            _hover = date;
            ApplyHighlight();
        }

        private void OnGridLeave()
        {
            if (_hover == null)
                return;

            _hover = null;
            ApplyHighlight();
        }

        private void ApplyHighlight()
        {
            DateTime today = DateTime.UtcNow.Date;
            DateTime from = _rangeStart;
            DateTime to = _rangeEnd;
            if (_rangeMode && _hover.HasValue)
            {
                if (_editingStart)
                    from = _hover.Value;
                else
                    to = _hover.Value;
            }

            if (from > to)
                (from, to) = (to, from);

            for (int i = 0; i < Cells; i++)
            {
                DateTime cell = _dates[i].Date;
                Button day = _days[i];
                bool endpoint = _rangeMode && (cell == from || cell == to);
                bool inner = _rangeMode && cell > from && cell < to;
                bool selectedCell = !_rangeMode && cell == _selected;
                day.EnableInClassList("mie-calendar-day--outside", _dates[i].Month != _month);
                day.EnableInClassList("mie-calendar-day--today", cell == today && !endpoint && !selectedCell);
                day.EnableInClassList("mie-calendar-day--in-range", inner);
                day.EnableInClassList("mie-calendar-day--selected", endpoint || selectedCell);
            }
        }
    }
}
