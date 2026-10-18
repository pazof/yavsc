Unicode true
RequestExecutionLevel user
SetCompressor /SOLID lzma

!include "MUI2.nsh"
!include "LogicLib.nsh"
!include "x64.nsh"

!define APPNAME "PostIt.Desktop"
!define APP_EXE "PostIt.Desktop.exe"
!define PUBLISHER "Yavsc"
!define UNINSTALL_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
!define PROTOCOL_KEY "Software\Classes\postit"

!ifndef APP_VERSION
  !error "Define APP_VERSION with /DAPP_VERSION=... (Windows) or -DAPP_VERSION=... (Linux)."
!endif
!ifndef PUBLISH_DIR
  !define PUBLISH_DIR "..\..\src\PostIt\PostIt.Desktop\bin\Release\net10.0\win-x64\publish"
!endif
!ifndef OUTPUT_FILE
  !define OUTPUT_FILE "PostIt-${APP_VERSION}-win-x64-Setup.exe"
!endif
!if ! /FileExists "${PUBLISH_DIR}\${APP_EXE}"
  !error "Publish PostIt.Desktop for win-x64 before compiling the installer (see INSTALL.md)."
!endif

Name "${APPNAME}"
OutFile "${OUTPUT_FILE}"
InstallDir "$LOCALAPPDATA\${PUBLISHER}\${APPNAME}"
InstallDirRegKey HKCU "${UNINSTALL_KEY}" "InstallLocation"
ShowInstDetails show
ShowUninstDetails show

!define MUI_ABORTWARNING
!define MUI_ICON "..\..\src\PostIt\PostIt\Assets\yavsc-logo.ico"
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "French"

Function .onInit
  SetShellVarContext current
  ${IfNot} ${RunningX64}
    MessageBox MB_OK|MB_ICONSTOP "Cette version de PostIt.Desktop requiert Windows 64 bits." /SD IDOK
    Abort
  ${EndIf}
FunctionEnd

Function un.onInit
  SetShellVarContext current
FunctionEnd

Section "PostIt.Desktop"
  ; Keep the recursive payload separate from user data and the uninstaller.
  ClearErrors
  SetOutPath "$INSTDIR\app"
  ${If} ${Errors}
    MessageBox MB_OK|MB_ICONSTOP "Impossible de creer le dossier d'installation. Verifiez les droits d'ecriture, puis relancez l'installation." /SD IDOK
    Abort
  ${EndIf}
  File /r "${PUBLISH_DIR}\*"
  WriteUninstaller "$INSTDIR\uninstall.exe"
  ${If} ${Errors}
    MessageBox MB_OK|MB_ICONSTOP "Installation impossible. Fermez PostIt.Desktop et verifiez les droits d'ecriture et l'espace disque, puis relancez l'installation." /SD IDOK
    Abort
  ${EndIf}

  ClearErrors
  CreateDirectory "$SMPROGRAMS\${PUBLISHER}"
  CreateShortcut "$SMPROGRAMS\${PUBLISHER}\${APPNAME}.lnk" "$INSTDIR\app\${APP_EXE}"

  WriteRegStr HKCU "${PROTOCOL_KEY}" "" "URL:PostIt"
  WriteRegStr HKCU "${PROTOCOL_KEY}" "URL Protocol" ""
  WriteRegStr HKCU "${PROTOCOL_KEY}\DefaultIcon" "" '"$INSTDIR\app\${APP_EXE}",0'
  WriteRegStr HKCU "${PROTOCOL_KEY}\shell\open\command" "" '"$INSTDIR\app\${APP_EXE}" "%1"'

  WriteRegStr HKCU "${UNINSTALL_KEY}" "DisplayName" "${APPNAME}"
  WriteRegStr HKCU "${UNINSTALL_KEY}" "DisplayVersion" "${APP_VERSION}"
  WriteRegStr HKCU "${UNINSTALL_KEY}" "Publisher" "${PUBLISHER}"
  WriteRegStr HKCU "${UNINSTALL_KEY}" "DisplayIcon" '"$INSTDIR\app\${APP_EXE}",0'
  WriteRegStr HKCU "${UNINSTALL_KEY}" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegStr HKCU "${UNINSTALL_KEY}" "QuietUninstallString" '"$INSTDIR\uninstall.exe" /S'
  WriteRegStr HKCU "${UNINSTALL_KEY}" "InstallLocation" "$INSTDIR"
  WriteRegDWORD HKCU "${UNINSTALL_KEY}" "NoModify" 1
  WriteRegDWORD HKCU "${UNINSTALL_KEY}" "NoRepair" 1
  ${If} ${Errors}
    MessageBox MB_OK|MB_ICONSTOP "Impossible de creer les raccourcis ou les entrees du registre utilisateur. Relancez l'installation." /SD IDOK
    Abort
  ${EndIf}
SectionEnd

Section "Uninstall"
  SetOutPath "$TEMP"
  ClearErrors
  RMDir /r "$INSTDIR\app"
  ${If} ${Errors}
    MessageBox MB_OK|MB_ICONSTOP "Des fichiers n'ont pas pu etre supprimes. Fermez PostIt.Desktop, puis relancez la desinstallation." /SD IDOK
    Abort
  ${EndIf}

  ; Do not remove a protocol handler subsequently registered by another install.
  ReadRegStr $0 HKCU "${PROTOCOL_KEY}\shell\open\command" ""
  ${If} $0 == '"$INSTDIR\app\${APP_EXE}" "%1"'
    DeleteRegKey HKCU "${PROTOCOL_KEY}"
  ${EndIf}
  Delete "$SMPROGRAMS\${PUBLISHER}\${APPNAME}.lnk"
  RMDir "$SMPROGRAMS\${PUBLISHER}"
  DeleteRegKey HKCU "${UNINSTALL_KEY}"
  Delete "$INSTDIR\uninstall.exe"
  RMDir "$INSTDIR"
SectionEnd
