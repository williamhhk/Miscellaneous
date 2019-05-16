$root = "D:\csharp"
$files = Get-ChildItem -Path $root -Recurse -Filter *.*
foreach($file in $files){
    $path = $file.DirectoryName
    $filename = $file.Name
    $filepath = "${path}\${filename}"
    Write-Host $filepath -ForegroundColor Green
    (Get-Content $filepath | Select-String -SimpleMatch -Pattern  "Logon")
}
