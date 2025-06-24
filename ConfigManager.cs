using System;
using System.Configuration;

namespace MediRecordConverter
{
    public class ConfigManager
    {
        public int WindowWidth { get; private set; } = 1100;
        public int WindowHeight { get; private set; } = 800;
        public int EditorWidth { get; private set; } = 800;
        public int EditorHeight { get; private set; } = 800;
        public int TextAreaFontSize { get; private set; } = 12;
        public string TextAreaFontName { get; private set; } = "MS Gothic";
        public string MainWindowPosition { get; private set; } = "+10+10";
        public string EditorWindowPosition { get; private set; } = "+10+10";
        public int ButtonWidth { get; private set; } = 120;
        public int ButtonHeight { get; private set; } = 40;
        public string OperationFilePath { get; private set; } = @"C:\Shinseikai\MediRecordConverter\mouseoperation.exe";
        public string SoapCopyFilePath { get; private set; } = @"C:\Shinseikai\MediRecordConverter\soapcopy.exe";

        public ConfigManager()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            try
            {
                WindowWidth = GetIntSetting("WindowWidth", 1100);
                WindowHeight = GetIntSetting("WindowHeight", 800);
                EditorWidth = GetIntSetting("EditorWidth", 800);
                EditorHeight = GetIntSetting("EditorHeight", 800);
                TextAreaFontSize = GetIntSetting("TextAreaFontSize", 12);
                TextAreaFontName = GetStringSetting("TextAreaFontName", "MS Gothic");
                MainWindowPosition = GetStringSetting("MainWindowPosition", "+10+10");
                EditorWindowPosition = GetStringSetting("EditorWindowPosition", "+10+10");
                ButtonWidth = GetIntSetting("ButtonWidth", 120);
                ButtonHeight = GetIntSetting("ButtonHeight", 40);
                OperationFilePath = GetStringSetting("OperationFilePath", @"C:\Shinseikai\TXT2JSON\mouseoperation.exe");
                SoapCopyFilePath = GetStringSetting("SoapCopyFilePath", @"C:\Shinseikai\TXT2JSON\soapcopy.exe");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"設定ファイル読み込みエラー: {ex.Message}");
            }
        }

        private int GetIntSetting(string key, int defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        private string GetStringSetting(string key, string defaultValue)
        {
            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }
    }
}