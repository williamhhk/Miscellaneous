Import-Module "C:\Program Files (x86)\Microsoft Configuration Manager\AdminConsole\bin\ConfigurationManager.psd1"


function BuildStepVarListXml
{
param($referenceApps)
    $modelNames =  $referenceApps.ModelName -join ','
    $count = $referenceApps.Length
    $applicationData = ""

    $index = 0;
    foreach ($app in $referenceApps)
    {
        $mn = $app.ModelName
        $displayname = $app.DisplayName
        $applicationData +=  "<variable name=`"OSDApp"+$index+"Description`" property=`"AppInfo"+$index+"Description`">Valid</variable>
                <variable name=`"OSDApp"+$index+"DisplayName`" property=`"AppInfo"+$index+"DisplayName`">$displayname</variable>
                <variable name=`"OSDApp"+$index+"Name`" property=`"AppInfo"+$index+"Name`">$mn</variable>"
        $index++;
    }
    $stepXml =  
            "<step type=`"SMS_TaskSequence_InstallApplicationAction`" name=`"Install`" Application`" description=`"`" runIn=`"FullOS`" successCodeList=`"0`" retryCount=`"0`" runFromNet=`"false`">
            <action>smsappinstall.exe /app:$modelNames /basevar: /continueOnError:False</action>
            <defaultVarList>
                <variable name=`"OSDApp`" property=`"AppInfo`" hidden=`"true`">$count</variable>
                $applicationData
                <variable name=`"ApplicationName`" property=`"ApplicationName`" hidden=`"true`">$modelNames</variable>
                <variable name=`"ContinueOnInstallError`" property=`"ContinueOnInstallError`" hidden=`"true`">false</variable>
                <variable name=`"OSDAppCount`" property=`"NumApps`">$count</variable>
                <variable name=`"RetryCount`" property=`"RetryCount`" hidden=`"true`">0</variable>
            </defaultVarList>
        </step>"

    return $stepXml
}

#Test
$referenceApps = New-Object System.Collections.ArrayList
$myObject = New-Object System.Object
$myObject | Add-Member -type NoteProperty -name ModelName -Value "12345"
$myObject | Add-Member -type NoteProperty -name DisplayName -Value "Hello World"

# or use above create xml file.
$SequenceFile = Get-Content "q:\object.xml"
$SequenceFile
#$Sequence = ([WmiClass]"\\server\ROOT\SMS\site_xx:SMS_TaskSequencePackage").ImportSequence($SequenceFile).TaskSequence
$Sequence = ([WmiClass]"\\server\ROOT\SMS\site_xx:SMS_TaskSequencePackage").ImportSequence($xml).TaskSequence


# create the new Task Sequence Package object
$EmptySequence = ([WmiClass]"\\server\ROOT\SMS\site_xx:SMS_TaskSequencePackage").CreateInstance()
$EmptySequence.Name = "Demo"
$EmptySequence.Description = "New Task Sequence"
$EmptySequence.Category = "OSD"
$EmptySequence.PackageID = "PackageID"

([WmiClass]"\\server\ROOT\SMS\site_xx:SMS_TaskSequencePackage").SetSequence($EmptySequence,$Sequence)


#Move Folder 

$SiteCode = "siteid"
$CMProvider = "testserver"
[int]$ObjectID = 20
[string]$TaskSequence = ""
$TargetFolderName = "Insight off domain process"
$TaskSequence = (Get-WmiObject -Class SMS_TaskSequencePackage -Namespace root\sms\site_$SiteCode -Filter "Name = 'Demo'" -ComputerName $CMProvider).PackageID
[int]$SourceFolder = (Get-WmiObject -Class SMS_ObjectContainerItem -Namespace root\sms\site_$SiteCode -Filter "ObjectType = 20 and InstanceKey = '$($TaskSequence)'" -ComputerName $CMProvider).ContainerNodeID
[int]$TargetFolder = (Get-WmiObject -Class SMS_ObjectContainerNode -Namespace root\sms\site_$SiteCode -Filter "ObjectType = '20' and Name = '$($TargetFolderName)'" -ComputerName $CMProvider).ContainerNodeID
$Parameters = ([wmiclass]"\\$($CMProvider)\root\SMS\Site_$($SiteCode):SMS_ObjectContainerItem").psbase.GetMethodParameters("MoveMembers")

$Parameters.ObjectType = $ObjectID
$Parameters.ContainerNodeID = $SourceFolder
$Parameters.TargetContainerNodeID = $TargetFolder
$Parameters.InstanceKeys = $TaskSequence
try {
    $Output = ([wmiclass]"\\$($CMProvider)\root\SMS\Site_$($SiteCode):SMS_ObjectContainerItem").psbase.InvokeMethod("MoveMembers",$Parameters,$null)
    if ($Output.ReturnValue -eq "0")
    {
    Write-Host "Task Sequence $($TaskSequence) successfully moved to Folder $($TargetFolderName)."
    }
}
catch [Exception]
{
    Write-Error -Message "Something went wrong."
}
