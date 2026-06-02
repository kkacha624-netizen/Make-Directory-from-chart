using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DirectoryTreeBuilder
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            if (args.Length > 0)
            {
                return CommandLineRunner.Run(args);
            }

            NativeMethods.FreeConsoleWindow();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            return 0;
        }
    }

    internal static class NativeMethods
    {
        private const int AttachParentProcess = -1;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        public static void FreeConsoleWindow()
        {
            FreeConsole();
        }
    }

    internal static class CommandLineRunner
    {
        public static int Run(string[] args)
        {
            try
            {
                if (IsHelp(args[0]))
                {
                    PrintUsage();
                    return 0;
                }

                if (args.Length > 2)
                {
                    throw new InvalidOperationException("引数が多すぎます。");
                }

                var diagramFilePath = Path.GetFullPath(args[0]);
                if (!File.Exists(diagramFilePath))
                {
                    throw new FileNotFoundException("ディレクトリ構成図の .txt ファイルが見つかりません。", diagramFilePath);
                }

                if (!string.Equals(Path.GetExtension(diagramFilePath), ".txt", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("第1引数には .txt ファイルを指定してください。");
                }

                var outputPath = args.Length == 2
                    ? Path.GetFullPath(args[1])
                    : Directory.GetCurrentDirectory();

                Directory.CreateDirectory(outputPath);

                var diagram = File.ReadAllText(diagramFilePath, Encoding.UTF8);
                var entries = TreeParser.Parse(diagram);
                var result = TreeCreator.Create(outputPath, entries);

                Console.WriteLine("作成しました。");
                Console.WriteLine("作成先: " + outputPath);
                Console.WriteLine("新規ディレクトリ: " + result.Directories);
                Console.WriteLine("新規ファイル: " + result.Files);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("エラー: " + ex.Message);
                PrintUsage();
                return 1;
            }
        }

        private static bool IsHelp(string value)
        {
            return string.Equals(value, "-h", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "--help", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "/?", StringComparison.OrdinalIgnoreCase);
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  DirectoryTreeBuilder.exe <tree.txt> [output_folder]");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  DirectoryTreeBuilder.exe examples\\experiment_tree.txt");
            Console.WriteLine("  DirectoryTreeBuilder.exe examples\\experiment_tree.txt C:\\work\\output");
        }
    }

    internal sealed class MainForm : Form
    {
        private readonly Label targetLabel;
        private readonly TextBox diagramTextBox;
        private readonly Label statusLabel;
        private string selectedPath;

        public MainForm()
        {
            Text = "ディレクトリ構成図から作成";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(760, 560);
            MinimumSize = new Size(640, 460);
            Font = new Font("Yu Gothic UI", 10);

            targetLabel = new Label
            {
                Text = "作成先: 未選択",
                AutoSize = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(16, 18),
                Size = new Size(570, 26)
            };

            var browseButton = new Button
            {
                Text = "作成先を選択...",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(600, 14),
                Size = new Size(120, 32)
            };
            browseButton.Click += BrowseButton_Click;

            diagramTextBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                AcceptsReturn = true,
                AcceptsTab = true,
                WordWrap = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(16, 58),
                Size = new Size(704, 385),
                Text =
                    "expXXX_experiment_name/" + Environment.NewLine +
                    "├─ config.yaml" + Environment.NewLine +
                    "├─ result.md" + Environment.NewLine +
                    "├─ metrics.csv" + Environment.NewLine +
                    "├─ logs/" + Environment.NewLine +
                    "└─ figures/"
            };

            var createButton = new Button
            {
                Text = "作成",
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(600, 462),
                Size = new Size(120, 34)
            };
            createButton.Click += CreateButton_Click;

            statusLabel = new Label
            {
                Text = "構成図を貼り付けて、作成先を選んでください。",
                AutoSize = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(16, 468),
                Size = new Size(570, 26)
            };

            Controls.Add(targetLabel);
            Controls.Add(browseButton);
            Controls.Add(diagramTextBox);
            Controls.Add(createButton);
            Controls.Add(statusLabel);
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "ディレクトリを作成する開始位置を選択してください";
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    selectedPath = dialog.SelectedPath;
                    targetLabel.Text = "作成先: " + selectedPath;
                    statusLabel.Text = "作成先を選択しました。";
                }
            }
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(selectedPath))
                {
                    throw new InvalidOperationException("先に作成先フォルダーを選択してください。");
                }

                var entries = TreeParser.Parse(diagramTextBox.Text);
                var result = TreeCreator.Create(selectedPath, entries);
                var message = string.Format(
                    "作成しました。新規ディレクトリ: {0}, 新規ファイル: {1}",
                    result.Directories,
                    result.Files);

                statusLabel.Text = message;
                MessageBox.Show(this, message, "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                statusLabel.Text = "エラー: " + ex.Message;
                MessageBox.Show(this, ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    internal sealed class TreeEntry
    {
        public int Depth { get; set; }
        public string Name { get; set; }
        public bool IsDirectory { get; set; }
    }

    internal sealed class CreateResult
    {
        public int Directories { get; set; }
        public int Files { get; set; }
    }

    internal static class TreeParser
    {
        private static readonly Regex BranchPattern =
            new Regex(@"^(?<prefix>[│\s]*)(?:├─+|└─+)\s*(?<name>.+)$", RegexOptions.Compiled);

        public static List<TreeEntry> Parse(string diagram)
        {
            var entries = new List<TreeEntry>();
            var lines = diagram.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

            foreach (var rawLine in lines)
            {
                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    continue;
                }

                var line = rawLine.TrimEnd();
                var depth = 0;
                var name = line.Trim();
                var match = BranchPattern.Match(line);

                if (match.Success)
                {
                    var prefix = match.Groups["prefix"].Value;
                    depth = (prefix.Length / 3) + 1;
                    name = match.Groups["name"].Value.Trim();
                }

                var isDirectory = name.EndsWith("/", StringComparison.Ordinal) ||
                                  name.EndsWith("\\", StringComparison.Ordinal);
                var cleanName = name.TrimEnd('/', '\\').Trim();

                if (string.IsNullOrWhiteSpace(cleanName))
                {
                    continue;
                }

                if (cleanName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    throw new InvalidOperationException("使用できない文字を含む名前があります: " + cleanName);
                }

                entries.Add(new TreeEntry
                {
                    Depth = depth,
                    Name = cleanName,
                    IsDirectory = isDirectory
                });
            }

            if (entries.Count == 0)
            {
                throw new InvalidOperationException("ディレクトリ構成図が空です。");
            }

            return entries;
        }
    }

    internal static class TreeCreator
    {
        public static CreateResult Create(string basePath, IEnumerable<TreeEntry> entries)
        {
            var stack = new Dictionary<int, string>();
            var result = new CreateResult();

            foreach (var entry in entries)
            {
                string path;

                if (entry.Depth == 0)
                {
                    path = Path.Combine(basePath, entry.Name);
                }
                else
                {
                    string parentPath;
                    if (!stack.TryGetValue(entry.Depth - 1, out parentPath))
                    {
                        throw new InvalidOperationException("親ディレクトリが見つかりません: " + entry.Name);
                    }

                    path = Path.Combine(parentPath, entry.Name);
                }

                if (entry.IsDirectory)
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                        result.Directories++;
                    }

                    stack[entry.Depth] = path;
                }
                else
                {
                    var parentPath = Path.GetDirectoryName(path);
                    if (!Directory.Exists(parentPath))
                    {
                        Directory.CreateDirectory(parentPath);
                        result.Directories++;
                    }

                    if (!File.Exists(path))
                    {
                        using (File.Create(path))
                        {
                        }

                        result.Files++;
                    }
                }
            }

            return result;
        }
    }
}
