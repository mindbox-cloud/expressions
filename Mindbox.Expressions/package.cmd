@echo off
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=Release35 /t:Clean,Rebuild
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=Release40 /t:Rebuild
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=Release45 /t:Rebuild
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=ReleaseSL4 /t:Rebuild
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=ReleaseSL5 /t:Rebuild
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=ReleaseCore45 /t:Rebuild
NuGet pack -Symbols
NuGet pack -Exclude **/*.pdb
