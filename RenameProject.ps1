#
# SETTINGS TO CHANGE
# Current values are examples.
# You must change ALL of them
# Company Official Name
$ASM_COMPANY="OSIsoft, LLC"

#Long product name - give a good description here
$ASM_PRODUCT="PI Data Replayer - Inserts oldest values into snapshot"

# Service name (short name)
$ASM_SERVICENAME="PIReplay"
# Service display name
$ASM_SERVICE_DISPLAY_NAME="PI Replay Service"
$ASM_SERVICE_DESCRIPTION="Inserts oldest values into snapshot to simulate a real time data flow."
# Current year
$YEAR="2016"

# New short name of the application (Short one word name) - used in Namespace and files naming 
# In one word, no space, no special character
$SHORT_NAME="PIReplay"

#
# END SETTINGS
#

# files excluded from the global search and replace
$excluded = @('.git','.nuget','.vs','RenameProject.ps1','*.nupkg','*.nuspec', '*.exe', '*.dll', '*.pdb', '*.resx')

$configFiles=get-childitem . -rec -exclude $excluded | Where-Object { !$_.PSIsContainer -and $_.fullname -notmatch 'package' }

# rename text inside files
foreach ($file in $configFiles)
{
    Write-Host $file.Name
    (Get-Content $file.PSPath) | 
    Foreach-Object {$_ -replace "NewApp", $SHORT_NAME -replace "%ASM_COMPANY%", $ASM_COMPANY -replace "%ASM_PRODUCT%", $ASM_PRODUCT -replace "%YEAR%", $YEAR -replace "%ASM_SERVICE_DISPLAY_NAME%",$ASM_SERVICE_DISPLAY_NAME -replace "%ASM_SERVICE_DESC%",$ASM_SERVICE_DESCRIPTION -replace "NewAppService",$ASM_SERVICENAME  } | 
    Set-Content $file.PSPath
    Rename-Item -Path $file.FullName -NewName ($file.Name -replace "NewApp",$SHORT_NAME)
}

# Rename file names:
Get-ChildItem . -rec -exclude $excluded | Where-Object { !$_.PSIsContainer -and $_.fullname -notmatch 'package'} | Where-Object {$_.Name -like "*NewApp*"} | %{Rename-Item $_ -NewName ($_.Name -replace "NewApp",$SHORT_NAME)}


Write-Host "Renaming Completed"