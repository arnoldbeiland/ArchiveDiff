using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArchiveDiff.Logic
{
    public class DirectoryTreeTraverser
    {
        public class Item
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public ItemType Type { get; set; }
            public int IndentationLevel { get; set; }
        }

        private string _path;
        private List<Item> _items;

        public DirectoryTreeTraverser(string path)
        {
            _path = path;
        }

        public List<Item> GetItems()
        {
            _items = new List<Item>();

            if (!string.IsNullOrEmpty(_path) && Directory.Exists(_path))
                Dfs(_path, 0);

            return _items;
        }

        public void Dfs(string path, int indentationLevel)
        {
            _items.Add(new Item
            {
                Name = new DirectoryInfo(path).Name,
                Path = RemovePrefix(path),
                Type = ItemType.Directory,
                IndentationLevel = indentationLevel
            });

            foreach (var dir in Directory.GetDirectories(path).OrderBy(x => x))
            {
                Dfs(dir, indentationLevel + 1);
            }

            foreach (var file in Directory.GetFiles(path))
            {
                _items.Add(new Item
                {
                    Name = Path.GetFileName(file),
                    Path = RemovePrefix(file),
                    Type = ItemType.File,
                    IndentationLevel = indentationLevel + 1
                });
            }
        }

        public string RemovePrefix(string currentPath)
        {
            return currentPath.Substring(_path.Length);
        }
    }
}
