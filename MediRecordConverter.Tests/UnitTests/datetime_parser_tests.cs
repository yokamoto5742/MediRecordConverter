using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediRecordConverter;

namespace MediRecordConverter.Tests.UnitTests
{
    /// <summary>
    /// DateTimeParserクラスのユニットテスト
    /// 日付と時刻の解析機能をテストします
    /// </summary>
    [TestClass]
    public class DateTimeParserTests
    {
        private DateTimeParser parser;

        /// <summary>
        /// 各テストの前に実行される初期化処理
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            parser = new DateTimeParser();
        }

        #region ExtractDate メソッドのテスト

        /// <summary>
        /// 正常な日付形式（YYYY/MM/DD）の抽出テスト
        /// </summary>
        [TestMethod]
        public void ExtractDate_ValidDateFormat_ReturnsFormattedDate()
        {
            // Arrange（準備）
            string input = "2024/12/25";
            string expected = "2024-12-25T";

            // Act（実行）
            string result = parser.ExtractDate(input);

            // Assert（検証）
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 曜日付き日付形式（YYYY/MM/DD(曜日)）の抽出テスト
        /// </summary>
        [TestMethod]
        public void ExtractDate_DateWithDayOfWeek_ReturnsFormattedDate()
        {
            // Arrange
            string input = "2024/12/25(水)";
            string expected = "2024-12-25T";

            // Act
            string result = parser.ExtractDate(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 1桁の月日を含む日付形式のテスト
        /// </summary>
        [TestMethod]
        public void ExtractDate_SingleDigitMonthDay_ReturnsFormattedDate()
        {
            // Arrange
            string input = "2024/1/5";
            string expected = "2024-01-05T";

            // Act
            string result = parser.ExtractDate(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 医師記録行は日付として抽出されないことをテスト
        /// </summary>
        [TestMethod]
        public void ExtractDate_DoctorRecordLine_ReturnsNull()
        {
            // Arrange
            string input = "内科　田中医師　14:30";

            // Act
            string result = parser.ExtractDate(input);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// 不正な日付形式の場合にnullが返されることをテスト
        /// </summary>
        [TestMethod]
        public void ExtractDate_InvalidDateFormat_ReturnsNull()
        {
            // Arrange
            string input = "これは日付ではありません";

            // Act
            string result = parser.ExtractDate(input);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// 存在しない日付（例：2月30日）の場合の処理テスト
        /// </summary>
        [TestMethod]
        public void ExtractDate_InvalidDate_ReturnsNull()
        {
            // Arrange
            string input = "2024/2/30"; // 存在しない日付

            // Act
            string result = parser.ExtractDate(input);

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region CombineDateAndTime メソッドのテスト

        /// <summary>
        /// 正常な日付と時刻の結合テスト
        /// </summary>
        [TestMethod]
        public void CombineDateAndTime_ValidDateAndTime_ReturnsFormattedDateTime()
        {
            // Arrange
            string date = "2024-12-25T";
            string time = "14:30";
            string expected = "2024-12-25T14:30:00Z";

            // Act
            string result = parser.CombineDateAndTime(date, time);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 1桁の時刻の場合の結合テスト
        /// </summary>
        [TestMethod]
        public void CombineDateAndTime_SingleDigitTime_ReturnsFormattedDateTime()
        {
            // Arrange
            string date = "2024-12-25T";
            string time = "9:05";
            string expected = "2024-12-25T09:05:00Z";

            // Act
            string result = parser.CombineDateAndTime(date, time);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 空の日付の場合のテスト
        /// </summary>
        [TestMethod]
        public void CombineDateAndTime_EmptyDate_ReturnsEmptyString()
        {
            // Arrange
            string date = "";
            string time = "14:30";

            // Act
            string result = parser.CombineDateAndTime(date, time);

            // Assert
            Assert.AreEqual("", result);
        }

        /// <summary>
        /// 空の時刻の場合のテスト
        /// </summary>
        [TestMethod]
        public void CombineDateAndTime_EmptyTime_ReturnsEmptyString()
        {
            // Arrange
            string date = "2024-12-25T";
            string time = "";

            // Act
            string result = parser.CombineDateAndTime(date, time);

            // Assert
            Assert.AreEqual("", result);
        }

        /// <summary>
        /// nullの日付の場合のテスト
        /// </summary>
        [TestMethod]
        public void CombineDateAndTime_NullDate_ReturnsEmptyString()
        {
            // Arrange
            string date = null;
            string time = "14:30";

            // Act
            string result = parser.CombineDateAndTime(date, time);

            // Assert
            Assert.AreEqual("", result);
        }

        /// <summary>
        /// nullの時刻の場合のテスト
        /// </summary>
        [TestMethod]
        public void CombineDateAndTime_NullTime_ReturnsEmptyString()
        {
            // Arrange
            string date = "2024-12-25T";
            string time = null;

            // Act
            string result = parser.CombineDateAndTime(date, time);

            // Assert
            Assert.AreEqual("", result);
        }

        /// <summary>
        /// 不正な時刻形式の場合のテスト
        /// </summary>
        [TestMethod]
        public void CombineDateAndTime_InvalidTimeFormat_ReturnsEmptyString()
        {
            // Arrange
            string date = "2024-12-25T";
            string time = "不正な時刻";

            // Act
            string result = parser.CombineDateAndTime(date, time);

            // Assert
            Assert.AreEqual("", result);
        }

        #endregion

        #region パラメータ化テスト（データ駆動テスト）

        /// <summary>
        /// 複数の日付パターンを一度にテストするデータ駆動テスト
        /// </summary>
        [TestMethod]
        [DataRow("2024/12/25", "2024-12-25T")]
        [DataRow("2024/1/1", "2024-01-01T")]
        [DataRow("2024/12/31", "2024-12-31T")]
        [DataRow("2024/2/29", "2024-02-29T")] // うるう年
        public void ExtractDate_MultipleDateFormats_ReturnsExpectedResults(string input, string expected)
        {
            // Act
            string result = parser.ExtractDate(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 複数の不正な日付パターンがnullを返すことをテスト
        /// </summary>
        [TestMethod]
        [DataRow("")]
        [DataRow("不正な日付")]
        [DataRow("2024/13/1")] // 13月は存在しない
        [DataRow("2024/2/30")] // 2月30日は存在しない
        [DataRow("内科　医師　14:30")] // 医師記録行
        public void ExtractDate_InvalidInputs_ReturnsNull(string input)
        {
            // Act
            string result = parser.ExtractDate(input);

            // Assert
            Assert.IsNull(result);
        }

        #endregion
    }
}