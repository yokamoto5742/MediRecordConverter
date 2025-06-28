using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MediRecordConverter
{
    /// <summary>
    /// SOAP形式の内容分類を担当するクラス
    /// </summary>
    public class SOAPClassifier
    {
        private readonly DoctorRecordExtractor doctorRecordExtractor;

        public SOAPClassifier()
        {
            doctorRecordExtractor = new DoctorRecordExtractor();
        }

        /// <summary>
        /// テキスト行をSOAP形式で分類し、医療記録に追加します
        /// </summary>
        /// <param name="line">処理する行</param>
        /// <param name="record">追加先の医療記録</param>
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

        /// <summary>
        /// 医療記録のフィールドにマッピングに基づいて内容を設定します
        /// </summary>
        /// <param name="record">対象の医療記録</param>
        /// <param name="fieldName">フィールド名</param>
        /// <param name="content">設定する内容</param>
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

        /// <summary>
        /// 継続行として内容を追加します
        /// </summary>
        /// <param name="record">対象の医療記録</param>
        /// <param name="content">追加する内容</param>
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

        /// <summary>
        /// 内容がObjective（客観的所見）かどうかを判定します
        /// </summary>
        /// <param name="content">判定する内容</param>
        /// <returns>Objectiveの場合true</returns>
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

        /// <summary>
        /// 内容がAssessment（評価）かどうかを判定します
        /// </summary>
        /// <param name="content">判定する内容</param>
        /// <returns>Assessmentの場合true</returns>
        private bool IsAssessmentContent(string content)
        {
            var assessmentKeywords = new string[]
            {
                "＃", "#", "診断", "評価", "慢性", "症", "病", "疾患", "状態", "不全",
                "出血", "結膜下出血", "白内障", "緑内障", "進行", "影響"
            };

            return assessmentKeywords.Any(keyword => content.Contains(keyword));
        }

        /// <summary>
        /// 内容がPlan（計画）かどうかを判定します
        /// </summary>
        /// <param name="content">判定する内容</param>
        /// <returns>Planの場合true</returns>
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

        /// <summary>
        /// ヘッダー行かどうかを判定します
        /// </summary>
        /// <param name="line">判定する行</param>
        /// <returns>ヘッダー行の場合true</returns>
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