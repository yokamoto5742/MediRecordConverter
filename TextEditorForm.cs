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
        private Button pasteButton;  // 【修正1】貼り付けボタンを追加
        private Button clearButton;  // 【修正2】クリアボタンを追加
        private string initialText;
        private ConfigManager config;

        public TextEditorForm(string text, ConfigManager configManager = null)
        {
            this.initialText = text ?? "";
            this.config = configManager ?? new ConfigManager();
            System.Diagnostics.Debug.WriteLine($"TextEditorForm初期化: ConfigManager EditorWindowPosition = '{this.config.EditorWindowPosition}'");
            InitializeComponent();
        }

        // 【追加】フォームのアイコンを設定するメソッド
        private void SetFormIcon()
        {
            try
            {
                // 実行ファイルのアイコンを使用
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                System.Diagnostics.Debug.WriteLine("エディターフォームのアイコンを設定しました");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"アイコン設定エラー: {ex.Message}");
                // アイコン設定に失敗した場合はデフォルトアイコンのまま
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // フォームの基本設定
            this.Text = "確認画面";
            this.Size = new Size(config.EditorWidth, config.EditorHeight);
            this.StartPosition = FormStartPosition.Manual;  // 【修正】ManualでConfigの位置を使用

            // 位置を取得してデバッグ情報を出力
            Point editorPosition = config.GetEditorWindowPosition(config.EditorWidth, config.EditorHeight);
            System.Diagnostics.Debug.WriteLine($"確認画面の位置設定: ({editorPosition.X}, {editorPosition.Y})");
            this.Location = editorPosition;  // 【修正】Configから位置を取得

            this.MinimumSize = new Size(400, 300);

            // 【追加】アイコンの設定
            SetFormIcon();

            // メインレイアウト
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 2;
            mainLayout.ColumnCount = 1;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 90F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));

            // テキストエディタ
            textEditor = new TextBox();
            textEditor.Multiline = true;
            textEditor.ScrollBars = ScrollBars.Both;
            textEditor.Dock = DockStyle.Fill;
            textEditor.Font = new Font(config.TextAreaFontName, config.TextAreaFontSize);
            textEditor.Text = initialText;

            // ボタンパネル
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Padding = new Padding(10);

            // 閉じるボタン
            closeButton = new Button();
            closeButton.Text = "閉じる";
            closeButton.Size = new Size(100, 30);
            closeButton.Margin = new Padding(5);
            closeButton.Click += CloseButton_Click;

            // 保存ボタン
            saveButton = new Button();
            saveButton.Text = "保存";
            saveButton.Size = new Size(100, 30);
            saveButton.Margin = new Padding(5);
            saveButton.Click += SaveButton_Click;

            // 【修正3】貼り付けボタンの作成
            pasteButton = new Button();
            pasteButton.Text = "貼り付け";
            pasteButton.Size = new Size(100, 30);
            pasteButton.Margin = new Padding(5);
            pasteButton.Click += PasteButton_Click;

            // 【修正4】クリアボタンの作成
            clearButton = new Button();
            clearButton.Text = "クリア";
            clearButton.Size = new Size(100, 30);
            clearButton.Margin = new Padding(5);
            clearButton.Click += ClearButton_Click;

            // 【修正5】ボタンパネルにボタンを追加（左から：貼り付け、クリア、保存、閉じる）
            buttonPanel.Controls.Add(closeButton);
            buttonPanel.Controls.Add(saveButton);
            buttonPanel.Controls.Add(clearButton);
            buttonPanel.Controls.Add(pasteButton);

            // レイアウトに追加
            mainLayout.Controls.Add(textEditor, 0, 0);
            mainLayout.Controls.Add(buttonPanel, 0, 1);

            this.Controls.Add(mainLayout);
            this.ResumeLayout(false);

            // ロードイベントで最終的な位置を確認
            this.Load += (sender, e) => {
                System.Diagnostics.Debug.WriteLine($"確認画面実際の表示位置: ({this.Location.X}, {this.Location.Y})");
                System.Diagnostics.Debug.WriteLine($"確認画面サイズ: ({this.Size.Width}, {this.Size.Height})");
            };
        }

        // 【修正6】保存ボタンの機能を拡張
        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // ダウンロードフォルダーのパスを取得
                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                // ファイル名を生成（日付時刻付き）
                string fileName = $"確認画面_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string filePath = Path.Combine(downloadsPath, fileName);

                // テキストファイルに保存
                File.WriteAllText(filePath, textEditor.Text, System.Text.Encoding.UTF8);

                MessageBox.Show($"テキストが保存されました。\nファイル: {fileName}", "保存完了",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);

                // ダウンロードフォルダーを開く
                System.Diagnostics.Process.Start("explorer.exe", downloadsPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存中にエラーが発生しました: {ex.Message}", "エラー",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 【修正7】貼り付けボタンのイベントハンドラーを追加
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

        // 【修正8】クリアボタンのイベントハンドラーを追加
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

        public string GetText()
        {
            return textEditor.Text;
        }

        public void SetText(string text)
        {
            textEditor.Text = text ?? "";
        }
    }
}