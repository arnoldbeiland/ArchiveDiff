using ArchiveDiff.Logic;
using System.Windows;
using System.Windows.Controls;

namespace ArchiveDiff.Ui
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        private bool _isDragSecondHalf;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel = new MainWindowViewModel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _viewModel.Dispose();
        }

        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _viewModel.DoubleClick?.Execute((sender as DataGridRow)?.DataContext as ComparisonRow);
        }

        private void DataGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length == 1)
                {
                    if (_isDragSecondHalf)
                        _viewModel.DropToCompFile.Execute(files[0]);
                    else
                        _viewModel.DropToBaseFile.Execute(files[0]);

                }
                else if (files.Length == 2)
                {
                    _viewModel.DropToBaseFile.Execute(files[0]);
                    _viewModel.DropToCompFile.Execute(files[1]);
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
