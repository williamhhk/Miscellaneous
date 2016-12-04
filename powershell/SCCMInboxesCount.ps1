
$server = "mypc"
$path = "C:\Program Files\Microsoft Configuration Manager\inboxes"
Invoke-Command -cn $server {
  param($path)
 #  (gci c:\ -r | ? {$_.PSIsContainer -eq $True}) | ? {$_.GetFiles().Count -eq 0} | Group Directory | Sort-Object Count -Descending |  Format-Table Name, Count -autosize 
    Get-ChildItem  $path -recurse | Where {!$_.PSIsContainer} | Group Directory | Sort-Object Count -Descending |  Format-Table Name, Count -autosize
} -ArgumentList $path
