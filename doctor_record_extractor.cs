using System.Text.RegularExpressions;

namespace MediRecordConverter
{
    public class DoctorRecordExtractor
    {
        public struct DoctorRecordInfo
        {
            public string Department { get; set; }
            public string Time { get; set; }
        }

        public DoctorRecordInfo? ExtractDoctorRecord(string line)
        {
            var patterns = new string[]
            {
                @"^(内科|外科|透析|整形外科|皮膚科|眼科|耳鼻科|泌尿器科|婦人科|小児科|精神科|放射線科|麻酔科|病理科|リハビリ科|薬剤科|検査科|栄養科)[\s　]+.*?[\s　]+(\d{1,2}:\d{2})[\s　]*\(最終更新.*?\)【救急】",
                @"^(内科|外科|透析|整形外科|皮膚科|眼科|耳鼻科|泌尿器科|婦人科|小児科|精神科|放射線科|麻酔科|病理科|リハビリ科|薬剤科|検査科|栄養科)[\s　]+.*?[\s　]+(\d{1,2}:\d{2})[\s　]*\(最終更新.*?\)(?!【救急】)",  
                @"^(内科|外科|透析|整形外科|皮膚科|眼科|耳鼻科|泌尿器科|婦人科|小児科|精神科|放射線科|麻酔科|病理科|リハビリ科|薬剤科|検査科|栄養科)[\s　]+.*?[\s　]+(\d{1,2}:\d{2})(?:\s|$)",
                @"^(内科|外科|透析|整形外科|皮膚科|眼科|耳鼻科|泌尿器科|婦人科|小児科|精神科|放射線科|麻酔科|病理科|リハビリ科|薬剤科|検査科|栄養科).*?(\d{1,2}:\d{2})"
            };

            for (int i = 0; i < patterns.Length; i++)
            {
                var pattern = patterns[i];
                try
                {
                    var match = Regex.Match(line, pattern);
                    if (match.Success)
                    {
                        var department = match.Groups[1].Value;
                        var time = match.Groups[2].Value;
                        
                        return new DoctorRecordInfo
                        {
                            Department = department,
                            Time = time
                        };
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"正規表現エラー(パターン{i}): {ex.Message}");
                }
            }

            return null;
        }

        public bool IsDoctorRecordLine(string line)
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