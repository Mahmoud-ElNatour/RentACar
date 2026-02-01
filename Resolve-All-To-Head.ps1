
$files = Get-ChildItem -Path "c:\Users\Mohammad\Downloads\Mahmoud\RentACar" -Recurse -Filter "*.cs"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    if ($content.Contains("<<<<<<< HEAD")) {
        Write-Host "Fixing conflicts in $($file.FullName)"
        
        # Regex to capture HEAD content and discard V3 content
        # Matches:
        # <<<<<<< HEAD
        # [HEAD Content]
        # =======
        # [V3 Content]
        # >>>>>>> Mahmoud-V3
        
        # We want to keep [HEAD Content].
        
        $pattern = "(?ms)^\s*<<<<<<< HEAD\r?\n(?<content>.*?)\r?\n\s*=======\r?\n.*?\r?\n\s*>>>>>>> .*?\r?\n"
        
        while ($content -match $pattern) {
            # Replace with captured HEAD content (group 'content', which is $1 or ${content})
            # Note: logic needed to handle the replace properly in loop
            $content = [regex]::Replace($content, $pattern, '${content}')
        }
        
        # Fallback for end of file or weird spacing
        # Just simple block replacement
        $pattern2 = "(?ms)\s*<<<<<<< HEAD\r?\n(.*?)\r?\n\s*=======\r?\n.*?\r?\n\s*>>>>>>> .*?$"
        while ($content -match $pattern2) {
            $content = [regex]::Replace($content, $pattern2, '$1')
        }
        
        # Clean up any leftover Start/End markers if format didn't match perfectly (e.g. inline)
        # Be careful here.
        
        Set-Content -Path $file.FullName -Value $content -NoNewline
    }
}
Write-Host "Bulk Resolution Complete"
