param([string]$SolutionDir,[string]$ProjectDir,[string]$TargetDir, [string]$TargetPath,[string]$ConfigurationName)
try{
	$targetVersion = [version][Reflection.Assembly]::LoadFile($TargetPath).GetName().Version
	Write-Host "Located version $($targetVersion.ToString(3))"

	$appveyorFile = "{0}/appveyor.yml" -f $SolutionDir
    $path = (Get-Item -Path $appveyorFile).FullName
	$finish = $false
    $pattern = '\bversion: (.*)'
    (Get-Content $path -ErrorAction SilentlyContinue) | ForEach-Object{
        if($_ -match $pattern -and $finish -eq $false){
            # We have found the matching line
			$oldVersion = [version]$matches[1]
			if ($oldVersion.Major -ne $targetVersion.Major -or $oldVersion.Minor -ne $targetVersion.Minor -or $oldVersion.Build -ne $targetVersion.Build -or $oldVersion.Revision -ne $targetVersion.Revision) {
				$newVersion = "{0}.{1}.{2}.{3}" -f $targetVersion.Major, $targetVersion.Minor, $targetVersion.Build, $targetVersion.Revision
				'version: {0}' -f $newVersion
				$finish = $true
				Write-Host "Patched appveyor version from $oldVersion to $newVersion!" -ForegroundColor Green
			}else{
				$finish = $true
				Write-Host "Appveyor version $oldVersion is up-to-date"
				$_
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