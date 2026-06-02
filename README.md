# Directory Tree Builder for Windows

ディレクトリ構成図を貼り付けるだけで、同じ構造のフォルダーと空ファイルを作成できる Windows 向け GUI ツールです。

よくある「指定されたディレクトリ構成を作る」「実験・納品用フォルダーを定型化する」といった作業を、手作業ではなく小さなツールとして自動化するために作成しました。

## 特徴

- `DirectoryTreeBuilder.exe` をダブルクリックするだけで起動できます。
- Explorer と同じ感覚で、作成開始位置をフォルダー選択ダイアログから指定できます。
- ツリー形式のディレクトリ構成図をそのまま貼り付けて使えます。
- `/` または `\` で終わる行はフォルダーとして作成します。
- それ以外の行は空ファイルとして作成します。
- Windows 標準の .NET Framework で動作します。

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

### GUI で実行する場合

1. このリポジトリをダウンロードまたは clone します。
2. `DirectoryTreeBuilder.exe` をダブルクリックします。
3. ディレクトリ構成図を入力欄に貼り付けます。
4. `作成先を選択...` で作成開始位置を選びます。
5. `作成` をクリックします。

### コマンドラインで実行する場合

第1引数にディレクトリ構成図を書いた `.txt` ファイルを指定します。

```powershell
.\DirectoryTreeBuilder.exe .\examples\experiment_tree.txt
```

作成先フォルダーを指定する場合は、第2引数に指定します。

```powershell
.\DirectoryTreeBuilder.exe .\examples\experiment_tree.txt .\output
```

第2引数を省略した場合は、コマンドを実行したカレントディレクトリに作成されます。

実行ファイルを再生成する場合:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build_exe.ps1
```

## 対応している記法

罫線を使った一般的なツリー表記に対応しています。

```text
project/
├─ src/
│  ├─ main.py
│  └─ utils.py
├─ docs/
└─ README.md
```

`├──` や `└──` のように罫線が複数ある表記にも対応しています。

`│` や `├` が入力しにくい場合は、半角スペース2個のインデントで書けます。

```text
project/
  src/
    main.py
    utils.py
  docs/
  README.md
```

1行1パスの形式でも書けます。

```text
project/src/main.py
project/src/utils.py
project/docs/
project/README.md
```

ASCII のツリー表記も使えます。

```text
project/
+-- src/
|  +-- main.py
|  `-- utils.py
+-- docs/
`-- README.md
```

## ファイル構成

```text
.
├─ DirectoryTreeBuilder.exe
├─ build_exe.ps1
├─ src/
│  └─ DirectoryTreeBuilder.cs
├─ examples/
│  └─ experiment_tree.txt
│  └─ ascii_tree.txt
│  └─ indented_tree.txt
│  └─ path_list_tree.txt
├─ README.md
├─ LICENSE
└─ .gitignore
```

## 制作意図

手作業でディレクトリを作るだけなら簡単ですが、何度も同じような構成を作る場合や、提示された構成図を正確に再現する場合はミスが起きやすくなります。

このツールでは、テキストとして渡された構成図を解析し、GUI で選んだ場所にそのまま展開することで、単純作業の自動化と再現性の向上を狙っています。

## 動作環境

- Windows
- .NET Framework

## ライセンス

MIT License
