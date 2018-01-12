param([string]$SolutionDir,[string]$ProjectDir,[string]$TargetDir, [string]$TargetPath,[string]$ConfigurationName)
try{
	$targetVersion = [Reflection.Assembly]::LoadFile($TargetPath).GetName().Version
	Write-Host "Located version $targetVersion"

	$appveyorFile = "{0}/appveyor.yml" -f $SolutionDir
    $path = (Get-Item -Path $appveyorFile).FullName
	$versionFound = $false
    $pattern = 'version: (.*)'
    (Get-Content $path) | ForEach-Object{
        if($_ -match $pattern -and $versionFound -eq $false){
            # We have found the matching line
			$oldVersion = [version]$matches[1]
			if ($oldVersion -ne $targetVersion) {
				$newVersion = "{0}.{1}.{2}" -f $targetVersion.Major, $targetVersion.Minor, $targetVersion.Build
				'version: {0}' -f $newVersion
				$versionFound = $true
				Write-Host "Patched appveyor version from $oldVersion to $newVersion!" -ForegroundColor Green
			}else{
				Write-Host "Appveyor version $oldVersion is up-to-date"
			}
        } else {
            # Output line as is
            $_
        }
    } | Set-Content $path
}
catch {
	If ($ConfigurationName -eq "Debug"){
		Write-Host $_.Exception -ForegroundColor Red
	}
    exit 321
}