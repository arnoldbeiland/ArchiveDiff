using System;
using System.IO;
using System.Xml.Serialization;

namespace ArchiveDiff.Logic
{
    public class Settings
    {
        public class SettingsContainer
        {
            public string OpenerProgram { get; set; }
            public string AttributeFormat { get; set; }
        }

        private SettingsContainer _current;
        private bool _saveIsNeeded;
        private string _filePath;

        public string OpenerProgram
        {
            get => _current.OpenerProgram;
            set
            {
                _current.OpenerProgram = value;
                _saveIsNeeded = true;
            }
        }

        public string AttributeFormat
        {
            get => _current.AttributeFormat;
            set
            {
                _current.AttributeFormat = value;
                _saveIsNeeded = true;
            }
        }

        public Settings()
        {
            var currentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            currentDir = new Uri(currentDir).LocalPath;
            _filePath = Path.Combine(currentDir, "ArchiveDiffSettings.xml");

            if (File.Exists(_filePath))
            {
                try
                {
                    using (var stream = File.Open(_filePath, FileMode.Open))
                    {
                        var serializer = new XmlSerializer(typeof(SettingsContainer));
                        //_current = (SettingsContainer)serializer.Deserialize(stream);
                    }
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            if (_current == null)
            {
                _current = new SettingsContainer
                {
                    OpenerProgram = @"C:\Program Files\Notepad++\notepad++.exe",
                    AttributeFormat = "{0} {1}"
                };
            }
        }

        public void TrySave()
        {
            try
            {
                if (_saveIsNeeded)
                {
                    using (var stream = File.OpenWrite(_filePath))
                    {
                        var serializer = new XmlSerializer(typeof(SettingsContainer));
                        serializer.Serialize(stream, _current);
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
