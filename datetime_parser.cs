using System;
using System.Text.RegularExpressions;

namespace MediRecordConverter
{
    public class DateTimeParser
    {
        public string ExtractDate(string line)
        {
            if (IsDoctorRecordLine(line))
            {
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
                        return result;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }
            return null;
        }

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
                return result;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

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