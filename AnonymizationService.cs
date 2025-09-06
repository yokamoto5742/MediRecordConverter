using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MediRecordConverter
{
    public class AnonymizationService
    {
        private HashSet<string> replacementWords;
        private string anonymizationSymbol;
        private string replacementListPath;
        private int totalReplacements;
        private DateTime lastLoadTime;

        public AnonymizationService(string anonymizationSymbol = "●●", string replacementListPath = "replacement_list.txt")
        {
            this.anonymizationSymbol = anonymizationSymbol;
            this.replacementListPath = replacementListPath;
            this.replacementWords = new HashSet<string>();
            this.totalReplacements = 0;
            this.lastLoadTime = DateTime.MinValue;
        }

        public bool LoadReplacementList()
        {
            try
            {
                // 複数の候補パスを試行
                List<string> candidatePaths = new List<string>();
                
                // 1. 設定ファイルで指定されたパス（相対パス）
                if (!Path.IsPathRooted(replacementListPath))
                {
                    string executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    string executableDir = Path.GetDirectoryName(executablePath);
                    candidatePaths.Add(Path.Combine(executableDir, replacementListPath));
                }
                else
                {
                    candidatePaths.Add(replacementListPath);
                }
                
                // 2. プロジェクトのルートディレクトリ
                string currentDir = Directory.GetCurrentDirectory();
                candidatePaths.Add(Path.Combine(currentDir, replacementListPath));
                candidatePaths.Add(Path.Combine(currentDir, Path.GetFileName(replacementListPath)));
                
                // 3. 実行ファイルの近く
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                candidatePaths.Add(Path.Combine(appDir, replacementListPath));
                candidatePaths.Add(Path.Combine(appDir, Path.GetFileName(replacementListPath)));
                
                
                string actualPath = null;
                foreach (var path in candidatePaths)
                {
                    if (File.Exists(path))
                    {
                        actualPath = path;
                        break;
                    }
                }
                
                if (actualPath == null)
                {
                    return false;
                }

                replacementWords.Clear();
                string[] lines = null;
                
                // 複数のエンコーディングで試行
                var encodings = new Encoding[] 
                { 
                    Encoding.UTF8, 
                    Encoding.GetEncoding("Shift_JIS"),
                    Encoding.Default 
                };
                
                Exception lastException = null;
                foreach (var encoding in encodings)
                {
                    try
                    {
                        lines = File.ReadAllLines(actualPath, encoding);
                        break;
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                    }
                }
                
                if (lines == null)
                {
                    throw lastException ?? new Exception("すべてのエンコーディングで読み込みに失敗しました");
                }
                
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    // BOM文字を除去
                    string cleanLine = line.Trim().Trim('\uFEFF', '\u200B', '\uFFFE');
                    
                    // シンプルな分割処理：「番号→単語」形式を想定
                    // まず→で分割を試行
                    if (cleanLine.Contains("→"))
                    {
                        var parts = cleanLine.Split('→');
                        
                        if (parts.Length >= 2)
                        {
                            var word = parts[1].Trim();
                            if (!string.IsNullOrEmpty(word))
                            {
                                replacementWords.Add(word);
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                        // 単純な単語形式として処理
                        if (!string.IsNullOrEmpty(cleanLine))
                        {
                            replacementWords.Add(cleanLine);
                        }
                    }
                }

                lastLoadTime = DateTime.Now;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string AnonymizeJsonString(string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
            {
                return jsonText;
            }

            if (replacementWords == null || replacementWords.Count == 0)
            {
                return jsonText;
            }

            totalReplacements = 0;
            string result = jsonText;


            // 各置換対象単語について処理
            foreach (var word in replacementWords)
            {
                if (string.IsNullOrEmpty(word)) continue;

                // 元の単語の出現回数をカウント（大文字小文字区別、部分マッチも確認）
                bool exactMatch = result.Contains(word);
                int originalCount = Regex.Matches(result, Regex.Escape(word)).Count;
                
                
                if (originalCount > 0)
                {
                    // 単語を置換
                    string oldResult = result;
                    result = result.Replace(word, anonymizationSymbol);
                    totalReplacements += originalCount;
                    
                }
                else if (exactMatch)
                {
                }
            }

            return result;
        }

        public List<MedicalRecord> AnonymizeMedicalRecords(List<MedicalRecord> records)
        {
            if (records == null || records.Count == 0)
            {
                return records;
            }

            if (replacementWords == null || replacementWords.Count == 0)
            {
                return records;
            }

            totalReplacements = 0;
            var anonymizedRecords = new List<MedicalRecord>();

            foreach (var record in records)
            {
                var anonymizedRecord = new MedicalRecord
                {
                    timestamp = AnonymizeText(record.timestamp),
                    department = AnonymizeText(record.department),
                    subject = AnonymizeText(record.subject),
                    objectData = AnonymizeText(record.objectData),
                    assessment = AnonymizeText(record.assessment),
                    plan = AnonymizeText(record.plan),
                    comment = AnonymizeText(record.comment),
                    summary = AnonymizeText(record.summary),
                    currentSoapSection = record.currentSoapSection
                };
                
                anonymizedRecords.Add(anonymizedRecord);
            }

            return anonymizedRecords;
        }

        private string AnonymizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            string result = text;

            foreach (var word in replacementWords)
            {
                if (string.IsNullOrEmpty(word)) continue;

                int occurrenceCount = Regex.Matches(result, Regex.Escape(word)).Count;
                if (occurrenceCount > 0)
                {
                    result = result.Replace(word, anonymizationSymbol);
                    totalReplacements += occurrenceCount;
                }
            }

            return result;
        }

        public AnonymizationStatistics GetStatistics()
        {
            return new AnonymizationStatistics
            {
                TotalReplacements = totalReplacements,
                LoadedWordsCount = replacementWords?.Count ?? 0,
                LastLoadTime = lastLoadTime,
                AnonymizationSymbol = anonymizationSymbol,
                ReplacementListPath = replacementListPath
            };
        }

        public bool IsLoaded()
        {
            return replacementWords != null && replacementWords.Count > 0;
        }

        public void ResetStatistics()
        {
            totalReplacements = 0;
        }
    }

    public class AnonymizationStatistics
    {
        public int TotalReplacements { get; set; }
        public int LoadedWordsCount { get; set; }
        public DateTime LastLoadTime { get; set; }
        public string AnonymizationSymbol { get; set; }
        public string ReplacementListPath { get; set; }

        public override string ToString()
        {
            return $"置換数: {TotalReplacements}件, 登録語数: {LoadedWordsCount}件, " +
                   $"読込日時: {LastLoadTime:yyyy/MM/dd HH:mm:ss}";
        }
    }
}