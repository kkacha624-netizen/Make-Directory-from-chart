# Directory Tree Builder for Windows

ディレクトリ構成図を貼り付けるだけで、同じ構造のフォルダーと空ファイルを作成できる Windows 向け GUI ツールです。
よくある「指定されたディレクトリ構成を作る」「実験・納品用フォルダーを定型化する」といった作業を、手作業ではなく小さなツールとして自動化するために作成しました。

## 特徴

- Explorer と同じ感覚で、作成開始位置をフォルダー選択ダイアログから指定できます。
- ツリー形式のディレクトリ構成図をそのまま貼り付けて使えます。
- `/` または `\` で終わる行はフォルダーとして作成します。
- それ以外の行は空ファイルとして作成します。
- Windows 標準の PowerShell と .NET Windows Forms のみで動作します。
- 追加ライブラリのインストールは不要です。

## 画面イメージ

1. 構成図を入力欄に貼り付けます。
2. `作成先を選択...` から開始フォルダーを選びます。
3. `作成` を押すと、指定した場所にフォルダーとファイルが作成されます。

## 入力例

```text
expXXX_experiment_name/
├─ config.yaml
├─ result.md
├─ metrics.csv
├─ logs/
└─ figures/
```

作成結果:

```text
選択した作成先/
└─ expXXX_experiment_name/
   ├─ config.yaml
   ├─ result.md
   ├─ metrics.csv
   ├─ logs/
   └─ figures/
```

## 使い方

1. このリポジトリをダウンロードまたは clone します。
2. `run_directory_builder.bat` をダブルクリックします。
3. ディレクトリ構成図を入力欄に貼り付けます。
4. `作成先を選択...` で作成開始位置を選びます。
5. `作成` をクリックします。

PowerShell から直接実行する場合:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\create_tree_from_diagram.ps1
```

## 対応している記法

このような一般的なツリー表記に対応しています。

```text
project/
├─ src/
│  ├─ main.py
│  └─ utils.py
├─ docs/
└─ README.md
```

`├──` や `└──` のように罫線が複数ある表記にも対応しています。

## ファイル構成

```text
.
├─ create_tree_from_diagram.ps1
├─ run_directory_builder.bat
├─ examples/
│  └─ experiment_tree.txt
├─ README.md
├─ LICENSE
└─ .gitignore
```

## 制作意図

手作業でディレクトリを作るだけなら簡単ですが、何度も同じような構成を作る場合や、構成図を正確に再現する場合はミスが起きやすくなります。

このツールでは、テキストとして渡された構成図を解析し、GUI で選んだ場所にそのまま展開することで、単純作業の自動化と再現性の向上を狙っています。

## 動作環境

- Windows
- Windows PowerShell
- .NET Windows Forms

## ライセンス

MIT License
