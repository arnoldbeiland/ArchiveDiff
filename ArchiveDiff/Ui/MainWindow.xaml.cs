using ArchiveDiff.Logic;
using System.Windows;
using System.Windows.Controls;

namespace ArchiveDiff.Ui
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (DataContext as MainWindowViewModel)?.Dispose();
        }

        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            (DataContext as MainWindowViewModel)?.DoubleClick?.Execute((sender as DataGridRow)?.DataContext as ComparisonRow);
        }
    }
}
