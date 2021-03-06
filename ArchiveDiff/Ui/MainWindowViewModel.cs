﻿using ArchiveDiff.Logic;
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
        private const string DefaultBaseHeader = "Base";
        private const string DefaultCompHeader = "Compared";

        public ICommand OpenBase { get; set; }
        public ICommand OpenComp { get; set; }
        public ICommand Refresh { get; set; }
        public ICommand DoubleClick { get; set; }
        public ICommand Exchange { get; set; }
        public ICommand DropToBaseFile { get; set; }
        public ICommand DropToCompFile { get; set; }
        public ICommand SaveBaseAs { get; set; }
        public ICommand SaveCompAs { get; set; }
        public ICommand CloseBase { get; set; }
        public ICommand CloseComp { get; set; }
        public ICommand FindInBase { get; set; }
        public ICommand FindInComp { get; set; }

        private string _baseHeader = DefaultBaseHeader; 
        public string BaseHeader
        {
            get => _baseHeader;
            set { _baseHeader = value; NotifyChanged(); }
        }

        private string _compHeader = DefaultCompHeader;
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

        public string DoubleClickProgram
        {
            get => _settings.OpenerProgram;
            set { _settings.OpenerProgram = value; NotifyChanged(); }
        }

        public string DoubleClickProgramArguments
        {
            get => _settings.AttributeFormat;
            set { _settings.AttributeFormat = value; NotifyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isDisposed;
        private ArchiveComparer _comparer;
        private Settings _settings;
        private List<Window> _windowsToClose = new List<Window>();

        public MainWindowViewModel()
        {
            _comparer = new ArchiveComparer();
            _rows = new List<ComparisonRow>();
            _settings = new Settings();

            OpenBase = new DelegateCommand(ExceptionCatchWrapper(OnOpenBaseFile));
            OpenComp = new DelegateCommand(ExceptionCatchWrapper(OnOpenCompFile));
            Refresh = new DelegateCommand(ExceptionCatchWrapper(OnRefresh));
            DoubleClick = new DelegateCommand<ComparisonRow>(ExceptionCatchWrapper<ComparisonRow>(OnDoubleClick));
            Exchange = new DelegateCommand(ExceptionCatchWrapper(OnExchange));
            DropToBaseFile = new DelegateCommand<string>(ExceptionCatchWrapper<string>(ChangeBaseFile));
            DropToCompFile = new DelegateCommand<string>(ExceptionCatchWrapper<string>(ChangeCompFile));
            SaveBaseAs = new DelegateCommand(ExceptionCatchWrapper(OnSaveBaseAs));
            SaveCompAs = new DelegateCommand(ExceptionCatchWrapper(OnSaveCompAs));
            CloseBase = new DelegateCommand(ExceptionCatchWrapper(OnCloseBase));
            CloseComp = new DelegateCommand(ExceptionCatchWrapper(OnCloseComp));
            FindInBase = new DelegateCommand(ExceptionCatchWrapper(OnFindInBase));
            FindInComp = new DelegateCommand(ExceptionCatchWrapper(OnFindInComp));
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                _comparer.Dispose();
                _settings.TrySave();
                while (_windowsToClose.Count > 0)
                    _windowsToClose[0].Close();
            }
        }

        private void NotifyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void OnOpenBaseFile()
        {
            var path = SelectFileToOpen();
            if (path != null)
                ChangeBaseFile(path);
        }

        private void OnOpenCompFile()
        {
            var path = SelectFileToOpen();
            if (path != null)
                ChangeCompFile(path);
        }

        private void ChangeBaseFile(string path)
        {
            Rows = _comparer.ChangeBaseFile(path);
            UpdateColumnHeaders();
        }

        private void ChangeCompFile(string path)
        {
            Rows = _comparer.ChangeCompFile(path);
            UpdateColumnHeaders();
        }

        private void OnRefresh()
        {
            Rows = _comparer.Refresh();
            UpdateColumnHeaders();
        }

        private void OnDoubleClick(ComparisonRow row)
        {
            _comparer.RunProgram(DoubleClickProgram, DoubleClickProgramArguments, row);
        }

        private void OnExchange()
        {
            Rows = null;
            Rows = _comparer.Exchange();

            UpdateColumnHeaders();
        }

        private void UpdateColumnHeaders()
        {
            BaseHeader = string.IsNullOrEmpty(_comparer.BasePath)
                ? DefaultBaseHeader
                : $"{Path.GetFileName(_comparer.BaseArchive)}  [extracted: {_comparer.BasePath}]";

            CompHeader = string.IsNullOrEmpty(_comparer.CompPath)
                ? DefaultCompHeader
                : $"{Path.GetFileName(_comparer.CompArchive)}  [extracted: {_comparer.CompPath}]";
        }

        private string SelectFileToOpen()
        {
            var dialog = new OpenFileDialog();
            var result = dialog.ShowDialog();

            return result == true ? dialog.FileName : null;
        }

        private Action ExceptionCatchWrapper(Action action)
        {
            return () => DoInsideTryCatch(action);
        }

        private Action<T> ExceptionCatchWrapper<T>(Action<T> action)
        {
            return param => DoInsideTryCatch(() => action(param));
        }

        private void DoInsideTryCatch(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
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

        private void OnSaveBaseAs()
        {
            var path = SaveFileDialog(
                Path.GetFileNameWithoutExtension(_comparer.BaseArchive)
                + "_modified"
                + Path.GetExtension(_comparer.BaseArchive));

            if (path != null)
                _comparer.SaveBaseAs(path);
        }

        private void OnSaveCompAs()
        {
            var path = SaveFileDialog(
                Path.GetFileNameWithoutExtension(_comparer.CompArchive)
                + "_modified"
                + Path.GetExtension(_comparer.CompArchive));

            if (path != null)
                _comparer.SaveCompAs(path);
        }

        private string SaveFileDialog(string suggestedName)
        {
            var dialog = new SaveFileDialog { FileName = suggestedName };
            dialog.ShowDialog();

            return string.IsNullOrEmpty(dialog.FileName) ? null : dialog.FileName;
        }

        private void OnCloseBase()
        {
            Rows = _comparer.CloseBase();
            UpdateColumnHeaders();
        }

        private void OnCloseComp()
        {
            Rows = _comparer.CloseComp();
            UpdateColumnHeaders();
        }

        private void OnFindInBase()
        {
            var path = _comparer.BasePath;
            var files = Rows
                .Where(r => r.Type == ItemType.File)
                .Select(r => path + r.RelativePath)
                .ToList();

            OpenFindWindow($"Find: {BaseHeader}", files);
        }

        private void OnFindInComp()
        {
            var path = _comparer.CompPath;
            var files = Rows
                .Where(r => r.Type == ItemType.File)
                .Select(r => path + r.RelativePath)
                .ToList();

            OpenFindWindow($"Find: {CompHeader}", files);
        }

        private void OpenFindWindow(string title, List<string> files)
        {
            var window = new FindWindow(title, _settings.OpenerProgram, files);
            window.Show();

            _windowsToClose.Add(window);
            window.Closed += (s, a) => _windowsToClose.Remove(window);
        }
    }
}
