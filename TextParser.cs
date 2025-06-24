using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MediRecordConverter
{
    public class TextParser
    {
        // 医療記録用のクラス（修正版）
        public class MedicalRecord
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string timestamp { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string department { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string subject { get; set; }

            [JsonProperty("object", NullValueHandling = NullValueHandling.Ignore)]
            public string objectData { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string assessment { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string plan { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string comment { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string summary { get; set; }

            // 空の文字列プロパティを除外するためのメソッド
            public bool ShouldSerializesubject()
            {
                return !string.IsNullOrEmpty(subject);
            }

            public bool ShouldSerializeobjectData()
            {
                return !string.IsNullOrEmpty(objectData);
            }

            public bool ShouldSerializeassessment()
            {
                return !string.IsNullOrEmpty(assessment);
            }

            public bool ShouldSerializeplan()
            {
                return !string.IsNullOrEmpty(plan);
            }

            public bool ShouldSerializecomment()
            {
                return !string.IsNullOrEmpty(comment);
            }

            public bool ShouldSerializesummary()
            {
                return !string.IsNullOrEmpty(summary);
            }
        }

        public TextParser()
        {
        }

        /// <summary>
        /// 医療テキストを解析してJSONデータに変換（修正版）
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
                            department = recordMatch.Value.Department,
                            subject = "",
                            objectData = "",
                            assessment = "",
                            plan = "",
                            comment = "",
                            summary = ""
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

                // 同じ時刻のレコードをマージ
                records = MergeRecordsByTimestamp(records);

                // 日付時刻順にソート（古い順）
                records = SortRecordsByDateTime(records);
            }
            catch (Exception ex)
            {
                // デバッグ用：エラーログを出力
                System.Diagnostics.Debug.WriteLine($"ParseMedicalText Error: {ex.Message}");
            }

            return records;
        }

        /// <summary>
        /// 同じ時刻とdepartmentのレコードをマージ
        /// </summary>
        private List<MedicalRecord> MergeRecordsByTimestamp(List<MedicalRecord> records)
        {
            var mergedRecords = new List<MedicalRecord>();
            var groupedRecords = records.GroupBy(r => new { r.timestamp, r.department });

            foreach (var group in groupedRecords)
            {
                if (group.Count() == 1)
                {
                    mergedRecords.Add(group.First());
                }
                else
                {
                    // 複数のレコードを統合
                    var mergedRecord = new MedicalRecord
                    {
                        timestamp = group.Key.timestamp,
                        department = group.Key.department,
                        subject = "",
                        objectData = "",
                        assessment = "",
                        plan = "",
                        comment = "",
                        summary = ""
                    };

                    foreach (var record in group)
                    {
                        if (!string.IsNullOrEmpty(record.subject))
                            mergedRecord.subject = AppendContent(mergedRecord.subject, record.subject);
                        if (!string.IsNullOrEmpty(record.objectData))
                            mergedRecord.objectData = AppendContent(mergedRecord.objectData, record.objectData);
                        if (!string.IsNullOrEmpty(record.assessment))
                            mergedRecord.assessment = AppendContent(mergedRecord.assessment, record.assessment);
                        if (!string.IsNullOrEmpty(record.plan))
                            mergedRecord.plan = AppendContent(mergedRecord.plan, record.plan);
                        if (!string.IsNullOrEmpty(record.comment))
                            mergedRecord.comment = AppendContent(mergedRecord.comment, record.comment);
                        if (!string.IsNullOrEmpty(record.summary))
                            mergedRecord.summary = AppendContent(mergedRecord.summary, record.summary);
                    }

                    mergedRecords.Add(mergedRecord);
                }
            }

            return mergedRecords;
        }

        /// <summary>
        /// レコードを日付時刻順（古い順）にソート
        /// </summary>
        private List<MedicalRecord> SortRecordsByDateTime(List<MedicalRecord> records)
        {
            return records.OrderBy(record =>
            {
                if (DateTime.TryParse(record.timestamp?.Replace("Z", ""), out DateTime parsedDate))
                {
                    return parsedDate;
                }
                return DateTime.MinValue; // パースできない場合は最も古い日付として扱う
            }).ToList();
        }

        /// <summary>
        /// 日付を抽出
        /// </summary>
        private string ExtractDate(string line)
        {
            // 修正: より柔軟な日付パターンに変更
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
            // 修正: より柔軟な正規表現パターンに変更
            var pattern = @"^(内科|外科|透析|整形外科|皮膚科|眼科|耳鼻科|泌尿器科|婦人科|小児科|精神科|放射線科|麻酔科|病理科|リハビリ科|薬剤科|検査科|栄養科)[\s　]+.*?(\d{1,2}:\d{2})";
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
            var trimmedLine = line.Trim();

            // SOAPマッピング
            var soapMapping = new Dictionary<string, string>
            {
                { "S", "subject" },
                { "O", "object" },
                { "A", "assessment" },
                { "P", "plan" },
                { "F", "comment" },
                { "サ", "summary" }
            };

            // SOAP項目の検出と分類
            foreach (var mapping in soapMapping)
            {
                var patterns = new string[]
                {
                    $"{mapping.Key} >",
                    $"{mapping.Key}>",
                    $"{mapping.Key} ＞",
                    $"{mapping.Key}＞"
                };

                foreach (var pattern in patterns)
                {
                    if (trimmedLine.StartsWith(pattern) || trimmedLine.Equals(pattern.TrimEnd()))
                    {
                        var content = "";
                        if (trimmedLine.Length > pattern.Length)
                        {
                            content = trimmedLine.Substring(pattern.Length).Trim();
                        }

                        if (!string.IsNullOrEmpty(content))
                        {
                            SetFieldByMapping(record, mapping.Value, content);
                        }
                        return;
                    }
                }
            }

            // 継続行の処理（SOAPパターンでない場合）
            if (!string.IsNullOrWhiteSpace(trimmedLine) && !IsHeaderLine(trimmedLine))
            {
                // 最後に更新されたフィールドに継続行を追加
                AddContinuationLine(record, trimmedLine);
            }
        }

        /// <summary>
        /// マッピングに基づいてフィールドを設定
        /// </summary>
        private void SetFieldByMapping(MedicalRecord record, string fieldName, string content)
        {
            switch (fieldName)
            {
                case "subject":
                    record.subject = AppendContent(record.subject, content);
                    break;
                case "object":
                    record.objectData = AppendContent(record.objectData, content);
                    break;
                case "assessment":
                    record.assessment = AppendContent(record.assessment, content);
                    break;
                case "plan":
                    record.plan = AppendContent(record.plan, content);
                    break;
                case "comment":
                    record.comment = AppendContent(record.comment, content);
                    break;
                case "summary":
                    record.summary = AppendContent(record.summary, content);
                    break;
            }
        }

        /// <summary>
        /// 継続行を最適なフィールドに追加
        /// </summary>
        private void AddContinuationLine(MedicalRecord record, string content)
        {
            // 最後に更新されたフィールドを特定し、そこに追加
            // 優先順位: comment -> plan -> assessment -> objectData -> subject -> summary
            if (!string.IsNullOrEmpty(record.comment))
            {
                record.comment = AppendContent(record.comment, content);
            }
            else if (!string.IsNullOrEmpty(record.plan))
            {
                record.plan = AppendContent(record.plan, content);
            }
            else if (!string.IsNullOrEmpty(record.assessment))
            {
                record.assessment = AppendContent(record.assessment, content);
            }
            else if (!string.IsNullOrEmpty(record.objectData))
            {
                record.objectData = AppendContent(record.objectData, content);
            }
            else if (!string.IsNullOrEmpty(record.subject))
            {
                record.subject = AppendContent(record.subject, content);
            }
            else if (!string.IsNullOrEmpty(record.summary))
            {
                record.summary = AppendContent(record.summary, content);
            }
            else
            {
                // デフォルトでsubjectに追加
                record.subject = AppendContent(record.subject, content);
            }
        }

        /// <summary>
        /// ヘッダー行かどうかを判定
        /// </summary>
        private bool IsHeaderLine(string line)
        {
            // 日付行、医師記録行、その他のヘッダーを除外
            return Regex.IsMatch(line, @"\d{4}/\d{1,2}/\d{1,2}") ||
                   Regex.IsMatch(line, @"^(内科|外科|透析|整形外科|皮膚科|眼科|耳鼻科|泌尿器科|婦人科|小児科|精神科|放射線科|麻酔科|病理科|リハビリ科|薬剤科|検査科|栄養科)");
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
                // 修正: タイムスタンプと部署名があれば有効なレコードとして扱う
                if (!string.IsNullOrEmpty(record.timestamp) && !string.IsNullOrEmpty(record.department))
                {
                    // nullフィールドを空文字に変換
                    var cleanRecord = new MedicalRecord
                    {
                        timestamp = record.timestamp ?? "",
                        department = record.department ?? "",
                        subject = string.IsNullOrEmpty(record.subject) ? null : record.subject,
                        objectData = string.IsNullOrEmpty(record.objectData) ? null : record.objectData,
                        assessment = string.IsNullOrEmpty(record.assessment) ? null : record.assessment,
                        plan = string.IsNullOrEmpty(record.plan) ? null : record.plan,
                        comment = string.IsNullOrEmpty(record.comment) ? null : record.comment,
                        summary = string.IsNullOrEmpty(record.summary) ? null : record.summary
                    };

                    cleanedRecords.Add(cleanRecord);
                }
            }

            return cleanedRecords;
        }
    }
}c