using ArchiveDiff.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ArchiveDiff.Ui
{
    public abstract class SimpleCastingConverter<T> : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is T typedValue)
                return Convert(typedValue);

            return null;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        protected abstract object Convert(T value);
    }

    public class BaseVisibilityConverter : SimpleCastingConverter<ComparisonState>
    {
        protected override object Convert(ComparisonState state)
        {
            return state == ComparisonState.Added ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public class CompVisibilityConverter : SimpleCastingConverter<ComparisonState>
    {
        protected override object Convert(ComparisonState state)
        {
            return state == ComparisonState.Deleted ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public class IndentationToMarginConverter : SimpleCastingConverter<int>
    {
        protected override object Convert(int level)
        {
            return new Thickness(10 + 25 * (level - 1), 1, 5, 1);
        }
    }

    public class ItemTypeToImageSourceConverter : SimpleCastingConverter<ItemType>
    {
        protected override object Convert(ItemType type)
        {
            return type == ItemType.Directory
                ? "Images/folder.png"
                : "Images/file.png";
        }
    }

    public class StateBackgroundColorConverter : SimpleCastingConverter<ComparisonState>
    {
        protected override object Convert(ComparisonState state)
        {
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
    }

    public class StateDisplayTextConverter : SimpleCastingConverter<ComparisonState>
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

        protected override object Convert(ComparisonState state)
        {
            if (DisplayTexts.TryGetValue(state, out var result))
                return result;
            else
                return state.ToString();
        }
    }
}
