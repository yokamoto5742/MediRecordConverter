using System;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;

namespace MediRecordConverter
{
    public class ConfigManager
    {
        public int WindowWidth { get; private set; } = 500;
        public int WindowHeight { get; private set; } = 600;
        public int EditorWidth { get; private set; } = 500;
        public int EditorHeight { get; private set; } = 600;
        public int TextAreaFontSize { get; private set; } = 10;
        public string TextAreaFontName { get; private set; } = "MS Gothic";
        public string MainWindowPosition { get; private set; } = "+10+10";
        public string EditorWindowPosition { get; private set; } = "+10+10";
        public int ButtonWidth { get; private set; } = 100;
        public int ButtonHeight { get; private set; } = 30;
        public string OperationFilePath { get; private set; } = @"C:\Shinseikai\MediRecordConverter\mouseoperation.exe";
        public string SoapCopyFilePath { get; private set; } = @"C:\Shinseikai\MediRecordConverter\soapcopy.exe";
        public int FileCleanupIntervalMinutes { get; private set; } = 60;

        public ConfigManager()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            try
            {
                WindowWidth = GetIntSetting("WindowWidth", 500);
                WindowHeight = GetIntSetting("WindowHeight", 600);
                EditorWidth = GetIntSetting("EditorWidth", 500);
                EditorHeight = GetIntSetting("EditorHeight", 600);
                TextAreaFontSize = GetIntSetting("TextAreaFontSize", 11);
                TextAreaFontName = GetStringSetting("TextAreaFontName", "MS Gothic");
                MainWindowPosition = GetStringSetting("MainWindowPosition", "+10+10");
                EditorWindowPosition = GetStringSetting("EditorWindowPosition", "+10+10");
                ButtonWidth = GetIntSetting("ButtonWidth", 100);
                ButtonHeight = GetIntSetting("ButtonHeight", 30);
                OperationFilePath = GetStringSetting("OperationFilePath", @"C:\Shinseikai\TXT2JSON\mouseoperation.exe");
                SoapCopyFilePath = GetStringSetting("SoapCopyFilePath", @"C:\Shinseikai\TXT2JSON\soapcopy.exe");
                FileCleanupIntervalMinutes = GetIntSetting("FileCleanupIntervalMinutes", 60);

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
            string value = ConfigurationManager.AppSettings[key];
            string result = value ?? defaultValue;
            return result;
        }

        private Point ParseWindowPosition(string positionString, int windowWidth, int windowHeight, int defaultX = 10, int defaultY = 10)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(positionString))
                {
                    return new Point(defaultX, defaultY);
                }

                var position = positionString.ToLower().Trim();

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
                else if (position.StartsWith("+"))
                {
                    var parts = position.Split('+');

                    if (parts.Length >= 3)
                    {
                        // parts[0]は空文字列、parts[1]がx座標、parts[2]がy座標
                        var x = int.Parse(parts[1]);
                        var y = int.Parse(parts[2]);

                        System.Diagnostics.Debug.WriteLine($"+形式計算結果: ({x},{y})");
                        return new Point(x, y);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"+形式の要素数が不足: {parts.Length} < 3");
                    }
                }
                else
                {
                    var parts = position.Split(new char[] { '+', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length >= 2)
                    {
                        var x = int.Parse(parts[0]);
                        var y = int.Parse(parts[1]);

                        return new Point(x, y);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"位置設定解析エラー: {ex.Message}");
            }

            return new Point(defaultX, defaultY);
        }
        public Point GetMainWindowPosition(int windowWidth, int windowHeight)
        {
            return ParseWindowPosition(MainWindowPosition, windowWidth, windowHeight, 10, 10);
        }
        public Point GetEditorWindowPosition(int windowWidth, int windowHeight)
        {
            var result = ParseWindowPosition(EditorWindowPosition, windowWidth, windowHeight, 0, 0);

            if (result.X == 0 && result.Y == 0 && EditorWindowPosition != "+0+0")
            {
                var screenBounds = Screen.PrimaryScreen.WorkingArea;
                var centerX = (screenBounds.Width - windowWidth) / 2;
                var centerY = (screenBounds.Height - windowHeight) / 2;

                return new Point(centerX, centerY);
            }

            return result;
        }
    }
}