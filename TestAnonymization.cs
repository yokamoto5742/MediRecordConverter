using System;
using System.IO;

namespace MediRecordConverter
{
    class TestAnonymization
    {
        static void Main(string[] args)
        {
            // テスト用エントリーポイント（通常のMain関数とは別）
            TestAnonymizationService();
        }

        static void TestAnonymizationService()
        {
            Console.WriteLine("=== AnonymizationService テスト開始 ===");
            
            var anonymizationService = new AnonymizationService("●●", "replacement_list.txt");
            
            // ファイル読み込みテスト
            Console.WriteLine("1. ファイル読み込みテスト:");
            bool loadResult = anonymizationService.LoadReplacementList();
            Console.WriteLine($"   読み込み結果: {loadResult}");
            
            var stats = anonymizationService.GetStatistics();
            Console.WriteLine($"   読み込み語数: {stats.LoadedWordsCount}件");
            
            // テストデータで置換テスト
            string testJson = @"{
  ""timestamp"": ""2025-05-19T14:47:00Z"",
  ""department"": ""内科"",
  ""summary"": ""さくら病棟の横山敏啓先生がわかば病棟で診察""
}";
            
            Console.WriteLine("\n2. 置換テスト:");
            Console.WriteLine("   元データ:");
            Console.WriteLine($"   {testJson}");
            
            string anonymizedJson = anonymizationService.AnonymizeJsonString(testJson);
            
            Console.WriteLine("\n   匿名化後:");
            Console.WriteLine($"   {anonymizedJson}");
            
            var finalStats = anonymizationService.GetStatistics();
            Console.WriteLine($"\n   置換実行数: {finalStats.TotalReplacements}件");
            
            Console.WriteLine("\n=== テスト完了 ===");
        }
    }
}