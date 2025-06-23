using System;
using System.Drawing;
using System.Windows.Forms;

namespace MediRecordConverter
{
    public class TextEditorForm : Form
    {
        private TextBox textEditor;
        private Button closeButton;
        private Button saveButton;
        private string initialText;

        public TextEditorForm(string text)
        {
            this.initialText = text ?? "";
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // フォームの基本設定
            this.Text = "テキスト確認画面";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(400, 300);

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
            textEditor.Font = new Font("MS Gothic", 12);
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

            buttonPanel.Controls.Add(closeButton);
            buttonPanel.Controls.Add(saveButton);

            // レイアウトに追加
            mainLayout.Controls.Add(textEditor, 0, 0);
            mainLayout.Controls.Add(buttonPanel, 0, 1);

            this.Controls.Add(mainLayout);
            this.ResumeLayout(false);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            // 簡単な保存処理（実際の要件に応じて実装）
            try
            {
                MessageBox.Show("テキストが保存されました。", "保存完了",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存中にエラーが発生しました: {ex.Message}", "エラー",
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