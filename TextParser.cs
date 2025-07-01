using System;
using System.Collections.Generic;

namespace MediRecordConverter
{
    public class TextParser
    {
        private readonly DateTimeParser dateTimeParser;
        private readonly DoctorRecordExtractor doctorRecordExtractor;
        private readonly SOAPClassifier soapClassifier;
        private readonly MedicalRecordProcessor recordProcessor;

        public TextParser()
        {
            dateTimeParser = new DateTimeParser();
            doctorRecordExtractor = new DoctorRecordExtractor();
            soapClassifier = new SOAPClassifier();
            recordProcessor = new MedicalRecordProcessor();
        }

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
                    var line = lines[i]?.Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var recordMatch = doctorRecordExtractor.ExtractDoctorRecord(line);
                    if (recordMatch.HasValue)
                    {
                        // 前のレコードを保存
                        if (currentRecord != null)
                        {
                            records.Add(currentRecord);
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
                        continue;
                    }

                    // 日付の抽出を試行
                    var dateMatch = dateTimeParser.ExtractDate(line);
                    if (!string.IsNullOrEmpty(dateMatch))
                    {
                        currentDate = dateMatch;
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
                }

                // 後処理の実行
                records = recordProcessor.CleanupRecords(records);
                records = recordProcessor.MergeRecordsByTimestamp(records);
                records = recordProcessor.SortRecordsByDateTime(records);
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