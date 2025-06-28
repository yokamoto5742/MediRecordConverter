using System;
using System.Text.RegularExpressions;

namespace MediRecordConverter
{
    /// <summary>
    /// 日付・時刻の抽出と処理を担当するクラス
    /// </summary>
    public class DateTimeParser
    {
        /// <summary>
        /// 行から日付を抽出します
        /// </summary>
        /// <param name="line">処理する行</param>
        /// <returns>抽出された日付文字列（ISO形式）、見つからない場合はnull</returns>
        public string ExtractDate(string line)
        {
            if (IsDoctorRecordLine(line))
            {
                System.Diagnostics.Debug.WriteLine($"医師記録行のため日付抽出をスキップ: {line}");
                return null;
            }

            var datePatterns = new string[]
            {
                @"^(\d{4}/\d{1,2}/\d{1,2})\([月火水木金土日]\)$",  // 行全体が日付(曜日)
                @"^(\d{4}/\d{1,2}/\d{1,2})$",                      // 行全体が日付
            };

            foreach (var pattern in datePatterns)
            {
                var match = Regex.Match(line, pattern);
                if (match.Success)
                {
                    try
                    {
                        var dateStr = match.Groups[1].Value;
                        var dateParts = dateStr.Split('/');
                        var year = int.Parse(dateParts[0]);
                        var month = int.Parse(dateParts[1]);
                        var day = int.Parse(dateParts[2]);

                        var result = new DateTime(year, month, day).ToString("yyyy-MM-ddT");
                        System.Diagnostics.Debug.WriteLine($"日付抽出成功: {line} → {result}");
                        return result;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"日付パース失敗: {line} - {ex.Message}");
                        return null;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"日付パターンマッチなし: {line}");
            return null;
        }

        /// <summary>
        /// 日付と時刻を組み合わせてタイムスタンプを作成します
        /// </summary>
        /// <param name="date">日付文字列</param>
        /// <param name="time">時刻文字列</param>
        /// <returns>組み合わされたタイムスタンプ</returns>
        public string CombineDateAndTime(string date, string time)
        {
            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(time))
            {
                System.Diagnostics.Debug.WriteLine($"タイムスタンプ作成失敗: date={date}, time={time}");
                return "";
            }

            try
            {
                var timeParts = time.Split(':');
                var hour = int.Parse(timeParts[0]);
                var minute = int.Parse(timeParts[1]);

                var result = $"{date}{hour:D2}:{minute:D2}:00Z";
                System.Diagnostics.Debug.WriteLine($"タイムスタンプ作成成功: {date} + {time} → {result}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"タイムスタンプ作成エラー: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// 行が医師記録行かどうかを判定します
        /// </summary>
        /// <param name="line">判定する行</param>
        /// <returns>医師記録行の場合true</returns>
        private bool IsDoctorRecordLine(string line)
        {
            var patterns = new string[]
            {
                @"^(内科|外科|透析|整形外科|皮膚科|眼科|耳鼻咽喉科|泌尿器科|小児科|精神科|放射線科|麻酔科|リハビリ科|薬剤科|検査科|栄養科)[\s　]+.*?\d{1,2}:\d{2}"
            };

            foreach (var pattern in patterns)
            {
                if (Regex.IsMatch(line, pattern))
                    return true;
            }

            return false;
        }
    }
}