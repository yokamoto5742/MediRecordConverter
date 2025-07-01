# MediRecordConverter

カルテテキストをSOAP形式に分類し、JSON形式へ変換するWindowsアプリケーションです。

## 概要

MediRecordConverterは、医療従事者が作成したカルテ記載を自動的に解析し、SOAP（Subjective, Objective, Assessment, Plan）形式に分類してJSON形式で出力するツールです。クリップボード監視機能により、効率的なデータ入力と変換が可能です。

## 主な機能

### 📋 クリップボード監視
- リアルタイムでクリップボードの内容を監視
- 自動的にテキストを取得・追加

### 🔍 自動解析機能
- **日付解析**: `2025/01/01(火)` 形式の日付を自動認識
- **医師記録抽出**: 診療科名と時刻を含む記録を自動識別
- **SOAP分類**: 医療記録をSOAP形式に自動分類
  - **S (Subjective)**: 主観的情報
  - **O (Objective)**: 客観的情報
  - **A (Assessment)**: 評価・診断
  - **P (Plan)**: 治療計画

### 📄 JSON変換
- 解析されたデータを構造化されたJSON形式で出力
- タイムスタンプ、診療科、SOAP内容を含む完全な記録

### 🔧 外部ツール連携
- **詳細検索設定**: マウス操作自動化ツールと連携して電子カルテの詳細検索設定を実行
- **カルテコピー**: 電子カルテの画面からテキストをコピー

## システム要件

- **OS**: Windows 10 以降
- **.NET Framework**: 4.7.2 以降
- **メモリ**: 最低 512MB の空きメモリ
- **ディスク容量**: 50MB の空き容量

## インストール

### 開発環境でのビルド

1. **前提条件**
   ```
   - Visual Studio 2022
   - .NET Framework 4.7.2 Developer Pack
   ```

2. **リポジトリのクローン**
   ```bash
   git clone [リポジトリURL]
   cd MediRecordConverter
   ```

3. **依存関係の復元**
   ```bash
   nuget restore
   ```

4. **ビルド**
   ```bash
   msbuild MediRecordConverter.sln /p:Configuration=Release
   ```

### 実行ファイルの使用

1. `bin/Release/` フォルダから `MediRecordConverter.exe` を実行
2. 必要に応じて設定ファイル（`App.config`）をカスタマイズ

## 使用方法

### 基本的な操作手順

1. **アプリケーション起動**
   - `MediRecordConverter.exe` をダブルクリック

2. **クリップボード監視開始**
   - 「新規登録」ボタンをクリック
   - 監視状態が「ON」になることを確認

3. **データ入力**
   - 電子カルテから必要なテキストをコピー
   - アプリケーションが自動的にテキストを取得

4. **JSON変換**
   - 「JSON形式変換」ボタンをクリック
   - 変換されたJSONがクリップボードにコピーされます

5. **確認・編集**
   - 「確認画面」ボタンで詳細な編集が可能
   - 必要に応じてテキストを修正

### ボタン機能説明

| ボタン名 | 機能 |
|----------|------|
| 新規登録 | クリップボード監視を開始 |
| 詳細検索設定 | 外部マウス操作ツールを実行 |
| カルテコピー | 電子カルテコピーを実行 |
| JSON形式変換 | テキストをJSON形式に変換 |
| 確認画面 | テキストエディタを開く |
| クリア | 入力内容をクリア |
| 閉じる | アプリケーションを終了 |

## 設定

### App.config設定項目

```xml
<!-- 外観設定 -->
<add key="WindowWidth" value="500"/>           <!-- ウィンドウ幅 -->
<add key="WindowHeight" value="600"/>          <!-- ウィンドウ高さ -->
<add key="TextAreaFontSize" value="11"/>       <!-- フォントサイズ -->
<add key="TextAreaFontName" value="MS Gothic"/> <!-- フォント名 -->

<!-- ウィンドウ位置設定 -->
<add key="MainWindowPosition" value="right+10+180"/>    <!-- メイン画面位置 -->
<add key="EditorWindowPosition" value="right+10+180"/>  <!-- エディタ画面位置 -->

<!-- 外部ツールパス -->
<add key="OperationFilePath" value="C:\path\to\mouseoperation.exe"/>
<add key="SoapCopyFilePath" value="C:\path\to\soapcopy.exe"/>
```

### ウィンドウ位置指定形式

- `right+10+180`: 画面右端から10ピクセル、上から180ピクセル
- `+100+200`: 画面左上から横100ピクセル、縦200ピクセル

## SOAP分類ルール

### 認識パターン

| 分類 | 認識パターン | 説明 |
|------|-------------|------|
| S (Subjective) | `S >`, `S>`, `S ` | 患者の訴え、主観的症状 |
| O (Objective) | `O >`, `O>`, `O ` | 検査結果、客観的所見 |
| A (Assessment) | `A >`, `A>`, `A ` | 診断、評価 |
| P (Plan) | `P >`, `P>`, `P ` | 治療計画、処方 |
| F (Comment) | `F >`, `F>`, `F ` | コメント、備考 |
| ｻ (Summary) | `ｻ >`, `ｻ>`, `ｻ ` | 要約 |

### 自動分類キーワード

**Objective（客観的情報）**
- 検査関連: `血圧`, `体温`, `脈拍`, `血液検査`, `画像`
- 眼科所見: `結膜`, `角膜`, `前房`, `水晶体`, `眼圧`, `視力`

**Assessment（評価・診断）**
- 診断関連: `診断`, `評価`, `症`, `病`, `疾患`, `#`

**Plan（治療計画）**
- 治療関連: `治療`, `処方`, `薬`, `mg`, `錠`, `再診`

## 出力JSON形式

```json
{
  "timestamp": "2025-01-01T14:30:00Z",
  "department": "眼科",
  "subject": "患者の主観的訴え",
  "object": "客観的検査所見",
  "assessment": "診断・評価",
  "plan": "治療計画",
  "comment": "コメント",
  "summary": "要約"
}
```

## 開発者向け情報

### プロジェクト構造

```
MediRecordConverter/
├── ConfigManager.cs          # 設定管理
├── DateTimeParser.cs         # 日時解析
├── DoctorRecordExtractor.cs  # 医師記録抽出
├── MainForm.cs              # メインフォーム
├── MedicalRecord.cs         # データモデル
├── MedicalRecordProcessor.cs # 記録後処理
├── SoapClassifier.cs        # SOAP分類
├── TextEditorForm.cs        # テキストエディタ
├── TextParser.cs            # テキスト解析エンジン
└── Program.cs               # エントリーポイント
```

### 主要クラス

- **TextParser**: テキスト解析のメインエンジン
- **SOAPClassifier**: SOAP分類ロジック
- **DateTimeParser**: 日付・時刻解析
- **ConfigManager**: 設定ファイル管理
- **MedicalRecord**: 医療記録データモデル

### テスト

```bash
# テストプロジェクトの実行
dotnet test MediRecordConverter.Tests
```

## トラブルシューティング

### よくある問題

**Q: クリップボード監視が動作しない**
- アプリケーションが最小化されていないか確認
- ウイルス対策ソフトがクリップボードアクセスをブロックしていないか確認

**Q: 外部ツールが実行されない**
- `App.config`のファイルパスが正しいか確認
- 外部ツールファイルが存在し、実行権限があるか確認

**Q: 日本語が文字化けする**
- フォント設定を `MS Gothic` または `メイリオ` に変更

**Q: ウィンドウ位置がおかしい**
- `App.config`の位置設定を確認
- 複数ディスプレイ環境では座標値を調整

## ライセンス

このプロジェクトは [MIT License](LICENSE.txt) の下で公開されています。

## 重要な注意事項

⚠️ **医療情報の取り扱いについて**

- 本ソフトウェアは医療記録の変換補助ツールです
- 患者の個人情報を含むデータの取り扱いには十分注意してください
- 個人情報保護法等の関連法規を遵守してください
- 変換結果は必ず医療従事者が確認・検証してください

## サポート

- 問題報告: [Issues](https://github.com/[username]/MediRecordConverter/issues)
- 機能要求: [Feature Requests](https://github.com/[username]/MediRecordConverter/discussions)

## 更新履歴

### v1.0.0
- 初回リリース
- SOAP分類機能
- JSON変換機能
- クリップボード監視機能

---

**開発者**: yasuhiro okamoto  
**最終更新**: 2025年7月