using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace ArchiveDiff.Logic
{
    public class ArchiveComparer : IDisposable
    {
        private List<ComparisonRow> _comparisonState;

        public string BasePath { get; private set; }
        public string CompPath { get; private set; }
        public string BaseArchive { get; private set; }
        public string CompArchive { get; private set; }

        public ArchiveComparer()
        {
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(BasePath))
                Directory.Delete(BasePath, true);

            if (!string.IsNullOrEmpty(CompPath))
                Directory.Delete(CompPath, true);
        }

        public List<ComparisonRow> ChangeBaseFile(string path)
        {
            if (!string.IsNullOrEmpty(BasePath))
                Directory.Delete(BasePath, true);

            BaseArchive = path;
            BasePath = OpenArchiveToTempPath(path);
            RunComparison();
            return _comparisonState;
        }

        public List<ComparisonRow> ChangeCompFile(string path)
        {
            if (!string.IsNullOrEmpty(CompPath))
                Directory.Delete(CompPath, true);

            CompArchive = path;
            CompPath = OpenArchiveToTempPath(path);
            RunComparison();
            return _comparisonState;
        }

        public List<ComparisonRow> Refresh()
        {
            if (!string.IsNullOrEmpty(BasePath))
            {
                Directory.Delete(BasePath, true);
                BasePath = OpenArchiveToTempPath(BaseArchive);
            }

            if (!string.IsNullOrEmpty(CompPath))
            {
                Directory.Delete(CompPath, true);
                CompPath = OpenArchiveToTempPath(CompArchive);
            }

            RunComparison();

            return _comparisonState;
        }

        public void RunProgram(string program, string argumentFormat, ComparisonRow row)
        {
            if (row.Type != ItemType.File)
                return;

            var baseFile = string.IsNullOrEmpty(BasePath) || row.State == ComparisonState.Added
                ? string.Empty
                : BasePath + row.RelativePath;

            var compFile = string.IsNullOrEmpty(CompPath) || row.State == ComparisonState.Deleted
               ? string.Empty
               : CompPath + row.RelativePath;

            var args = string.Format(argumentFormat, baseFile, compFile);
            Process.Start(program, args);
        }

        public List<ComparisonRow> Exchange()
        {
            var tmp = BaseArchive;
            BaseArchive = CompArchive;
            CompArchive = tmp;

            tmp = BasePath;
            BasePath = CompPath;
            CompPath = tmp;

            foreach (var row in _comparisonState)
            {
                if (row.State == ComparisonState.Added)
                    row.State = ComparisonState.Deleted;
                else if (row.State == ComparisonState.Deleted)
                    row.State = ComparisonState.Added;
            }

            return _comparisonState;
        }

        private void RunComparison()
        {
            _comparisonState = new List<ComparisonRow>();

            var baseList = new DirectoryTreeTraverser(BasePath).GetItems();
            var compList = new DirectoryTreeTraverser(CompPath).GetItems();

            var baseEnumerator = new EnumeratorWithHasCurrent<DirectoryTreeTraverser.Item>(baseList.Skip(1).GetEnumerator());
            var compEnumerator = new EnumeratorWithHasCurrent<DirectoryTreeTraverser.Item>(compList.Skip(1).GetEnumerator());

            while (baseEnumerator.HasCurrent || compEnumerator.HasCurrent)
                GetNextComparisonRow(baseEnumerator, compEnumerator);
        }

        private void GetNextComparisonRow(
            EnumeratorWithHasCurrent<DirectoryTreeTraverser.Item> baseEnumerator,
            EnumeratorWithHasCurrent<DirectoryTreeTraverser.Item> compEnumerator)
        {
            if (!baseEnumerator.HasCurrent)
            {
                AddItem(compEnumerator.Current, ComparisonState.Added);
                compEnumerator.MoveNext();
            }
            else if (!compEnumerator.HasCurrent)
            {
                AddItem(baseEnumerator.Current, ComparisonState.Deleted);
                baseEnumerator.MoveNext();
            }
            else
            {
                var compareResult = CompareItems(baseEnumerator.Current, compEnumerator.Current);

                if (compareResult < 0)
                {
                    AddItem(baseEnumerator.Current, ComparisonState.Deleted);
                    baseEnumerator.MoveNext();
                }
                else if (compareResult > 0)
                {
                    AddItem(compEnumerator.Current, ComparisonState.Added);
                    compEnumerator.MoveNext();
                }
                else
                {
                    if (baseEnumerator.Current.Type == ItemType.Directory)
                        AddItem(baseEnumerator.Current, ComparisonState.Blank);
                    else if (CheckExactFileMatch(baseEnumerator.Current.Path, compEnumerator.Current.Path))
                        AddItem(baseEnumerator.Current, ComparisonState.Match);
                    else if (CheckWhitespaceFileMatch(baseEnumerator.Current.Path, compEnumerator.Current.Path))
                        AddItem(baseEnumerator.Current, ComparisonState.WhitespacesChanged);
                    else
                        AddItem(baseEnumerator.Current, ComparisonState.Changed);

                    baseEnumerator.MoveNext();
                    compEnumerator.MoveNext();
                }
            }
        }

        private void AddItem(DirectoryTreeTraverser.Item item, ComparisonState state)
        {
            _comparisonState.Add(new ComparisonRow
            {
                Name = item.Name,
                Type = item.Type,
                State = state,
                IndentationLevel = item.IndentationLevel,
                RelativePath = item.Path
            });
        }

        private static string OpenArchiveToTempPath(string archivePath = @"C:\arnold\testdata\single_sheet.xlsx")
        {
            var directory = Path.Combine(Path.GetTempPath(), $"ArchiveDiff_{Path.GetRandomFileName()}");
            ZipFile.ExtractToDirectory(archivePath, directory);
            return directory;
        }

        private int CompareItems(DirectoryTreeTraverser.Item x, DirectoryTreeTraverser.Item y)
        {
            var xp = x.Path;
            var yp = y.Path;

            int len = 0;
            while (xp.Length > len && yp.Length > len && xp[len] == yp[len])
                ++len;

            // remove common prefix
            xp = xp.Substring(len);
            yp = yp.Substring(len);

            var isXDir = xp.Contains('\\');
            var isYDir = yp.Contains('\\');

            if (isXDir && !isYDir)
                return -1;
            else if (isYDir && !isXDir)
                return 1;

            return string.Compare(xp, yp, StringComparison.InvariantCulture);
        }

        private bool CheckExactFileMatch(string baseFile, string compFile)
        {
            baseFile = BasePath + baseFile;
            compFile = CompPath + compFile;

            using (FileStream bfile = new FileStream(baseFile, FileMode.Open), cfile = new FileStream(compFile, FileMode.Open))
            {
                using (BufferedStream bstream = new BufferedStream(bfile, 32 << 10), cstream = new BufferedStream(cfile, 32 << 10))
                {
                    int last = 0;
                    while (last != -1)
                    {
                        last = bstream.ReadByte();
                        if (last != cstream.ReadByte())
                            return false;
                    }
                   
                }
            }

            return true;
        }

        private bool CheckWhitespaceFileMatch(string baseFile, string compFile)
        {
            baseFile = BasePath + baseFile;
            compFile = CompPath + compFile;

            using (FileStream bfile = new FileStream(baseFile, FileMode.Open), cfile = new FileStream(compFile, FileMode.Open))
            {
                using (BufferedStream bstream = new BufferedStream(bfile, 32 << 10), cstream = new BufferedStream(cfile, 32 << 10))
                {
                    using (StreamReader breader = new StreamReader(bstream), creader = new StreamReader(cstream))
                    {
                        int blast = breader.Read(), clast = creader.Read();
                        while (blast != -1 || clast != -1)
                        {
                            while (blast != -1 && char.IsWhiteSpace((char)blast))
                                blast = breader.Read();
                            while (clast != -1 && char.IsWhiteSpace((char)clast))
                                clast = creader.Read();

                            if (blast != clast)
                                return false;

                            blast = breader.Read();
                            clast = creader.Read();
                        }
                    }
                }
            }

            return true;
        }
    }
}
