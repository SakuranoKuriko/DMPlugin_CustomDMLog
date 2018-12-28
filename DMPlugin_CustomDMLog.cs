using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DMPluginTest
{
    public class Class1 : BilibiliDM_PluginFramework.DMPlugin
    {
        const string defaultConfig = "{\"AutoStart\": 0, \"Format\": \"[{time:HH:mm:ss}]{name}: {comment}\"}";
        string filepath = null;
        string configFile = null;
        JObject config = null;
        public Class1()
        {
            this.Connected += Class1_Connected;
            this.Disconnected += Class1_Disconnected;
            this.ReceivedDanmaku += Class1_ReceivedDanmaku;
            this.ReceivedRoomCount += Class1_ReceivedRoomCount;
            this.PluginAuth = "桜野くりこ";
            this.PluginName = "自訂彈幕Log";
            this.PluginCont = "mao.liz.baka@gmail.com";
            this.PluginVer = "v1.0.0";
            this.PluginDesc = "停用再啟用即可刷新設置";
        }

        public void loadConfig()
        {
            string configstr = defaultConfig;
            if (File.Exists(configFile))
                configstr = File.ReadAllText(configFile);
            else File.WriteAllText(configFile, defaultConfig);
            config = JObject.Parse(configstr);
        }

        public void pushComment(BilibiliDM_PluginFramework.DanmakuModel dm)
        {
            string comment = config["Format"].ToString().Replace("{name}", dm.UserName);
            comment = comment.Replace("{comment}", dm.CommentText);
            DateTime timenow = DateTime.Now;
            comment = comment.Replace("{time}", timenow.ToString("HH:mm:ss"));
            MatchCollection matches = Regex.Matches(comment, "{time:(?<format>.*?)}");
#if DEBUG
            Log(matches.Count.ToString());
#endif
            if (matches.Count>0)
            {
                foreach (Match match in matches)
                {
                    comment = comment.Replace("{time:"+match.Groups["format"].Value+"}", timenow.ToString(match.Groups["format"].Value));
#if DEBUG
                    Log(match.Groups["format"].Value);
#endif
                }
            }
#if DEBUG
            Log(dm.RawData);
#endif
            File.AppendAllText(filepath, comment+"\r\n");
        }

        private void Class1_ReceivedRoomCount(object sender, BilibiliDM_PluginFramework.ReceivedRoomCountArgs e)
        {

        }

        private void Class1_ReceivedDanmaku(object sender, BilibiliDM_PluginFramework.ReceivedDanmakuArgs e)
        {
            if (e.Danmaku.MsgType==BilibiliDM_PluginFramework.MsgTypeEnum.Comment)
                pushComment(e.Danmaku);
        }

        private void Class1_Disconnected(object sender, BilibiliDM_PluginFramework.DisconnectEvtArgs e)
        {
            
        }

        private void Class1_Connected(object sender, BilibiliDM_PluginFramework.ConnectedEvtArgs e)
        {
            
        }

        public override void Inited(){
            string p = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\弹幕姬\\";
            filepath = p+"comment.txt";
            configFile = p+"Plugins\\CustomDMLog.conf";
            loadConfig();
            if (config["AutoStart"].ToString()=="1")
                Start();
            base.Inited();
        }

        public override void Admin()
        {
            //請勿使用任何阻塞方法
            Process.Start("notepad", configFile);
            base.Admin();
        }

        public override void Stop()
        {
            //請勿使用任何阻塞方法
            base.Stop();
        }

        public override void Start()
        {
            //請勿使用任何阻塞方法
            loadConfig();
            base.Start();
        }
    }
}
