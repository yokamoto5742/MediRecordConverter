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

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string objectData { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string assessment { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string plan { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string comment { get; set; }

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
                            comment = ""
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

            // 修正: SOAPパターンの検出を改善
            if (trimmedLine.StartsWith("S >") || trimmedLine.StartsWith("S>") || trimmedLine.Equals("S >"))
            {
                if (trimmedLine.Length > 3)
                {
                    var content = trimmedLine.StartsWith("S >") ? trimmedLine.Substring(3).Trim() : trimmedLine.Substring(2).Trim();
                    if (!string.IsNullOrEmpty(content))
                    {
                        record.subject = AppendContent(record.subject, content);
                    }
                }
                // 次の行もチェック（SOAPヘッダーの後に内容が続く場合）
                return;
            }
            else if (trimmedLine.StartsWith("O >") || trimmedLine.StartsWith("O>") || trimmedLine.Equals("O >"))
            {
                if (trimmedLine.Length > 3)
                {
                    var content = trimmedLine.StartsWith("O >") ? trimmedLine.Substring(3).Trim() : trimmedLine.Substring(2).Trim();
                    if (!string.IsNullOrEmpty(content))
                    {
                        record.objectData = AppendContent(record.objectData, content);
                    }
                }
                return;
            }
            else if (trimmedLine.StartsWith("A >") || trimmedLine.StartsWith("A>") || trimmedLine.Equals("A >"))
            {
                if (trimmedLine.Length > 3)
                {
                    var content = trimmedLine.StartsWith("A >") ? trimmedLine.Substring(3).Trim() : trimmedLine.Substring(2).Trim();
                    if (!string.IsNullOrEmpty(content))
                    {
                        record.assessment = AppendContent(record.assessment, content);
                    }
                }
                return;
            }
            else if (trimmedLine.StartsWith("P >") || trimmedLine.StartsWith("P>") || trimmedLine.Equals("P >"))
            {
                if (trimmedLine.Length > 3)
                {
                    var content = trimmedLine.StartsWith("P >") ? trimmedLine.Substring(3).Trim() : trimmedLine.Substring(2).Trim();
                    if (!string.IsNullOrEmpty(content))
                    {
                        record.plan = AppendContent(record.plan, content);
                    }
                }
                return;
            }
            else if (trimmedLine.StartsWith("F >") || trimmedLine.StartsWith("F>") || trimmedLine.Equals("F >"))
            {
                if (trimmedLine.Length > 3)
                {
                    var content = trimmedLine.StartsWith("F >") ? trimmedLine.Substring(3).Trim() : trimmedLine.Substring(2).Trim();
                    if (!string.IsNullOrEmpty(content))
                    {
                        record.comment = AppendContent(record.comment, content);
                    }
                }
                return;
            }

            // 修正: 継続行の処理を改善
            if (!string.IsNullOrWhiteSpace(trimmedLine) && !IsHeaderLine(trimmedLine))
            {
                // 現在のコンテキストに基づいて適切なフィールドに追加
                if (record.comment != null && record.comment.Length > 0)
                {
                    record.comment = AppendContent(record.comment, trimmedLine);
                }
                else if (record.plan != null && record.plan.Length > 0)
                {
                    record.plan = AppendContent(record.plan, trimmedLine);
                }
                else if (record.assessment != null && record.assessment.Length > 0)
                {
                    record.assessment = AppendContent(record.assessment, trimmedLine);
                }
                else if (record.objectData != null && record.objectData.Length > 0)
                {
                    record.objectData = AppendContent(record.objectData, trimmedLine);
                }
                else if (record.subject != null && record.subject.Length > 0)
                {
                    record.subject = AppendContent(record.subject, trimmedLine);
                }
                else
                {
                    // デフォルトでsubjectに追加
                    record.subject = AppendContent(record.subject, trimmedLine);
                }
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
                        comment = string.IsNullOrEmpty(record.comment) ? null : record.comment
                    };

                    cleanedRecords.Add(cleanRecord);
                }
            }

            return cleanedRecords;
        }
    }
}