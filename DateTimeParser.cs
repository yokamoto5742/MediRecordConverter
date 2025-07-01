using System;
using System.Text.RegularExpressions;

namespace MediRecordConverter
{
    public class DateTimeParser
    {
        public string ExtractDate(string line)
        {
            // null または空文字列のチェックを追加
            if (string.IsNullOrEmpty(line))
            {
                return null;
            }

            if (IsDoctorRecordLine(line))
            {
                return null;
            }

            var datePatterns = new string[]
            {
                @"^(\d{4}/\d{1,2}/\d{1,2})\([月火水木金土日]\)",  // 日付(曜日) + 追加テキスト可
                @"^(\d{4}/\d{1,2}/\d{1,2})\([月火水木金土日]\)$", // 行全体が日付(曜日)のみ
                @"^(\d{4}/\d{1,2}/\d{1,2})$",                      // 行全体が日付のみ
            };

            foreach (var pattern in datePatterns)
            {
                try
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
                            return result;
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ExtractDate正規表現エラー: {ex.Message}");
                }
            }
            return null;
        }

        public string CombineDateAndTime(string date, string time)
        {
            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(time))
            {
                return "";
            }

            try
            {
                var timeParts = time.Split(':');
                var hour = int.Parse(timeParts[0]);
                var minute = int.Parse(timeParts[1]);

                var result = $"{date}{hour:D2}:{minute:D2}:00Z";
                return result;
            }
            catch (Exception)
            {
                return "";
            }
        }

        private bool IsDoctorRecordLine(string line)
        {
            // null または空文字列のチェックを追加
            if (string.IsNullOrEmpty(line))
            {
                return false;
            }

            var patterns = new string[]
            {
                @"^(内科|外科|透析|整形外科|皮膚科|眼科|耳鼻咽喉科|泌尿器科|小児科|精神科|放射線科|麻酔科|リハビリ科|薬剤科|検査科|栄養科)[\s　]+.*?\d{1,2}:\d{2}"
            };

            try
            {
                foreach (var pattern in patterns)
                {
                    if (Regex.IsMatch(line, pattern))
                        return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IsDoctorRecordLine正規表現エラー: {ex.Message}");
                return false;
            }

            return false;
        }
    }
}