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
                
                System.Diagnostics.Debug.WriteLine($"置換リストファイル候補パス:");
                foreach (var path in candidatePaths)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {path} (存在: {File.Exists(path)})");
                }
                
                string actualPath = null;
                foreach (var path in candidatePaths)
                {
                    if (File.Exists(path))
                    {
                        actualPath = path;
                        System.Diagnostics.Debug.WriteLine($"使用するパス: {actualPath}");
                        break;
                    }
                }
                
                if (actualPath == null)
                {
                    System.Diagnostics.Debug.WriteLine($"すべての候補パスで置換リストファイルが見つかりません");
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
                        System.Diagnostics.Debug.WriteLine($"ファイル読み込み成功: {encoding.EncodingName}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        System.Diagnostics.Debug.WriteLine($"エンコーディング {encoding.EncodingName} で読み込み失敗: {ex.Message}");
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
                    System.Diagnostics.Debug.WriteLine($"処理行: '{cleanLine}' (元: '{line}')");
                    
                    // シンプルな分割処理：「番号→単語」形式を想定
                    // まず→で分割を試行
                    if (cleanLine.Contains("→"))
                    {
                        var parts = cleanLine.Split('→');
                        System.Diagnostics.Debug.WriteLine($"→で分割: {parts.Length}個 [{string.Join("|", parts)}]");
                        
                        if (parts.Length >= 2)
                        {
                            var word = parts[1].Trim();
                            if (!string.IsNullOrEmpty(word))
                            {
                                replacementWords.Add(word);
                                System.Diagnostics.Debug.WriteLine($"単語追加成功: '{word}'");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"単語が空でした: parts[1]='{parts[1]}'");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"分割結果が不正: {parts.Length}個");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"矢印文字→が見つかりませんでした: '{cleanLine}'");
                        
                        // デバッグ用：文字コードをチェック
                        for (int i = 0; i < cleanLine.Length; i++)
                        {
                            char c = cleanLine[i];
                            System.Diagnostics.Debug.WriteLine($"  文字[{i}]: '{c}' (U+{((int)c).ToString("X4")})");
                        }
                    }
                }

                lastLoadTime = DateTime.Now;
                System.Diagnostics.Debug.WriteLine($"置換リスト読み込み完了: {replacementWords.Count}件");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"置換リスト読み込みエラー: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine("置換リストが未読み込みまたは空です");
                return jsonText;
            }

            totalReplacements = 0;
            string result = jsonText;

            System.Diagnostics.Debug.WriteLine($"匿名化開始: {replacementWords.Count}個の単語で処理開始");
            System.Diagnostics.Debug.WriteLine($"元JSON: {jsonText.Substring(0, Math.Min(200, jsonText.Length))}...");

            // 各置換対象単語について処理
            foreach (var word in replacementWords)
            {
                if (string.IsNullOrEmpty(word)) continue;

                // 元の単語の出現回数をカウント（大文字小文字区別、部分マッチも確認）
                bool exactMatch = result.Contains(word);
                int originalCount = Regex.Matches(result, Regex.Escape(word)).Count;
                
                System.Diagnostics.Debug.WriteLine($"単語チェック: '{word}' - 含有:{exactMatch}, 正規表現マッチ数:{originalCount}");
                
                if (originalCount > 0)
                {
                    // 単語を置換
                    string oldResult = result;
                    result = result.Replace(word, anonymizationSymbol);
                    totalReplacements += originalCount;
                    
                    System.Diagnostics.Debug.WriteLine($"置換実行: '{word}' → '{anonymizationSymbol}' ({originalCount}件)");
                    System.Diagnostics.Debug.WriteLine($"置換前後比較: '{oldResult.Substring(0, Math.Min(100, oldResult.Length))}...' → '{result.Substring(0, Math.Min(100, result.Length))}...'");
                }
                else if (exactMatch)
                {
                    System.Diagnostics.Debug.WriteLine($"警告: '{word}' は含まれているが正規表現でマッチしません");
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"匿名化完了: 総置換数={totalReplacements}件");

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
                System.Diagnostics.Debug.WriteLine("置換リストが未読み込みまたは空です");
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