@echo off
setlocal 

call "%VS100COMNTOOLS%\vsvars32.bat"

pushd ..\bplist
 start /wait "vc" VCExpress.exe bplist.sln /build "Debug DLL"
 start /wait "vc" VCExpress.exe bplist.sln /build "Release DLL"
popd

if "%~1" == "d" (
  xcopy /y/d "..\bplist\Debug DLL\bplist.dll" bin\debug
  xcopy /y/d "..\bplist\Debug DLL\bplist.dll" bin\release
  xcopy /y/d "..\bplist\Debug DLL\bplist.pdb" bin\debug
  xcopy /y/d "..\bplist\Debug DLL\bplist.pdb" bin\release
) else (
  xcopy /y/d "..\bplist\Release DLL\bplist.dll" bin\debug
  xcopy /y/d "..\bplist\Release DLL\bplist.dll" bin\release
  xcopy /y/d "..\bplist\Release DLL\bplist.pdb" bin\debug
  xcopy /y/d "..\bplist\Release DLL\bplist.pdb" bin\release
)

endlocal
