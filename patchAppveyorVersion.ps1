param([string]$SolutionDir,[string]$ProjectDir,[string]$TargetDir, [string]$TargetPath,[string]$ConfigurationName)
try{
	$targetVersion = [Reflection.Assembly]::LoadFile($TargetPath).GetName().Version
	Write-Host "Located $targetVersion"

	$appveyorFile = "{0}/appveyor.yml" -f $SolutionDir
    $path = (Get-Item -Path $appveyorFile).FullName
    $pattern = 'version: (.*)'
    (Get-Content $path) | ForEach-Object{
        if($_ -match $pattern){
            # We have found the matching line
            $newVersion = "{0}.{1}.{2}" -f $targetVersion.Major, $targetVersion.Minor, $targetVersion.Build
            'version: {0}' -f $newVersion
			Write-Host "Patched appveyor version!" -ForegroundColor Green
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