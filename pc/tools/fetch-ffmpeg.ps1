<#
.SYNOPSIS
    Populates pc/tools/ffmpeg/ with the ffmpeg binary the PC screen recorder needs.

.DESCRIPTION
    The recorder encodes captured frames by invoking ffmpeg as a child process. An installed
    launcher must ship its own copy, because customer machines will not have ffmpeg on PATH and
    air-gapped deployments are explicitly in scope (see docs/OSS_NODE_PLAN.md). Anything placed in
    pc/tools/ffmpeg/ is copied beside the executable at build time by Febris.PCScreenRecorderV3.csproj,
    which is the first location ScreenRecorder.ResolveFfmpegPath probes.

    The binary is NOT committed. This script is the sanctioned way to obtain it, and it is run at
    build or release time, never at runtime.

    Run with -Path to install from a file you already have (the correct choice for an air-gapped or
    otherwise controlled build machine). Run with -Url to download from a build server you trust.

.PARAMETER Path
    Path to an ffmpeg.exe, or to a .zip containing one, that is already on this machine.

.PARAMETER Url
    URL of a Windows ffmpeg .zip to download. No default is baked in on purpose: the build you ship
    is a licensing and provenance decision, see the LICENSING note below.

.PARAMETER Sha256
    Optional expected SHA-256 of the downloaded or supplied artifact. Strongly recommended, and
    required in practice for anything fetched over the network.

.EXAMPLE
    ./fetch-ffmpeg.ps1 -Path C:\downloads\ffmpeg-release-essentials.zip

.EXAMPLE
    ./fetch-ffmpeg.ps1 -Url https://<your-trusted-mirror>/ffmpeg-win64.zip -Sha256 <hash>

.NOTES
    LICENSING. A typical Windows ffmpeg build is compiled with --enable-gpl so that libx264 is
    available, which makes that binary GPL-licensed. Redistributing it alongside the AGPL launcher
    is mere aggregation, so it does not affect the licensing of Febris code, but it does carry the
    usual GPL obligations for the ffmpeg binary itself: ship its license text and provide or offer
    its corresponding source. Confirm which build you are shipping before a public release. An
    LGPL build avoids those obligations but cannot use libx264, so H.264 would then have to come
    from a hardware encoder or from OpenH264. This is an owner decision, tracked in
    docs/OSS_RELEASE_MANIFEST.md.
#>
[CmdletBinding(DefaultParameterSetName = 'FromPath')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'FromPath')]
    [string]$Path,

    [Parameter(Mandatory = $true, ParameterSetName = 'FromUrl')]
    [string]$Url,

    [Parameter()]
    [string]$Sha256,

    # Where the Corresponding Source for this exact build can be obtained. Recorded in
    # PROVENANCE.txt so the GPL-3.0 section 6 duty can be discharged from the shipped artifact
    # rather than reconstructed months later. See pc/THIRD-PARTY-NOTICES.md.
    [Parameter()]
    [string]$SourceUrl
)

$ErrorActionPreference = 'Stop'

$toolsRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$targetDir = Join-Path $toolsRoot 'ffmpeg'
$staging = Join-Path ([System.IO.Path]::GetTempPath()) ("febris-ffmpeg-" + [System.Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $staging | Out-Null

try {
    if ($PSCmdlet.ParameterSetName -eq 'FromUrl') {
        Write-Host "Downloading $Url"
        # Keep the extension. Expand-Archive dispatches on it and rejects an extensionless file.
        $extension = [System.IO.Path]::GetExtension(([System.Uri]$Url).AbsolutePath)
        if ([string]::IsNullOrWhiteSpace($extension)) { $extension = '.zip' }
        $artifact = Join-Path $staging ("ffmpeg-download" + $extension)
        $previousProgress = $ProgressPreference
        $ProgressPreference = 'SilentlyContinue'   # the progress bar makes large downloads far slower
        try { Invoke-WebRequest -Uri $Url -OutFile $artifact -UseBasicParsing }
        finally { $ProgressPreference = $previousProgress }
    }
    else {
        if (-not (Test-Path $Path)) { throw "Not found: $Path" }
        $artifact = (Resolve-Path $Path).Path
    }

    if ($Sha256) {
        $actual = (Get-FileHash -Path $artifact -Algorithm SHA256).Hash
        if ($actual -ne $Sha256.ToUpperInvariant()) {
            throw "SHA-256 mismatch. Expected $($Sha256.ToUpperInvariant()), got $actual"
        }
        Write-Host "SHA-256 verified."
    }
    elseif ($PSCmdlet.ParameterSetName -eq 'FromUrl') {
        Write-Warning "No -Sha256 supplied, so the download is unverified. Pass one for a release build."
    }

    # Accept either a bare ffmpeg.exe or a zip that contains one somewhere inside it.
    if ([System.IO.Path]::GetExtension($artifact) -ieq '.exe') {
        $sourceExe = $artifact
    }
    else {
        $extracted = Join-Path $staging 'extracted'
        Write-Host "Extracting archive"
        Expand-Archive -Path $artifact -DestinationPath $extracted -Force
        $sourceExe = Get-ChildItem -Path $extracted -Recurse -Filter 'ffmpeg.exe' |
            Select-Object -First 1 -ExpandProperty FullName
        if (-not $sourceExe) { throw "No ffmpeg.exe inside $artifact" }
    }

    New-Item -ItemType Directory -Force -Path $targetDir | Out-Null
    Copy-Item -Path $sourceExe -Destination (Join-Path $targetDir 'ffmpeg.exe') -Force

    # Ship the licence next to the binary when the archive carries one.
    if ($sourceExe -ne $artifact) {
        Get-ChildItem -Path (Split-Path -Parent (Split-Path -Parent $sourceExe)) -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -match '^(LICENSE|COPYING)' } |
            ForEach-Object { Copy-Item $_.FullName -Destination $targetDir -Force }
    }

    $installed = Join-Path $targetDir 'ffmpeg.exe'
    $size = (Get-Item $installed).Length
    Write-Host ""
    Write-Host ("Installed {0} ({1:N0} bytes)" -f $installed, $size)
    Write-Host "It will be copied beside the executable on the next build."

    $versionOutput = & $installed -hide_banner -version 2>&1
    $versionLine = ($versionOutput | Select-Object -First 1)
    $configureLine = ($versionOutput | Select-String '^configuration:').Line
    $installedHash = (Get-FileHash -Path $installed -Algorithm SHA256).Hash
    Write-Host $versionLine

    # Licence posture, read off the build rather than assumed.
    $isGpl = $configureLine -match '--enable-gpl'
    $isV3 = $configureLine -match '--enable-version3'
    $isNonfree = $configureLine -match '--enable-nonfree'
    $licence = if ($isNonfree) { 'NONFREE, NOT REDISTRIBUTABLE' }
               elseif ($isGpl -and $isV3) { 'GPL-3.0' }
               elseif ($isGpl) { 'GPL-2.0-or-later' }
               elseif ($isV3) { 'LGPL-3.0' }
               else { 'LGPL-2.1-or-later' }

    $provenance = @(
        "Febris PC client tier, bundled ffmpeg provenance"
        "Generated by pc/tools/fetch-ffmpeg.ps1. See pc/THIRD-PARTY-NOTICES.md for the obligations."
        ""
        "fetched-utc      : $((Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ'))"
        "version          : $versionLine"
        "binary-sha256    : $installedHash"
        "binary-bytes     : $size"
        "origin           : $(if ($PSCmdlet.ParameterSetName -eq 'FromUrl') { $Url } else { "local file: $Path" })"
        "artifact-sha256  : $(if ($Sha256) { $Sha256.ToUpperInvariant() } else { 'not supplied' })"
        "licence          : $licence"
        "corresponding-source : $(if ($SourceUrl) { $SourceUrl } else { 'NOT RECORDED. Required by GPL-3.0 section 6, re-run with -SourceUrl.' })"
        ""
        "Corresponding Source under GPL-3.0 section 6 means this source PLUS the scripts controlling"
        "compilation. The configure string below is that second half, so keep them together."
        ""
        $configureLine
    ) -join "`r`n"
    Set-Content -Path (Join-Path $targetDir 'PROVENANCE.txt') -Value $provenance -Encoding utf8

    Write-Host "licence detected : $licence"
    if ($isNonfree) {
        Write-Warning "This build is NONFREE and CANNOT be redistributed. Do not ship it. Obtain a build without --enable-nonfree."
    }
    if (-not $SourceUrl) {
        Write-Warning "No -SourceUrl recorded. GPL builds require you to make the Corresponding Source available, so re-run with -SourceUrl before cutting a release."
    }
    if (-not (Get-ChildItem -Path $targetDir -Filter 'LICENSE*') -and -not (Get-ChildItem -Path $targetDir -Filter 'COPYING*')) {
        Write-Warning "No LICENSE or COPYING file was found in the archive. One must ship beside the binary."
    }

    $encoders = & $installed -hide_banner -encoders 2>&1 | Select-String 'libx264'
    if ($encoders) {
        Write-Host "libx264 present."
    }
    else {
        Write-Warning "This build has no libx264. The recorder requests it by default, so set ScreenRecorderStaticDetails.videoCodec to an encoder this build supports."
    }
    Write-Host "Wrote $(Join-Path $targetDir 'PROVENANCE.txt')"
}
finally {
    Remove-Item -Recurse -Force $staging -ErrorAction SilentlyContinue
}
