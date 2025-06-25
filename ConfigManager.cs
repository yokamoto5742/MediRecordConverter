using System;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;

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
        public Point GetMainWindowPosition(int windowWidth, int windowHeight)
        {
            try
            {
                var position = MainWindowPosition.ToLower();

                if (position.StartsWith("right"))
                {
                    var coords = position.Replace("right", "").Trim();
                    var parts = coords.Split('+');

                    if (parts.Length >= 3)
                    {
                        var xOffset = int.Parse(parts[1]);
                        var yOffset = int.Parse(parts[2]);

                        var screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
                        var x = screenWidth - windowWidth - xOffset;
                        var y = yOffset;

                        return new Point(x, y);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"位置設定解析エラー: {ex.Message}");
            }

            return new Point(10, 10);
        }
    }
}