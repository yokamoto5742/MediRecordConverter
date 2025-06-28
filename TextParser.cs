using System;
using System.Collections.Generic;

namespace MediRecordConverter
{
    /// <summary>
    /// 医療テキストの解析を統合的に処理するメインクラス
    /// </summary>
    public class TextParser
    {
        private readonly DateTimeParser dateTimeParser;
        private readonly DoctorRecordExtractor doctorRecordExtractor;
        private readonly SOAPClassifier soapClassifier;
        private readonly MedicalRecordProcessor recordProcessor;

        /// <summary>
        /// TextParserのコンストラクタ
        /// </summary>
        public TextParser()
        {
            dateTimeParser = new DateTimeParser();
            doctorRecordExtractor = new DoctorRecordExtractor();
            soapClassifier = new SOAPClassifier();
            recordProcessor = new MedicalRecordProcessor();
        }

        /// <summary>
        /// 医療テキストを解析してMedicalRecordのリストに変換します
        /// </summary>
        /// <param name="text">解析対象のテキスト</param>
        /// <returns>解析されたMedicalRecordのリスト</returns>
        public List<MedicalRecord> ParseMedicalText(string text)
        {
            var records = new List<MedicalRecord>();

            try
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    System.Diagnostics.Debug.WriteLine("ParseMedicalText: 入力テキストが空です");
                    return records;
                }

                var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                MedicalRecord currentRecord = null;
                string currentDate = "";

                System.Diagnostics.Debug.WriteLine($"ParseMedicalText: {lines.Length}行のテキストを処理開始");

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    System.Diagnostics.Debug.WriteLine($"処理中の行[{i}]: {line}");

                    // 医師記録の抽出を試行
                    var recordMatch = doctorRecordExtractor.ExtractDoctorRecord(line);
                    if (recordMatch.HasValue)
                    {
                        System.Diagnostics.Debug.WriteLine($"医師記録を検出: 科={recordMatch.Value.Department}, 時刻={recordMatch.Value.Time}");

                        // 前のレコードを保存
                        if (currentRecord != null)
                        {
                            records.Add(currentRecord);
                            System.Diagnostics.Debug.WriteLine($"レコード追加: {currentRecord.timestamp} - {currentRecord.department}");
                        }

                        // 新しいレコードを開始
                        currentRecord = new MedicalRecord
                        {
                            timestamp = dateTimeParser.CombineDateAndTime(currentDate, recordMatch.Value.Time),
                            department = recordMatch.Value.Department,
                            subject = "",
                            objectData = "",
                            assessment = "",
                            plan = "",
                            comment = "",
                            summary = "",
                            currentSoapSection = ""
                        };

                        System.Diagnostics.Debug.WriteLine($"新しいレコード開始: {currentRecord.timestamp} - {currentRecord.department}");
                        continue;
                    }

                    // 日付の抽出を試行
                    var dateMatch = dateTimeParser.ExtractDate(line);
                    if (!string.IsNullOrEmpty(dateMatch))
                    {
                        currentDate = dateMatch;
                        System.Diagnostics.Debug.WriteLine($"日付を検出: {currentDate}");
                        continue;
                    }

                    // SOAP内容の分類
                    if (currentRecord != null)
                    {
                        soapClassifier.ClassifySOAPContent(line, currentRecord);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"警告: 医師記録なしでSOAP行を検出: {line}");
                    }
                }

                // 最後のレコードを追加
                if (currentRecord != null)
                {
                    records.Add(currentRecord);
                    System.Diagnostics.Debug.WriteLine($"最後のレコード追加: {currentRecord.timestamp} - {currentRecord.department}");
                }

                System.Diagnostics.Debug.WriteLine($"解析完了: {records.Count}個のレコードを作成");

                // 後処理の実行
                records = recordProcessor.CleanupRecords(records);
                records = recordProcessor.MergeRecordsByTimestamp(records);
                records = recordProcessor.SortRecordsByDateTime(records);

                System.Diagnostics.Debug.WriteLine($"最終結果: {records.Count}個のレコード");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ParseMedicalText Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }

            return records;
        }
    }
}