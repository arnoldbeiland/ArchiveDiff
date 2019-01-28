using ArchiveDiff.Logic;
using System.Windows;
using System.Windows.Controls;

namespace ArchiveDiff.Ui
{
    public partial class MainWindow : Window
    {
        private bool _isDragSecondHalf;


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

        private void DataGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var vm = (MainWindowViewModel)DataContext;
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length == 1 && _isDragSecondHalf)
                    vm.DropToCompFile.Execute(files[0]);
                else if (files.Length == 1)
                    vm.DropToBaseFile.Execute(files[0]);
                else if (files.Length == 2)
                {
                    vm.DropToBaseFile.Execute(files[0]);
                    vm.DropToCompFile.Execute(files[1]);
                }
            }
        }

        private void Datagrid_DragOver(object sender, DragEventArgs e)
        {
            var grid = (DataGrid) sender;
            var pos = e.GetPosition(grid);

            _isDragSecondHalf = pos.X > grid.ActualWidth / 2;
                
        }
    }
}
