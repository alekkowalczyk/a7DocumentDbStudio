using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;
using System.Windows.Controls.Primitives;
using Calendar = System.Windows.Controls.Calendar;
using a7DocumentDbStudio.Utils;
using a7ExtensionMethods;

namespace a7DocumentDbStudio.Controls
{
    public class a7DateTimePicker : DateTimePicker
    {
        public static readonly DependencyProperty HasTimeProperty =
            DependencyProperty.Register("HasTime", typeof(bool), typeof(a7DateTimePicker), new PropertyMetadata(true));

        public bool HasTime
        {
            get { return (bool)GetValue(HasTimeProperty); }
            set { SetValue(HasTimeProperty, value); }
        }


        public bool IsTextReadOnly
        {
            get { return (bool)GetValue(IsTextReadOnlyProperty); }
            set { SetValue(IsTextReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTextReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTextReadOnlyProperty =
            DependencyProperty.Register("IsTextReadOnly", typeof(bool), typeof(a7DateTimePicker), new PropertyMetadata(false));



        public bool TwoDatesSelectable
        {
            get { return (bool)GetValue(TwoDatesSelectableProperty); }
            set { SetValue(TwoDatesSelectableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TwoDatesSelectable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TwoDatesSelectableProperty =
            DependencyProperty.Register("TwoDatesSelectable", typeof(bool), typeof(a7DateTimePicker), new PropertyMetadata(false));



        public DateTime? Value2
        {
            get { return (DateTime?)GetValue(Value2Property); }
            set { SetValue(Value2Property, value); }
        }

        // Using a DependencyProperty as the backing store for Value2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Value2Property =
            DependencyProperty.Register("Value2", typeof(DateTime?), typeof(a7DateTimePicker), new PropertyMetadata(null));

        //Due to a bug in Visual Studio, you cannot create event handlers for generic T args in XAML, so I have to use object instead.
        public static readonly RoutedEvent Value2ChangedEvent = EventManager.RegisterRoutedEvent("Value2Changed", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(a7DateTimePicker));
        public event RoutedPropertyChangedEventHandler<object> Value2Changed
        {
            add
            {
                AddHandler(Value2ChangedEvent, value);
            }
            remove
            {
                RemoveHandler(Value2ChangedEvent, value);
            }
        }


        public string DateTimeText
        {
            get { return (string)GetValue(DateTimeTextProperty); }
            set { SetValue(DateTimeTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DateTimeText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DateTimeTextProperty =
            DependencyProperty.Register("DateTimeText", typeof(string), typeof(a7DateTimePicker), new PropertyMetadata(null));



        public ICommand ClearValuesCommand
        {
            get { return (ICommand)GetValue(ClearValuesCommandProperty); }
            set { SetValue(ClearValuesCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClearValues.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClearValuesCommandProperty =
            DependencyProperty.Register("ClearValuesCommand", typeof(ICommand), typeof(a7DateTimePicker), new PropertyMetadata(null));

        public ICommand SelectValuesCommand
        {
            get { return (ICommand)GetValue(SelectValuesCommandProperty); }
            set { SetValue(SelectValuesCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectValues.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectValuesCommandProperty =
            DependencyProperty.Register("SelectValuesCommand", typeof(ICommand), typeof(a7DateTimePicker), new PropertyMetadata(null));


        public event EventHandler SelectedDateChanged;


        Calendar _calendar;
        Calendar _calendar2;
        ComboBox _cbMonthSelection;
        Label _lbMonthSelection;
        Button _bSelect;
        ToggleButton _calendarToggleButton;
        bool _opened;
        bool _valueChanged;
        bool _value2Changed;
        private bool _valuesChangedProgrammatically;
        private bool _dateTimeChangedProgrammatically;

        public a7DateTimePicker()
            : base()
        {
            _opened = false;
            _value2Changed = false;
            _valueChanged = false;
            this.Style = ResourcesManager.Instance.GetStyle("DateTimePickerStyle");
            this.Format = DateTimeFormat.Custom;
            this.FormatString = Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern + " HH:mm";
            ClearValuesCommand = new LambdaCommand((o) =>
                {
                    Value = null;
                    Value2 = null;
                    this.IsOpen = false;
                    if (SelectedDateChanged != null)
                        SelectedDateChanged(this, null);
                });
            SelectValuesCommand = new LambdaCommand((o) =>
            {
                IsOpen = false;
                if (SelectedDateChanged != null)
                    SelectedDateChanged(this, null);
            });
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_calendar != null)
                _calendar.SelectedDatesChanged -= _calendar_SelectedDatesChanged;
            if (_calendarToggleButton != null)
                _calendarToggleButton.Checked -= _calendarToggleButton_Checked;

            _calendar = GetTemplateChild("PART_Calendar") as Calendar;
            _calendarToggleButton = GetTemplateChild("_calendarToggleButton") as ToggleButton;

            _cbMonthSelection = GetTemplateChild("PART_MonthSelection") as ComboBox;
            _lbMonthSelection = GetTemplateChild("PART_LablebMonthSelection") as Label;
            _bSelect = GetTemplateChild("PART_bSelect") as Button;

            if (_cbMonthSelection != null && _lbMonthSelection != null && _bSelect != null)
            {
                _cbMonthSelection.ItemsSource = new Dictionary<int, string>
                {
                    [1] = "January",
                    [2] = "February",
                    [3] = "March",
                    [4] = "April",
                    [5] = "May",
                    [6] = "June",
                    [7] = "July",
                    [8] = "August",
                    [9] = "September",
                    [10] = "October",
                    [11] = "November",
                    [12] = "December"
                };
                _cbMonthSelection.SelectionChanged += _cbMonthSelection_SelectionChanged;
                if (TwoDatesSelectable)
                {
                    _cbMonthSelection.Visibility = System.Windows.Visibility.Visible;
                    _lbMonthSelection.Visibility = System.Windows.Visibility.Visible;
                    _bSelect.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    _cbMonthSelection.Visibility = System.Windows.Visibility.Collapsed;
                    _lbMonthSelection.Visibility = System.Windows.Visibility.Collapsed;
                    _bSelect.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

            if (_calendarToggleButton != null)
            {
                _calendarToggleButton.Checked += _calendarToggleButton_Checked;
            }

            if (_calendar != null)
            {
                _calendar.SelectedDatesChanged += _calendar_SelectedDatesChanged;
                _calendar.SelectedDate = Value ?? null;
                _calendar.DisplayDate = Value ?? DateTime.Now;
            }

            if (_calendar2 != null)
                _calendar2.SelectedDatesChanged -= _calendar2_SelectedDatesChanged;

            _calendar2 = GetTemplateChild("PART_Calendar2") as Calendar;

            if (_calendar2 != null)
            {
                _calendar2.SelectedDatesChanged += _calendar2_SelectedDatesChanged;
                _calendar2.SelectedDate = Value ?? null;
                _calendar2.DisplayDate = Value ?? DateTime.Now;
            }
        }

        void _cbMonthSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_cbMonthSelection.SelectedItem is KeyValuePair<int, string>)
            {
                var itemMonth = (KeyValuePair<int, string>)_cbMonthSelection.SelectedItem ;

                if (itemMonth.Key > 0)
                {
                    DateTime startDate = _calendar.SelectedDate ?? DateTime.Now;
                    DateTime beginningOfMonth = new DateTime(startDate.Year, itemMonth.Key, 1);
                    DateTime endOfMonth = new DateTime(beginningOfMonth.Year, beginningOfMonth.Month, DateTime.DaysInMonth(beginningOfMonth.Year, beginningOfMonth.Month));

                    _calendar.SelectedDate = beginningOfMonth;
                    _calendar.DisplayDate = beginningOfMonth;
                    _calendar2.SelectedDate = endOfMonth;
                    _calendar2.DisplayDate = endOfMonth;
                }
            }
        }

        void _calendarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (Value == null)
            {

                if (TwoDatesSelectable)
                    _calendar.DisplayDate = DateTime.Now - TimeSpan.FromDays(30);
                else
                {
                    _calendar.DisplayDate = DateTime.Now;
                }
            }
            if (TwoDatesSelectable && Value2 == null)
                _calendar2.DisplayDate = DateTime.Now;
        }

        void _calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        void _calendar2_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var newDate = (DateTime?)e.AddedItems[0];

                if ((Value2 != null) && (newDate != null) && newDate.HasValue)
                {
                    // Only change the year, month, and day part of the value. Keep everything to the last "tick."
                    // "Milliseconds" aren't precise enough. Use a mathematical scheme instead.
                    newDate = newDate.Value.Date + Value2.Value.TimeOfDay;
                }

                if (!object.Equals(newDate, Value2))
                    Value2 = newDate;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == HasTimeProperty)
            {
                if (e.NewValue.ToBool(true))
                    this.FormatString = Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern + " HH:mm";
                else
                    this.FormatString = Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern;
            }
            else if (e.Property == TwoDatesSelectableProperty)
            {
                if (!TwoDatesSelectable)
                    Value2 = null;

                if (_cbMonthSelection != null && _lbMonthSelection != null && _bSelect != null)
                {
                    if (TwoDatesSelectable)
                    {
                        _lbMonthSelection.Visibility = System.Windows.Visibility.Visible;
                        _cbMonthSelection.Visibility = System.Windows.Visibility.Visible;
                        _bSelect.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        _lbMonthSelection.Visibility = System.Windows.Visibility.Collapsed;
                        _cbMonthSelection.Visibility = System.Windows.Visibility.Collapsed;
                        _bSelect.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            }
            else if (e.Property == ValueProperty)
            {
                if (!_valuesChangedProgrammatically)
                {
                    _dateTimeChangedProgrammatically = true;
                    this.DateTimeText = valueToText(Value, Value2);
                    if (!TwoDatesSelectable && SelectedDateChanged != null)
                    {
                        IsOpen = false;
                        SelectedDateChanged(this, null);
                    }
                    _dateTimeChangedProgrammatically = false;
                }
            }
            else if (e.Property == Value2Property)
            {
                if (!_valuesChangedProgrammatically)
                {
                    _dateTimeChangedProgrammatically = true;
                    this.DateTimeText = valueToText(Value, Value2);
                    _dateTimeChangedProgrammatically = false;
                }
            }
            else if (e.Property == DateTimeTextProperty)
            {
                if (!_dateTimeChangedProgrammatically)
                {
                    _valuesChangedProgrammatically = true;
                    DateTime? val1, val2;
                    textToValue(DateTimeText, out val1, out val2);
                    Value = val1;
                    Value2 = val2;
                    _valuesChangedProgrammatically = false;
                }
            }
            else if (e.Property == IsOpenProperty)
            {
                if (IsOpen)
                    _opened = true;
                else
                {
                    _opened = false;
                    _valueChanged = false;
                    _value2Changed = false;
                }
            }
            base.OnPropertyChanged(e);
        }

        private string valueToText(DateTime? val1, DateTime? val2)
        {
            if (!TwoDatesSelectable)
            {
                if (val1 != null)
                    return val1.Value.ToString(this.FormatString);
                else
                    return "";
            }
            else
            {
                if (val1 != null && val2 != null)
                    return val1.Value.ToString(this.FormatString) + ";" + val2.Value.ToString(this.FormatString);
                else if (val1 != null)
                    return val1.Value.ToString(this.FormatString) + ";";
                else if (val2 != null)
                    return ";" + val2.Value.ToString(this.FormatString);
                else
                    return "";
            }
        }

        private void textToValue(string srcValue,
            out DateTime? val1, out DateTime? val2)
        {
            val1 = null;
            val2 = null;
            // ReSharper disable InconsistentNaming
            DateTime val1_;
            // ReSharper restore InconsistentNaming
            if (!TwoDatesSelectable)
            {
                bool success = DateTime.TryParseExact(srcValue, FormatString, this.CultureInfo, DateTimeStyles.None, out val1_);
                if (success)
                {
                    val1 = val1_;
                }
            }
            else
            {
                if (srcValue.IndexOf(";", StringComparison.Ordinal) >= 0)
                {
                    string[] srcValues = srcValue.Split(';');
                    bool success = DateTime.TryParseExact(srcValues[0], FormatString, this.CultureInfo, DateTimeStyles.None, out val1_);
                    // ReSharper disable InconsistentNaming
                    DateTime val2_;
                    // ReSharper restore InconsistentNaming
                    bool success2 = DateTime.TryParseExact(srcValues[1], FormatString, this.CultureInfo, DateTimeStyles.None, out val2_);
                    if (success && success2)
                    {
                        val1 = val1_;
                        val2 = val2_;
                    }
                }
                else
                {
                    bool success = DateTime.TryParseExact(srcValue, FormatString, this.CultureInfo, DateTimeStyles.None, out val1_);
                    if (success)
                    {
                        val1 = val1_;
                    }
                }
            }
        }
    }
}
