using System;
using System.Collections.Generic;
using System.Linq;

namespace MediRecordConverter
{
    public class MedicalRecordProcessor
    {
        public List<MedicalRecord> CleanupRecords(List<MedicalRecord> records)
        {
            if (records == null)
            {
                throw new System.NullReferenceException("records cannot be null");
            }

            var cleanedRecords = new List<MedicalRecord>();

            foreach (var record in records)
            {
                if (!string.IsNullOrEmpty(record.timestamp) && !string.IsNullOrEmpty(record.department))
                {
                    var cleanRecord = new MedicalRecord
                    {
                        timestamp = record.timestamp ?? "",
                        department = record.department ?? "",
                        subject = string.IsNullOrWhiteSpace(record.subject) ? null : record.subject,
                        objectData = string.IsNullOrWhiteSpace(record.objectData) ? null : record.objectData,
                        assessment = string.IsNullOrWhiteSpace(record.assessment) ? null : record.assessment,
                        plan = string.IsNullOrWhiteSpace(record.plan) ? null : record.plan,
                        comment = string.IsNullOrWhiteSpace(record.comment) ? null : record.comment,
                        summary = string.IsNullOrWhiteSpace(record.summary) ? null : record.summary
                    };

                    cleanedRecords.Add(cleanRecord);
                }
            }

            return cleanedRecords;
        }

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