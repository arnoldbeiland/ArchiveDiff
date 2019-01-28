namespace ArchiveDiff.Logic
{
    public class ComparisonRow
    {
        public ItemType Type { get; set; }
        public string Name { get; set; }
        public ComparisonState State { get; set; }
        public int IndentationLevel { get; set; }
        public string RelativePath { get; set; }
    }

    public enum ItemType { File, Directory }
    public enum ComparisonState { Blank, Match, WhitespacesChanged, Changed, Added, Deleted }
}
