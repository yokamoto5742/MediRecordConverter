using MediRecordConverter;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace MediRecordConverter
{
    public partial class MainForm : Form
    {
        private bool isMonitoringClipboard = false;
        private string clipboardContent = "";
        private bool isFirstCheck = true;
        private System.Windows.Forms.Timer clipboardTimer;
        private ConfigManager config;
        private TextParser textParser;
        private TextBox textInput;
        private TextBox textOutput;
        private Label statsLabel;
        private Label monitorStatusLabel;

        public MainForm()
        {
            config = new ConfigManager();
            textParser = new TextParser();
            InitializeComponent();
            InitializeCustomComponents();
            SetupClipboardMonitoring();
            UpdateStats();
        }

        private void InitializeCustomComponents()
        {
            // フォームの基本設定
            this.Text = $"MediRecordConverter";
            this.Size = new Size(config.WindowWidth, config.WindowHeight);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(10, 10);
            this.MinimumSize = new Size(500, 400);

            // フォントの設定
            Font textFont = new Font(config.TextAreaFontName, config.TextAreaFontSize);

            // レイアウトパネル
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 4;
            mainLayout.ColumnCount = 1;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));

            // カルテ記載グループボックス
            GroupBox karteGroup = new GroupBox();
            karteGroup.Text = "カルテ記載";
            karteGroup.Dock = DockStyle.Fill;
            karteGroup.Padding = new Padding(5);

            textInput = new TextBox();
            textInput.Multiline = true;
            textInput.ScrollBars = ScrollBars.Vertical;
            textInput.Dock = DockStyle.Fill;
            textInput.Font = textFont;
            textInput.TextChanged += (s, e) => UpdateStats();
            karteGroup.Controls.Add(textInput);

            // JSON形式グループボックス
            GroupBox jsonGroup = new GroupBox();
            jsonGroup.Text = "JSON形式";
            jsonGroup.Dock = DockStyle.Fill;
            jsonGroup.Padding = new Padding(5);

            textOutput = new TextBox();
            textOutput.Multiline = true;
            textOutput.ScrollBars = ScrollBars.Vertical;
            textOutput.Dock = DockStyle.Fill;
            textOutput.Font = textFont;
            textOutput.ReadOnly = true;
            jsonGroup.Controls.Add(textOutput);

            // ステータスパネル
            Panel statsPanel = new Panel();
            statsPanel.Dock = DockStyle.Fill;

            statsLabel = new Label();
            statsLabel.Text = "カルテ記載行数: 0  文字数: 0";
            statsLabel.Location = new Point(5, 10);
            statsLabel.AutoSize = true;

            monitorStatusLabel = new Label();
            monitorStatusLabel.Text = "クリップボード監視: OFF";
            monitorStatusLabel.ForeColor = Color.Red;
            monitorStatusLabel.Location = new Point(300, 10);
            monitorStatusLabel.AutoSize = true;

            statsPanel.Controls.Add(statsLabel);
            statsPanel.Controls.Add(monitorStatusLabel);

            // ボタンパネル
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.LeftToRight;
            buttonPanel.WrapContents = true;
            buttonPanel.Padding = new Padding(10);

            // ボタン作成（統計表示ボタンを削除）
            Button newButton = CreateButton("新規登録", StartMonitoring);
            Button soapButton = CreateButton("詳細検索設定", RunMouseAutomation);
            Button soapCopyButton = CreateButton("カルテコピー", SoapCopy);
            Button convertButton = CreateButton("JSON形式変換", ConvertToJson);
            Button clearButton = CreateButton("テキストクリア", ClearText);
            Button editorButton = CreateButton("確認画面", OpenTextEditor);
            Button closeButton = CreateButton("閉じる", (s, e) => this.Close());

            buttonPanel.Controls.AddRange(new Control[] {
                newButton, soapButton, soapCopyButton, convertButton,
                clearButton, editorButton, closeButton
            });

            // メインレイアウトに追加
            mainLayout.Controls.Add(karteGroup, 0, 0);
            mainLayout.Controls.Add(jsonGroup, 0, 1);
            mainLayout.Controls.Add(statsPanel, 0, 2);
            mainLayout.Controls.Add(buttonPanel, 0, 3);

            this.Controls.Add(mainLayout);
        }

        private Button CreateButton(string text, EventHandler clickHandler)
        {
            Button button = new Button();
            button.Text = text;
            button.Size = new Size(100, 30);
            button.Margin = new Padding(5);
            button.Click += clickHandler;
            return button;
        }

        private void SetupClipboardMonitoring()
        {
            clipboardTimer = new System.Windows.Forms.Timer();
            clipboardTimer.Interval = 500;
            clipboardTimer.Tick += CheckClipboard;
            clipboardTimer.Start();
        }

        private void CheckClipboard(object sender, EventArgs e)
        {
            if (!isMonitoringClipboard) return;

            try
            {
                if (Clipboard.ContainsText())
                {
                    string clipboardText = Clipboard.GetText();
                    if (clipboardText != clipboardContent)
                    {
                        clipboardContent = clipboardText;
                        if (!isFirstCheck && !string.IsNullOrEmpty(clipboardText))
                        {
                            if (!string.IsNullOrEmpty(textInput.Text))
                            {
                                textInput.Text += Environment.NewLine + clipboardText;
                            }
                            else
                            {
                                textInput.Text = clipboardText;
                            }

                            ShowNotification("コピーしました");
                        }
                        isFirstCheck = false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"クリップボード監視エラー: {ex.Message}");
            }
        }

        private void ShowNotification(string message)
        {
            string originalTitle = this.Text;
            this.Text = $"{originalTitle} - {message}";

            System.Windows.Forms.Timer notificationTimer = new System.Windows.Forms.Timer();
            notificationTimer.Interval = 2000;
            notificationTimer.Tick += (s, e) =>
            {
                this.Text = originalTitle;
                notificationTimer.Stop();
                notificationTimer.Dispose();
            };
            notificationTimer.Start();
        }

        private void UpdateStats()
        {
            string text = textInput?.Text ?? "";
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

        private void SetMonitoringState(bool enabled)
        {
            isMonitoringClipboard = enabled;
            if (monitorStatusLabel != null)
            {
                if (enabled)
                {
                    monitorStatusLabel.Text = "クリップボード監視: ON";
                    monitorStatusLabel.ForeColor = Color.Green;
                }
                else
                {
                    monitorStatusLabel.Text = "クリップボード監視: OFF";
                    monitorStatusLabel.ForeColor = Color.Red;
                }
            }
        }

        private void StartMonitoring(object sender, EventArgs e)
        {
            SetMonitoringState(true);
            ClearText(sender, e);
            Clipboard.Clear();
            clipboardContent = "";
            isFirstCheck = false;
        }

        private void RunMouseAutomation(object sender, EventArgs e)
        {
            try
            {
                this.WindowState = FormWindowState.Minimized;

                if (File.Exists(config.OperationFilePath))
                {
                    Thread.Sleep(500); // 0.5秒待機
                    Process.Start(config.OperationFilePath);
                    ShowNotification("設定完了");
                }
                else
                {
                    MessageBox.Show($"ファイルが見つかりません: {config.OperationFilePath}", "エラー",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"マウス操作中にエラーが発生しました: {ex.Message}", "エラー",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void SoapCopy(object sender, EventArgs e)
        {
            try
            {
                this.WindowState = FormWindowState.Minimized;

                if (File.Exists(config.SoapCopyFilePath))
                {
                    Process.Start(config.SoapCopyFilePath);
                }
                else
                {
                    MessageBox.Show($"ファイルが見つかりません: {config.SoapCopyFilePath}", "エラー",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SOAPコピー中にエラーが発生しました: {ex.Message}", "エラー",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.WindowState = FormWindowState.Normal;
            }
        }

        // 簡素化されたJSON変換メソッド
        // MainForm.csのConvertToJsonメソッドの修正版
        // この部分のみを既存のMainForm.csに置き換えてください

        // 簡素化されたJSON変換メソッド（修正版）
        private void ConvertToJson(object sender, EventArgs e)
        {
            try
            {
                string text = textInput.Text;
                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show("変換するテキストがありません。", "警告",
                                   MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SetMonitoringState(false);

                // 医療記録データを解析（統計機能削除、ソート機能追加）
                var parsedData = textParser.ParseMedicalText(text);

                // JSON形式に変換（空のフィールドを除外する設定）
                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };

                string jsonData = JsonConvert.SerializeObject(parsedData, jsonSettings);

                textOutput.Text = jsonData;
                Clipboard.SetText(jsonData);

                MessageBox.Show($"JSON形式に変換してクリップボードにコピーしました。",
                               "変換完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"変換中にエラーが発生しました: {ex.Message}",
                               "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearText(object sender, EventArgs e)
        {
            textInput?.Clear();
            textOutput?.Clear();
            UpdateStats();
        }

        private void OpenTextEditor(object sender, EventArgs e)
        {
            SetMonitoringState(false);
            this.Hide();

            TextEditorForm editor = new TextEditorForm("");
            editor.FormClosed += (s, args) =>
            {
                this.Show();
                isMonitoringClipboard = false;
            };
            editor.Show();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            clipboardTimer?.Stop();
            clipboardTimer?.Dispose();
            base.OnFormClosed(e);
        }
    }
}