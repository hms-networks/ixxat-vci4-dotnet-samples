Param (
)

#
#  Usage:
#  
#  .\getpackages.ps1
#
#  This script is needed because VS2010 does no support a NuGet Package Manager
#  Downloads nuget client and nuget package and unpacks it in a way it is usable for the .Net 3.5 projects.
#  If you specify the AssemblyKeyFile parameter the given key is used to crete a strong named assembly.
#  In this case the resulting package is named Ixxat.Vci4.StrongName.<version>.nupkg.
#  To use the first snk file use
#
#  .\build.ps1 -Version 4.1.0 -AssemblyKeyFile $(Get-ChildItem -Filter *.snk | Select-Object -First 1)
#

$ErrorActionPreference = "Stop"

# locate vcsbuild
$VcBuildHome = "C:\Program Files (x86)\Microsoft Visual Studio 9.0\VC" # | Where-Object { Test-Path "$_\bin\amd64\vcbuild.exe" } | Select-Object -First 1

if (!$VcBuildHome)
{
	throw "Failed to locate vsbuild home directory"
}

$VcBuild = "$VcBuildHome\bin\amd64\vcbuild.exe"

if ($VcBuild -ne "")
{
}
else
{
    throw "Failed to locate vcbuild $VcBuild"
}

# locate download nuget client
$NugetLocalDir=".\downloads"
$NugetFileName="nuget.exe"
$NugetLocalPath=Join-Path -path $NugetLocalDir -childPath $NugetFileName
$NugetCliUrl="https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
if (!(Test-Path $NugetLocalPath)) {
    if (!(Test-Path $NugetLocalDir)) {
        New-Item -path . -name $NugetLocalDir -type directory
    }

    Invoke-WebRequest -Uri $NugetCliUrl -OutFile $NugetLocalPath
}

$PackageName = "Ixxat.Vci4.net35"
$PackageVersion = "4.1.6"

# download package via nuget
& $NugetLocalPath install $PackageName -Version $PackageVersion  -Source nuget.org -OutputDirectory "packages\$PackageName"

# copy native DLLs to the projects output directories
$OutputDirs = @( 
    "CanConNet\bin\Debug", "CanConNet\bin\Release",
	"LinConNet\bin\Debug", "LinConNet\bin\Release"
)

foreach($d in $OutputDirs) {
	New-Item -ItemType Directory -Path "$d\vcinet" -Force
	Copy-Item "packages\$PackageName\$PackageName.$PackageVersion\build\net35\x86" -Destination "$d\vcinet" -Recurse -Force
	Copy-Item "packages\$PackageName\$PackageName.$PackageVersion\build\net35\x64" -Destination "$d\vcinet" -Recurse -Force
}
