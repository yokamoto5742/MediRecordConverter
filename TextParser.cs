using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MediRecordConverter
{
    // 医療記録を表すデータクラス
    public class MedicalRecord
    {
        public string Date { get; set; }
        public string Department { get; set; }
        public string Time { get; set; }
        public string SoapSection { get; set; }
        public string Content { get; set; }
    }

    // グループ化された医療記録を表すクラス
    public class GroupedMedicalRecord
    {
        public string Timestamp { get; set; }
        public string Department { get; set; }
        public string Subject { get; set; }
        public string Object { get; set; }
        public string Assessment { get; set; }
        public string Plan { get; set; }
        public string Comment { get; set; }
        public string Summary { get; set; }

        // JSON出力用のクリーンアップされたオブジェクトを返す
        public object ToJsonObject()
        {
            var result = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(Timestamp))
                result["timestamp"] = Timestamp;
            if (!string.IsNullOrEmpty(Department))
                result["department"] = Department;
            if (!string.IsNullOrEmpty(Subject))
                result["subject"] = Subject;
            if (!string.IsNullOrEmpty(Object))
                result["object"] = Object;
            if (!string.IsNullOrEmpty(Assessment))
                result["assessment"] = Assessment;
            if (!string.IsNullOrEmpty(Plan))
                result["plan"] = Plan;
            if (!string.IsNullOrEmpty(Comment))
                result["comment"] = Comment;
            if (!string.IsNullOrEmpty(Summary))
                result["summary"] = Summary;

            return result;
        }
    }

    public class TextParser
    {
        // 正規表現パターン
        private readonly Regex datePattern = new Regex(@"(\d{4}/\d{2}/\d{2}\(.?\))(?:\s*（入院\s*(\d+)\s*日目）)?");
        private readonly Regex entryPattern = new Regex(@"(.+?)\s+(.+?)\s+(.+?)\s+(\d{2}:\d{2})");
        private readonly Regex soapPattern = new Regex(@"([SOAPFサ])\s*>");

        public object ParseMedicalText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<object>();
            }

            var records = ParseRecords(text);
            var groupedRecords = GroupRecordsByDateTime(records);
            var finalRecords = RemoveDuplicates(groupedRecords);

            // JSON出力用にクリーンアップされたオブジェクトのリストを返す
            return finalRecords.Select(r => r.ToJsonObject()).ToList();
        }

        private List<MedicalRecord> ParseRecords(string text)
        {
            var records = new List<MedicalRecord>();
            var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.None);

            var currentRecord = new MedicalRecord();
            var contentBuffer = new StringBuilder();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine))
                    continue;

                // 日付パターンをチェック
                var dateMatch = datePattern.Match(trimmedLine);
                if (dateMatch.Success)
                {
                    ProcessRecord(currentRecord, contentBuffer.ToString(), records);
                    currentRecord = new MedicalRecord { Date = dateMatch.Groups[1].Value };
                    contentBuffer.Clear();
                    continue;
                }

                // エントリーパターンをチェック（部門、時間など）
                var entryMatch = entryPattern.Match(trimmedLine);
                if (entryMatch.Success && !string.IsNullOrEmpty(currentRecord.Date))
                {
                    ProcessRecord(currentRecord, contentBuffer.ToString(), records);
                    currentRecord.Department = entryMatch.Groups[1].Value.Trim();
                    currentRecord.Time = entryMatch.Groups[4].Value.Trim();
                    contentBuffer.Clear();
                    continue;
                }

                // SOAPパターンをチェック
                var soapMatch = soapPattern.Match(trimmedLine);
                if (soapMatch.Success && !string.IsNullOrEmpty(currentRecord.Department))
                {
                    ProcessRecord(currentRecord, contentBuffer.ToString(), records);
                    currentRecord.SoapSection = soapMatch.Groups[1].Value;
                    contentBuffer.Clear();
                    continue;
                }

                // コンテンツの蓄積
                if (!string.IsNullOrEmpty(currentRecord.SoapSection))
                {
                    contentBuffer.AppendLine(trimmedLine);
                }
            }

            // 最後のレコードを処理
            ProcessRecord(currentRecord, contentBuffer.ToString(), records);

            return records;
        }
    }
}