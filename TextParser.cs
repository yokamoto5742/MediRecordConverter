using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MediRecordConverter
{
    public class TextParser
    {
        // 解析統計用のクラス
        public class ParsingStatistics
        {
            public int TotalLines { get; set; }
            public int ProcessedLines { get; set; }
            public int EmptyLines { get; set; }
            public int DateTimeEntries { get; set; }
            public int DuplicateRecords { get; set; }
            public int ValidRecords { get; set; }
            public DateTime ProcessingTime { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
            public List<string> Warnings { get; set; } = new List<string>();
        }

        // 医療記録用のクラス
        public class MedicalRecord
        {
            public string Id { get; set; }
            public DateTime? DateTime { get; set; }
            public string Content { get; set; }
            public string Type { get; set; }
            public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        }

        // 解析結果用のクラス
        public class ParsedMedicalData
        {
            public List<MedicalRecord> Records { get; set; } = new List<MedicalRecord>();
            public ParsingStatistics Statistics { get; set; }
            public DateTime ProcessedAt { get; set; } = DateTime.Now;
            public string Version { get; set; } = "1.0";
        }

        public TextParser()
        {
        }

        /// <summary>
        /// テキストの解析統計を取得
        /// </summary>
        public ParsingStatistics GetParsingStatistics(string text)
        {
            var statistics = new ParsingStatistics
            {
                ProcessingTime = DateTime.Now
            };

            try
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return statistics;
                }

                var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
                statistics.TotalLines = lines.Length;
                statistics.EmptyLines = lines.Count(line => string.IsNullOrWhiteSpace(line));
                statistics.ProcessedLines = statistics.TotalLines - statistics.EmptyLines;

                // 日時パターンの検出
                var dateTimePattern = @"\d{4}[-/]\d{1,2}[-/]\d{1,2}|\d{1,2}[-/]\d{1,2}[-/]\d{4}|\d{1,2}:\d{1,2}";
                statistics.DateTimeEntries = lines.Count(line => 
                    !string.IsNullOrWhiteSpace(line) && Regex.IsMatch(line, dateTimePattern));

                // 重複行の検出
                var nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
                var uniqueLines = nonEmptyLines.Distinct().ToList();
                statistics.DuplicateRecords = nonEmptyLines.Count - uniqueLines.Count;
                statistics.ValidRecords = uniqueLines.Count;

                // 基本的な検証
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (line.Length > 1000)
                        {
                            statistics.Warnings.Add($"長いレコードが検出されました: {line.Substring(0, 50)}...");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                statistics.Errors.Add($"統計取得エラー: {ex.Message}");
            }

            return statistics;
        }

        /// <summary>
        /// 医療テキストを解析してJSONデータに変換
        /// </summary>
        public ParsedMedicalData ParseMedicalText(string text)
        {
            var result = new ParsedMedicalData();
            var statistics = GetParsingStatistics(text);
            result.Statistics = statistics;

            try
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return result;
                }

                var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var records = new List<MedicalRecord>();

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var record = ProcessRecord(line, i + 1);
                    if (record != null)
                    {
                        records.Add(record);
                    }
                }

                // 重複除去
                records = RemoveDuplicates(records);

                // 日時でグループ化
                var groupedRecords = GroupRecordsByDateTime(records);

                result.Records = groupedRecords;
            }
            catch (Exception ex)
            {
                statistics.Errors.Add($"テキスト解析エラー: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 個別のレコードを処理
        /// </summary>
        private MedicalRecord ProcessRecord(string line, int lineNumber)
        {
            try
            {
                var record = new MedicalRecord
                {
                    Id = $"record_{lineNumber}_{DateTime.Now.Ticks}",
                    Content = line,
                    Type = DetermineRecordType(line)
                };

                // 日時の抽出を試行
                record.DateTime = ExtractDateTime(line);

                // メタデータの設定
                record.Metadata["lineNumber"] = lineNumber;
                record.Metadata["contentLength"] = line.Length;
                record.Metadata["hasDateTime"] = record.DateTime.HasValue;

                return record;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// レコードタイプを判定
        /// </summary>
        private string DetermineRecordType(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return "unknown";

            // 日時パターンがある場合
            if (Regex.IsMatch(content, @"\d{4}[-/]\d{1,2}[-/]\d{1,2}|\d{1,2}[-/]\d{1,2}[-/]\d{4}|\d{1,2}:\d{1,2}"))
                return "timestamped_entry";

            // 数値が含まれる場合
            if (Regex.IsMatch(content, @"\d+"))
                return "measurement";

            // 薬剤名などのパターン
            if (content.Contains("mg") || content.Contains("ml") || content.Contains("錠"))
                return "medication";

            // 症状や所見
            if (content.Contains("痛み") || content.Contains("症状") || content.Contains("所見"))
                return "symptom";

            return "general_note";
        }

        /// <summary>
        /// 文字列から日時を抽出
        /// </summary>
        private DateTime? ExtractDateTime(string content)
        {
            try
            {
                // 日時パターンのリスト
                var patterns = new[]
                {
                    @"(\d{4})[-/](\d{1,2})[-/](\d{1,2})\s+(\d{1,2}):(\d{1,2})",
                    @"(\d{4})[-/](\d{1,2})[-/](\d{1,2})",
                    @"(\d{1,2})[-/](\d{1,2})[-/](\d{4})",
                    @"(\d{1,2}):(\d{1,2})"
                };

                foreach (var pattern in patterns)
                {
                    var match = Regex.Match(content, pattern);
                    if (match.Success)
                    {
                        try
                        {
                            if (pattern.Contains("yyyy") || match.Groups.Count >= 4)
                            {
                                // 完全な日付
                                var year = int.Parse(match.Groups[1].Value);
                                var month = int.Parse(match.Groups[2].Value);
                                var day = int.Parse(match.Groups[3].Value);
                                
                                if (match.Groups.Count >= 6)
                                {
                                    // 時刻も含む
                                    var hour = int.Parse(match.Groups[4].Value);
                                    var minute = int.Parse(match.Groups[5].Value);
                                    return new DateTime(year, month, day, hour, minute, 0);
                                }
                                else
                                {
                                    return new DateTime(year, month, day);
                                }
                            }
                            else if (match.Groups.Count == 3)
                            {
                                // 時刻のみ
                                var hour = int.Parse(match.Groups[1].Value);
                                var minute = int.Parse(match.Groups[2].Value);
                                return DateTime.Today.AddHours(hour).AddMinutes(minute);
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            catch
            {
                // 日時抽出に失敗した場合は null を返す
            }

            return null;
        }

        /// <summary>
        /// 重複レコードを除去
        /// </summary>
        private List<MedicalRecord> RemoveDuplicates(List<MedicalRecord> records)
        {
            var uniqueRecords = new List<MedicalRecord>();
            var seenContents = new HashSet<string>();

            foreach (var record in records)
            {
                var normalizedContent = record.Content?.Trim().ToLower();
                if (!string.IsNullOrEmpty(normalizedContent) && !seenContents.Contains(normalizedContent))
                {
                    seenContents.Add(normalizedContent);
                    uniqueRecords.Add(record);
                }
            }

            return uniqueRecords;
        }

        /// <summary>
        /// 日時でレコードをグループ化
        /// </summary>
        private List<MedicalRecord> GroupRecordsByDateTime(List<MedicalRecord> records)
        {
            // 日時順にソート
            return records
                .OrderBy(r => r.DateTime ?? DateTime.MaxValue)
                .ThenBy(r => r.Metadata.ContainsKey("lineNumber") ? (int)r.Metadata["lineNumber"] : 0)
                .ToList();
        }
    }
}