$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $PSScriptRoot '../Assets/Scripts/HoldSpawnInput.cs')
function Assert-Equal($actual, $expected, $message) {
    if ([Math]::Abs($actual - $expected) -gt 0.0001) { throw "$message : expected $expected, got $actual" }
}
$inputState = New-Object ChickenRush.HoldSpawnInput
Assert-Equal ($inputState.Step($false, 0.5, 1, 5, 0.25, 2)) 0 'Idle does not spawn'
Assert-Equal ($inputState.Step($true, 0.5, 0, 5, 0.25, 2)) 1 'Press spawns immediately'
Assert-Equal ($inputState.Step($true, 0.75, 0.1, 5, 0.25, 2)) 0 'Wait for interval'
Assert-Equal $inputState.HorizontalSpeed 2 'Right drag reaches speed limit'
Assert-Equal ($inputState.Step($true, 0, 0.1, 5, 0.25, 2)) 1 'Interval emits one'
Assert-Equal $inputState.HorizontalSpeed -2 'Left drag is clamped'
Assert-Equal ($inputState.Step($false, 0, 10, 5, 0.25, 2)) 0 'Release cancels emission'
Assert-Equal $inputState.HorizontalSpeed 0 'Release resets aim'
Assert-Equal ($inputState.Step($true, 0.8, 0, 5, 0.25, 2)) 1 'Repress starts fresh'
Assert-Equal $inputState.HorizontalSpeed 0 'Repress resets anchor'
$inputState.Reset()
Assert-Equal ($inputState.Step($true, 0.5, 1, 0, 0.25, 2)) 0 'Zero rate disables emission'
foreach ($fps in @(30, 60, 120)) {
    $inputState.Reset()
    $count = $inputState.Step($true, 0.5, 0, 7, 0.25, 2)
    for ($i = 0; $i -lt $fps; $i++) { $count += $inputState.Step($true, 0.5, (1.0 / $fps), 7, 0.25, 2) }
    Assert-Equal $count 8 "One immediate plus seven per second at $fps FPS"
}
$inputState.Reset()
$null = $inputState.Step($true, 0.5, 0, 20, 0.25, 2)
Assert-Equal ($inputState.Step($true, 0.5, 0.2, 20, 0.25, 2)) 4 'Low frame rate catches up'
Write-Output 'PASS: hold, release, rate, drag limits, reset and frame-rate independence.'
