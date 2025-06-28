using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MediRecordConverter
{
    public class TextParser
    {
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

            [JsonIgnore]
            public string currentSoapSection { get; set; } = "";

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

                    var recordMatch = ExtractDoctorRecord(line);
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
                            timestamp = CombineDateAndTime(currentDate, recordMatch.Value.Time),
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

                    var dateMatch = ExtractDate(line);
                    if (!string.IsNullOrEmpty(dateMatch))
                    {
                        currentDate = dateMatch;
                        System.Diagnostics.Debug.WriteLine($"日付を検出: {currentDate}");
                        continue;
                    }

                    if (currentRecord != null)
                    {
                        ClassifySOAPContent(line, currentRecord);
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

                // 空のフィールドを持つレコードをクリーンアップ
                records = CleanupRecords(records);

                // 同じ時刻のレコードをマージ
                records = MergeRecordsByTimestamp(records);

                // 日付時刻順にソート（古い順）
                records = SortRecordsByDateTime(records);

                System.Diagnostics.Debug.WriteLine($"最終結果: {records.Count}個のレコード");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ParseMedicalText Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }

            return records;
        }

        private string ExtractDate(string line)
        {
            if (IsDoctorRecordLine(line))
            {
                System.Diagnostics.Debug.WriteLine($"医師記録行のため日付抽出をスキップ: {line}");
                return null;
            }

            var datePatterns = new string[]
            {
                @"^(\d{4}/\d{1,2}/\d{1,2})\([月火水木金土日]\)$",  // 行全体が日付(曜日)
                @"^(\d{4}/\d{1,2}/\d{1,2})$",                      // 行全体が日付
            };

            foreach (var pattern in datePatterns)
            {
                var match = Regex.Match(line, pattern);
                if (match.Success)
                {
                    try
                    {
                        var dateStr = match.Groups[1].Value;
                        var dateParts = dateStr.Split('/');
                        var year = int.Parse(dateParts[0]);
                        var month = int.Parse(dateParts[1]);
                        var day = int.Parse(dateParts[2]);

                        var result = new DateTime(year, month, day).ToString("yyyy-MM-ddT");
                        System.Diagnostics.Debug.WriteLine($"日付抽出成功: {line} → {result}");
                        return result;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"日付パース失敗: {line} - {ex.Message}");
                        return null;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"日付パターンマッチなし: {line}");
            return null;
        }

        private bool IsDoctorRecordLine(string line)
        {
            var patterns = new string[]
            {
                @"^(内科|外科|透析|整形外科|皮膚科|眼科|耳鼻咽喉科|泌尿器科|小児科|精神科|放射線科|麻酔科|リハビリ科|薬剤科|検査科|栄養科)[\s　]+.*?\d{1,2}:\d{2}"
            };

            foreach (var pattern in patterns)
            {
                if (Regex.IsMatch(line, pattern))
                    return true;
            }

            return false;
        }

        private (string Department, string Time)? ExtractDoctorRecord(string line)
        {
            // より具体的な正規表現パターン
            var patterns = new string[]
            {
           
                @"^(内科|外科|透析|整形外科|皮膚科|眼科|耳鼻科|泌尿器科|婦人科|小児科|精神科|放射線科|麻酔科|病理科|リハビリ科|薬剤科|検査科|栄養科)[\s　]+.*?[\s　]+(\d{1,2}:\d{2})[\s　]*\(最終更新.*?\)【救急】",
               
                @"^(内科|外科|透析|整形外科|皮膚科|眼科|耳鼻科|泌尿器科|婦人科|小児科|精神科|放射線科|麻酔科|病理科|リハビリ科|薬剤科|検査科|栄養科)[\s　]+.*?[\s　]+(\d{1,2}:\d{2})[\s　]*\(最終更新.*?\)(?!【救急】)",  
                
                @"^(内科|外科|透析|整形外科|皮膚科|眼科|耳鼻科|泌尿器科|婦人科|小児科|精神科|放射線科|麻酔科|病理科|リハビリ科|薬剤科|検査科|栄養科)[\s　]+.*?[\s　]+(\d{1,2}:\d{2})(?:\s|$)",
                
                @"^(内科|外科|透析|整形外科|皮膚科|眼科|耳鼻科|泌尿器科|婦人科|小児科|精神科|放射線科|麻酔科|病理科|リハビリ科|薬剤科|検査科|栄養科).*?(\d{1,2}:\d{2})"
            };

            for (int i = 0; i < patterns.Length; i++)
            {
                var pattern = patterns[i];
                try
                {
                    var match = Regex.Match(line, pattern);
                    if (match.Success)
                    {
                        var department = match.Groups[1].Value;
                        var time = match.Groups[2].Value;
                        System.Diagnostics.Debug.WriteLine($"医師記録抽出成功(パターン{i}): {line} → 科={department}, 時刻={time}");
                        return (department, time);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"正規表現エラー(パターン{i}): {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"医師記録パターンマッチなし: {line}");
            return null;
        }

        private string CombineDateAndTime(string date, string time)
        {
            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(time))
            {
                System.Diagnostics.Debug.WriteLine($"タイムスタンプ作成失敗: date={date}, time={time}");
                return "";
            }

            try
            {
                var timeParts = time.Split(':');
                var hour = int.Parse(timeParts[0]);
                var minute = int.Parse(timeParts[1]);

                var result = $"{date}{hour:D2}:{minute:D2}:00Z";
                System.Diagnostics.Debug.WriteLine($"タイムスタンプ作成成功: {date} + {time} → {result}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"タイムスタンプ作成エラー: {ex.Message}");
                return "";
            }
        }

        private void ClassifySOAPContent(string line, MedicalRecord record)
        {
            var trimmedLine = line.Trim();

            var soapMapping = new Dictionary<string, string>
            {
                { "S", "subject" },
                { "O", "object" },
                { "A", "assessment" },
                { "P", "plan" },
                { "F", "comment" },
                { "サ", "summary" }
            };

            bool foundSoapPattern = false;
            foreach (var mapping in soapMapping)
            {
                var patterns = new string[]
                {
                    $"{mapping.Key} >",
                    $"{mapping.Key}>",
                    $"{mapping.Key} ＞",
                    $"{mapping.Key}＞",
                    $"{mapping.Key} ",
                    $"{mapping.Key}　"
                };

                foreach (var pattern in patterns)
                {
                    if (trimmedLine.StartsWith(pattern))
                    {
                        record.currentSoapSection = mapping.Value;
                        foundSoapPattern = true;

                        var content = "";
                        if (trimmedLine.Length > pattern.Length)
                        {
                            content = trimmedLine.Substring(pattern.Length).Trim();
                        }

                        System.Diagnostics.Debug.WriteLine($"SOAPセクション検出: {mapping.Key} → {mapping.Value}, 内容: {content}");

                        if (!string.IsNullOrEmpty(content))
                        {
                            SetFieldByMapping(record, mapping.Value, content);
                        }
                        return;
                    }
                }
            }

            if (!foundSoapPattern && !string.IsNullOrWhiteSpace(trimmedLine) && !IsHeaderLine(trimmedLine))
            {
                System.Diagnostics.Debug.WriteLine($"継続行として処理: {trimmedLine} (現在のセクション: {record.currentSoapSection})");
                AddContinuationLine(record, trimmedLine);
            }
        }

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
            System.Diagnostics.Debug.WriteLine($"フィールド設定: {fieldName} = {content}");
        }

        private void AddContinuationLine(MedicalRecord record, string content)
        {
            switch (record.currentSoapSection)
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
                default:
                    // インテリジェントに分類
                    if (IsObjectiveContent(content))
                    {
                        record.objectData = AppendContent(record.objectData, content);
                        record.currentSoapSection = "object";
                    }
                    else if (IsAssessmentContent(content))
                    {
                        record.assessment = AppendContent(record.assessment, content);
                        record.currentSoapSection = "assessment";
                    }
                    else if (IsPlanContent(content))
                    {
                        record.plan = AppendContent(record.plan, content);
                        record.currentSoapSection = "plan";
                    }
                    else
                    {
                        record.subject = AppendContent(record.subject, content);
                        record.currentSoapSection = "subject";
                    }
                    break;
            }
        }

        private bool IsObjectiveContent(string content)
        {
            var objectiveKeywords = new string[]
            {
                "結膜", "角膜", "前房", "水晶体", "乳頭", "網膜", "眼圧", "視力",
                "血圧", "体温", "脈拍", "呼吸", "血液検査", "検査結果", "画像", "所見",
                "slit", "cor", "ac", "lens", "disc", "fds", "AVG", "mmHg"
            };

            return objectiveKeywords.Any(keyword => content.Contains(keyword));
        }

        private bool IsAssessmentContent(string content)
        {
            var assessmentKeywords = new string[]
            {
                "＃", "#", "診断", "評価", "慢性", "症", "病", "疾患", "状態", "不全",
                "出血", "結膜下出血", "白内障", "緑内障", "進行", "影響"
            };

            return assessmentKeywords.Any(keyword => content.Contains(keyword));
        }
>
        private bool IsPlanContent(string content)
        {
            var planKeywords = new string[]
            {
                "治療", "処方", "継続", "指導", "制限", "予定", "検討", "再開",
                "維持", "採血", "注射", "薬", "mg", "錠", "単位", "再診",
                "medi", "終了", "指示", "週間後", "視力", "眼圧"
            };

            return planKeywords.Any(keyword => content.Contains(keyword));
        }

        private bool IsHeaderLine(string line)
        {
            // 日付行の判定
            if (Regex.IsMatch(line, @"^\d{4}/\d{1,2}/\d{1,2}"))
                return true;

            // 医師記録行の判定
            if (IsDoctorRecordLine(line))
                return true;

            // その他のヘッダー情報
            if (line.Contains("【救急】") || line.Contains("最終更新"))
                return true;

            return false;
        }

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

        private List<MedicalRecord> SortRecordsByDateTime(List<MedicalRecord> records)
        {
            return records.OrderBy(record =>
            {
                if (DateTime.TryParse(record.timestamp?.Replace("Z", ""), out DateTime parsedDate))
                {
                    return parsedDate;
                }
                return DateTime.MinValue;
            }).ToList();
        }

        private string AppendContent(string existing, string newContent)
        {
            if (string.IsNullOrEmpty(existing))
                return newContent;

            if (string.IsNullOrEmpty(newContent))
                return existing;

            return existing + "\n" + newContent;
        }

        private List<MedicalRecord> CleanupRecords(List<MedicalRecord> records)
        {
            var cleanedRecords = new List<MedicalRecord>();

            foreach (var record in records)
            {
                if (!string.IsNullOrEmpty(record.timestamp) && !string.IsNullOrEmpty(record.department))
                {
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
}