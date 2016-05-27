param([String]$key)

$span = New-TimeSpan -Start 2010/1/1 -End (Get-Date)
$version = "beta-" + $span.Days.ToString() + ($span.Hours * 60 + $span.Minutes).ToString("0000")
& dotnet restore
cd .\src\Folke.Elm
& dotnet pack --version-suffix $version
$file = Get-Item "bin\Debug\*-$version.nupkg"
nuget push $file.FullName $key 
cd ..\..

cd .\src\Folke.Elm.Mysql
& dotnet pack --version-suffix $version
$file = Get-Item "bin\Debug\*-$version.nupkg"
nuget push $file.FullName $key 
cd ..\..

cd .\src\Folke.Elm.Sqlite
& dotnet pack --version-suffix $version
$file = Get-Item "bin\Debug\*-$version.nupkg"
nuget push $file.FullName $key
cd ..\..

cd .\src\Folke.Elm.MicrosoftSqlServer
& dotnet pack --version-suffix $version
$file = Get-Item "bin\Debug\*-$version.nupkg"
nuget push $file.FullName $key
cd ..\..

cd .\src\Folke.Elm.PostgreSql
& dotnet pack --version-suffix $version
$file = Get-Item "bin\Debug\*-$version.nupkg"
nuget push $file.FullName $key
cd ..\..
