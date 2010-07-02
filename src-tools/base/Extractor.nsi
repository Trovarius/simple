Name "Simple.Net project"
OutFile "Simple.exe"
Icon "..\..\gfx\Icon.ico"
RequestExecutionLevel user
AutoCloseWindow true
BrandingText "juanplopes@gmail.com"

;Default installation folder
InstallDir "$EXEDIR\sample-project\"

Page directory
Page instfiles

Section
  SetOutPath $INSTDIR
  File /r /x _svn /x .svn /x bin /x build /x obj /x *.nsi /x *.suo /x *.cache /x *.log /x Simple.exe /x Simple3.dll /x TestResult.xml *.* 
  File /oname=lib\Simple3.dll lib\Simple3.dll 
  ExecWait $OUTDIR\Simple.Gui.exe
  Delete $OUTDIR\Simple.Gui.exe
SectionEnd