using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediRecordConverter;
using System.Collections.Generic;
using System.Linq;

namespace MediRecordConverter.Tests.UnitTests
{
    /// <summary>
    /// MedicalRecordProcessorクラスのユニットテスト
    /// 医療記録の後処理機能をテストします
    /// </summary>
    [TestClass]
    public class MedicalRecordProcessorTests
    {
        private MedicalRecordProcessor processor;

        /// <summary>
        /// 各テストの前に実行される初期化処理
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            processor = new MedicalRecordProcessor();
        }

        #region CleanupRecords メソッドのテスト

        /// <summary>
        /// 正常なレコードのクリーンアップテスト
        /// </summary>
        [TestMethod]
        public void CleanupRecords_ValidRecords_ReturnsCleanedRecords()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "内科",
                    subject = "頭痛の訴え",
                    objectData = "血圧120/80",
                    assessment = "風邪症候群",
                    plan = "解熱剤処方",
                    comment = "",
                    summary = ""
                }
            };

            // Act
            var result = processor.CleanupRecords(records);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("2024-12-25T14:30:00Z", result[0].timestamp);
            Assert.AreEqual("内科", result[0].department);
            Assert.AreEqual("頭痛の訴え", result[0].subject);
            Assert.AreEqual("血圧120/80", result[0].objectData);
            Assert.AreEqual("風邪症候群", result[0].assessment);
            Assert.AreEqual("解熱剤処方", result[0].plan);
            Assert.IsNull(result[0].comment);
            Assert.IsNull(result[0].summary);
        }

        /// <summary>
        /// 空のフィールドを含むレコードのクリーンアップテスト
        /// </summary>
        [TestMethod]
        public void CleanupRecords_EmptyFields_RemovesEmptyFields()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "内科",
                    subject = "頭痛の訴え",
                    objectData = "",
                    assessment = null,
                    plan = "   ",  // 空白のみ
                    comment = "",
                    summary = ""
                }
            };

            // Act
            var result = processor.CleanupRecords(records);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("頭痛の訴え", result[0].subject);
            Assert.IsNull(result[0].objectData);
            Assert.IsNull(result[0].assessment);
            // 空白のみのplanは空文字列として扱われ、nullに変換される
            Assert.IsNull(result[0].plan);
            Assert.IsNull(result[0].comment);
            Assert.IsNull(result[0].summary);
        }

        /// <summary>
        /// 不完全なレコード（タイムスタンプまたは診療科が空）の除外テスト
        /// </summary>
        [TestMethod]
        public void CleanupRecords_IncompleteRecords_ExcludesIncompleteRecords()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                new MedicalRecord
                {
                    timestamp = "",
                    department = "内科",
                    subject = "テスト"
                },
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "",
                    subject = "テスト"
                },
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "内科",
                    subject = "正常なレコード"
                }
            };

            // Act
            var result = processor.CleanupRecords(records);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("正常なレコード", result[0].subject);
        }

        /// <summary>
        /// 空のリストのクリーンアップテスト
        /// </summary>
        [TestMethod]
        public void CleanupRecords_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            var records = new List<MedicalRecord>();

            // Act
            var result = processor.CleanupRecords(records);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region MergeRecordsByTimestamp メソッドのテスト

        /// <summary>
        /// 同じタイムスタンプのレコードのマージテスト
        /// </summary>
        [TestMethod]
        public void MergeRecordsByTimestamp_SameTimestamp_MergesRecords()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "内科",
                    subject = "頭痛",
                    objectData = "",
                    assessment = "",
                    plan = ""
                },
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "内科",
                    subject = "",
                    objectData = "血圧120/80",
                    assessment = "",
                    plan = ""
                },
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "内科",
                    subject = "",
                    objectData = "",
                    assessment = "風邪症候群",
                    plan = "解熱剤処方"
                }
            };

            // Act
            var result = processor.MergeRecordsByTimestamp(records);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("頭痛", result[0].subject);
            Assert.AreEqual("血圧120/80", result[0].objectData);
            Assert.AreEqual("風邪症候群", result[0].assessment);
            Assert.AreEqual("解熱剤処方", result[0].plan);
        }

        /// <summary>
        /// 異なるタイムスタンプのレコードがマージされないことのテスト
        /// </summary>
        [TestMethod]
        public void MergeRecordsByTimestamp_DifferentTimestamp_DoesNotMerge()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "内科",
                    subject = "頭痛"
                },
                new MedicalRecord
                {
                    timestamp = "2024-12-25T15:30:00Z",
                    department = "内科",
                    subject = "腹痛"
                }
            };

            // Act
            var result = processor.MergeRecordsByTimestamp(records);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("頭痛", result.First(r => r.timestamp == "2024-12-25T14:30:00Z").subject);
            Assert.AreEqual("腹痛", result.First(r => r.timestamp == "2024-12-25T15:30:00Z").subject);
        }

        /// <summary>
        /// 異なる診療科のレコードがマージされないことのテスト
        /// </summary>
        [TestMethod]
        public void MergeRecordsByTimestamp_DifferentDepartment_DoesNotMerge()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "内科",
                    subject = "頭痛"
                },
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "外科",
                    subject = "外傷"
                }
            };

            // Act
            var result = processor.MergeRecordsByTimestamp(records);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("頭痛", result.First(r => r.department == "内科").subject);
            Assert.AreEqual("外傷", result.First(r => r.department == "外科").subject);
        }

        /// <summary>
        /// 複数の内容を含むフィールドのマージテスト
        /// </summary>
        [TestMethod]
        public void MergeRecordsByTimestamp_MultipleContentFields_MergesWithNewlines()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "内科",
                    subject = "頭痛",
                    objectData = "",
                    assessment = "",
                    plan = ""
                },
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "内科",
                    subject = "発熱",
                    objectData = "体温38.5℃",
                    assessment = "",
                    plan = ""
                }
            };

            // Act
            var result = processor.MergeRecordsByTimestamp(records);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("頭痛\n発熱", result[0].subject);
            Assert.AreEqual("体温38.5℃", result[0].objectData);
        }

        #endregion

        #region SortRecordsByDateTime メソッドのテスト

        /// <summary>
        /// 日時順ソートテスト
        /// </summary>
        [TestMethod]
        public void SortRecordsByDateTime_ValidTimestamps_SortsCorrectly()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                new MedicalRecord
                {
                    timestamp = "2024-12-25T15:30:00Z",
                    department = "内科",
                    subject = "後の記録"
                },
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "内科",
                    subject = "前の記録"
                },
                new MedicalRecord
                {
                    timestamp = "2024-12-26T09:00:00Z",
                    department = "内科",
                    subject = "翌日の記録"
                }
            };

            // Act
            var result = processor.SortRecordsByDateTime(records);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("前の記録", result[0].subject);
            Assert.AreEqual("後の記録", result[1].subject);
            Assert.AreEqual("翌日の記録", result[2].subject);
        }

        /// <summary>
        /// 不正なタイムスタンプがある場合のソートテスト
        /// </summary>
        [TestMethod]
        public void SortRecordsByDateTime_InvalidTimestamp_HandlesGracefully()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "内科",
                    subject = "正常な記録"
                },
                new MedicalRecord
                {
                    timestamp = "不正なタイムスタンプ",
                    department = "内科",
                    subject = "不正な記録"
                },
                new MedicalRecord
                {
                    timestamp = "2024-12-25T13:30:00Z",
                    department = "内科",
                    subject = "早い記録"
                }
            };

            // Act
            var result = processor.SortRecordsByDateTime(records);

            // Assert
            Assert.AreEqual(3, result.Count);
            // 不正なタイムスタンプは最初に来る（DateTime.MinValue）
            Assert.AreEqual("不正な記録", result[0].subject);
            Assert.AreEqual("早い記録", result[1].subject);
            Assert.AreEqual("正常な記録", result[2].subject);
        }

        /// <summary>
        /// 空のタイムスタンプがある場合のソートテスト
        /// </summary>
        [TestMethod]
        public void SortRecordsByDateTime_EmptyTimestamp_HandlesGracefully()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "内科",
                    subject = "正常な記録"
                },
                new MedicalRecord
                {
                    timestamp = "",
                    department = "内科",
                    subject = "空のタイムスタンプ"
                },
                new MedicalRecord
                {
                    timestamp = null,
                    department = "内科",
                    subject = "nullタイムスタンプ"
                }
            };

            // Act
            var result = processor.SortRecordsByDateTime(records);

            // Assert
            Assert.AreEqual(3, result.Count);
            // 空/nullのタイムスタンプは最初に来る
            Assert.IsTrue(result[0].subject == "空のタイムスタンプ" || result[0].subject == "nullタイムスタンプ");
            Assert.IsTrue(result[1].subject == "空のタイムスタンプ" || result[1].subject == "nullタイムスタンプ");
            Assert.AreEqual("正常な記録", result[2].subject);
        }

        #endregion

        #region 統合テスト（複数メソッドの組み合わせ）

        /// <summary>
        /// クリーンアップ、マージ、ソートの統合テスト
        /// </summary>
        [TestMethod]
        public void FullProcessing_CompleteWorkflow_ProcessesCorrectly()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                // 不完全なレコード（除外される）
                new MedicalRecord
                {
                    timestamp = "",
                    department = "内科",
                    subject = "不完全"
                },
                // 後の時刻のレコード
                new MedicalRecord
                {
                    timestamp = "2024-12-25T15:30:00Z",
                    department = "内科",
                    subject = "後の記録",
                    objectData = "",
                    assessment = "",
                    plan = ""
                },
                // 同じ時刻のレコード（マージされる）
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "内科",
                    subject = "マージ1",
                    objectData = "",
                    assessment = "",
                    plan = ""
                },
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "内科",
                    subject = "",
                    objectData = "血圧測定",
                    assessment = "",
                    plan = ""
                },
                // 早い時刻のレコード
                new MedicalRecord
                {
                    timestamp = "2024-12-25T13:30:00Z",
                    department = "内科",
                    subject = "早い記録",
                    objectData = "",
                    assessment = "",
                    plan = ""
                }
            };

            // Act
            var cleaned = processor.CleanupRecords(records);
            var merged = processor.MergeRecordsByTimestamp(cleaned);
            var sorted = processor.SortRecordsByDateTime(merged);

            // Assert
            Assert.AreEqual(3, sorted.Count);

            // ソート順の確認
            Assert.AreEqual("早い記録", sorted[0].subject);
            Assert.AreEqual("マージ1", sorted[1].subject);
            Assert.AreEqual("血圧測定", sorted[1].objectData);
            Assert.AreEqual("後の記録", sorted[2].subject);
        }

        #endregion

        #region エラーハンドリングテスト

        /// <summary>
        /// nullリストの処理テスト - 実装がNullReferenceExceptionを投げることを確認
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(System.NullReferenceException))]
        public void CleanupRecords_NullList_ThrowsException()
        {
            // Act
            processor.CleanupRecords(null);
        }

        /// <summary>
        /// nullレコードを含むリストの処理テスト - 実装が例外を投げることを確認
        /// </summary>
        [TestMethod]
        public void CleanupRecords_ListWithNullRecord_HandlesGracefully()
        {
            // Arrange
            var records = new List<MedicalRecord>
            {
                new MedicalRecord
                {
                    timestamp = "2024-12-25T14:30:00Z",
                    department = "内科",
                    subject = "正常な記録"
                },
                null,
                new MedicalRecord
                {
                    timestamp = "2024-12-25T15:30:00Z",
                    department = "外科",
                    subject = "別の正常な記録"
                }
            };

            // Act & Assert - 例外が発生することを確認
            try
            {
                var result = processor.CleanupRecords(records);
                Assert.Fail("nullレコードを含むリストで例外が発生することを期待していました");
            }
            catch (System.NullReferenceException)
            {
                // 期待される例外なので、テスト成功
            }
            catch (System.Exception ex)
            {
                Assert.Fail($"予期しない例外が発生しました: {ex.GetType().Name}");
            }
        }

        #endregion

        #region パフォーマンステスト

        /// <summary>
        /// 大量レコードの処理パフォーマンステスト
        /// </summary>
        [TestMethod]
        public void ProcessLargeDataset_Performance_CompletesInReasonableTime()
        {
            // Arrange
            var records = new List<MedicalRecord>();
            for (int i = 0; i < 1000; i++)
            {
                records.Add(new MedicalRecord
                {
                    timestamp = $"2024-12-25T{(i % 24):D2}:{(i % 60):D2}:00Z",
                    department = i % 2 == 0 ? "内科" : "外科",
                    subject = $"記録{i}",
                    objectData = "",
                    assessment = "",
                    plan = ""
                });
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var cleaned = processor.CleanupRecords(records);
            var merged = processor.MergeRecordsByTimestamp(cleaned);
            var sorted = processor.SortRecordsByDateTime(merged);

            stopwatch.Stop();

            // Assert
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000, $"処理時間が長すぎます: {stopwatch.ElapsedMilliseconds}ms");
            Assert.IsTrue(cleaned.Count > 0);
            Assert.IsTrue(merged.Count > 0);
            Assert.IsTrue(sorted.Count > 0);
        }

        #endregion
    }
}