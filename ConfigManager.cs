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

                // デバッグ情報を出力
                System.Diagnostics.Debug.WriteLine($"設定読み込み完了:");
                System.Diagnostics.Debug.WriteLine($"  EditorWidth: {EditorWidth}");
                System.Diagnostics.Debug.WriteLine($"  EditorHeight: {EditorHeight}");
                System.Diagnostics.Debug.WriteLine($"  EditorWindowPosition: '{EditorWindowPosition}'");
                System.Diagnostics.Debug.WriteLine($"  MainWindowPosition: '{MainWindowPosition}'");
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
            System.Diagnostics.Debug.WriteLine($"設定取得: {key} = '{value}' (デフォルト: '{defaultValue}') → 結果: '{result}'");
            return result;
        }

        /// <summary>
        /// 位置設定文字列を解析してPointを返す汎用メソッド
        /// </summary>
        /// <param name="positionString">位置設定文字列（例："+10+10", "right+10+180"）</param>
        /// <param name="windowWidth">ウィンドウ幅</param>
        /// <param name="windowHeight">ウィンドウ高さ</param>
        /// <param name="defaultX">デフォルトX座標</param>
        /// <param name="defaultY">デフォルトY座標</param>
        /// <returns>計算されたPoint</returns>
        private Point ParseWindowPosition(string positionString, int windowWidth, int windowHeight, int defaultX = 10, int defaultY = 10)
        {
            System.Diagnostics.Debug.WriteLine($"位置設定解析開始: '{positionString}', windowSize=({windowWidth},{windowHeight}), default=({defaultX},{defaultY})");

            try
            {
                if (string.IsNullOrWhiteSpace(positionString))
                {
                    System.Diagnostics.Debug.WriteLine("位置設定が空文字列のためデフォルト位置を使用");
                    return new Point(defaultX, defaultY);
                }

                var position = positionString.ToLower().Trim();
                System.Diagnostics.Debug.WriteLine($"正規化後の位置設定: '{position}'");

                if (position.StartsWith("right"))
                {
                    var coords = position.Replace("right", "").Trim();
                    var parts = coords.Split('+');
                    System.Diagnostics.Debug.WriteLine($"right形式解析: parts=[{string.Join(",", parts)}]");

                    if (parts.Length >= 3)
                    {
                        var xOffset = int.Parse(parts[1]);
                        var yOffset = int.Parse(parts[2]);

                        var screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
                        var x = screenWidth - windowWidth - xOffset;
                        var y = yOffset;

                        System.Diagnostics.Debug.WriteLine($"right形式計算結果: ({x},{y})");
                        return new Point(x, y);
                    }
                }
                else if (position.StartsWith("+"))
                {
                    var parts = position.Split('+');
                    System.Diagnostics.Debug.WriteLine($"+形式解析: parts=[{string.Join(",", parts)}], length={parts.Length}");

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
                    System.Diagnostics.Debug.WriteLine($"その他形式解析: parts=[{string.Join(",", parts)}]");

                    if (parts.Length >= 2)
                    {
                        var x = int.Parse(parts[0]);
                        var y = int.Parse(parts[1]);

                        System.Diagnostics.Debug.WriteLine($"その他形式計算結果: ({x},{y})");
                        return new Point(x, y);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"位置設定解析エラー: {ex.Message}");
            }

            System.Diagnostics.Debug.WriteLine($"解析失敗、デフォルト位置を使用: ({defaultX},{defaultY})");
            return new Point(defaultX, defaultY);
        }

        /// <summary>
        /// メインウィンドウの位置を取得
        /// </summary>
        public Point GetMainWindowPosition(int windowWidth, int windowHeight)
        {
            return ParseWindowPosition(MainWindowPosition, windowWidth, windowHeight, 10, 10);
        }

        /// <summary>
        /// エディターウィンドウの位置を取得
        /// </summary>
        public Point GetEditorWindowPosition(int windowWidth, int windowHeight)
        {
            System.Diagnostics.Debug.WriteLine($"エディターウィンドウ位置取得: EditorWindowPosition='{EditorWindowPosition}', size=({windowWidth},{windowHeight})");

            // App.configの設定値を直接使用し、解析失敗時のみ画面中央をデフォルトにする
            var result = ParseWindowPosition(EditorWindowPosition, windowWidth, windowHeight, 0, 0);

            // ParseWindowPositionで(0,0)が返された場合は解析が失敗したと判断し、画面中央に設定
            if (result.X == 0 && result.Y == 0 && EditorWindowPosition != "+0+0")
            {
                var screenBounds = Screen.PrimaryScreen.WorkingArea;
                var centerX = (screenBounds.Width - windowWidth) / 2;
                var centerY = (screenBounds.Height - windowHeight) / 2;

                System.Diagnostics.Debug.WriteLine($"解析失敗のため画面中央に設定: ({centerX},{centerY})");
                return new Point(centerX, centerY);
            }

            System.Diagnostics.Debug.WriteLine($"エディターウィンドウ位置決定: ({result.X},{result.Y})");
            return result;
        }
    }
}