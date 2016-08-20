param([String]$key,[String]$version)

function setProjectVersion([String]$fileName, [String]$version) {
    $content = (Get-Content $fileName) -join "`n" | ConvertFrom-Json
    $content.version = $version
    $newContent = ConvertTo-Json -Depth 10 $content
    Set-Content $fileName $newContent
}

if ($version -ne "") {
	setProjectVersion ".\src\Folke.Elm\project.json" $version
	setProjectVersion ".\src\Folke.Elm.Mysql\project.json" $version
	setProjectVersion ".\src\Folke.Elm.Sqlite\project.json" $version
	setProjectVersion ".\src\Folke.Elm.MicrosoftSqlServer\project.json" $version
	setProjectVersion ".\src\Folke.Elm.PostgreSql\project.json" $version

	& dotnet restore

	cd .\src\Folke.Elm
	& dotnet pack -c Release
	$file = Get-Item "bin\Release\*.$version.nupkg"
	nuget push $file.FullName $key -Source https://api.nuget.org/v3/index.json
	cd ..\..

	cd .\src\Folke.Elm.Mysql
	& dotnet pack -c Release
	$file = Get-Item "bin\Release\*.$version.nupkg"
	nuget push $file.FullName $key -Source https://api.nuget.org/v3/index.json
	cd ..\..

	cd .\src\Folke.Elm.Sqlite
	& dotnet pack -c Release
	$file = Get-Item "bin\Release\*.$version.nupkg"
	nuget push $file.FullName $key -Source https://api.nuget.org/v3/index.json
	cd ..\..

	cd .\src\Folke.Elm.MicrosoftSqlServer
	& dotnet pack -c Release
	$file = Get-Item "bin\Release\*.$version.nupkg"
	nuget push $file.FullName $key -Source https://api.nuget.org/v3/index.json
	cd ..\..

	cd .\src\Folke.Elm.PostgreSql
	& dotnet pack -c Release
	$file = Get-Item "bin\Release\*.$version.nupkg"
	nuget push $file.FullName $key -Source https://api.nuget.org/v3/index.json
	cd ..\..
}
