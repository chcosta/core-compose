@if "%_echo%" neq "on" echo off
setlocal EnableDelayedExpansion

:: Note: We've disabled node reuse because it causes file locking issues.
::       The issue is that we extend the build with our own targets which
::       means that that rebuilding cannot successfully delete the task
::       assembly.

:ReadArguments
:: Read in the args to determine whether to run the native build, managed build, or both (default)
set "__args= %*"
set processedArgs=
set unprocessedBuildArgs=

:Loop
if [%1]==[] goto Tools

if [!processedArgs!]==[] (
  call set unprocessedBuildArgs=!__args!
) else (
  call set unprocessedBuildArgs=%%__args:*!processedArgs!=%%
)

:Next
shift /1
goto Loop

:Tools
:: Setup VS
if not defined VisualStudioVersion (
    if defined VS140COMNTOOLS (
        call "%VS140COMNTOOLS%\VsDevCmd.bat"
        goto :Build
    )

    echo Error: build.cmd requires Visual Studio 2015.
    echo        Please see https://github.com/dotnet/corefx/blob/master/Documentation/project-docs/developer-guide.md for build instructions.
    exit /b 1
)

:Build
:: Restore the Tools directory
call %~dp0init-tools.cmd

:: Clear the 'Platform' env variable for this session,
:: as it's a per-project setting within the build, and
:: misleading value (such as 'MCD' in HP PCs) may lead
:: to build breakage (issue: #69).
set Platform=

:: Log build command line
set _buildproj=%~dp0build.proj
set _buildlog=%~dp0msbuild.log
set _buildprefix=echo
set _buildpostfix=^> "%_buildlog%"
call :build %__args%

:: Build
set _buildprefix=
set _buildpostfix=
echo [%time%] Building Managed Libraries...
call :build %__args%

goto :AfterBuild

:build
%_buildprefix% msbuild "%_buildproj%" /nologo /maxcpucount /v:minimal /clp:Summary /p:OSGroup=Windows_NT /nodeReuse:false /flp:v=normal;LogFile="%_buildlog%";Append /flp2:warningsonly;logfile=%~dp0msbuild.wrn /flp3:errorsonly;logfile=%~dp0msbuild.err !unprocessedBuildArgs! %_buildpostfix%
set BUILDERRORLEVEL=%ERRORLEVEL%
goto :eof

:AfterBuild

echo.
:: Pull the build summary from the log file
findstr /ir /c:".*Warning(s)" /c:".*Error(s)" /c:"Time Elapsed.*" "%_buildlog%"
echo [%time%] Build Exit Code = %BUILDERRORLEVEL%

exit /b %BUILDERRORLEVEL%
