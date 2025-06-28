using System;
using System.Collections.Generic;
using System.Linq;

namespace MediRecordConverter
{
    /// <summary>
    /// 医療記録の後処理（マージ、ソート、クリーンアップ）を担当するクラス
    /// </summary>
    public class MedicalRecordProcessor
    {
        /// <summary>
        /// 空のフィールドを持つレコードをクリーンアップします
        /// </summary>
        /// <param name="records">処理対象のレコードリスト</param>
        /// <returns>クリーンアップされたレコードリスト</returns>
        public List<MedicalRecord> CleanupRecords(List<MedicalRecord> records)
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

        /// <summary>
        /// 同じタイムスタンプのレコードをマージします
        /// </summary>
        /// <param name="records">処理対象のレコードリスト</param>
        /// <returns>マージされたレコードリスト</returns>
        public List<MedicalRecord> MergeRecordsByTimestamp(List<MedicalRecord> records)
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

        /// <summary>
        /// レコードを日時順にソートします
        /// </summary>
        /// <param name="records">処理対象のレコードリスト</param>
        /// <returns>ソートされたレコードリスト</returns>
        public List<MedicalRecord> SortRecordsByDateTime(List<MedicalRecord> records)
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

        /// <summary>
        /// 既存の内容に新しい内容を追加します
        /// </summary>
        /// <param name="existing">既存の内容</param>
        /// <param name="newContent">新しい内容</param>
        /// <returns>結合された内容</returns>
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