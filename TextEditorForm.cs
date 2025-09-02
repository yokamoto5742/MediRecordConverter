using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace MediRecordConverter
{
    public class TextEditorForm : Form
    {
        private TextBox textEditor;
        private Button closeButton;
        private Button saveButton;
        private Button pasteButton;
        private Button clearButton;
        private Label statsLabel;
        private ConfigManager config;
        private string initialText;

        public TextEditorForm(string text, ConfigManager configManager = null)
        {
            this.config = configManager ?? new ConfigManager();
            this.initialText = text ?? "";
            System.Diagnostics.Debug.WriteLine($"TextEditorForm初期化: ConfigManager EditorWindowPosition = '{this.config.EditorWindowPosition}'");
            InitializeComponent();
        }

        private void SetFormIcon()
        {
            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                System.Diagnostics.Debug.WriteLine("エディターフォームのアイコンを設定しました");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"アイコン設定エラー: {ex.Message}");
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // フォームの基本設定
            this.Text = "確認画面";
            this.Size = new Size(config.EditorWidth, config.EditorHeight);
            this.StartPosition = FormStartPosition.Manual;

            Point editorPosition = config.GetEditorWindowPosition(config.EditorWidth, config.EditorHeight);
            System.Diagnostics.Debug.WriteLine($"確認画面の位置設定: ({editorPosition.X}, {editorPosition.Y})");
            this.Location = editorPosition;
            this.MinimumSize = new Size(400, 300);

            SetFormIcon();

            // メインレイアウト
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 3;
            mainLayout.ColumnCount = 1;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 85F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));

            // テキストエディタ
            textEditor = new TextBox();
            textEditor.Multiline = true;
            textEditor.WordWrap = true;
            textEditor.ScrollBars = ScrollBars.Vertical;
            textEditor.Dock = DockStyle.Fill;
            textEditor.Font = new Font(config.TextAreaFontName, config.TextAreaFontSize);
            textEditor.AcceptsReturn = true;
            textEditor.AcceptsTab = true;
            textEditor.TextChanged += (s, e) => UpdateStats();

            // ステータスパネル
            Panel statsPanel = new Panel();
            statsPanel.Dock = DockStyle.Fill;

            statsLabel = new Label();
            statsLabel.Text = "行数: 0  文字数: 0";
            statsLabel.Location = new Point(20, 5);
            statsLabel.AutoSize = true;

            statsPanel.Controls.Add(statsLabel);

            // ボタンパネル
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.WrapContents = true;
            buttonPanel.Padding = new Padding(10);

            closeButton = new Button();
            closeButton.Text = "閉じる";
            closeButton.Size = new Size(100, 30);
            closeButton.Margin = new Padding(5);
            closeButton.Click += CloseButton_Click;

            saveButton = new Button();
            saveButton.Text = "保存";
            saveButton.Size = new Size(100, 30);
            saveButton.Margin = new Padding(5);
            saveButton.Click += SaveButton_Click;

            pasteButton = new Button();
            pasteButton.Text = "貼り付け";
            pasteButton.Size = new Size(100, 30);
            pasteButton.Margin = new Padding(5);
            pasteButton.Click += PasteButton_Click;

            clearButton = new Button();
            clearButton.Text = "クリア";
            clearButton.Size = new Size(100, 30);
            clearButton.Margin = new Padding(5);
            clearButton.Click += ClearButton_Click;

            buttonPanel.Controls.Add(closeButton);
            buttonPanel.Controls.Add(saveButton);
            buttonPanel.Controls.Add(clearButton);
            buttonPanel.Controls.Add(pasteButton);

            mainLayout.Controls.Add(textEditor, 0, 0);
            mainLayout.Controls.Add(statsPanel, 0, 1);
            mainLayout.Controls.Add(buttonPanel, 0, 2);

            this.Controls.Add(mainLayout);
            this.ResumeLayout(false);

            // 初期テキストを設定
            textEditor.Text = initialText;
            UpdateStats();

            this.Load += (sender, e) => {
                System.Diagnostics.Debug.WriteLine($"確認画面実際の表示位置: ({this.Location.X}, {this.Location.Y})");
                System.Diagnostics.Debug.WriteLine($"確認画面サイズ: ({this.Size.Width}, {this.Size.Height})");
            };
        }

        private void UpdateStats()
        {
            string text = textEditor?.Text ?? "";
            int lines = string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;
            int chars = text.Length;

            if (string.IsNullOrWhiteSpace(text))
            {
                lines = 0;
                chars = 0;
            }

            if (statsLabel != null)
            {
                statsLabel.Text = $"行数: {lines}  文字数: {chars}";
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                string fileName = $"カルテ変換{DateTime.Now:yyyyMMddHHmmss}.txt";
                string filePath = Path.Combine(downloadsPath, fileName);

                File.WriteAllText(filePath, textEditor.Text, System.Text.Encoding.UTF8);

                MessageBox.Show($"テキストが保存されました。\nファイル: {fileName}", "保存完了",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);

                // ダウンロードフォルダを開く
                System.Diagnostics.Process.Start("explorer.exe", downloadsPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存中にエラーが発生しました: {ex.Message}", "エラー",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PasteButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string clipboardText = Clipboard.GetText();
                    if (!string.IsNullOrEmpty(clipboardText))
                    {
                        // カーソル位置にテキストを挿入
                        int selectionStart = textEditor.SelectionStart;
                        textEditor.Text = textEditor.Text.Insert(selectionStart, clipboardText);
                        textEditor.SelectionStart = selectionStart + clipboardText.Length;
                        textEditor.Focus();
                    }
                }
                else
                {
                    MessageBox.Show("クリップボードにテキストがありません。", "情報",
                                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"貼り付け中にエラーが発生しました: {ex.Message}", "エラー",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("テキストをクリアしますか？", "確認",
                                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    textEditor.Clear();
                    textEditor.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"クリア中にエラーが発生しました: {ex.Message}", "エラー",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}