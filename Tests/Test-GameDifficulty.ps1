$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $PSScriptRoot '../Assets/Scripts/GameDifficulty.cs')
function Equal($actual, $expected) {
    if ([Math]::Abs($actual - $expected) -gt 0.0001) { throw "Expected $expected, got $actual" }
}
$easy = [ChickenRush.GameDifficultySettings]::For([ChickenRush.GameDifficulty]::Relaxed)
$extreme = [ChickenRush.GameDifficultySettings]::For([ChickenRush.GameDifficulty]::Extreme)
Equal $easy.ObstacleCount 0
Equal $easy.NestSpeed 0
Equal $easy.BaseBounciness 0.4
Equal $extreme.ObstacleCount 3
Equal $extreme.NestSpeed 4
Equal $extreme.BaseBounciness 0.7
$hell = [ChickenRush.GameDifficultySettings]::For([ChickenRush.GameDifficulty]::Hell)
$demon = [ChickenRush.GameDifficultySettings]::For([ChickenRush.GameDifficulty]::Demon)
Equal $hell.ObstacleCount 1
Equal $hell.NestSpeed 1.5
Equal $hell.BaseBounciness 0.4
Equal $demon.ObstacleCount 2
Equal $demon.NestSpeed 2.5
Equal $demon.BaseBounciness 0.55
foreach ($difficulty in [Enum]::GetValues([ChickenRush.GameDifficulty])) {
    $settings = [ChickenRush.GameDifficultySettings]::For($difficulty)
    if ($settings.ObstacleCount -lt 0 -or $settings.ObstacleCount -gt 3) { throw 'Invalid obstacle count' }
}
$rejected = $false
try { [ChickenRush.GameDifficultySettings]::For([ChickenRush.GameDifficulty]99) } catch { $rejected = $true }
if (!$rejected) { throw 'Invalid difficulty must be rejected' }
Write-Output 'PASS: difficulty endpoints, obstacle bounds and invalid selection.'
