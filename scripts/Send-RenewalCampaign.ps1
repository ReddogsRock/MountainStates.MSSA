param(
    [Parameter(Mandatory = $true)]
    [string]$ApiKey,

    [string]$CsvPath = "expiring-members.csv",
    [string]$Domain = "mail.mssadogs.com",
    [string]$BaseUrl = "https://api.mailgun.net/v3",
    [string]$FromAddress = "Mountain States Stockdog Association <events@mail.mssadogs.com>",
    [string]$Subject = "Don't Miss Out on 2027 - Renew Your MSSA Membership",

    # Safety default - prints what WOULD be sent without actually calling Mailgun.
    # Pass -ReallySend to actually fire the campaign.
    [switch]$ReallySend
)

$ErrorActionPreference = "Stop"

# The dash is written as the &mdash; entity, not a literal "—" character - a literal
# typographic dash is multi-byte UTF-8, and if anything downstream (an email client,
# a relay, Mailgun itself) ever guesses the wrong charset for this HTML, a literal
# dash silently turns into mojibake ("â€¦"-style garbage). An HTML entity is plain
# ASCII, so there's no encoding step left that can corrupt it. Same reasoning extends
# the wrapper to a real <head><meta charset="utf-8"> document instead of a bare
# fragment, so nothing has to guess the encoding in the first place.
$htmlBody = @"
<!DOCTYPE html>
<html>
<head>
<meta charset="UTF-8">
</head>
<body>
<div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333;">
  <img src="https://mssadogs.com/files/id/2" alt="Mountain States Stockdog Association" style="max-width: 400px; margin-bottom: 16px;">

<p>Hi %recipient.FirstName%,</p>

<p>Your MSSA membership is set to expire at the end of 2026. Don't miss out on 2027 &mdash; renew now to keep your membership active without interruption.</p>

<p><a href="%recipient.RenewalLink%" style="background: #6b4226; color: #fff; padding: 10px 16px; text-decoration: none; border-radius: 4px;">Renew Now</a></p>

<p>Thanks for being part of the Mountain States Stockdog Association!</p>

  <hr style="margin-top: 24px; border: none; border-top: 1px solid #ddd;">
  <p><a href="https://mssadogs.com">Visit mssadogs.com</a></p>
  <p style="font-size: 12px; color: #888;">
    <a href="%unsubscribe_url%" style="color: #888;">Unsubscribe from MSSA</a>
  </p>
</div>
</body>
</html>
"@

$allRows = @(Import-Csv -Path $CsvPath)

$hasBothFields = @($allRows | Where-Object { $_.Email -and $_.RenewalLink })
$missingFieldCount = $allRows.Count - $hasBothFields.Count

# Mailgun validates every address in "to" before accepting the request - one
# malformed email (e.g. missing the @domain part) fails the WHOLE batch, not just
# that recipient. Catching it here means one bad row gets reported and skipped
# instead of silently blocking everyone else in the batch.
$emailPattern = '^[^@\s]+@[^@\s]+\.[^@\s]+$'
$malformed = @($hasBothFields | Where-Object { $_.Email -notmatch $emailPattern })
$rows = @($hasBothFields | Where-Object { $_.Email -match $emailPattern })

Write-Host "Loaded $($allRows.Count) rows - $missingFieldCount skipped (missing email or renewal link)."
if ($malformed.Count -gt 0) {
    Write-Host "$($malformed.Count) skipped for a malformed email address - fix these on file and re-run separately if you want them included:"
    $malformed | ForEach-Object { Write-Host "  $($_.FirstName) $($_.LastName): '$($_.Email)'" }
}
Write-Host "$($rows.Count) will be emailed."

if (-not $ReallySend) {
    Write-Host ""
    Write-Host "DRY RUN - nothing was sent. Re-run with -ReallySend to actually email these $($rows.Count) people."
    Write-Host "Sample recipient:"
    $rows | Select-Object -First 1 | Format-List
    return
}

$credentials = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("api:$ApiKey"))
$headers = @{ Authorization = "Basic $credentials" }
$requestUrl = "$BaseUrl/$Domain/messages"

$batchSize = 1000
for ($i = 0; $i -lt $rows.Count; $i += $batchSize) {
    $end = [Math]::Min($i + $batchSize - 1, $rows.Count - 1)
    $batch = $rows[$i..$end]

    $to = ($batch | ForEach-Object { $_.Email }) -join ","

    $recipientVariables = @{}
    foreach ($r in $batch) {
        $recipientVariables[$r.Email] = @{ FirstName = $r.FirstName; RenewalLink = $r.RenewalLink }
    }
    $recipientVariablesJson = $recipientVariables | ConvertTo-Json -Depth 3 -Compress

    $body = @{
        from                    = $FromAddress
        to                      = $to
        subject                 = $Subject
        html                    = $htmlBody
        "recipient-variables"   = $recipientVariablesJson
        "o:tracking-clicks"     = "no"
    }

    # Explicit charset here, rather than letting Invoke-RestMethod pick a default for
    # the hashtable body - the actual bytes it sends are already correct UTF-8 (verified
    # separately), but there's no reason to leave Mailgun's end guessing too.
    $response = Invoke-RestMethod -Uri $requestUrl -Method Post -Headers $headers -Body $body -ContentType "application/x-www-form-urlencoded; charset=utf-8"
    Write-Host "Batch of $($batch.Count) sent: $($response.message)"
}
