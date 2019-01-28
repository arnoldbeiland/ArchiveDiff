using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ArchiveDiff.Ui
{
    public class FindWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _searchString;
        public string SearchString
        {
            get => _searchString;
            set { _searchString = value; NotifyChanged(); }
        }

        private List<string> _rows;
        public List<string> Rows
        {
            get => _rows;
            set { _rows = value; NotifyChanged(); }
        }

        public ICommand Find { get; set; }

        private List<string> _fileList;
        private string _programToRun;


        public FindWindowViewModel(List<string> fileList, string programToRun)
        {
            _fileList = fileList;
            _programToRun = programToRun;

            Find = new DelegateCommand(OnFind);
        }

        public void OpenFile(string path)
        {
            Process.Start(_programToRun, path);
        }

        private void NotifyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void OnFind()
        {
            try
            {
                Rows = GetFindResults(_fileList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private List<string> GetFindResults(List<string> paths)
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(SearchString))
                return result;

            foreach (var file in paths)
            {
                try
                {
                    if (File.ReadLines(file).Any(line => line.Contains(SearchString.ToLower())))
                        result.Add(file);
                }
                catch (Exception) { /* ignore */ }
            }

            return result;
        }
    }
}
