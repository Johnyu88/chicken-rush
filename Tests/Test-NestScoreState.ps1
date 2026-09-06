$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $PSScriptRoot '../Assets/Scripts/NestScoreState.cs')
function Equal($actual, $expected) {
    if ([Math]::Abs($actual - $expected) -gt 0.0001) { throw "Expected $expected, got $actual" }
}
$score = New-Object ChickenRush.NestScoreState
Equal ($score.Begin(100, 5)) 1
Equal $score.Score 0
Equal $score.Combo 1
Equal ($score.Begin(100, 5)) 0
Equal ($score.Pitch(0.1)) 1
Equal ($score.Settle()) 100
Equal ($score.Settle()) 0
Equal ($score.Begin(100, 5)) 1
Equal ($score.Pitch(0.1)) 1.1
$score.Miss()
Equal ($score.Settle()) 200
Equal $score.Score 300
Equal $score.Combo 0
Equal ($score.Begin(100, 5)) 1
Equal ($score.Settle()) 100
for ($i = 0; $i -lt 20; $i++) { $null = $score.Begin(100, 5); $null = $score.Settle() }
Equal ($score.Pitch(0.1)) 2
Equal ($score.Begin(100, 5)) 1
Equal ($score.Settle()) 500
Write-Output 'PASS: deferred score, duplicate guards, combo, miss, multiplier and pitch caps.'
