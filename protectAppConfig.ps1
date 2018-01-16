param([string]$SolutionDir,[string]$ProjectDir,[string]$TargetDir, [string]$TargetPath,[string]$ConfigurationName)
try{
	$config = [System.Configuration.ConfigurationManager]::OpenExeConfiguration($TargetPath)
	foreach($section in $config.Sections){
		try{
			if ($section.SectionInformation.IsLocked -eq $false -and $section.SectionInformation.IsProtected -eq $false){
				$section.SectionInformation.ProtectSection("ProtectionProvider")
				Write-Host "Protected $($section.SectionInformation.Name)!" -ForegroundColor Green
			}
		}catch{
		}
	}
	foreach($group in $config.SectionGroups){
		$sections = $group.Sections
		Foreach ($section in $sections){
			try{
				if ($section.SectionInformation.IsLocked -eq $false -and $section.SectionInformation.IsProtected -eq $false){
					$section.SectionInformation.ProtectSection("ProtectionProvider")
					Write-Host "Protected $($section.SectionInformation.Name)!" -ForegroundColor Green
				}
			}catch{
			}
		}
	}
	$config.Save()
	Write-Host "Protected $TargetPath app config file!" -ForegroundColor Green
	# now copy config to project dir
	$appConfigFile = "$([System.IO.Path]::GetFileName($TargetPath)).config"
	$appConfigPath = [System.IO.Path]::Combine($TargetDir,$appConfigFile)
	$projectAppConfigPath = [System.IO.Path]::Combine($ProjectDir,"App.config")
	Write-Host "Copying from $appConfigPath to $projectAppConfigPath"
	if ([System.IO.File]::Exists($appConfigPath) -and [System.IO.File]::Exists($projectAppConfigPath)){
		[System.IO.File]::Copy($appConfigPath,$projectAppConfigPath, $true)
		Write-Host "Copied!" -ForegroundColor Green
	}
}
catch {
	If ($ConfigurationName -eq "Debug"){
		Write-Host $_.Exception -ForegroundColor Red
	}
    exit 321
}