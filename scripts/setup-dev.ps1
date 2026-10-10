$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$envPath = Join-Path $repoRoot '.env'
$settingsPath = Join-Path $repoRoot 'src/Api/appsettings.Local.json'
if ((Test-Path -LiteralPath $envPath) -or (Test-Path -LiteralPath $settingsPath)) {
    throw 'Ya existe configuración local. Se conserva; revisa .env y src/Api/appsettings.Local.json.'
}

function New-RandomSecret {
    $bytes = New-Object byte[] 32
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    return [Convert]::ToBase64String($bytes)
}

$sqlPassword = 'Dev1!' + (New-RandomSecret)
$jwtSecret = New-RandomSecret
$settings = @{
    ConnectionStrings = @{ UsersDatabase = "Server=127.0.0.1,14333;Database=UsersDemo;User Id=sa;Password=$sqlPassword;Encrypt=True;TrustServerCertificate=True" }
    Jwt = @{ SigningKey = $jwtSecret }
    Seed = @{
        Enabled = $true
        Passwords = @{ Admin = 'AdminDemo!2026'; Consulta = 'ConsultaDemo!2026'; Inactivo = 'InactivoDemo!2026' }
    }
}
$utf8 = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($envPath, "MSSQL_SA_PASSWORD=$sqlPassword`n", $utf8)
[System.IO.File]::WriteAllText($settingsPath, ($settings | ConvertTo-Json -Depth 5), $utf8)
Write-Output 'Configuración local creada. La contraseña SQL está en .env; ambos archivos están excluidos de Git.'
