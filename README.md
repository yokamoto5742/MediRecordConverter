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
  - **F (Comment)**: コメント・備考
  - **ｻ (Summary)**: 要約

### 📄 JSON変換
- 解析されたデータを構造化されたJSON形式で出力
- タイムスタンプ、診療科、SOAP内容を含む完全な記録
- Newtonsoft.Jsonライブラリを使用した高精度変換

### 🔧 外部ツール連携
- **詳細検索設定**: マウス操作自動化ツールと連携
- **カルテコピー**: 電子カルテからのテキストコピー自動化
- カスタマイズ可能な外部ツールパス設定

### 📝 テキスト編集機能
- 専用テキストエディタでの詳細編集
- リアルタイム文字数・行数カウント
- ファイル保存機能（Downloadsフォルダ）

## システム要件

- **OS**: Windows 10 以降
- **.NET Framework**: 4.7.2 以降
- **メモリ**: 最低 512MB の空きメモリ
- **ディスク容量**: 50MB の空き容量
- **依存関係**: Newtonsoft.Json 13.0.3

## インストール

### 開発環境でのビルド

1. **前提条件**
   ```
   - Visual Studio 2022 (推奨) または Visual Studio 2019
   - .NET Framework 4.7.2 Developer Pack
   - NuGet Package Manager
   ```

2. **リポジトリのクローン**
   ```bash
   git clone [リポジトリURL]
   cd MediRecordConverter
   ```

3. **依存関係の復元**
   ```bash
   nuget restore MediRecordConverter.sln
   ```

4. **ビルド**
   ```bash
   msbuild MediRecordConverter.sln /p:Configuration=Release /p:Platform="Any CPU"
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
   - 監視状態が「ON」（緑色）になることを確認

3. **データ入力**
   - 電子カルテから必要なテキストをコピー
   - アプリケーションが自動的にテキストを取得・統合

4. **JSON変換**
   - 「JSON形式変換」ボタンをクリック
   - 変換されたJSONがクリップボードにコピーされます
   - 変換完了通知が表示されます

5. **確認・編集**
   - 「確認画面」ボタンで詳細な編集が可能
   - 必要に応じてテキストを修正・保存

### ボタン機能説明

| ボタン名 | 機能 | 詳細 |
|----------|------|------|
| 新規登録 | クリップボード監視を開始 | テキストエリアをクリアし、監視を有効化 |
| 詳細検索設定 | 外部マウス操作ツールを実行 | 画面最小化後、mouseoperation.exeを実行 |
| カルテコピー | 電子カルテコピーツールを実行 | soapcopy.exeを実行し、自動でカーソル位置復元 |
| JSON形式変換 | テキストをJSON形式に変換 | SOAP分類後、JSON形式でクリップボードにコピー |
| 確認画面 | テキストエディタを開く | 別ウィンドウで詳細編集・保存が可能 |
| クリア | 入力内容をクリア | カルテ記載とJSON出力の両方をクリア |
| 変換前コピー | 変換前のテキストをコピー | JSON変換前の原文をクリップボードにコピー |
| 閉じる | アプリケーションを終了 | クリップボード監視を停止して終了 |

## 設定

### App.config設定項目

```xml
<appSettings>
  <!-- 外観設定 -->
  <add key="WindowWidth" value="500"/>           <!-- メインウィンドウ幅 -->
  <add key="WindowHeight" value="600"/>          <!-- メインウィンドウ高さ -->
  <add key="EditorWidth" value="500"/>           <!-- エディタウィンドウ幅 -->
  <add key="EditorHeight" value="600"/>          <!-- エディタウィンドウ高さ -->
  <add key="TextAreaFontSize" value="11"/>       <!-- フォントサイズ -->
  <add key="TextAreaFontName" value="MS Gothic"/> <!-- フォント名 -->
  
  <!-- ウィンドウ位置設定 -->
  <add key="MainWindowPosition" value="right+10+180"/>    <!-- メイン画面位置 -->
  <add key="EditorWindowPosition" value="right+10+180"/>  <!-- エディタ画面位置 -->
  
  <!-- ボタンサイズ設定 -->
  <add key="ButtonWidth" value="100"/>           <!-- ボタン幅 -->
  <add key="ButtonHeight" value="30"/>           <!-- ボタン高さ -->
  
  <!-- 外部ツールパス -->
  <add key="OperationFilePath" value="C:\Shinseikai\MediRecordConverter\mouseoperation.exe"/>
  <add key="SoapCopyFilePath" value="C:\Shinseikai\MediRecordConverter\soapcopy.exe"/>
</appSettings>
```

### ウィンドウ位置指定形式

- `right+10+180`: 画面右端から10ピクセル、上から180ピクセル
- `+100+200`: 画面左上から横100ピクセル、縦200ピクセル
- 複数ディスプレイ環境では適切に調整されます

## SOAP分類ルール

### 明示的なSOAPパターン

| 分類 | 認識パターン | 説明 |
|------|-------------|------|
| S (Subjective) | `S >`, `S>`, `S `, `S　` | 患者の訴え、主観的症状 |
| O (Objective) | `O >`, `O>`, `O `, `O　` | 検査結果、客観的所見 |
| A (Assessment) | `A >`, `A>`, `A `, `A　` | 診断、評価 |
| P (Plan) | `P >`, `P>`, `P `, `P　` | 治療計画、処方 |
| F (Comment) | `F >`, `F>`, `F `, `F　` | コメント、備考 |
| ｻ (Summary) | `ｻ >`, `ｻ>`, `ｻ `, `ｻ　` | 要約 |

### 自動分類キーワード

**Objective（客観的情報）**
```
眼科所見: 結膜, 角膜, 前房, 水晶体, 乳頭, 網膜, 眼圧, 視力
バイタル: 血圧, 体温, 脈拍, 呼吸
検査: 血液検査, 検査結果, 画像, 所見
英語表記: slit, cor, ac, lens, disc, fds, AVG, mmHg
```

**Assessment（評価・診断）**
```
診断マーカー: ＃, #, 診断, 評価
病態: 慢性, 症, 病, 疾患, 状態, 不全
眼科疾患: 出血, 結膜下出血, 白内障, 緑内障, 進行, 影響
```

**Plan（治療計画）**
```
治療: 治療, 処方, 継続, 指導, 制限, 予定, 検討, 再開
薬物: 維持, 採血, 注射, 薬, mg, 錠, 単位
フォロー: 再診, medi, 終了, 指示, 週間後, 視力, 眼圧
```

## 診療科認識

### 対応診療科
```
内科, 外科, 透析, 整形外科, 皮膚科, 眼科, 耳鼻科, 泌尿器科, 
婦人科, 小児科, 精神科, 放射線科, 麻酔科, 病理科, 
リハビリ科, 薬剤科, 検査科, 栄養科
```

### 医師記録形式
```
[診療科名] [内容] [時刻]
例: 眼科 山田医師 14:30
```

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

### JSON出力特徴
- **null値除外**: 空のフィールドは出力されません
- **タイムスタンプ**: ISO 8601形式（UTC）
- **インデント**: 読みやすい形式で整形
- **エンコーディング**: UTF-8対応

## 開発者向け情報

### プロジェクト構造

```
MediRecordConverter/
├── ConfigManager.cs          # 設定管理（App.config読み込み）
├── DateTimeParser.cs         # 日時解析（日付・時刻抽出）
├── DoctorRecordExtractor.cs  # 医師記録抽出（診療科・時刻）
├── MainForm.cs              # メインフォーム（UI制御）
├── MainForm.Designer.cs     # フォームデザイナーコード
├── MedicalRecord.cs         # データモデル（JSON属性含む）
├── MedicalRecordProcessor.cs # 記録後処理（統合・ソート）
├── SoapClassifier.cs        # SOAP分類ロジック
├── TextEditorForm.cs        # テキストエディタフォーム
├── TextParser.cs            # テキスト解析エンジン（メイン処理）
├── Program.cs               # エントリーポイント
└── App.config               # アプリケーション設定
```

### 主要クラスの責務

**TextParser**
- メインの解析エンジン
- 各パーサーを統合して全体処理を制御
- 医療記録の完全な解析フロー

**SOAPClassifier**
- SOAP分類の中核ロジック
- 明示的パターンとキーワードベース分類
- 継続行の適切な処理

**DateTimeParser**
- 日付形式の認識と変換
- ISO 8601形式への統一
- 医師記録行の除外処理

**ConfigManager**
- App.config設定の一元管理
- ウィンドウ位置計算
- デフォルト値の提供

**MedicalRecord**
- データモデルの定義
- JSON属性とシリアライゼーション制御
- null値の適切な処理

### 依存関係

```xml
<packages>
  <package id="Newtonsoft.Json" version="13.0.3" targetFramework="net472" />
</packages>
```

### テスト

```bash
# テストプロジェクトの実行（利用可能な場合）
dotnet test MediRecordConverter.Tests
```

## トラブルシューティング

### よくある問題と解決方法

**Q: クリップボード監視が動作しない**
- **確認事項**: アプリケーションが最小化されていないか
- **対処法**: ウイルス対策ソフトのクリップボードアクセス制限を確認
- **代替手段**: 手動でテキストを貼り付けて使用

**Q: 外部ツールが実行されない**
- **確認事項**: `App.config`のファイルパスが正しいか
- **対処法**: 
  ```xml
  <add key="OperationFilePath" value="C:\正しいパス\mouseoperation.exe"/>
  <add key="SoapCopyFilePath" value="C:\正しいパス\soapcopy.exe"/>
  ```
- **権限確認**: 外部ツールファイルの実行権限を確認

**Q: 日本語が文字化けする**
- **対処法**: フォント設定を変更
  ```xml
  <add key="TextAreaFontName" value="メイリオ"/>
  ```
- **推奨フォント**: MS Gothic, メイリオ, Yu Gothic UI

**Q: ウィンドウ位置がおかしい**
- **確認事項**: 複数ディスプレイ環境での座標値
- **対処法**: 
  ```xml
  <!-- 単一ディスプレイ用 -->
  <add key="MainWindowPosition" value="+100+100"/>
  <!-- 右端配置用 -->
  <add key="MainWindowPosition" value="right+10+10"/>
  ```

**Q: JSON変換が正しく動作しない**
- **確認事項**: 入力テキストに日付と診療科情報が含まれているか
- **デバッグ**: Debug.WriteLineでコンソール出力を確認
- **対処法**: 手動でSOAPパターン（S >, O >, A >, P >）を明記

### ログとデバッグ

アプリケーションは `System.Diagnostics.Debug.WriteLine` を使用してデバッグ情報を出力します。Visual Studioの出力ウィンドウで確認できます。

```csharp
System.Diagnostics.Debug.WriteLine($"ParseMedicalText: {lines.Length}行のテキストを処理開始");
```

## ライセンス

このプロジェクトのライセンス情報については、LICENSEファイルを参照してください。

## 重要な注意事項

⚠️ **医療情報の取り扱いについて**

- 本ソフトウェアは医療記録の変換補助ツールです
- 患者の個人情報を含むデータの取り扱いには十分注意してください
- 個人情報保護法、医療法等の関連法規を遵守してください
- **変換結果は必ず医療従事者が確認・検証してください**
- 診断や治療の判断には使用しないでください

⚠️ **システムセキュリティ**

- クリップボード監視機能は機密情報を扱う可能性があります
- セキュアな環境での使用を推奨します
- 外部ツール連携時はマルウェア対策を確認してください

## サポート

- **問題報告**: [Issues](https://github.com/[username]/MediRecordConverter/issues)
- **機能要求**: [Feature Requests](https://github.com/[username]/MediRecordConverter/discussions)
- **技術的質問**: READMEとコードコメントを参照
