using Newtonsoft.Json;
using System;
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
        private AnonymizationService anonymizationService;
        private TextBox textInput;
        private TextBox textOutput;
        private Label statsLabel;
        private Label monitorStatusLabel;
        private Button soapCopyButton;

        public MainForm()
        {
            config = new ConfigManager();
            textParser = new TextParser();
            anonymizationService = new AnonymizationService(config.AnonymizationSymbol, config.ReplacementListPath);
            InitializeComponent();
            InitializeCustomComponents();
            SetupClipboardMonitoring();
            UpdateStats();
            CleanupOldFiles();
            InitializeAnonymizationService();
        }

        private void InitializeAnonymizationService()
        {
            System.Diagnostics.Debug.WriteLine($"匿名化サービス初期化開始: パス='{config.ReplacementListPath}'");
            
            try
            {
                if (!anonymizationService.LoadReplacementList())
                {
                    System.Diagnostics.Debug.WriteLine("匿名化リスト読み込み失敗");
                    var result = MessageBox.Show($"匿名化リストファイルの読み込みに失敗しました。\n" +
                                   $"パス: {config.ReplacementListPath}\n\n" +
                                   "匿名化機能は無効になります。\n" +
                                   "詳細なデバッグ情報を表示しますか？", 
                                   "匿名化機能エラー", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    
                    if (result == DialogResult.Yes)
                    {
                        ShowAnonymizationDebugInfo();
                    }
                }
                else
                {
                    var stats = anonymizationService.GetStatistics();
                    System.Diagnostics.Debug.WriteLine($"匿名化サービス初期化完了: {stats.LoadedWordsCount}件の単語を読み込み");
                    
                    // 成功メッセージを表示
                    MessageBox.Show($"匿名化機能が正常に初期化されました。\n\n" +
                                   $"読み込み単語数: {stats.LoadedWordsCount}件\n" +
                                   $"匿名化記号: {config.AnonymizationSymbol}\n" +
                                   $"リストファイル: {config.ReplacementListPath}", 
                                   "匿名化機能初期化完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // テスト用：いくつかのサンプル単語で確認
                    if (stats.LoadedWordsCount > 0)
                    {
                        string testText = "横山先生がさくら病棟で診察";
                        string anonymizedTest = anonymizationService.AnonymizeJsonString(testText);
                        System.Diagnostics.Debug.WriteLine($"テスト置換: '{testText}' → '{anonymizedTest}'");
                        
                        if (testText != anonymizedTest)
                        {
                            MessageBox.Show($"匿名化テスト成功！\n\n" +
                                           $"元テキスト: {testText}\n" +
                                           $"匿名化後: {anonymizedTest}", 
                                           "匿名化テスト結果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show($"匿名化テスト失敗！\n\n" +
                                           $"テストテキストが変更されませんでした。\n" +
                                           $"テキスト: {testText}", 
                                           "匿名化テスト結果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"匿名化サービスの初期化中にエラーが発生しました。\n\n" +
                               $"エラー: {ex.Message}\n" +
                               $"スタックトレース: {ex.StackTrace}", 
                               "匿名化機能エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAnonymizationDebugInfo()
        {
            try
            {
                var currentDir = Directory.GetCurrentDirectory();
                var appDir = AppDomain.CurrentDomain.BaseDirectory;
                var executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var executableDir = Path.GetDirectoryName(executablePath);
                
                var debugInfo = $"匿名化機能デバッグ情報:\n\n" +
                               $"設定ファイルのパス: {config.ReplacementListPath}\n" +
                               $"現在のディレクトリ: {currentDir}\n" +
                               $"アプリケーションディレクトリ: {appDir}\n" +
                               $"実行ファイルパス: {executablePath}\n" +
                               $"実行ファイルディレクトリ: {executableDir}\n\n" +
                               $"候補ファイルパス:\n" +
                               $"1. {Path.Combine(executableDir, config.ReplacementListPath)} (存在: {File.Exists(Path.Combine(executableDir, config.ReplacementListPath))})\n" +
                               $"2. {Path.Combine(currentDir, config.ReplacementListPath)} (存在: {File.Exists(Path.Combine(currentDir, config.ReplacementListPath))})\n" +
                               $"3. {Path.Combine(appDir, config.ReplacementListPath)} (存在: {File.Exists(Path.Combine(appDir, config.ReplacementListPath))})\n";
                
                MessageBox.Show(debugInfo, "匿名化機能デバッグ情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"デバッグ情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeCustomComponents()
        {
            // フォームの基本設定
            this.Text = $"MediRecordConverter";
            this.Size = new Size(config.WindowWidth, config.WindowHeight);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = config.GetMainWindowPosition(config.WindowWidth, config.WindowHeight);
            this.MinimumSize = new Size(500, 600);

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
            statsLabel.Location = new Point(20, 10);
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

            // ボタン作成
            Button newButton = CreateButton("新規登録", StartMonitoring);
            Button soapButton = CreateButton("詳細検索設定", RunMouseAutomation);
            soapCopyButton = CreateButton("カルテコピー", SoapCopy);
            Button convertButton = CreateButton("JSON形式変換", ConvertToJson);
            Button editorButton = CreateButton("確認画面", OpenTextEditor);
            Button clearButton = CreateButton("クリア", ClearText);
            Button originalCopyButton = CreateButton("変換前コピー", CopyOriginalText);
            Button closeButton = CreateButton("閉じる", (s, e) => this.Close());

            buttonPanel.Controls.AddRange(new Control[] {
                newButton, soapButton, soapCopyButton, convertButton,
                editorButton, clearButton, originalCopyButton, closeButton
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
            button.Size = new Size(config.ButtonWidth, config.ButtonHeight);
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
                    ShowAutoCloseMessage("次ページに進みます");
                }
                else
                {
                    MessageBox.Show($"ファイルが見つかりません: {config.SoapCopyFilePath}", "エラー",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"カルテコピー中にエラーが発生しました: {ex.Message}", "エラー",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.WindowState = FormWindowState.Normal;

                // フォームが復元されてからマウスカーソルを戻す
                this.BeginInvoke((Action)(() =>
                {
                    System.Threading.Thread.Sleep(100); // 少し待機

                    if (soapCopyButton != null && soapCopyButton.Visible)
                    {
                        try
                        {
                            // ボタンの画面座標を正しく取得
                            var screenPoint = soapCopyButton.PointToScreen(new Point(
                                soapCopyButton.Width / 2,
                                soapCopyButton.Height / 2
                            ));

                            Cursor.Position = screenPoint;
                            System.Diagnostics.Debug.WriteLine($"マウスカーソル移動: ({screenPoint.X}, {screenPoint.Y})");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"マウスカーソル移動エラー: {ex.Message}");
                        }
                    }
                }));
            }
        }

        private void ShowAutoCloseMessage(string message)
        {
            Form popup = new Form();
            popup.Text = "処理完了";
            popup.Size = new Size(350, 120);
            popup.StartPosition = FormStartPosition.CenterScreen;
            popup.FormBorderStyle = FormBorderStyle.FixedDialog;
            popup.MaximizeBox = false;
            popup.MinimizeBox = false;
            popup.TopMost = true;

            Label label = new Label();
            label.Text = message;
            label.AutoSize = false;
            label.Size = new Size(320, 60);
            label.Location = new Point(15, 20);
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Font = new Font("MS Gothic", 11);

            popup.Controls.Add(label);

            System.Windows.Forms.Timer autoCloseTimer = new System.Windows.Forms.Timer();
            autoCloseTimer.Interval = 1500;
            autoCloseTimer.Tick += (s, e) =>
            {
                autoCloseTimer.Stop();
                autoCloseTimer.Dispose();
                popup.Close();
                popup.Dispose();
            };

            autoCloseTimer.Start();
            popup.ShowDialog(this);
        }

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

                // カルテテキストを解析
                var parsedData = textParser.ParseMedicalText(text);

                // JSON形式に変換
                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };

                string jsonData = JsonConvert.SerializeObject(parsedData, jsonSettings);

                // 匿名化処理（必須）
                string anonymizedJsonData = jsonData;
                var anonymizationStats = new AnonymizationStatistics();
                
                System.Diagnostics.Debug.WriteLine("=== 匿名化処理開始 ===");
                System.Diagnostics.Debug.WriteLine($"元JSON（最初の200文字）: {jsonData.Substring(0, Math.Min(200, jsonData.Length))}...");
                System.Diagnostics.Debug.WriteLine($"匿名化サービス状態: IsLoaded={anonymizationService.IsLoaded()}");
                
                if (anonymizationService.IsLoaded())
                {
                    anonymizationService.ResetStatistics();
                    anonymizedJsonData = anonymizationService.AnonymizeJsonString(jsonData);
                    anonymizationStats = anonymizationService.GetStatistics();
                    
                    System.Diagnostics.Debug.WriteLine($"匿名化処理完了: {anonymizationStats.TotalReplacements}件の置換を実行");
                    System.Diagnostics.Debug.WriteLine($"匿名化後JSON（最初の200文字）: {anonymizedJsonData.Substring(0, Math.Min(200, anonymizedJsonData.Length))}...");
                    
                    // 変更があったかチェック
                    if (jsonData != anonymizedJsonData)
                    {
                        System.Diagnostics.Debug.WriteLine("JSONに変更が確認されました");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("JSONに変更がありませんでした");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("匿名化サービスが利用できません - 匿名化なしで処理継続");
                }

                textOutput.Text = anonymizedJsonData;
                Clipboard.SetText(anonymizedJsonData);

                // 統計情報を含むメッセージ表示
                string message = anonymizationStats.TotalReplacements > 0 
                    ? $"変換完了: {anonymizationStats.TotalReplacements}件を匿名化してコピーしました"
                    : "変換したテキストをコピーしました";
                    
                ShowAutoCloseMessage(message);
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

            TextEditorForm editor = new TextEditorForm("", config);
            editor.FormClosed += (s, args) =>
            {
                this.Show();
                isMonitoringClipboard = false;
            };
            editor.Show();
        }

        private void CopyOriginalText(object sender, EventArgs e)
        {
            try
            {
                string text = textInput?.Text ?? "";

                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show("コピーするカルテ記載がありません。", "情報",
                                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Clipboard.SetText(text);
                ShowAutoCloseMessage("変換前のカルテ記載をコピーしました");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"コピー中にエラーが発生しました: {ex.Message}", "エラー",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CleanupOldFiles()
        {
            try
            {
                string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
                if (!Directory.Exists(downloadsPath)) return;

                string[] karteFiles = Directory.GetFiles(downloadsPath, "カルテ変換*.txt");
                DateTime cutoffTime = DateTime.Now.AddMinutes(-config.FileCleanupIntervalMinutes);

                foreach (string filePath in karteFiles)
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (fileInfo.CreationTime < cutoffTime)
                    {
                        try
                        {
                            File.Delete(filePath);
                            System.Diagnostics.Debug.WriteLine($"古いファイルを削除: {Path.GetFileName(filePath)}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"ファイル削除エラー: {Path.GetFileName(filePath)} - {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ファイルクリーンアップエラー: {ex.Message}");
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            clipboardTimer?.Stop();
            clipboardTimer?.Dispose();
            base.OnFormClosed(e);
        }
    }
}