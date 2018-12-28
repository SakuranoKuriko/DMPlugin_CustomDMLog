set path=%path%;"%windir%\Microsoft.NET\Framework64\v4.0.30319\"
csc.exe /o /t:library /r:BilibiliDM_PluginFramework.dll,Newtonsoft.Json.dll,"%windir%\Microsoft.NET\Framework64\v4.0.30319\WPF\WindowsBase.dll" "%1"
