param([string]$BaseUrl = 'http://localhost:5080')

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http
$client = New-Object System.Net.Http.HttpClient
$passed = 0

function Invoke-Check {
    param([string]$Method, [string]$Path, [int]$Expected, [object]$Body = $null, [string]$Token = '')
    $request = New-Object System.Net.Http.HttpRequestMessage
    $request.Method = New-Object System.Net.Http.HttpMethod($Method)
    $request.RequestUri = [Uri]($BaseUrl.TrimEnd('/') + $Path)
    if ($Token) { $request.Headers.Authorization = New-Object System.Net.Http.Headers.AuthenticationHeaderValue('Bearer', $Token) }
    if ($null -ne $Body) {
        $request.Content = New-Object System.Net.Http.StringContent(($Body | ConvertTo-Json), [System.Text.Encoding]::UTF8, 'application/json')
    }
    $response = $null
    try {
        $response = $client.SendAsync($request).GetAwaiter().GetResult()
        $content = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        if ([int]$response.StatusCode -ne $Expected) {
            throw "$Method $Path : esperado $Expected, recibido $([int]$response.StatusCode). $content"
        }
        $script:passed++
        Write-Host "OK $Expected $Method $Path"
        if ($content -and $response.Content.Headers.ContentType.MediaType -match 'json') { return ($content | ConvertFrom-Json) }
    }
    finally {
        if ($null -ne $response) { $response.Dispose() }
        $request.Dispose()
    }
}

try {
    $admin = Invoke-Check POST '/api/v1/auth/token' 200 @{ email = 'admin@demo.com'; password = 'AdminDemo!2026' }
    $consulta = Invoke-Check POST '/api/v1/auth/token' 200 @{ email = 'consulta@demo.com'; password = 'ConsultaDemo!2026' }
    if ($admin.tokenType -ne 'Bearer' -or $admin.expiresIn -le 0 -or -not $admin.accessToken) { throw 'Respuesta de token inválida.' }
    $null = Invoke-Check POST '/api/v1/auth/token' 401 @{ email = 'inactivo@demo.com'; password = 'InactivoDemo!2026' }
    $null = Invoke-Check POST '/api/v1/auth/token' 401 @{ email = 'admin@demo.com'; password = 'incorrecta' }
    $null = Invoke-Check POST '/api/v1/auth/token' 400 @{ email = 'no-es-email'; password = 'incorrecta' }
    $null = Invoke-Check GET '/api/v1/users' 401
    $null = Invoke-Check GET '/api/v1/users' 401 -Token 'invalid-token'
    $page = Invoke-Check GET '/api/v1/users?page=1&pageSize=2' 200 -Token $consulta.accessToken
    if ($page.items.Count -ne 2 -or $page.totalCount -lt 3) { throw 'Paginación incorrecta.' }
    foreach ($item in $page.items) {
        if ($item.PSObject.Properties.Name -contains 'password' -or $item.PSObject.Properties.Name -contains 'passwordHash') {
            throw 'La respuesta expone una contraseña o su hash.'
        }
    }
    $empty = Invoke-Check GET '/api/v1/users?page=100000&pageSize=100' 200 -Token $admin.accessToken
    if ($empty.items.Count -ne 0) { throw 'Se esperaba una página vacía.' }
    $null = Invoke-Check GET '/api/v1/users?page=0&pageSize=101' 400 -Token $admin.accessToken
    $path = '/api/v1/users/22222222-2222-2222-2222-222222222222'
    $body = @{ name = 'Consulta'; email = 'consulta@demo.com'; isActive = $true }
    $null = Invoke-Check PUT $path 403 $body $consulta.accessToken
    $result = Invoke-Check PUT $path 200 $body $admin.accessToken
    if ($result.email -ne $body.email -or -not $result.isActive) { throw 'Actualización incorrecta.' }
    $null = Invoke-Check PUT '/api/v1/users/99999999-9999-9999-9999-999999999999' 404 $body $admin.accessToken
    $null = Invoke-Check PUT $path 400 @{ name = 'Consulta'; email = 'consulta@demo.com' } $admin.accessToken
    $body.email = 'ADMIN@DEMO.COM'
    $null = Invoke-Check PUT $path 409 $body $admin.accessToken
    $body.email = 'no-es-email'
    $null = Invoke-Check PUT $path 400 $body $admin.accessToken
    Write-Host "$passed verificaciones HTTP correctas."
}
finally { $client.Dispose() }
