using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MediRecordConverter
{
    public class TextParser
    {
        // 医療記録用のクラス（簡素化）
        public class MedicalRecord
        {
            public string timestamp { get; set; }
            public string department { get; set; }
            public string subject { get; set; }
            public string objectData { get; set; }
            public string assessment { get; set; }
            public string plan { get; set; }
            public string comment { get; set; }
        }

        public TextParser()
        {
        }

        /// <summary>
        /// 医療テキストを解析してJSONデータに変換
        /// </summary>
        public List<MedicalRecord> ParseMedicalText(string text)
        {
            var records = new List<MedicalRecord>();

            try
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return records;
                }

                var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                MedicalRecord currentRecord = null;
                string currentDate = "";

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // 日付行の検出
                    var dateMatch = ExtractDate(line);
                    if (!string.IsNullOrEmpty(dateMatch))
                    {
                        currentDate = dateMatch;
                        continue;
                    }

                    // 医師記録行の検出
                    var recordMatch = ExtractDoctorRecord(line);
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
                            timestamp = CombineDateAndTime(currentDate, recordMatch.Value.Time),
                            department = recordMatch.Value.Department
                        };
                        continue;
                    }

                    // SOAP記録の検出と分類
                    if (currentRecord != null)
                    {
                        ClassifySOAPContent(line, currentRecord);
                    }
                }

                // 最後のレコードを追加
                if (currentRecord != null)
                {
                    records.Add(currentRecord);
                }

                // 空のフィールドを持つレコードをクリーンアップ
                records = CleanupRecords(records);
            }
            catch (Exception)
            {
                // エラーは無視してこれまでの結果を返す
            }

            return records;
        }

        /// <summary>
        /// 日付を抽出
        /// </summary>
        private string ExtractDate(string line)
        {
            var datePattern = @"(\d{4}/\d{1,2}/\d{1,2})";
            var match = Regex.Match(line, datePattern);
            if (match.Success)
            {
                try
                {
                    var dateStr = match.Groups[1].Value;
                    var dateParts = dateStr.Split('/');
                    var year = int.Parse(dateParts[0]);
                    var month = int.Parse(dateParts[1]);
                    var day = int.Parse(dateParts[2]);

                    return new DateTime(year, month, day).ToString("yyyy-MM-ddT");
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// 医師記録行を抽出
        /// </summary>
        private (string Department, string Time)? ExtractDoctorRecord(string line)
        {
            // パターン: 科名　　医師名　　分類　　時刻
            var pattern = @"^(内科|外科|透析|整形外科|皮膚科|眼科|耳鼻科|泌尿器科|婦人科|小児科|精神科|放射線科|麻酔科|病理科|リハビリ科|薬剤科|検査科|栄養科)\s+.*?\s+(\d{1,2}:\d{1,2})";
            var match = Regex.Match(line, pattern);

            if (match.Success)
            {
                return (match.Groups[1].Value, match.Groups[2].Value);
            }

            return null;
        }

        /// <summary>
        /// 日付と時刻を結合してISO形式のタイムスタンプを作成
        /// </summary>
        private string CombineDateAndTime(string date, string time)
        {
            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(time))
                return "";

            try
            {
                var timeParts = time.Split(':');
                var hour = int.Parse(timeParts[0]);
                var minute = int.Parse(timeParts[1]);

                return $"{date}{hour:D2}:{minute:D2}:00Z";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// SOAPコンテンツを分類
        /// </summary>
        private void ClassifySOAPContent(string line, MedicalRecord record)
        {
            if (line.StartsWith("S >"))
            {
                record.subject = AppendContent(record.subject, line.Substring(3).Trim());
            }
            else if (line.StartsWith("O >"))
            {
                record.objectData = AppendContent(record.objectData, line.Substring(3).Trim());
            }
            else if (line.StartsWith("A >"))
            {
                record.assessment = AppendContent(record.assessment, line.Substring(3).Trim());
            }
            else if (line.StartsWith("P >"))
            {
                record.plan = AppendContent(record.plan, line.Substring(3).Trim());
            }
            else if (line.StartsWith("F >"))
            {
                record.comment = AppendContent(record.comment, line.Substring(3).Trim());
            }
            else if (!string.IsNullOrWhiteSpace(line) && !line.Contains("　　"))
            {
                // 継続行として前のフィールドに追加
                if (!string.IsNullOrEmpty(record.plan))
                {
                    record.plan = AppendContent(record.plan, line);
                }
                else if (!string.IsNullOrEmpty(record.assessment))
                {
                    record.assessment = AppendContent(record.assessment, line);
                }
                else if (!string.IsNullOrEmpty(record.objectData))
                {
                    record.objectData = AppendContent(record.objectData, line);
                }
                else if (!string.IsNullOrEmpty(record.subject))
                {
                    record.subject = AppendContent(record.subject, line);
                }
            }
        }

        /// <summary>
        /// コンテンツを改行で結合
        /// </summary>
        private string AppendContent(string existing, string newContent)
        {
            if (string.IsNullOrEmpty(existing))
                return newContent;

            if (string.IsNullOrEmpty(newContent))
                return existing;

            return existing + "\n" + newContent;
        }

        /// <summary>
        /// 空のフィールドを持つレコードをクリーンアップ
        /// </summary>
        private List<MedicalRecord> CleanupRecords(List<MedicalRecord> records)
        {
            var cleanedRecords = new List<MedicalRecord>();

            foreach (var record in records)
            {
                // すべてのフィールドが空でないレコードのみを保持

                if (!string.IsNullOrEmpty(record.timestamp) &&
                    (!string.IsNullOrEmpty(record.subject) ||
                     !string.IsNullOrEmpty(record.objectData) ||
                     !string.IsNullOrEmpty(record.assessment) ||
                     !string.IsNullOrEmpty(record.plan) ||
                     !string.IsNullOrEmpty(record.comment)))
                {
                    // nullフィールドを空文字に変換
                    var cleanRecord = new MedicalRecord
                    {
                        timestamp = record.timestamp ?? "",
                        department = record.department ?? "",
                        subject = record.subject ?? "",
                        objectData = record.objectData ?? "",
                        assessment = record.assessment ?? "",
                        plan = record.plan ?? "",
                        comment = record.comment ?? ""
                    };

                    // 空の文字列フィールドは含めない（JSONサイズ削減）
                    cleanedRecords.Add(cleanRecord);
                }
            }

            return cleanedRecords;
        }
    }
}