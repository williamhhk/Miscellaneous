$ComputerName = ""
if (Test-Connection -Quiet -Count 2 -ComputerName $ComputerName -ErrorAction SilentlyContinue)
{$Online = "Yes"}else{$Online = "No"}
if ($Online -eq "No")
{
    write-host "$ComputerName is not online!" 
    Exit
}
else 
{
    $s = New-PSSession -ComputerName $ComputerName
    Try {Invoke-Command -Session $s -ErrorAction Stop -ScriptBlock -ScriptBlock {Start-Process -FilePath "C:\windows\ccmsetup\ccmsetup.exe" -ArgumentList  "/uninstall"}} Catch {Exit}
    $ccmsetupStatus = $null 

    do
    {
        $ccmsetupStatus = Invoke-Command -Session $s -ScriptBlock {get-process | where-object {$_.ProcessName -eq "ccmsetup"}}
        Sleep 5
    } while ($ccmsetupStatus -ne $null)
    Write-Host "Checking ccmsetup.log"
    $status = Invoke-Command -Session $s -ScriptBlock {Get-Content "C:\windows\ccmsetup\logs\ccmsetup.log" -Tail 1}
    if ($status -match "\[[^\[\]]+\]")
    {
        Write-Host $Matches[0]
    }
}

if ($status.Contains("CcmSetup is exiting with return code 0"))
{
    Try {Invoke-Command -Session $s -ErrorAction SilentlyContinue -ScriptBlock {Remove-Item -Recurse -Force c:\windows\ccm\* }}  Catch {}  
    Try {Invoke-Command -Session $s -ErrorAction SilentlyContinue -ScriptBlock {Remove-Item -Recurse -Force c:\windows\ccmcache\* }} Catch {}      
    Try {Invoke-Command -Session $s -ErrorAction SilentlyContinue -ScriptBlock {Start-Process -FilePath "C:\windows\ccmsetup\ccmsetup.exe" -ArgumentList  ""}} Catch { Exit}
    $ccmsetupStatus = $null 
    do  
    {
        $ccmsetupStatus = Invoke-Command -Session $s -ScriptBlock {get-process | where-object {$_.ProcessName -eq "ccmsetup"}}
        Sleep 5
    } while ($ccmsetupStatus -ne $null)
    $status = Invoke-Command -Session $s -ScriptBlock {Get-Content "C:\windows\ccmsetup\logs\ccmsetup.log" -Tail 1}
    if ($status -match "\[[^\[\]]+\]")
    {
        Write-Host $Matches[0]
    }
}
