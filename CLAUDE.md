# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## House Rules:
- 文章ではなくパッチの差分を返す。
- 変更範囲は最小限に抑える。
- コードの修正は直接適用する。

## Automatic Notifications (Hooks)
自動通知は`.claude/settings.local.json` で設定済：

- **Stop Hook**: ユーザーがClaude Codeを停止した時に「作業が完了しました」と通知
- **SessionEnd Hook**: セッション終了時に「Claude Code セッションが終了しました」と通知

## Development Commands

### Build Commands
```bash
# Build solution (Debug)
msbuild MediRecordConverter.sln /p:Configuration=Debug /p:Platform="Any CPU"

# Build solution (Release)
msbuild MediRecordConverter.sln /p:Configuration=Release /p:Platform="Any CPU"

# Restore NuGet packages
nuget restore MediRecordConverter.sln
```

### Testing
```bash
# Run all tests using MSTest framework
vstest.console.exe MediRecordConverter.Tests\bin\Debug\MediRecordConverter.Tests.dll

# Test project uses MSTest with Newtonsoft.Json dependency
# Tests are organized in UnitTests/ and IntegrationTests/ folders
```

### Running the Application
```bash
# Run from build output
.\bin\Debug\MediRecordConverter.exe
# or
.\bin\Release\MediRecordConverter.exe
```

## Architecture Overview

This is a Windows Forms medical record converter application (.NET Framework 4.7.2) that transforms Japanese medical text into structured SOAP format JSON.

### Core Processing Pipeline
1. **TextParser** - Main processing engine that orchestrates the entire conversion flow
2. **DateTimeParser** - Extracts and normalizes Japanese date formats (`2025/01/01(火)`)
3. **DoctorRecordExtractor** - Identifies medical department records with timestamps
4. **SOAPClassifier** - Categorizes text into SOAP format (Subjective, Objective, Assessment, Plan)
5. **MedicalRecordProcessor** - Post-processes and consolidates records
6. **ConfigManager** - Handles App.config settings and window positioning

### Key Components

**UI Layer:**
- `MainForm.cs` - Primary interface with clipboard monitoring and conversion controls
- `TextEditorForm.cs` - Secondary editing interface with file operations

**Data Models:**
- `MedicalRecord.cs` - Core data structure with JSON serialization attributes

**Configuration:**
- App.config drives window positioning, external tool paths, and UI settings
- Supports complex positioning syntax like `right+10+180`

### Medical Text Processing Logic

The application processes Japanese medical records through these stages:

1. **Date Recognition** - Identifies date headers in format `YYYY/MM/DD(曜日)`
2. **Department Extraction** - Recognizes 18 medical departments (内科, 外科, 眼科, etc.)
3. **SOAP Classification** - Uses both explicit markers (`S >`, `O >`, etc.) and keyword-based classification:
   - Objective: バイタル signs, 検査 results, 眼科所見
   - Assessment: 診断 markers, 病態 terms, 慢性/急性 conditions
   - Plan: 治療/処方 terms, 再診 instructions

### External Tool Integration

The application launches external automation tools:
- `mouseoperation.exe` - Mouse automation for UI interaction
- `soapcopy.exe` - Automated clipboard operations from electronic medical records

These paths are configurable via App.config `OperationFilePath` and `SoapCopyFilePath` settings.

### Clipboard Integration

Real-time clipboard monitoring enables seamless workflow integration with electronic medical record systems. The application automatically aggregates copied text segments before processing.

## Development Notes

- Uses Newtonsoft.Json 13.0.3 for JSON serialization with null value exclusion
- Debug output via `System.Diagnostics.Debug.WriteLine` for processing visibility
- Platform target: x86 for main project, AnyCPU for test project
- Japanese text processing with UTF-8 support and configurable MS Gothic fonts