using System;
using System.IO;

namespace MediRecordConverter
{
    // シンプルテスト（MainプログラムのProgram.csを一時的に置き換える）
    class SimpleTest
    {
        static void TestAnonymization()
        {
            Console.WriteLine("=== AnonymizationService テスト開始 ===");
            
            // テスト用の小さなリストファイルを作成
            string testListPath = "test_replacement.txt";
            File.WriteAllText(testListPath, "1→横山\n2→さくら\n3→わかば\n4→敏啓\n");
            
            Console.WriteLine($"テストファイル作成: {testListPath}");
            Console.WriteLine($"ファイル存在確認: {File.Exists(testListPath)}");
            
            var anonymizationService = new AnonymizationService("●●", testListPath);
            
            // ファイル読み込みテスト
            Console.WriteLine("\n1. ファイル読み込みテスト:");
            bool loadResult = anonymizationService.LoadReplacementList();
            Console.WriteLine($"   読み込み結果: {loadResult}");
            
            var stats = anonymizationService.GetStatistics();
            Console.WriteLine($"   読み込み語数: {stats.LoadedWordsCount}件");
            
            if (stats.LoadedWordsCount == 0)
            {
                Console.WriteLine("   エラー: 単語が読み込まれませんでした");
                return;
            }
            
            // テストデータで置換テスト
            string testJson = @"{""summary"": ""横山先生がさくら病棟で敏啓さんとわかば地区を訪問""}";
            
            Console.WriteLine("\n2. 置換テスト:");
            Console.WriteLine($"   元データ: {testJson}");
            
            string anonymizedJson = anonymizationService.AnonymizeJsonString(testJson);
            
            Console.WriteLine($"   匿名化後: {anonymizedJson}");
            
            var finalStats = anonymizationService.GetStatistics();
            Console.WriteLine($"\n   置換実行数: {finalStats.TotalReplacements}件");
            
            // 期待値確認
            bool isCorrect = anonymizedJson.Contains("●●先生が●●病棟で●●さんと●●地区を訪問");
            Console.WriteLine($"   置換確認: {(isCorrect ? "成功" : "失敗")}");
            
            // クリーンアップ
            if (File.Exists(testListPath))
            {
                File.Delete(testListPath);
            }
            
            Console.WriteLine("\n=== テスト完了 ===");
        }
    }
}