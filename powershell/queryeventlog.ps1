$server = "anypc"
Get-EventLog  -cn $server -Newest 50 -LogName "Application"

$Events = Get-Eventlog -LogName system -Newest 1000
$Events | Group-Object -Property source -noelement | Sort-Object -Property count -Descending

$logs = Get-EventLog -cn $server -LogName Application
$logs | Where-Object {$_.Message -like '*IIS*'}
