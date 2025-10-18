using System;
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
            // null入力やnullレコードの処理を追加
            if (string.IsNullOrEmpty(line) || record == null)
            {
                return;
            }

            var trimmedLine = line.Trim();

            // 空白のみの行は処理しない
            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                return;
            }

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

            // SOAPセクション内の日付行（手術日など）をcontinuation lineとして扱う
            bool isDateInSoapSection = !string.IsNullOrEmpty(record.currentSoapSection) &&
                                       Regex.IsMatch(trimmedLine, @"^\d{4}/\d{1,2}/\d{1,2}");

            if (!foundSoapPattern && !string.IsNullOrWhiteSpace(trimmedLine) &&
                (isDateInSoapSection || !IsHeaderLine(trimmedLine)))
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
                    record.subject = AppendContent(record.subject, content);
                    record.currentSoapSection = "comment";
                    break;
            }
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

            return existing + Environment.NewLine + newContent;
        }
    }
}