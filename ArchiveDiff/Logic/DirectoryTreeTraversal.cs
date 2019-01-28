using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArchiveDiff.Logic
{
    public static class DirectoryTreeTraversal
    {
        public class Item
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public ItemType Type { get; set; }
            public int IndentationLevel { get; set; }
        }

        public static List<Item> GetItems(string path)
        {
            var items = new List<Item>();

            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                Dfs(path, 0, items, path);

            return items;
        }

        private static void Dfs(string path, int indentationLevel, List<Item> items, string basePath)
        {
            items.Add(new Item
            {
                Name = new DirectoryInfo(path).Name,
                Path = RemovePrefix(path, basePath),
                Type = ItemType.Directory,
                IndentationLevel = indentationLevel
            });

            foreach (var dir in Directory.GetDirectories(path).OrderBy(x => x))
            {
                Dfs(dir, indentationLevel + 1, items, basePath);
            }

            foreach (var file in Directory.GetFiles(path))
            {
                items.Add(new Item
                {
                    Name = Path.GetFileName(file),
                    Path = RemovePrefix(file, basePath),
                    Type = ItemType.File,
                    IndentationLevel = indentationLevel + 1
                });
            }
        }

        private static string RemovePrefix(string currentPath, string originalPath)
        {
            return currentPath.Substring(originalPath.Length);
        }
    }
}
