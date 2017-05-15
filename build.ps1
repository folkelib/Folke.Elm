param([String]$key,[String]$version)

if ($version -ne "") {
	nuget install Folke.Build -ExcludeVersion
	& .\Folke.Build\tools\build.ps1 $key $version
}
