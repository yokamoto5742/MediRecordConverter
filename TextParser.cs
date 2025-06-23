using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediRecordConverter
{
    public class TextParser
    {
        public object ParseMedicalText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new { content = "", lines = new string[0], metadata = new { } };
            }

            var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // 基本的なJSONオブジェクトを作成
            var result = new
            {
                content = text,
                lines = lines,
                metadata = new
                {
                    lineCount = lines.Length,
                    characterCount = text.Length,
                    processedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    sections = ParseSections(lines)
                }
            };

            return result;
        }

        private object ParseSections(string[] lines)
        {
            var sections = new List<object>();

            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sections.Add(new
                    {
                        text = line.Trim(),
                        length = line.Trim().Length,
                        type = DetermineLineType(line.Trim())
                    });
                }
            }

            return sections;
        }

        private string DetermineLineType(string line)
        {
            // 簡単な分類ロジック
            if (line.Contains("：") || line.Contains(":"))
                return "header";
            else if (line.Length < 20)
                return "short";
            else
                return "content";
        }
    }
}