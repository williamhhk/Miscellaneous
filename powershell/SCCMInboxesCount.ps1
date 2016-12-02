
$server = "mypc"
$path = "C:\Program Files\Microsoft Configuration Manager\inboxes"
Invoke-Command -cn $server {
  param($path)
    Get-ChildItem  $path -recurse | Where {!$_.PSIsContainer} | Group Directory | Sort-Object Count -Descending |  Format-Table Name, Count -autosize
} -ArgumentList $path
