using System.Collections.Generic;
using System.Windows;

namespace ArchiveDiff.Ui
{
    public partial class FindWindow : Window
    {
        private FindWindowViewModel _viewModel;

        public FindWindow(string title, string programToRun, List<string> fileList)
        {
            InitializeComponent();
            Title = title;
            DataContext = _viewModel = new FindWindowViewModel(fileList, programToRun);
        }

        private void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as string;
            _viewModel.OpenFile(item);
        }
    }
}
