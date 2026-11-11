<#
    Branding for an installation: the client's company name and their logo.

    A client hands over a logo in whatever their designer had open - PNG, JPG, sometimes a CorelDRAW
    .cdr or an Illustrator .ai. This turns whatever arrived into the two things the panel actually
    needs (a logo image and a favicon) and writes them into the running installation, with no
    rebuild: the panel reads config.json in the browser at load time.

    Formats that cannot be converted on a bare Windows server (.cdr, .ai, .psd, .eps) are refused
    with instructions rather than a failure - the fix is a two-minute export by whoever sent it, and
    the install should not stall for it either, so the logo can always be added later by re-running
    Set-Branding.ps1.

    Dot-source after FinoCommon.ps1.
#>

Add-Type -AssemblyName System.Drawing -ErrorAction SilentlyContinue

# What GDI+ on a stock Windows Server can actually open.
$script:RasterExtensions = @('.png', '.jpg', '.jpeg', '.bmp', '.gif', '.tif', '.tiff')
# Vector the browser renders natively - no conversion needed, and it stays sharp at every size.
$script:VectorExtensions = @('.svg')
# Design-tool formats. Converting these needs the design tool itself.
$script:DesignExtensions = @('.cdr', '.ai', '.eps', '.psd', '.indd', '.sketch', '.fig', '.xd', '.pdf', '.webp')

function Get-LogoKind {
    param([string] $Path)
    $extension = [System.IO.Path]::GetExtension($Path).ToLower()
    if ($extension -in $script:RasterExtensions) { return 'raster' }
    if ($extension -in $script:VectorExtensions) { return 'vector' }
    if ($extension -in $script:DesignExtensions) { return 'design' }
    return 'unknown'
}

function Show-LogoFormatHelp {
    param([string] $Path)
    $extension = [System.IO.Path]::GetExtension($Path).ToUpper().TrimStart('.')

    Write-Host ''
    Write-Warn2 "$extension files cannot be used directly on a web page."
    Write-Host '  Ask the client (or their designer) to send the logo as PNG instead:' -ForegroundColor Yellow
    Write-Host ''
    if ($extension -eq 'CDR') {
        Write-Host '    In CorelDRAW:  File -> Export -> choose PNG' -ForegroundColor DarkGray
    } elseif ($extension -eq 'AI' -or $extension -eq 'EPS') {
        Write-Host '    In Illustrator:  File -> Export -> Export As -> PNG' -ForegroundColor DarkGray
    } elseif ($extension -eq 'PSD') {
        Write-Host '    In Photoshop:  File -> Export -> Quick Export as PNG' -ForegroundColor DarkGray
    } elseif ($extension -eq 'PDF') {
        Write-Host '    Open the PDF, export or screenshot the logo, and save it as PNG' -ForegroundColor DarkGray
    } else {
        Write-Host '    Export or "Save as" PNG from whichever program made the file' -ForegroundColor DarkGray
    }
    Write-Host '    Roughly 512 x 512 pixels, transparent background if possible.' -ForegroundColor DarkGray
    Write-Host ''
    Write-Host '  An SVG file also works if they have one.' -ForegroundColor DarkGray
    Write-Host '  You can finish the installation now and add the logo later by running' -ForegroundColor DarkGray
    Write-Host '  Set-Branding.ps1 - nothing has to be reinstalled.' -ForegroundColor DarkGray
    Write-Host ''
}

function Read-LogoPath {
    <#
        Asks for the logo file. The console accepts a dragged-and-dropped file, which arrives
        wrapped in quotes, so those are stripped rather than turned into "file not found".
    #>
    param([switch] $AllowSkip)

    while ($true) {
        Write-Host ''
        Write-Host '  Client logo' -ForegroundColor White
        Write-Host '  Drag the logo file into this window and press Enter (PNG, JPG or SVG).' -ForegroundColor DarkGray
        if ($AllowSkip) { Write-Host '  Press Enter on its own to skip - the company name is shown as text until a logo is added.' -ForegroundColor DarkGray }

        $answer = ([string](Read-Answer '  Logo file')).Trim().Trim('"').Trim("'")
        if (-not $answer) {
            # No console to ask (the graphical setup, or a scheduled run): carry on without a logo
            # rather than looping forever. It can be added later with Set-Branding.ps1.
            if ($AllowSkip -or $script:PromptingUnavailable) { return $null }
            Write-Warn2 'A logo file is needed.'
            continue
        }

        if (-not (Test-Path $answer)) {
            Write-Warn2 "There is no file at: $answer"
            continue
        }

        $kind = Get-LogoKind $answer
        if ($kind -eq 'design' -or $kind -eq 'unknown') {
            Show-LogoFormatHelp $answer
            if ($AllowSkip -and (Ask-YesNo '  Continue without a logo for now?' $true)) { return $null }
            continue
        }

        return (Resolve-Path $answer).Path
    }
}

# ---------------------------------------------------------------------------
# Image work
# ---------------------------------------------------------------------------
function Get-InkBounds {
    <#
        The rectangle the artwork actually occupies, ignoring the empty margin around it.

        Designers export logos with generous whitespace - the file this was written for carried 15%
        of its height as empty space above the mark. Nothing on a web page wants that: the margin
        eats the height the header gave the logo, and the mark comes out a third smaller than the
        box it sits in. Trimming is what makes a logo look bigger without a taller header.

        Handles both kinds of export: transparent margins (alpha) and a flat background colour
        (sampled from the corners, so a white-on-white or navy-on-navy export trims correctly).

        Returns $null when there is nothing to trim - an image with no clear background, or one
        that is already tight.
    #>
    param([Parameter(Mandatory = $true)] [System.Drawing.Bitmap] $Bitmap)

    $width = $Bitmap.Width
    $height = $Bitmap.Height

    # What the background is, according to the four corners. If they disagree, the artwork runs to
    # the edge and there is nothing to trim.
    $corners = @(
        $Bitmap.GetPixel(0, 0), $Bitmap.GetPixel($width - 1, 0),
        $Bitmap.GetPixel(0, $height - 1), $Bitmap.GetPixel($width - 1, $height - 1)
    )
    $transparent = ($corners | Where-Object { $_.A -lt 24 }).Count -eq 4

    $bgR = 0; $bgG = 0; $bgB = 0
    if (-not $transparent) {
        foreach ($c in $corners) { $bgR += $c.R; $bgG += $c.G; $bgB += $c.B }
        $bgR = [int]($bgR / 4); $bgG = [int]($bgG / 4); $bgB = [int]($bgB / 4)
        foreach ($c in $corners) {
            if ([Math]::Abs($c.R - $bgR) -gt 12 -or [Math]::Abs($c.G - $bgG) -gt 12 -or [Math]::Abs($c.B - $bgB) -gt 12) {
                return $null   # corners disagree - not a plain background
            }
        }
    }

    $minX = $width; $minY = $height; $maxX = -1; $maxY = -1
    # Every other pixel is plenty for a bounding box and four times faster on a big export.
    $step = [Math]::Max(1, [int]($width / 600))

    for ($y = 0; $y -lt $height; $y += $step) {
        for ($x = 0; $x -lt $width; $x += $step) {
            $p = $Bitmap.GetPixel($x, $y)
            $isInk = if ($transparent) { $p.A -gt 24 }
                     else {
                         $p.A -gt 24 -and (
                             [Math]::Abs($p.R - $bgR) -gt 18 -or
                             [Math]::Abs($p.G - $bgG) -gt 18 -or
                             [Math]::Abs($p.B - $bgB) -gt 18)
                     }
            if ($isInk) {
                if ($x -lt $minX) { $minX = $x }
                if ($x -gt $maxX) { $maxX = $x }
                if ($y -lt $minY) { $minY = $y }
                if ($y -gt $maxY) { $maxY = $y }
            }
        }
    }

    if ($maxX -lt 0) { return $null }                       # nothing but background
    $minX = [Math]::Max(0, $minX - $step); $minY = [Math]::Max(0, $minY - $step)
    $maxX = [Math]::Min($width - 1, $maxX + $step); $maxY = [Math]::Min($height - 1, $maxY + $step)

    $trimmed = ($maxX - $minX + 1) * ($maxY - $minY + 1)
    if ($trimmed -ge ($width * $height * 0.97)) { return $null }   # already tight

    return New-Object System.Drawing.Rectangle($minX, $minY, ($maxX - $minX + 1), ($maxY - $minY + 1))
}

function Resize-LogoImage {
    <#
        Fits the logo inside a box of $Size without stretching it, on a transparent background, and
        saves it as PNG. Aspect ratio is kept because a squashed logo is worse than no logo.

        The empty margin around the artwork is trimmed off first (see Get-InkBounds), so the height
        a header gives the logo is filled by the mark instead of by the designer's whitespace.

        -Square pads the result out to a $Size x $Size canvas with the logo centred. That is what a
        favicon needs: most client logos are wide, and a 64x21 strip declared as a 64x64 icon is
        rendered inconsistently across browsers.

        -NoTrim keeps the artwork exactly as supplied - for a square app icon, whose margin is part
        of the design.
    #>
    param([string] $SourcePath, [string] $DestinationPath, [int] $Size, [switch] $Square, [switch] $NoTrim)

    $bytes  = [System.IO.File]::ReadAllBytes($SourcePath)
    $stream = New-Object System.IO.MemoryStream(, $bytes)
    try {
        $source = [System.Drawing.Image]::FromStream($stream)
        try {
            if (-not $NoTrim) {
                $bitmap = New-Object System.Drawing.Bitmap($source)
                try {
                    $ink = Get-InkBounds -Bitmap $bitmap
                    if ($ink) {
                        $cropped = $bitmap.Clone($ink, $bitmap.PixelFormat)
                        $source.Dispose()
                        $source = $cropped
                    }
                } finally { $bitmap.Dispose() }
            }
            $scale = [Math]::Min($Size / $source.Width, $Size / $source.Height)
            # Never scale a small logo up past its own size - that only adds blur.
            if ($scale -gt 1) { $scale = 1 }
            $width  = [Math]::Max(1, [int]($source.Width  * $scale))
            $height = [Math]::Max(1, [int]($source.Height * $scale))

            $canvasWidth  = if ($Square) { $Size } else { $width }
            $canvasHeight = if ($Square) { $Size } else { $height }
            $offsetX = [int](($canvasWidth  - $width)  / 2)
            $offsetY = [int](($canvasHeight - $height) / 2)

            $canvas = New-Object System.Drawing.Bitmap($canvasWidth, $canvasHeight)
            try {
                $graphics = [System.Drawing.Graphics]::FromImage($canvas)
                try {
                    $graphics.Clear([System.Drawing.Color]::Transparent)
                    $graphics.InterpolationMode  = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                    $graphics.SmoothingMode      = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
                    $graphics.PixelOffsetMode    = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                    $graphics.DrawImage($source, $offsetX, $offsetY, $width, $height)
                } finally { $graphics.Dispose() }

                $canvas.Save($DestinationPath, [System.Drawing.Imaging.ImageFormat]::Png)
            } finally { $canvas.Dispose() }
        } finally { $source.Dispose() }
    } finally { $stream.Dispose() }
}

# ---------------------------------------------------------------------------
# Colour: the palette comes out of the client's own logo
# ---------------------------------------------------------------------------
function ConvertTo-HslTriple {
    <#
        RGB 0-255 -> @(hue 0-1, saturation 0-1, lightness 0-1).

        The parameters are spelled out rather than $R/$G/$B because PowerShell variable names are
        case-insensitive: $r and $R are one variable, and since the parameter carries an [int]
        constraint, $r = $R / 255.0 would silently store 0.
    #>
    param([int] $Red, [int] $Green, [int] $Blue)

    $rf = $Red / 255.0
    $gf = $Green / 255.0
    $bf = $Blue / 255.0

    $max = [Math]::Max($rf, [Math]::Max($gf, $bf))
    $min = [Math]::Min($rf, [Math]::Min($gf, $bf))
    $lightness = ($max + $min) / 2.0

    if ($max -eq $min) { return @(0.0, 0.0, $lightness) }

    $delta = $max - $min

    [double] $saturation = 0.0
    if ($lightness -gt 0.5) { $saturation = $delta / (2.0 - $max - $min) }
    else                    { $saturation = $delta / ($max + $min) }

    [double] $hue = 0.0
    if ($max -eq $rf) {
        $hue = ($gf - $bf) / $delta
        if ($gf -lt $bf) { $hue = $hue + 6.0 }
    } elseif ($max -eq $gf) {
        $hue = (($bf - $rf) / $delta) + 2.0
    } else {
        $hue = (($rf - $gf) / $delta) + 4.0
    }
    $hue = $hue / 6.0

    return @($hue, $saturation, $lightness)
}

function ConvertFrom-HslTriple {
    <# hue/saturation/lightness (all 0-1) -> "#rrggbb". #>
    param([double] $Hue, [double] $Saturation, [double] $Lightness)

    if ($Saturation -le 0) {
        $grey = [int][Math]::Round($Lightness * 255)
        return ('#{0:x2}{1:x2}{2:x2}' -f $grey, $grey, $grey)
    }

    $upper = if ($Lightness -lt 0.5) { $Lightness * (1.0 + $Saturation) }
             else { $Lightness + $Saturation - ($Lightness * $Saturation) }
    $lower = (2.0 * $Lightness) - $upper

    $channel = {
        param([double] $Offset)
        $t = $Offset
        if ($t -lt 0) { $t += 1.0 }
        if ($t -gt 1) { $t -= 1.0 }
        if ($t -lt (1.0 / 6.0)) { return $lower + (($upper - $lower) * 6.0 * $t) }
        if ($t -lt 0.5)         { return $upper }
        if ($t -lt (2.0 / 3.0)) { return $lower + (($upper - $lower) * ((2.0 / 3.0) - $t) * 6.0) }
        return $lower
    }

    $red   = [int][Math]::Round((& $channel ($Hue + 1.0 / 3.0)) * 255)
    $green = [int][Math]::Round((& $channel $Hue) * 255)
    $blue  = [int][Math]::Round((& $channel ($Hue - 1.0 / 3.0)) * 255)
    return ('#{0:x2}{1:x2}{2:x2}' -f $red, $green, $blue)
}

function Get-LogoPalette {
    <#
        Reads the client's logo and returns the colours the panel should be painted in:

            Primary  - the brand colour (seeds the whole 50-950 ramp: buttons, links, active menu)
            Navbar   - the sidebar, same hue taken almost to black
            Appbar   - the header, one step lighter than the sidebar

        Why this exists: every client sends a logo, nobody sends a hex code, and a portal in the
        stock blue next to an orange logo is the first thing their customers notice. Picking the
        colour off the artwork means a non-technical installer gets a coherent portal without being
        asked a question they cannot answer.

        How: white, black, grey and near-grey pixels are ignored (a logo is mostly background), the
        rest are bucketed by hue, and the biggest bucket wins - that is the colour a person would
        point at. Its lightness is then normalised, because artwork colours are often too dark or
        too pale to use as a UI accent.

        Returns $null when the file is not a raster image or has no colour in it at all (a pure
        black-and-white logo), which leaves the caller on its default theme.
    #>
    param([Parameter(Mandatory = $true)] [string] $Path)

    if (-not (Test-Path $Path)) { return $null }
    if ((Get-LogoKind $Path) -ne 'raster') { return $null }

    $bytes  = [System.IO.File]::ReadAllBytes($Path)
    $stream = New-Object System.IO.MemoryStream(, $bytes)
    $buckets = @{}   # hue bucket -> @{ Count; H; S; L } running totals

    try {
        $source = [System.Drawing.Image]::FromStream($stream)
        try {
            # Sample a small copy: a 512px logo is a quarter of a million pixels, and the answer
            # does not change. Also flattens any oddities of the source pixel format.
            $scale  = [Math]::Min(1.0, 160.0 / [Math]::Max($source.Width, $source.Height))
            $width  = [Math]::Max(1, [int]($source.Width  * $scale))
            $height = [Math]::Max(1, [int]($source.Height * $scale))

            $small = New-Object System.Drawing.Bitmap($width, $height)
            try {
                $graphics = [System.Drawing.Graphics]::FromImage($small)
                try {
                    $graphics.Clear([System.Drawing.Color]::Transparent)
                    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                    $graphics.DrawImage($source, 0, 0, $width, $height)
                } finally { $graphics.Dispose() }

                for ($y = 0; $y -lt $height; $y++) {
                    for ($x = 0; $x -lt $width; $x++) {
                        $pixel = $small.GetPixel($x, $y)
                        if ($pixel.A -lt 200) { continue }          # transparent background

                        $hsl = ConvertTo-HslTriple -Red $pixel.R -Green $pixel.G -Blue $pixel.B
                        $h = $hsl[0]; $s = $hsl[1]; $l = $hsl[2]

                        # Grey, white and near-black carry no brand colour.
                        if ($s -lt 0.22) { continue }
                        if ($l -lt 0.10 -or $l -gt 0.92) { continue }

                        $key = [int][Math]::Floor($h * 24) % 24     # 15-degree buckets
                        if (-not $buckets.ContainsKey($key)) {
                            $buckets[$key] = @{ Count = 0; H = 0.0; S = 0.0; L = 0.0 }
                        }
                        $b = $buckets[$key]
                        $b.Count++
                        # Hue is circular; bucketing first means a plain average is safe inside one.
                        $b.H += $h; $b.S += $s; $b.L += $l
                    }
                }
            } finally { $small.Dispose() }
        } finally { $source.Dispose() }
    } catch {
        return $null
    } finally { $stream.Dispose() }

    if ($buckets.Count -eq 0) { return $null }

    $top = $buckets.GetEnumerator() | Sort-Object { $_.Value.Count } -Descending | Select-Object -First 1
    $winner = $top.Value
    $hue = $winner.H / $winner.Count
    $sat = $winner.S / $winner.Count
    $lig = $winner.L / $winner.Count

    # A UI accent needs to hold white text and stay legible on white. Artwork rarely lands there
    # by itself, so the hue is kept and the other two are pulled into a usable band.
    $accentSat = [Math]::Min(0.92, [Math]::Max(0.45, $sat))
    $accentLig = [Math]::Min(0.58, [Math]::Max(0.42, $lig))

    # The dark surfaces stay on the same hue - a navy sidebar under a navy logo reads as one
    # design, while the stock near-black next to a coloured logo reads as a template.
    $surfaceSat = [Math]::Min(0.55, [Math]::Max(0.25, $sat))

    return [pscustomobject]@{
        Primary = ConvertFrom-HslTriple -Hue $hue -Saturation $accentSat  -Lightness $accentLig
        # A lighter step of the same hue - the Android app uses it for links and highlights on
        # its dark splash, where the primary alone is too heavy.
        Accent  = ConvertFrom-HslTriple -Hue $hue -Saturation $accentSat  -Lightness ([Math]::Min(0.72, $accentLig + 0.16))
        Navbar  = ConvertFrom-HslTriple -Hue $hue -Saturation $surfaceSat -Lightness 0.11
        Appbar  = ConvertFrom-HslTriple -Hue $hue -Saturation $surfaceSat -Lightness 0.16
        Pixels  = $winner.Count
    }
}

function New-FaviconSvg {
    <#
        The panel's index.html links /favicon.svg, so the browser tab is branded by replacing that
        one file. A raster logo is wrapped in an SVG that carries the PNG inline as a data URI -
        valid SVG, renders everywhere, and avoids editing the built index.html.
    #>
    param([string] $PngPath, [string] $DestinationPath)

    $base64 = [Convert]::ToBase64String([System.IO.File]::ReadAllBytes($PngPath))
    $svg = @"
<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 0 64 64">
  <image xlink:href="data:image/png;base64,$base64" x="0" y="0" width="64" height="64"
         preserveAspectRatio="xMidYMid meet"/>
</svg>
"@
    [System.IO.File]::WriteAllText($DestinationPath, $svg, [System.Text.UTF8Encoding]::new($false))
}

function New-FaviconIco {
    <#
        Some browsers and most link-preview scrapers ask for /favicon.ico regardless of what the
        page declares. An .ico may contain a PNG as-is (Vista onwards), so this is a 22-byte header
        around the PNG rather than a real bitmap encoder.
    #>
    param([string] $PngPath, [string] $DestinationPath, [int] $Size = 32)

    $png = [System.IO.File]::ReadAllBytes($PngPath)
    $stream = [System.IO.File]::Create($DestinationPath)
    $writer = New-Object System.IO.BinaryWriter($stream)
    try {
        $writer.Write([uint16]0)              # reserved
        $writer.Write([uint16]1)              # type: icon
        $writer.Write([uint16]1)              # image count
        $writer.Write([byte]($Size % 256))    # width  (0 means 256)
        $writer.Write([byte]($Size % 256))    # height
        $writer.Write([byte]0)                # palette size
        $writer.Write([byte]0)                # reserved
        $writer.Write([uint16]1)              # colour planes
        $writer.Write([uint16]32)             # bits per pixel
        $writer.Write([uint32]$png.Length)    # image size
        $writer.Write([uint32]22)             # offset to the image data
        $writer.Write($png)
    } finally {
        $writer.Dispose()
        $stream.Dispose()
    }
}

# ---------------------------------------------------------------------------
# The Android app's runtime configuration
#
# The Android app is a portal shell: it reads app-config.json from the panel's root on every
# launch and takes its name, colours, support numbers, portal address and push keys from there.
# One APK per client is built once; everything else is this file.
#
# It has to be written per installation, because the file that ships inside the panel build is
# whatever the last client had in it. A client whose app-config.json still names someone else's
# portal gets an APK that opens someone else's panel - which is exactly what happened before this
# function existed.
# ---------------------------------------------------------------------------
function Write-AndroidAppConfig {
    param(
        [Parameter(Mandatory = $true)] [string] $UiFolder,
        [Parameter(Mandatory = $true)] [string] $BrandName,
        [Parameter(Mandatory = $true)] [string] $PortalUrl,
        [string] $SupportWhatsApp = '',
        [string] $SupportEmail = '',
        [string] $SupportPhone = '',
        [string] $ApkUrl = '',
        # Taken off the client's logo by Get-LogoPalette, so the phone app opens in the same
        # colours as their portal instead of the stock blue.
        [string] $ColorPrimary = '',
        [string] $ColorAccent = '',
        [string] $ColorSplash = '',
        # From the client's Firebase project (Project settings -> Your apps -> Android). Blank is
        # fine: the app still works, alerts just stay in the in-app bell instead of the phone tray.
        [string] $FcmProjectId = '',
        [string] $FcmAppId = '',
        [string] $FcmApiKey = '',
        [string] $FcmSenderId = ''
    )

    $path = Join-Path $UiFolder 'app-config.json'

    $portal = $PortalUrl.TrimEnd('/')

    # Start from what is there, so colours or support numbers someone tuned by hand survive - but
    # only if the file belongs to THIS portal. The panel build ships whichever client's file was
    # last committed, and inheriting another client's colours and support number is worse than
    # starting from the defaults.
    $config = [ordered]@{}
    if (Test-Path $path) {
        try {
            $existing = Get-Content $path -Raw | ConvertFrom-Json
            $existingPortal = ([string]$existing.portalUrl).TrimEnd('/')
            if ($existingPortal -and $existingPortal -ne $portal) {
                Write-Note "app-config.json belonged to $existingPortal - starting fresh for $portal"
            } else {
                foreach ($property in $existing.PSObject.Properties) { $config[$property.Name] = $property.Value }
            }
        } catch { }
    }
    $host_ = try { ([Uri]$portal).Host } catch { '' }

    $config['brandName'] = $BrandName
    $config['portalUrl'] = $portal
    if (-not $config.Contains('startPath')) { $config['startPath'] = '/login' }

    # Links to these hosts stay inside the app; anything else opens in the phone's browser.
    $internal = @()
    if ($host_) {
        $internal += $host_
        if ($host_ -like 'www.*')      { $internal += $host_.Substring(4) } else { $internal += "www.$host_" }
        if ($host_ -notlike 'api.*')   { $internal += "api.$host_" }
    }
    $config['internalHosts'] = @($internal | Select-Object -Unique)

    if ($ColorPrimary) { $config['colorPrimary'] = $ColorPrimary }
    if ($ColorAccent)  { $config['colorAccent']  = $ColorAccent }
    if ($ColorSplash)  { $config['colorSplash']  = $ColorSplash }

    foreach ($pair in @(
        @{ k = 'colorPrimary'; v = '#1D4ED8' }, @{ k = 'colorAccent'; v = '#0EA5E9' },
        @{ k = 'colorSplash';  v = '#0B1220' }, @{ k = 'pullToRefresh'; v = $false },
        @{ k = 'blockScreenshots'; v = $true }, @{ k = 'allowFileUpload'; v = $true },
        @{ k = 'allowDownloads'; v = $true },   @{ k = 'confirmExit'; v = $true },
        @{ k = 'minVersionCode'; v = 1 },       @{ k = 'forceUpdate'; v = $false },
        @{ k = 'maintenance'; v = $false })) {
        if (-not $config.Contains($pair.k)) { $config[$pair.k] = $pair.v }
    }

    $config['updateMessage']      = 'A new version is available with fixes and improvements.'
    $config['maintenanceMessage'] = 'We are upgrading the system. Please try again in a few minutes.'

    if ($SupportWhatsApp) { $config['supportWhatsApp'] = $SupportWhatsApp } elseif (-not $config.Contains('supportWhatsApp')) { $config['supportWhatsApp'] = '' }
    if ($SupportEmail)    { $config['supportEmail']    = $SupportEmail }    elseif (-not $config.Contains('supportEmail'))    { $config['supportEmail'] = '' }
    if ($SupportPhone)    { $config['supportPhone']    = $SupportPhone }    elseif (-not $config.Contains('supportPhone'))    { $config['supportPhone'] = '' }

    # Where the app sends someone when a newer version is required. The APK on this server by
    # default; a Play Store listing once there is one.
    $config['updateUrl'] = if ($ApkUrl) { $ApkUrl } elseif ($config.Contains('updateUrl') -and $config['updateUrl'] -notmatch 'ripay|finoconsole') { $config['updateUrl'] } else { "$portal/app/$(($BrandName -replace '[^A-Za-z0-9]','').ToLower()).apk" }

    $config['fcmProjectId'] = $FcmProjectId
    $config['fcmAppId']     = $FcmAppId
    $config['fcmApiKey']    = $FcmApiKey
    $config['fcmSenderId']  = $FcmSenderId

    # The sample's explanatory keys are noise in a real installation.
    foreach ($key in @('_comment', '_push')) { if ($config.Contains($key)) { $config.Remove($key) } }

    [System.IO.File]::WriteAllText($path, ($config | ConvertTo-Json -Depth 10), [System.Text.UTF8Encoding]::new($false))
    return $path
}

<#
    Writes the public "download the app" page at <portal>/app/.

    The Android app is handed out as an APK, not through the Play Store, so there has to be a page
    to send people to. It is deliberately one static file with no build step: it must keep working
    when the panel is rebuilt, and whoever publishes a new APK should only have to drop the file in
    beside it.

    If the APK is not there yet the page says so instead of offering a broken link.
#>
function Write-AppDownloadPage {
    param(
        [Parameter(Mandatory = $true)] [string] $UiFolder,
        [Parameter(Mandatory = $true)] [string] $BrandName,
        [Parameter(Mandatory = $true)] [string] $PortalUrl,
        [string] $LogoUrl = '',
        [string] $ApkFileName = '',
        # Folder in the release that holds the built APK (release\android). When it is there, the
        # app is published as part of the install.
        [string] $ApkSource = ''
    )

    $appFolder = Join-Path $UiFolder 'app'
    New-Item -ItemType Directory -Path $appFolder -Force | Out-Null

    if (-not $ApkFileName) { $ApkFileName = (($BrandName -replace '[^A-Za-z0-9]', '').ToLower()) + '.apk' }

    # If the release carries an APK, publish it now. The point is that finishing the installation
    # finishes the app as well - nobody should have to come back later and copy a file onto the
    # server for the download page to start working.
    if ($ApkSource -and (Test-Path $ApkSource)) {
        $incoming = Get-ChildItem $ApkSource -Filter '*.apk' -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($incoming) {
            # An APK has ONE client's portal address compiled into it (that is where it reads its
            # own settings from at every launch). Publishing another client's build here would give
            # their agents a link that opens somebody else's portal - so the file name, which the
            # release names after the client it was built for, has to agree with this brand.
            $expected = [System.IO.Path]::GetFileNameWithoutExtension($ApkFileName)
            $actual   = [System.IO.Path]::GetFileNameWithoutExtension($incoming.Name)

            if ($actual -ne $expected) {
                Write-Warn2 ("The release carries {0}, but this installation is '{1}'." -f $incoming.Name, $BrandName)
                Write-Warn2 'That APK opens the portal it was built for, not this one, so it has not been published.'
                Write-Note  ("Build this client's app:  .\Build-Release.ps1 -Version <v> -AndroidClient {0}" -f $expected)
            } else {
                Copy-Item $incoming.FullName (Join-Path $appFolder $ApkFileName) -Force
                Write-Ok ("Android app published: /app/{0} ({1} MB)" -f $ApkFileName, [math]::Round($incoming.Length / 1MB, 1))
            }
        }
    }
    $portal = $PortalUrl.TrimEnd('/')
    $safeBrand = $BrandName -replace '<', '' -replace '>', ''

    $html = @"
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1" />
<title>$safeBrand - Android app</title>
<link rel="icon" href="/favicon.svg" />
<style>
  :root { color-scheme: dark; }
  * { box-sizing: border-box; }
  body {
    margin: 0; min-height: 100vh; display: flex; align-items: center; justify-content: center;
    background: #0b1220; color: #e2e8f0; padding: 24px;
    font-family: 'Segoe UI', system-ui, -apple-system, Roboto, Arial, sans-serif;
  }
  .card {
    width: 100%; max-width: 440px; background: #111a2e; border: 1px solid #1e293b;
    border-radius: 18px; padding: 32px 28px; text-align: center;
  }
  .logo { max-width: 240px; max-height: 88px; margin: 0 auto 18px; display: block; }
  h1 { font-size: 21px; margin: 0 0 6px; color: #f8fafc; font-weight: 650; }
  p  { margin: 0 0 20px; color: #94a3b8; font-size: 14px; line-height: 1.6; }
  .btn {
    display: block; width: 100%; padding: 15px 20px; border-radius: 12px; border: 0;
    background: #2563eb; color: #fff; font-size: 16px; font-weight: 600;
    text-decoration: none; cursor: pointer; transition: background .15s;
  }
  .btn:hover { background: #1d4ed8; }
  .btn[aria-disabled="true"] { background: #1e293b; color: #64748b; cursor: not-allowed; }
  .meta { margin-top: 14px; font-size: 12.5px; color: #64748b; }
  .steps { margin: 22px 0 0; padding: 18px 18px 14px; background: #0d1526; border: 1px solid #1e293b;
           border-radius: 12px; text-align: left; }
  .steps h2 { font-size: 13px; text-transform: uppercase; letter-spacing: .06em; color: #94a3b8;
              margin: 0 0 10px; font-weight: 600; }
  .steps ol { margin: 0; padding-left: 20px; color: #cbd5e1; font-size: 13.5px; line-height: 1.75; }
  .back { display: inline-block; margin-top: 20px; color: #60a5fa; text-decoration: none; font-size: 13.5px; }
  .back:hover { text-decoration: underline; }
</style>
</head>
<body>
  <div class="card">
    $(if ($LogoUrl) { "<img class=""logo"" src=""$LogoUrl"" alt=""$safeBrand"" />" } else { "<h1 style=""font-size:26px"">$safeBrand</h1>" })
    <h1>Android app</h1>
    <p>Install the $safeBrand app on your phone to work without opening a browser.</p>

    <a class="btn" id="download" href="$ApkFileName" download>Download for Android</a>
    <div class="meta" id="meta">Checking&hellip;</div>

    <div class="steps">
      <h2>Installing it</h2>
      <ol>
        <li>Tap <strong>Download for Android</strong> above.</li>
        <li>Open the downloaded file.</li>
        <li>Android will ask to allow installing from this source - allow it.</li>
        <li>Tap <strong>Install</strong>, then open the app and sign in.</li>
      </ol>
    </div>

    <a class="back" href="$portal/">&larr; Back to $safeBrand</a>
  </div>

<script>
  // The page ships before the APK does. Rather than offering a link that 404s, ask the server
  // whether the file is there and say so plainly.
  (function () {
    var button = document.getElementById('download');
    var meta = document.getElementById('meta');
    fetch('$ApkFileName', { method: 'HEAD' })
      .then(function (response) {
        if (!response.ok) throw new Error('not published');
        var size = Number(response.headers.get('content-length') || 0);
        meta.textContent = size
          ? 'Android 7.0 and newer - ' + (size / 1048576).toFixed(1) + ' MB'
          : 'Android 7.0 and newer';
      })
      .catch(function () {
        button.setAttribute('aria-disabled', 'true');
        button.removeAttribute('href');
        button.textContent = 'Not published yet';
        meta.textContent = 'The app has not been uploaded to this server yet.';
      });
  })();
</script>
</body>
</html>
"@

    $path = Join-Path $appFolder 'index.html'
    [System.IO.File]::WriteAllText($path, $html, [System.Text.UTF8Encoding]::new($false))
    return $path
}

# ---------------------------------------------------------------------------
# Applying branding to an installation
# ---------------------------------------------------------------------------
function Set-FinoBranding {
    <#
        Writes the client's name and logo into an installed copy.

        Safe to run on a live installation: it only touches config.json and the branding files, and
        it preserves the transport keys already in config.json. Changes are visible on the next
        browser refresh - no restart, no rebuild.

        Returns a summary of what it wrote.
    #>
    param(
        [Parameter(Mandatory = $true)] [string] $InstallPath,
        [Parameter(Mandatory = $true)] [string] $BrandName,
        [string] $LogoPath,
        # The square artwork the phone app is built around (its launcher tile). Shown inside the
        # app in place of the wide logo, which has nowhere to go on a phone.
        [string] $AppIconPath,
        [string] $Tagline = 'Console',
        [string] $LoginHeadline,
        [string] $LoginSubtext,
        [string] $Theme = 'azure',
        # Only supplied by the installer, which has just generated them. A re-branding run leaves
        # whatever is already in config.json alone.
        [string] $ApiUrl,
        [string] $EncKey,
        [string] $EncIv,
        # The address operators type. Also what the Android app is pointed at, since in this
        # hosting the panel and the API are the same origin.
        [string] $PublicUrl,
        [string] $PortalUrl,
        # The release folder that may carry a built APK (releaseandroid).
        [string] $ApkSource
    )

    $appDir      = Join-Path $InstallPath 'app'
    $panelDir    = Join-Path $appDir 'wwwroot\panel'
    $brandingDir = Join-Path $appDir 'wwwroot\uploads\branding'
    $configPath  = Join-Path $panelDir 'config.json'

    if (-not (Test-Path $panelDir)) {
        throw "No operator console found at $panelDir - is $InstallPath really a Fino installation?"
    }
    New-Item -ItemType Directory -Path $brandingDir -Force | Out-Null

    # Start from whatever is already configured, so a re-brand cannot lose the AES keys.
    $config = [ordered]@{}
    if (Test-Path $configPath) {
        try {
            $existing = Get-Content $configPath -Raw | ConvertFrom-Json
            foreach ($property in $existing.PSObject.Properties) { $config[$property.Name] = $property.Value }
        } catch {
            Write-Warn2 'The existing config.json could not be read; writing a fresh one.'
        }
    }

    $logoUrl = if ($config.Contains('logoUrl')) { [string]$config['logoUrl'] } else { '' }
    # Set when a raster logo is written: the file the palette is sampled from.
    $paletteSource = $null

    if ($LogoPath) {
        if (-not (Test-Path $LogoPath)) { throw "Logo file not found: $LogoPath" }
        $kind = Get-LogoKind $LogoPath

        # The original is kept beside the generated files: when a client later asks for the logo
        # bigger, or on the receipt, the source is on the server rather than in an old email.
        $originalName = 'logo-original' + [System.IO.Path]::GetExtension($LogoPath).ToLower()
        Copy-Item $LogoPath (Join-Path $brandingDir $originalName) -Force

        if ($kind -eq 'vector') {
            Copy-Item $LogoPath (Join-Path $brandingDir 'logo.svg') -Force
            Copy-Item $LogoPath (Join-Path $panelDir 'favicon.svg') -Force
            $logoUrl = '/uploads/branding/logo.svg'
            Write-Ok 'Logo applied (SVG - stays sharp at any size)'
        } elseif ($kind -eq 'raster') {
            $logoPng    = Join-Path $brandingDir 'logo.png'
            $faviconPng = Join-Path $brandingDir 'favicon-64.png'

            Resize-LogoImage -SourcePath $LogoPath -DestinationPath $logoPng    -Size 512
            Resize-LogoImage -SourcePath $LogoPath -DestinationPath $faviconPng -Size 64 -Square

            New-FaviconSvg -PngPath $faviconPng -DestinationPath (Join-Path $panelDir 'favicon.svg')
            New-FaviconIco -PngPath $faviconPng -DestinationPath (Join-Path $panelDir 'favicon.ico')

            $logoUrl = '/uploads/branding/logo.png'
            $paletteSource = $logoPng
            Write-Ok 'Logo applied (panel, public site and browser tab)'
        } else {
            Show-LogoFormatHelp $LogoPath
            throw "Cannot use a $([System.IO.Path]::GetExtension($LogoPath)) file as a logo."
        }
    }

    # The square app artwork. Not trimmed and not squared off: its margin and rounded corners are
    # the design, and the same file is what the launcher icon is built from.
    $appIconUrl = if ($config.Contains('appIconUrl')) { [string]$config['appIconUrl'] } else { '' }
    if ($AppIconPath) {
        if (-not (Test-Path $AppIconPath)) { throw "App icon file not found: $AppIconPath" }
        if ((Get-LogoKind $AppIconPath) -eq 'vector') {
            Copy-Item $AppIconPath (Join-Path $brandingDir 'app-icon.svg') -Force
            $appIconUrl = '/uploads/branding/app-icon.svg'
        } else {
            Resize-LogoImage -SourcePath $AppIconPath -DestinationPath (Join-Path $brandingDir 'app-icon.png') -Size 512 -NoTrim
            $appIconUrl = '/uploads/branding/app-icon.png'
        }
        Write-Ok 'App icon applied (shown inside the phone app)'
    }

    $config['appName']  = $BrandName
    $config['tagline']  = $Tagline
    $config['logoUrl']  = $logoUrl
    $config['appIconUrl'] = $appIconUrl
    $config['theme']    = if ($config.Contains('theme') -and $config['theme']) { $config['theme'] } else { $Theme }
    if ($Theme) { $config['theme'] = $Theme }

    # Paint the portal in the client's own colours, read off the logo they sent. Only when a new
    # logo arrives: on a plain re-brand the operator may have tuned the colours since, and those
    # are theirs to keep.
    if ($paletteSource) {
        $palette = Get-LogoPalette -Path $paletteSource
        if ($palette) {
            $config['brandColor']  = $palette.Primary
            $config['navbarColor'] = $palette.Navbar
            $config['appbarColor'] = $palette.Appbar
            # Not read by the panel - carried here so the Android app gets the same accent.
            $config['accentColor'] = $palette.Accent
            Write-Ok "Portal colours taken from the logo (brand $($palette.Primary), sidebar $($palette.Navbar))"
        } else {
            Write-Note 'The logo has no strong colour in it - keeping the default palette.'
        }
    }

    $config['loginHeadline'] = if ($LoginHeadline) { $LoginHeadline } else { "The control room for your $BrandName network." }
    $config['loginSubtext']  = if ($LoginSubtext) { $LoginSubtext } else {
        'Recharges, money transfer, AEPS, wallets and settlements - one secure console for every role in your distribution chain.'
    }

    # Same origin as the panel, written out rather than left blank: a blank value falls back to
    # whatever was baked in at build time, and a build made in the wrong shell can bake in something
    # that is not a URL at all.
    if ($ApiUrl) { $config['apiUrl'] = $ApiUrl }
    elseif (-not $config.Contains('apiUrl') -or -not $config['apiUrl']) { $config['apiUrl'] = '/api/' }

    if ($EncKey) { $config['encKey'] = $EncKey }
    if ($EncIv)  { $config['encIv']  = $EncIv }

    # No BOM: the browser fetches this and parses it as JSON.
    [System.IO.File]::WriteAllText($configPath, ($config | ConvertTo-Json -Depth 10), [System.Text.UTF8Encoding]::new($false))

    # The Android app reads its own file from the same folder. Written here so a re-brand cannot
    # leave the phone app pointing at the previous client's portal.
    $appConfigPath = $null
    try {
        $portal = if ($PortalUrl) { $PortalUrl } else { '' }
        if (-not $portal) {
            # Combined hosting: the panel and the API are the same origin, and the installer knows
            # it as the public URL.
            $portal = $PublicUrl
        }
        if ($portal) {
            $appConfigPath = Write-AndroidAppConfig -UiFolder $panelDir -BrandName $BrandName -PortalUrl $portal `
                -ColorPrimary ([string]$config['brandColor']) -ColorAccent ([string]$config['accentColor']) `
                -ColorSplash ([string]$config['navbarColor'])
            Write-AppDownloadPage -UiFolder $panelDir -BrandName $BrandName -PortalUrl $portal -LogoUrl $logoUrl -ApkSource $ApkSource | Out-Null
        }
    } catch {
        Write-Warn2 "The Android app-config.json could not be written: $($_.Exception.Message)"
    }

    return [pscustomobject]@{
        BrandName     = $BrandName
        LogoUrl       = $logoUrl
        ConfigPath    = $configPath
        AppConfigPath = $appConfigPath
    }
}
