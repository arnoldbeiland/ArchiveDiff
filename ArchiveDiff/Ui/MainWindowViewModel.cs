using ArchiveDiff.Logic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ArchiveDiff.Ui
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public ICommand OpenBase { get; set; }
        public ICommand OpenComp { get; set; }
        public ICommand Refresh { get; set; }
        public ICommand DoubleClick { get; set; }
        public ICommand Exchange { get; set; }

        private string _baseHeader = "Base"; 
        public string BaseHeader
        {
            get => _baseHeader;
            set { _baseHeader = value; NotifyChanged(); }
        }

        private string _compHeader = "Compared";
        public string CompHeader
        {
            get => _compHeader;
            set { _compHeader = value; NotifyChanged(); }
        }

        private List<ComparisonRow> _rows;
        public List<ComparisonRow> Rows
        {
            get => _rows;
            set { _rows = value; NotifyChanged(); }
        }

        private string _doubleClickProgram = @"C:\Program Files\Notepad++\notepad++.exe";
        public string DoubleClickProgram
        {
            get => _doubleClickProgram;
            set { _doubleClickProgram = value; NotifyChanged(); }
        }

        private string _doubleClickProgramArguments = @"{0} {1}";
        public string DoubleClickProgramArguments
        {
            get => _doubleClickProgramArguments;
            set { _doubleClickProgramArguments = value; NotifyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isDisposed;
        private ArchiveComparer _comparer;

        public MainWindowViewModel()
        {
            _comparer = new ArchiveComparer();
            _rows = new List<ComparisonRow>();

            OpenBase = new DelegateCommand(ExceptionCatchWrapper(ChangeBaseFile));
            OpenComp = new DelegateCommand(ExceptionCatchWrapper(ChangeCompFile));
            Refresh = new DelegateCommand(ExceptionCatchWrapper(OnRefresh));
            DoubleClick = new DelegateCommand<ComparisonRow>((row) => ExceptionCatchWrapper(() => OnDoubleClick(row))());
            Exchange = new DelegateCommand(ExceptionCatchWrapper(OnExchange));
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                _comparer.Dispose();
            }
        }

        private void NotifyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ChangeBaseFile()
        {
            var path = SelectFileToOpen();
            if (path == null)
                return;

            Rows = _comparer.ChangeBaseFile(path);
            BaseHeader = $"{Path.GetFileName(path)}  [extracted: {_comparer.BasePath}]";
        }

        private void ChangeCompFile()
        {
            var path = SelectFileToOpen();
            if (path == null)
                return;

            Rows = _comparer.ChangeCompFile(path);
            CompHeader = $"{Path.GetFileName(path)}  [extracted: {_comparer.CompPath}]";
        }

        private void OnRefresh()
        {
            Rows = _comparer.Refresh();

            if (!string.IsNullOrEmpty(_comparer.BasePath))
                BaseHeader = $"{Path.GetFileName(_comparer.BaseArchive)}  [extracted: {_comparer.BasePath}]";
            if (!string.IsNullOrEmpty(_comparer.CompPath))
                CompHeader = $"{Path.GetFileName(_comparer.CompArchive)}  [extracted: {_comparer.CompPath}]";
        }

        private void OnExchange()
        {
            var tmp = BaseHeader;
            BaseHeader = CompHeader;
            CompHeader = tmp;

            Rows = null;
            Rows = _comparer.Exchange();
        }

        private string SelectFileToOpen()
        {
            var dialog = new OpenFileDialog();
            var result = dialog.ShowDialog();

            return result == true ? dialog.FileName : null;
        }

        private Action ExceptionCatchWrapper(Action action)
        {
            return () =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            };
        }

        private void HandleException(Exception ex, bool tryDisposing = true)
        {
            MessageBox.Show(ex.Message, "Error");

            if (tryDisposing)
            {
                try
                {
                    Dispose();
                }
                catch (Exception anotherEx)
                {
                    HandleException(anotherEx, false);
                }
            }

            Application.Current.Shutdown();
        }

        private void OnDoubleClick(ComparisonRow row)
        {
            _comparer.RunProgram(DoubleClickProgram, DoubleClickProgramArguments, row);
        }
    }
}
