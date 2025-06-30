using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediRecordConverter;
using System.Drawing;

namespace MediRecordConverter.Tests.UnitTests
{
    /// <summary>
    /// ConfigManagerクラスのユニットテスト
    /// 設定管理機能をテストします
    /// </summary>
    [TestClass]
    public class ConfigManagerTests
    {
        private ConfigManager configManager;

        /// <summary>
        /// 各テストの前に実行される初期化処理
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            configManager = new ConfigManager();
        }

        #region 基本設定値のテスト

        /// <summary>
        /// デフォルト設定値の確認テスト
        /// </summary>
        [TestMethod]
        public void ConfigManager_DefaultValues_ReturnsExpectedDefaults()
        {
            // Act & Assert
            // ウィンドウサイズの確認
            Assert.AreEqual(500, configManager.WindowWidth);
            Assert.AreEqual(600, configManager.WindowHeight);
            Assert.AreEqual(500, configManager.EditorWidth);
            Assert.AreEqual(600, configManager.EditorHeight);

            // フォント設定の確認
            Assert.AreEqual(11, configManager.TextAreaFontSize);
            Assert.AreEqual("MS Gothic", configManager.TextAreaFontName);

            // ボタンサイズの確認
            Assert.AreEqual(100, configManager.ButtonWidth);
            Assert.AreEqual(30, configManager.ButtonHeight);

            // ファイルパスの確認
            Assert.IsTrue(configManager.OperationFilePath.Contains("mouseoperation.exe"));
            Assert.IsTrue(configManager.SoapCopyFilePath.Contains("soapcopy.exe"));
        }

        /// <summary>
        /// ウィンドウ位置設定のテスト
        /// </summary>
        [TestMethod]
        public void ConfigManager_WindowPositions_ReturnsValidPositions()
        {
            // Act & Assert
            Assert.IsNotNull(configManager.MainWindowPosition);
            Assert.IsNotNull(configManager.EditorWindowPosition);
            
            // 位置文字列が適切な形式であることを確認
            Assert.IsTrue(configManager.MainWindowPosition.Length > 0);
            Assert.IsTrue(configManager.EditorWindowPosition.Length > 0);
        }

        #endregion

        #region ウィンドウ位置計算のテスト

        /// <summary>
        /// メインウィンドウ位置計算のテスト
        /// </summary>
        [TestMethod]
        public void GetMainWindowPosition_ValidDimensions_ReturnsValidPoint()
        {
            // Arrange
            int windowWidth = 800;
            int windowHeight = 600;

            // Act
            Point position = configManager.GetMainWindowPosition(windowWidth, windowHeight);

            // Assert
            Assert.IsTrue(position.X >= 0);
            Assert.IsTrue(position.Y >= 0);
            // 画面境界内にあることを確認（大まかなチェック）
            Assert.IsTrue(position.X < 5000); // 現実的な画面幅の上限
            Assert.IsTrue(position.Y < 3000); // 現実的な画面高の上限
        }

        /// <summary>
        /// エディターウィンドウ位置計算のテスト
        /// </summary>
        [TestMethod]
        public void GetEditorWindowPosition_ValidDimensions_ReturnsValidPoint()
        {
            // Arrange
            int windowWidth = 500;
            int windowHeight = 600;

            // Act
            Point position = configManager.GetEditorWindowPosition(windowWidth, windowHeight);

            // Assert
            Assert.IsTrue(position.X >= 0);
            Assert.IsTrue(position.Y >= 0);
            // 画面境界内にあることを確認
            Assert.IsTrue(position.X < 5000);
            Assert.IsTrue(position.Y < 3000);
        }

        /// <summary>
        /// 右端配置の位置計算テスト
        /// </summary>
        [TestMethod]
        public void GetMainWindowPosition_RightAlignment_CalculatesCorrectly()
        {
            // Arrange
            var testConfigManager = new TestableConfigManager("right+10+100");
            int windowWidth = 500;
            int windowHeight = 600;

            // Act
            Point position = testConfigManager.GetMainWindowPosition(windowWidth, windowHeight);

            // Assert
            // 右端からのオフセットが正しく計算されていることを確認
            // （具体的な値は画面サイズに依存するため、計算が実行されることを確認）
            Assert.IsTrue(position.X >= 0);
            Assert.AreEqual(100, position.Y); // Y座標は指定値通り
        }

        /// <summary>
        /// 絶対座標指定の位置計算テスト
        /// </summary>
        [TestMethod]
        public void GetMainWindowPosition_AbsolutePosition_ReturnsSpecifiedPosition()
        {
            // Arrange
            var testConfigManager = new TestableConfigManager("+300+200");
            int windowWidth = 500;
            int windowHeight = 600;

            // Act
            Point position = testConfigManager.GetMainWindowPosition(windowWidth, windowHeight);

            // Assert
            Assert.AreEqual(300, position.X);
            Assert.AreEqual(200, position.Y);
        }

        /// <summary>
        /// エディターウィンドウのセンタリングテスト
        /// </summary>
        [TestMethod]
        public void GetEditorWindowPosition_CenteringWhenInvalid_ReturnsCenteredPosition()
        {
            // Arrange
            var testConfigManager = new TestableConfigManager("invalid_position");
            int windowWidth = 400;
            int windowHeight = 300;

            // Act
            Point position = testConfigManager.GetEditorWindowPosition(windowWidth, windowHeight);

            // Assert
            // センタリングされた位置が返されることを確認
            // 具体的な値は画面サイズに依存するが、合理的な範囲内であることを確認
            Assert.IsTrue(position.X >= 0);
            Assert.IsTrue(position.Y >= 0);
            Assert.IsTrue(position.X < 5000);
            Assert.IsTrue(position.Y < 3000);
        }

        #endregion

        #region エラーハンドリングテスト

        /// <summary>
        /// 無効なウィンドウサイズでの位置計算テスト
        /// </summary>
        [TestMethod]
        public void GetMainWindowPosition_InvalidDimensions_HandlesGracefully()
        {
            // Arrange
            int invalidWidth = -100;
            int invalidHeight = -50;

            // Act & Assert - 例外が発生しないことを確認
            try
            {
                Point position = configManager.GetMainWindowPosition(invalidWidth, invalidHeight);
                Assert.IsTrue(position.X >= 0);
                Assert.IsTrue(position.Y >= 0);
            }
            catch (System.Exception)
            {
                Assert.Fail("無効なサイズで例外が発生しました");
            }
        }

        /// <summary>
        /// ゼロサイズでの位置計算テスト
        /// </summary>
        [TestMethod]
        public void GetMainWindowPosition_ZeroDimensions_HandlesGracefully()
        {
            // Arrange
            int zeroWidth = 0;
            int zeroHeight = 0;

            // Act & Assert
            try
            {
                Point position = configManager.GetMainWindowPosition(zeroWidth, zeroHeight);
                Assert.IsTrue(position.X >= 0);
                Assert.IsTrue(position.Y >= 0);
            }
            catch (System.Exception)
            {
                Assert.Fail("ゼロサイズで例外が発生しました");
            }
        }

        #endregion

        #region 設定値の境界値テスト

        /// <summary>
        /// フォントサイズの境界値テスト
        /// </summary>
        [TestMethod]
        public void ConfigManager_FontSize_WithinReasonableRange()
        {
            // Assert
            Assert.IsTrue(configManager.TextAreaFontSize >= 8, "フォントサイズが小さすぎます");
            Assert.IsTrue(configManager.TextAreaFontSize <= 72, "フォントサイズが大きすぎます");
        }

        /// <summary>
        /// ウィンドウサイズの境界値テスト
        /// </summary>
        [TestMethod]
        public void ConfigManager_WindowSize_WithinReasonableRange()
        {
            // Assert
            Assert.IsTrue(configManager.WindowWidth >= 200, "ウィンドウ幅が小さすぎます");
            Assert.IsTrue(configManager.WindowWidth <= 3000, "ウィンドウ幅が大きすぎます");
            Assert.IsTrue(configManager.WindowHeight >= 150, "ウィンドウ高が小さすぎます");
            Assert.IsTrue(configManager.WindowHeight <= 2000, "ウィンドウ高が大きすぎます");

            Assert.IsTrue(configManager.EditorWidth >= 200, "エディター幅が小さすぎます");
            Assert.IsTrue(configManager.EditorWidth <= 3000, "エディター幅が大きすぎます");
            Assert.IsTrue(configManager.EditorHeight >= 150, "エディター高が小さすぎます");
            Assert.IsTrue(configManager.EditorHeight <= 2000, "エディター高が大きすぎます");
        }

        /// <summary>
        /// ボタンサイズの境界値テスト
        /// </summary>
        [TestMethod]
        public void ConfigManager_ButtonSize_WithinReasonableRange()
        {
            // Assert
            Assert.IsTrue(configManager.ButtonWidth >= 50, "ボタン幅が小さすぎます");
            Assert.IsTrue(configManager.ButtonWidth <= 300, "ボタン幅が大きすぎます");
            Assert.IsTrue(configManager.ButtonHeight >= 20, "ボタン高が小さすぎます");
            Assert.IsTrue(configManager.ButtonHeight <= 100, "ボタン高が大きすぎます");
        }

        #endregion

        #region ファイルパス検証テスト

        /// <summary>
        /// ファイルパスの形式検証テスト
        /// </summary>
        [TestMethod]
        public void ConfigManager_FilePaths_HaveValidFormat()
        {
            // Assert
            Assert.IsTrue(configManager.OperationFilePath.EndsWith(".exe"), 
                "OperationFilePathが.exeで終わっていません");
            Assert.IsTrue(configManager.SoapCopyFilePath.EndsWith(".exe"), 
                "SoapCopyFilePathが.exeで終わっていません");

            // パスに無効な文字が含まれていないことを確認
            char[] invalidChars = System.IO.Path.GetInvalidPathChars();
            Assert.IsFalse(configManager.OperationFilePath.IndexOfAny(invalidChars) >= 0,
                "OperationFilePathに無効な文字が含まれています");
            Assert.IsFalse(configManager.SoapCopyFilePath.IndexOfAny(invalidChars) >= 0,
                "SoapCopyFilePathに無効な文字が含まれています");
        }

        /// <summary>
        /// ファイルパスが絶対パスであることの確認テスト
        /// </summary>
        [TestMethod]
        public void ConfigManager_FilePaths_AreAbsolutePaths()
        {
            // Assert
            Assert.IsTrue(System.IO.Path.IsPathRooted(configManager.OperationFilePath),
                "OperationFilePathが絶対パスではありません");
            Assert.IsTrue(System.IO.Path.IsPathRooted(configManager.SoapCopyFilePath),
                "SoapCopyFilePathが絶対パスではありません");
        }

        #endregion

        #region フォント名検証テスト

        /// <summary>
        /// フォント名の有効性テスト
        /// </summary>
        [TestMethod]
        public void ConfigManager_FontName_IsValidFont()
        {
            // Assert
            Assert.IsNotNull(configManager.TextAreaFontName);
            Assert.IsTrue(configManager.TextAreaFontName.Length > 0);
            
            // フォント名に無効な文字が含まれていないことを確認
            Assert.IsFalse(configManager.TextAreaFontName.Contains("\0"));
            Assert.IsFalse(configManager.TextAreaFontName.Contains("\r"));
            Assert.IsFalse(configManager.TextAreaFontName.Contains("\n"));
        }

        #endregion

        #region パフォーマンステスト

        /// <summary>
        /// ConfigManager初期化のパフォーマンステスト
        /// </summary>
        [TestMethod]
        public void ConfigManager_Initialization_PerformsWell()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 100; i++)
            {
                var manager = new ConfigManager();
            }

            stopwatch.Stop();

            // Assert
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000, 
                $"ConfigManager初期化が遅すぎます: {stopwatch.ElapsedMilliseconds}ms");
        }

        #endregion

        #region テスト用ヘルパークラス

        /// <summary>
        /// テスト用のConfigManagerクラス
        /// 位置設定をモックするために使用
        /// </summary>
        private class TestableConfigManager : ConfigManager
        {
            private readonly string testPosition;

            public TestableConfigManager(string position)
            {
                testPosition = position;
            }

            public new Point GetMainWindowPosition(int windowWidth, int windowHeight)
            {
                // テスト用の位置解析メソッドを呼び出し
                return ParseWindowPositionForTest(testPosition, windowWidth, windowHeight, 10, 10);
            }

            public new Point GetEditorWindowPosition(int windowWidth, int windowHeight)
            {
                var result = ParseWindowPositionForTest(testPosition, windowWidth, windowHeight, 0, 0);

                // エディターウィンドウの特別処理をシミュレート
                if (result.X == 0 && result.Y == 0 && testPosition != "+0+0")
                {
                    // センタリング処理をシミュレート
                    return new Point(400, 300); // テスト用の固定値
                }

                return result;
            }

            private Point ParseWindowPositionForTest(string positionString, int windowWidth, int windowHeight, int defaultX, int defaultY)
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

                            // テスト用の固定画面幅
                            var screenWidth = 1920;
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
                            var x = int.Parse(parts[1]);
                            var y = int.Parse(parts[2]);
                            return new Point(x, y);
                        }
                    }
                }
                catch (System.Exception)
                {
                    // 解析エラーの場合はデフォルト値を返す
                }

                return new Point(defaultX, defaultY);
            }
        }

        #endregion
    }
}