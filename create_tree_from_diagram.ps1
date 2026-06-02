Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

function Get-TreeEntries {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Diagram
    )

    $entries = New-Object System.Collections.Generic.List[object]
    $lines = $Diagram -split "\r?\n"

    foreach ($rawLine in $lines) {
        if ([string]::IsNullOrWhiteSpace($rawLine)) {
            continue
        }

        $line = $rawLine.TrimEnd()
        $depth = 0
        $name = $line.Trim()

        if ($line -match '^(?<prefix>[笏・s]*)(?:笏懌楳+|笏披楳+)\s*(?<name>.+)$') {
            $prefix = $Matches['prefix']
            $depth = [int]([Math]::Floor($prefix.Length / 3)) + 1
            $name = $Matches['name'].Trim()
        }

        $isDirectory = $name.EndsWith('/') -or $name.EndsWith('\')
        $cleanName = $name.TrimEnd('/', '\').Trim()

        if ([string]::IsNullOrWhiteSpace($cleanName)) {
            continue
        }

        $invalidChars = [System.IO.Path]::GetInvalidFileNameChars()
        if ($cleanName.IndexOfAny($invalidChars) -ge 0) {
            throw "菴ｿ逕ｨ縺ｧ縺阪↑縺・枚蟄励ｒ蜷ｫ繧蜷榊燕縺後≠繧翫∪縺・ $cleanName"
        }

        $entries.Add([PSCustomObject]@{
            Depth = $depth
            Name = $cleanName
            IsDirectory = $isDirectory
        })
    }

    if ($entries.Count -eq 0) {
        throw "繝・ぅ繝ｬ繧ｯ繝医Μ讒区・蝗ｳ縺檎ｩｺ縺ｧ縺吶・
    }

    return $entries
}

function New-TreeFromEntries {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BasePath,

        [Parameter(Mandatory = $true)]
        [System.Collections.IEnumerable]$Entries
    )

    $stack = @{}
    $createdDirectories = 0
    $createdFiles = 0

    foreach ($entry in $Entries) {
        if ($entry.Depth -eq 0) {
            $path = Join-Path -Path $BasePath -ChildPath $entry.Name
        } else {
            $parentDepth = $entry.Depth - 1
            if (-not $stack.ContainsKey($parentDepth)) {
                throw "隕ｪ繝・ぅ繝ｬ繧ｯ繝医Μ縺瑚ｦ九▽縺九ｊ縺ｾ縺帙ｓ: $($entry.Name)"
            }

            $path = Join-Path -Path $stack[$parentDepth] -ChildPath $entry.Name
        }

        if ($entry.IsDirectory) {
            if (-not (Test-Path -LiteralPath $path -PathType Container)) {
                New-Item -ItemType Directory -Path $path -Force | Out-Null
                $createdDirectories++
            }
            $stack[$entry.Depth] = $path
        } else {
            $parentPath = Split-Path -Path $path -Parent
            if (-not (Test-Path -LiteralPath $parentPath -PathType Container)) {
                New-Item -ItemType Directory -Path $parentPath -Force | Out-Null
                $createdDirectories++
            }

            if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
                New-Item -ItemType File -Path $path -Force | Out-Null
                $createdFiles++
            }
        }
    }

    return [PSCustomObject]@{
        Directories = $createdDirectories
        Files = $createdFiles
    }
}

$form = New-Object System.Windows.Forms.Form
$form.Text = "繝・ぅ繝ｬ繧ｯ繝医Μ讒区・蝗ｳ縺九ｉ菴懈・"
$form.StartPosition = "CenterScreen"
$form.Size = New-Object System.Drawing.Size(760, 560)
$form.MinimumSize = New-Object System.Drawing.Size(640, 460)

$font = New-Object System.Drawing.Font("Yu Gothic UI", 10)
$form.Font = $font

$targetLabel = New-Object System.Windows.Forms.Label
$targetLabel.Text = "菴懈・蜈・ 譛ｪ驕ｸ謚・
$targetLabel.AutoSize = $false
$targetLabel.Anchor = "Top,Left,Right"
$targetLabel.Location = New-Object System.Drawing.Point(16, 18)
$targetLabel.Size = New-Object System.Drawing.Size(570, 26)

$browseButton = New-Object System.Windows.Forms.Button
$browseButton.Text = "菴懈・蜈医ｒ驕ｸ謚・.."
$browseButton.Anchor = "Top,Right"
$browseButton.Location = New-Object System.Drawing.Point(600, 14)
$browseButton.Size = New-Object System.Drawing.Size(120, 32)

$textBox = New-Object System.Windows.Forms.TextBox
$textBox.Multiline = $true
$textBox.ScrollBars = "Both"
$textBox.AcceptsReturn = $true
$textBox.AcceptsTab = $true
$textBox.WordWrap = $false
$textBox.Anchor = "Top,Bottom,Left,Right"
$textBox.Location = New-Object System.Drawing.Point(16, 58)
$textBox.Size = New-Object System.Drawing.Size(704, 385)
$textBox.Text = @"
expXXX_experiment_name/
笏懌楳 config.yaml
笏懌楳 result.md
笏懌楳 metrics.csv
笏懌楳 logs/
笏披楳 figures/
"@

$createButton = New-Object System.Windows.Forms.Button
$createButton.Text = "菴懈・"
$createButton.Anchor = "Bottom,Right"
$createButton.Location = New-Object System.Drawing.Point(600, 462)
$createButton.Size = New-Object System.Drawing.Size(120, 34)

$statusLabel = New-Object System.Windows.Forms.Label
$statusLabel.Text = "讒区・蝗ｳ繧定ｲｼ繧贋ｻ倥￠縺ｦ縲∽ｽ懈・蜈医ｒ驕ｸ繧薙〒縺上□縺輔＞縲・
$statusLabel.AutoSize = $false
$statusLabel.Anchor = "Bottom,Left,Right"
$statusLabel.Location = New-Object System.Drawing.Point(16, 468)
$statusLabel.Size = New-Object System.Drawing.Size(570, 26)

$selectedPath = $null

$browseButton.Add_Click({
    $dialog = New-Object System.Windows.Forms.FolderBrowserDialog
    $dialog.Description = "繝・ぅ繝ｬ繧ｯ繝医Μ繧剃ｽ懈・縺吶ｋ髢句ｧ倶ｽ咲ｽｮ繧帝∈謚槭＠縺ｦ縺上□縺輔＞"
    $dialog.ShowNewFolderButton = $true

    if ($dialog.ShowDialog($form) -eq [System.Windows.Forms.DialogResult]::OK) {
        $script:selectedPath = $dialog.SelectedPath
        $targetLabel.Text = "菴懈・蜈・ $script:selectedPath"
        $statusLabel.Text = "菴懈・蜈医ｒ驕ｸ謚槭＠縺ｾ縺励◆縲・
    }
})

$createButton.Add_Click({
    try {
        if ([string]::IsNullOrWhiteSpace($script:selectedPath)) {
            throw "蜈医↓菴懈・蜈医ヵ繧ｩ繝ｫ繝繝ｼ繧帝∈謚槭＠縺ｦ縺上□縺輔＞縲・
        }

        $entries = Get-TreeEntries -Diagram $textBox.Text
        $result = New-TreeFromEntries -BasePath $script:selectedPath -Entries $entries
        $message = "菴懈・縺励∪縺励◆縲よ眠隕上ョ繧｣繝ｬ繧ｯ繝医Μ: $($result.Directories), 譁ｰ隕上ヵ繧｡繧､繝ｫ: $($result.Files)"
        $statusLabel.Text = $message
        [System.Windows.Forms.MessageBox]::Show($form, $message, "螳御ｺ・, "OK", "Information") | Out-Null
    } catch {
        $statusLabel.Text = "繧ｨ繝ｩ繝ｼ: $($_.Exception.Message)"
        [System.Windows.Forms.MessageBox]::Show($form, $_.Exception.Message, "繧ｨ繝ｩ繝ｼ", "OK", "Error") | Out-Null
    }
})

$form.Controls.Add($targetLabel)
$form.Controls.Add($browseButton)
$form.Controls.Add($textBox)
$form.Controls.Add($createButton)
$form.Controls.Add($statusLabel)

[void]$form.ShowDialog()
