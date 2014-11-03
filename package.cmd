@echo off

if not exist obj md obj
cd obj
del *.* /q
cd ..

%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=Release35 /t:Clean,Rebuild
if %errorlevel% neq 0 exit /b 1
"%VS120COMNTOOLS%..\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" Mindbox.Expressions.Tests\bin\Release35\Mindbox.Expressions.Tests.dll /InIsolation
if %errorlevel% neq 0 exit /b 2

%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=Release40 /t:Rebuild
if %errorlevel% neq 0 exit /b 1
"%VS120COMNTOOLS%..\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" Mindbox.Expressions.Tests\bin\Release40\Mindbox.Expressions.Tests.dll /InIsolation
if %errorlevel% neq 0 exit /b 2

%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=Release45 /t:Rebuild
if %errorlevel% neq 0 exit /b 1
"%VS120COMNTOOLS%..\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" Mindbox.Expressions.Tests\bin\Release45\Mindbox.Expressions.Tests.dll /InIsolation
if %errorlevel% neq 0 exit /b 2

%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=ReleaseSL3 /t:Rebuild
if %errorlevel% neq 0 exit /b 1
StatLight.exe -x=".\Mindbox.Expressions.Tests\bin\ReleaseSL3\Mindbox.Expressions.Tests.xap"
if %errorlevel% neq 0 exit /b 2

%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=ReleaseSL4 /t:Rebuild
if %errorlevel% neq 0 exit /b 1
StatLight.exe -x=".\Mindbox.Expressions.Tests\bin\ReleaseSL4\Mindbox.Expressions.Tests.xap"
if %errorlevel% neq 0 exit /b 2

%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=ReleaseSL5 /t:Rebuild
if %errorlevel% neq 0 exit /b 1
StatLight.exe -x=".\Mindbox.Expressions.Tests\bin\ReleaseSL5\Mindbox.Expressions.Tests.xap"
if %errorlevel% neq 0 exit /b 2

%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=ReleaseWP71 /t:Rebuild
if %errorlevel% neq 0 exit /b 1

%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=ReleaseWP8 /p:Platform=x86 /t:Rebuild
if %errorlevel% neq 0 exit /b 1
"%VS120COMNTOOLS%..\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" Mindbox.Expressions.Tests\bin\ReleaseWP8\Mindbox.Expressions.Tests.xap /InIsolation
if %errorlevel% neq 0 exit /b 2

%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=ReleaseWPA81 /t:Rebuild
if %errorlevel% neq 0 exit /b 1
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=ReleaseCore45 /t:Rebuild
if %errorlevel% neq 0 exit /b 1
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=ReleasePortable36 /t:Rebuild
if %errorlevel% neq 0 exit /b 1
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=ReleasePortable88 /t:Rebuild
if %errorlevel% neq 0 exit /b 1
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /p:Configuration=ReleasePortable328 /t:Rebuild
if %errorlevel% neq 0 exit /b 1

cd obj
NuGet pack ..\Mindbox.Expressions\Mindbox.Expressions.csproj -Symbols
if %errorlevel% neq 0 exit /b 3
NuGet pack ..\Mindbox.Expressions\Mindbox.Expressions.csproj -Exclude **/*.pdb
if %errorlevel% neq 0 exit /b 4
