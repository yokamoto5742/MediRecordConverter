using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MediRecordConverter
{
    public class SOAPClassifier
    {
        private readonly DoctorRecordExtractor doctorRecordExtractor;

        public SOAPClassifier()
        {
            doctorRecordExtractor = new DoctorRecordExtractor();
        }

        public void ClassifySOAPContent(string line, MedicalRecord record)
        {
            var trimmedLine = line.Trim();

            var soapMapping = new Dictionary<string, string>
            {
                { "S", "subject" },
                { "O", "object" },
                { "A", "assessment" },
                { "P", "plan" },
                { "F", "comment" },
                { "ｻ", "summary" }
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
            if (doctorRecordExtractor.IsDoctorRecordLine(line))
                return true;

            // その他のヘッダー情報
            if (line.Contains("【救急】") || line.Contains("最終更新"))
                return true;

            return false;
        }

        private string AppendContent(string existing, string newContent)
        {
            if (string.IsNullOrEmpty(existing))
                return newContent;

            if (string.IsNullOrEmpty(newContent))
                return existing;

            return existing + "\n" + newContent;
        }
    }
}