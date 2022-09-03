$PSScriptPathDir = Split-Path $MyInvocation.MyCommand.Path -Parent
Set-Location $PSScriptPathDir

function FindFiles($findPath) {
	$desiredPath = Join-Path $PSScriptRoot $findPath
	$foundCsFiles = Get-ChildItem $desiredPath -Filter *.cs -Recurse
	
	foreach($file in $foundCsFiles) {
		$pathToInclude = $file | Resolve-Path -Relative
		$pathRelative = $pathToInclude.Substring($findPath.Length + 1);
		
		if (!$pathRelative.StartsWith("bin") -and !$pathRelative.StartsWith("obj")) {
			$outputString = "<Compile Include=`"$pathToInclude`" Link=`"$pathRelative`" />"
			Write-Host $outputString
		}
	}
}

FindFiles "..\DeveMazeGeneratorCore"
FindFiles "..\DeveMazeGeneratorCore.MonoGame.Core"
