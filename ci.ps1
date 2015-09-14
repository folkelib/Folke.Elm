$span = New-TimeSpan -Start 2010/1/1 -End (Get-Date)
$version = "beta-" + $span.Days.ToString() + ($span.Hours * 60 + $span.Minutes).ToString("0000")
$env:DNX_BUILD_VERSION = $version
& dnu restore
cd .\src\Folke.Orm
& dnu pack
$file = Get-Item "bin\Debug\*-$version.nupkg"
nuget push $file.FullName 4429cea5-062d-4e73-8b1b-7a593d988f06
cd ..\..
cd .\src\Folke.Orm.Mysql
& dnu pack
$file = Get-Item "bin\Debug\*-$version.nupkg"
nuget push $file.FullName 4429cea5-062d-4e73-8b1b-7a593d988f06
cd ..\..