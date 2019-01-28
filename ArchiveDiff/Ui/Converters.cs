using ArchiveDiff.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ArchiveDiff.Ui
{
    public class BaseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (ComparisonState)value;
            return state == ComparisonState.Added ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CompVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (ComparisonState)value;
            return state == ComparisonState.Deleted ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IndentationToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var level = (int)value;

            return new Thickness(10 + 25 * (level - 1), 1, 5, 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ItemTypeToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (ItemType)value;

            return type == ItemType.Directory
                ? "Images/folder.png"
                : "Images/file.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StateBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (ComparisonState)value;

            switch (state)
            {
                case ComparisonState.Added:
                    return new SolidColorBrush(Color.FromRgb(193, 252, 153));
                case ComparisonState.Deleted:
                    return new SolidColorBrush(Color.FromRgb(255, 105, 94));
                case ComparisonState.Changed:
                    return new SolidColorBrush(Color.FromRgb(229, 200, 0));
                case ComparisonState.WhitespacesChanged:
                    return new SolidColorBrush(Color.FromRgb(255, 252, 175));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StateDisplayTextConverter : IValueConverter
    {
        private static readonly Dictionary<ComparisonState, string> DisplayTexts = new Dictionary<ComparisonState, string>
        {
            [ComparisonState.Added] = "ADD",
            [ComparisonState.Deleted] = "DEL",
            [ComparisonState.Changed] = "CHG",
            [ComparisonState.Match] = "MATCH",
            [ComparisonState.Blank] = "",
            [ComparisonState.WhitespacesChanged] = "WS"
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (ComparisonState)value;

            if (DisplayTexts.TryGetValue(state, out var result))
                return result;
            else
                return state.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
