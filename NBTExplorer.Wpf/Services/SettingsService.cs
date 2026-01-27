using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace NBTExplorer.Wpf.Services
{
    public class AppSettings
    {
        public List<string> RecentFiles { get; set; } = new List<string>();
        public List<string> RecentDirectories { get; set; } = new List<string>();
    }

    public class SettingsService
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NBTExplorer",
            "settings.json");

        private AppSettings _settings;

        public SettingsService()
        {
            Load();
        }

        public AppSettings Settings => _settings;

        public void Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json);
                }
                else
                {
                    _settings = new AppSettings();
                }
            }
            catch
            {
                _settings = new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }

        public void AddRecentFile(string path)
        {
            if (_settings.RecentFiles.Contains(path))
            {
                _settings.RecentFiles.Remove(path);
            }

            _settings.RecentFiles.Insert(0, path);

            while (_settings.RecentFiles.Count > 10)
            {
                _settings.RecentFiles.RemoveAt(_settings.RecentFiles.Count - 1);
            }

            Save();
        }

        public void AddRecentDirectory(string path)
        {
            if (_settings.RecentDirectories.Contains(path))
            {
                _settings.RecentDirectories.Remove(path);
            }

            _settings.RecentDirectories.Insert(0, path);

            while (_settings.RecentDirectories.Count > 10)
            {
                _settings.RecentDirectories.RemoveAt(_settings.RecentDirectories.Count - 1);
            }

            Save();
        }

        public void AddRecent(string path)
        {
            if (Directory.Exists(path))
            {
                AddRecentDirectory(path);
            }
            else if (File.Exists(path))
            {
                AddRecentFile(path);
            }
        }

        public void ClearRecentFiles()
        {
            _settings.RecentFiles.Clear();
            Save();
        }

        public void ClearRecentDirectories()
        {
            _settings.RecentDirectories.Clear();
            Save();
        }
    }
}
