using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediRecordConverter;

namespace MediRecordConverter.Tests.UnitTests
{
    /// <summary>
    /// DoctorRecordExtractorクラスのユニットテスト
    /// 医師記録の抽出機能をテストします
    /// </summary>
    [TestClass]
    public class DoctorRecordExtractorTests
    {
        private DoctorRecordExtractor extractor;

        /// <summary>
        /// 各テストの前に実行される初期化処理
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            extractor = new DoctorRecordExtractor();
        }

        #region ExtractDoctorRecord メソッドのテスト

        /// <summary>
        /// 基本的な医師記録行の抽出テスト
        /// </summary>
        [TestMethod]
        public void ExtractDoctorRecord_BasicFormat_ReturnsValidRecord()
        {
            // Arrange
            string input = "内科 田中医師 14:30";

            // Act
            var result = extractor.ExtractDoctorRecord(input);

            // Assert
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual("内科", result.Value.Department);
            Assert.AreEqual("14:30", result.Value.Time);
        }

        /// <summary>
        /// 救急タグ付きの医師記録行の抽出テスト
        /// </summary>
        [TestMethod]
        public void ExtractDoctorRecord_WithEmergencyTag_ReturnsValidRecord()
        {
            // Arrange
            string input = "外科 山田医師 09:15 (最終更新 2024/12/25 09:16)【救急】";

            // Act
            var result = extractor.ExtractDoctorRecord(input);

            // Assert
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual("外科", result.Value.Department);
            Assert.AreEqual("09:15", result.Value.Time);
        }

        /// <summary>
        /// 最終更新時刻付きの医師記録行の抽出テスト
        /// </summary>
        [TestMethod]
        public void ExtractDoctorRecord_WithLastUpdate_ReturnsValidRecord()
        {
            // Arrange
            string input = "整形外科 佐藤医師 16:45 (最終更新 2024/12/25 16:46)";

            // Act
            var result = extractor.ExtractDoctorRecord(input);

            // Assert
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual("整形外科", result.Value.Department);
            Assert.AreEqual("16:45", result.Value.Time);
        }

        /// <summary>
        /// 透析科の医師記録行の抽出テスト
        /// </summary>
        [TestMethod]
        public void ExtractDoctorRecord_DialysisDepartment_ReturnsValidRecord()
        {
            // Arrange
            string input = "透析 鈴木医師 08:00";

            // Act
            var result = extractor.ExtractDoctorRecord(input);

            // Assert
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual("透析", result.Value.Department);
            Assert.AreEqual("08:00", result.Value.Time);
        }

        /// <summary>
        /// 1桁時刻の医師記録行の抽出テスト
        /// </summary>
        [TestMethod]
        public void ExtractDoctorRecord_SingleDigitTime_ReturnsValidRecord()
        {
            // Arrange
            string input = "眼科 高橋医師 9:30";

            // Act
            var result = extractor.ExtractDoctorRecord(input);

            // Assert
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual("眼科", result.Value.Department);
            Assert.AreEqual("9:30", result.Value.Time);
        }

        /// <summary>
        /// 全角スペースを含む医師記録行の抽出テスト
        /// </summary>
        [TestMethod]
        public void ExtractDoctorRecord_WithFullWidthSpaces_ReturnsValidRecord()
        {
            // Arrange
            string input = "皮膚科　田村医師　13:20";

            // Act
            var result = extractor.ExtractDoctorRecord(input);

            // Assert
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual("皮膚科", result.Value.Department);
            Assert.AreEqual("13:20", result.Value.Time);
        }

        /// <summary>
        /// 複数の診療科パターンをテスト
        /// </summary>
        [TestMethod]
        [DataRow("内科", "14:30")]
        [DataRow("外科", "09:15")]
        [DataRow("透析", "08:00")]
        [DataRow("整形外科", "16:45")]
        [DataRow("皮膚科", "13:20")]
        [DataRow("眼科", "10:30")]
        [DataRow("耳鼻科", "11:45")]
        [DataRow("泌尿器科", "15:15")]
        [DataRow("婦人科", "12:00")]
        [DataRow("小児科", "14:45")]
        [DataRow("精神科", "16:30")]
        [DataRow("放射線科", "09:30")]
        [DataRow("麻酔科", "07:45")]
        [DataRow("病理科", "18:00")]
        [DataRow("リハビリ科", "15:30")]
        [DataRow("薬剤科", "13:45")]
        [DataRow("検査科", "08:30")]
        [DataRow("栄養科", "12:15")]
        public void ExtractDoctorRecord_VariousDepartments_ReturnsValidRecord(string department, string time)
        {
            // Arrange
            string input = $"{department} 医師名 {time}";

            // Act
            var result = extractor.ExtractDoctorRecord(input);

            // Assert
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(department, result.Value.Department);
            Assert.AreEqual(time, result.Value.Time);
        }

        /// <summary>
        /// 医師記録行ではない文字列の場合のテスト
        /// </summary>
        [TestMethod]
        public void ExtractDoctorRecord_NonDoctorRecord_ReturnsNull()
        {
            // Arrange
            string input = "これは医師記録ではありません";

            // Act
            var result = extractor.ExtractDoctorRecord(input);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        /// <summary>
        /// 日付行の場合のテスト
        /// </summary>
        [TestMethod]
        public void ExtractDoctorRecord_DateLine_ReturnsNull()
        {
            // Arrange
            string input = "2024/12/25(水)";

            // Act
            var result = extractor.ExtractDoctorRecord(input);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        /// <summary>
        /// 空文字列の場合のテスト
        /// </summary>
        [TestMethod]
        public void ExtractDoctorRecord_EmptyString_ReturnsNull()
        {
            // Arrange
            string input = "";

            // Act
            var result = extractor.ExtractDoctorRecord(input);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        /// <summary>
        /// 時刻が含まれていない場合のテスト
        /// </summary>
        [TestMethod]
        public void ExtractDoctorRecord_NoTime_ReturnsNull()
        {
            // Arrange
            string input = "内科 田中医師";

            // Act
            var result = extractor.ExtractDoctorRecord(input);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        #endregion

        #region IsDoctorRecordLine メソッドのテスト

        /// <summary>
        /// 医師記録行の判定テスト（true）
        /// </summary>
        [TestMethod]
        public void IsDoctorRecordLine_ValidDoctorRecord_ReturnsTrue()
        {
            // Arrange
            string input = "内科 田中医師 14:30";

            // Act
            bool result = extractor.IsDoctorRecordLine(input);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// 医師記録行ではない場合の判定テスト（false）
        /// </summary>
        [TestMethod]
        public void IsDoctorRecordLine_NonDoctorRecord_ReturnsFalse()
        {
            // Arrange
            string input = "これは医師記録ではありません";

            // Act
            bool result = extractor.IsDoctorRecordLine(input);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// 日付行の場合の判定テスト（false）
        /// </summary>
        [TestMethod]
        public void IsDoctorRecordLine_DateLine_ReturnsFalse()
        {
            // Arrange
            string input = "2024/12/25(水)";

            // Act
            bool result = extractor.IsDoctorRecordLine(input);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// 複数の医師記録パターンの判定テスト
        /// </summary>
        [TestMethod]
        [DataRow("内科 田中医師 14:30", true)]
        [DataRow("外科 山田医師 09:15 (最終更新 2024/12/25 09:16)【救急】", true)]
        [DataRow("整形外科　佐藤医師　16:45", true)]
        [DataRow("透析 鈴木医師 8:00", true)]
        [DataRow("2024/12/25(水)", false)]
        [DataRow("S > 患者の主訴", false)]
        [DataRow("O > 検査所見", false)]
        [DataRow("", false)]
        [DataRow("これは医師記録ではありません", false)]
        public void IsDoctorRecordLine_VariousInputs_ReturnsExpectedResults(string input, bool expected)
        {
            // Act
            bool result = extractor.IsDoctorRecordLine(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        #endregion

        #region エラーハンドリングテスト

        /// <summary>
        /// null文字列の場合のテスト
        /// </summary>
        [TestMethod]
        public void ExtractDoctorRecord_NullString_ReturnsNull()
        {
            // Arrange
            string input = null;

            // Act
            var result = extractor.ExtractDoctorRecord(input);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        /// <summary>
        /// IsDoctorRecordLineでnull文字列の場合のテスト
        /// </summary>
        [TestMethod]
        public void IsDoctorRecordLine_NullString_ReturnsFalse()
        {
            // Arrange
            string input = null;

            // Act
            bool result = extractor.IsDoctorRecordLine(input);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion
    }
}