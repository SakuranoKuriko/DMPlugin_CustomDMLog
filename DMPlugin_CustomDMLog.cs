using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DMPluginTest
{
    public class CustomDMLog : BilibiliDM_PluginFramework.DMPlugin
    {
        const string defaultConfigStr = "{\"AutoStart\":0,\"LogFile\":\"comment.txt\",\"Format\":\"[{time:HH:mm:ss}]{name}:{comment}\",\"FileWritePeroid\":\"500\",\"Debug\":0}";
        bool debug = false;
        string filepath = null;
        string configFile = null;
        string savepath = null;
        JObject config = null;
        FileStream logfs = null;
        StreamWriter logfsw = null;
        Timer savt = null;
        int savp = 0;
        bool autosav = false;
        
        public CustomDMLog()
        {
            this.Connected += Class1_Connected;
            this.Disconnected += Class1_Disconnected;
            this.ReceivedDanmaku += Class1_ReceivedDanmaku;
            this.ReceivedRoomCount += Class1_ReceivedRoomCount;
            this.PluginAuth = "桜野くりこ";
            this.PluginName = "CustomDMLog";
            this.PluginCont = "mao.liz.baka@gmail.com";
            this.PluginVer = "v1.0.1";
            this.PluginDesc = "停用再啟用即可刷新設置";
        }
        
        public void dmlog(string str)
        {
            Console.WriteLine(str);
            this.Log(str);
            this.AddDM(str, true);
        }

        public void loadConfig()
        {
            string configstr = defaultConfigStr;
            if (File.Exists(configFile))
                configstr = File.ReadAllText(configFile);
            else saveConfig();
            if (configstr.IndexOf("\r\n")==-1)
            {
                configstr = defaultConfigStr;
                saveConfig();
            }
            try
            {
                config = JObject.Parse(configstr);
                filepath = savepath+"comment.txt";
                if (getConfig("LogFile")!="")
                    filepath = savepath+getConfig("LogFile");
                if (!int.TryParse(getConfig("FileWritePeroid"), out savp) || savp<100)
                    savp = 1000;
                if (getConfig("Debug")=="1")
                    debug = true;
            }
            catch { saveConfig(); }
        }

        public void saveConfig()
        {
            File.WriteAllText(configFile, JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        public string getConfig(string key)
        {
            if (config[key]!=null)
                return config[key].ToString();
            else return "";
        }

        public void pushComment(BilibiliDM_PluginFramework.DanmakuModel dm)
        {
            string comment = getConfig("Format").Replace("{name}", dm.UserName);
            comment = comment.Replace("{comment}", dm.CommentText);
            DateTime timenow = DateTime.Now;
            comment = comment.Replace("{time}", timenow.ToString("HH:mm:ss"));
            MatchCollection matches = Regex.Matches(comment, "{time:(?<format>.*?)}");
            if (debug)
                Log(matches.Count.ToString());
            if (matches.Count>0)
            {
                foreach (Match match in matches)
                {
                    comment = comment.Replace("{time:"+match.Groups["format"].Value+"}", timenow.ToString(match.Groups["format"].Value));
                    if (debug)
                        Log(match.Groups["format"].Value);
                }
            }
            if (debug)
                Log(dm.RawData);
            //File.AppendAllText(filepath, comment+"\r\n");
            if (logfsw!=null)
                logfsw.Write(comment+"\r\n");
            else Stop();
        }

        public void savelog(object o)
        {
            if (debug)
                Log("flush at "+DateTime.Now.ToString("yyyy-mm-dd|hh:mm:ss.fff"));
            logfsw.Flush();
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
            savepath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\弹幕姬\\";
            configFile = savepath+"Plugins\\CustomDMLog.conf";
            config = JObject.Parse(defaultConfigStr);
            savt = new Timer(new TimerCallback(savelog), null, -1, -1);
            loadConfig();
            if (getConfig("AutoStart")=="1")
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
            savt.Change(-1, -1);
            logfsw.Close();
            logfs.Close();
            base.Stop();
        }

        public override void Start()
        {
            //請勿使用任何阻塞方法
            loadConfig();
            try
            {
                if (logfsw != null) logfsw.Close();
                if (logfs != null) logfs.Close();
                logfs = File.Open(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                logfs.Seek(0, SeekOrigin.End);
                logfsw = new StreamWriter(logfs);
                if (autosav)
                {
                    logfsw.AutoFlush = true;
                    savt.Change(-1, -1);
                }
                else
                {
                    logfsw.AutoFlush = false;
                    savt.Change(0, savp);
                }
            }
            catch(Exception e)
            {
                dmlog("CustomDMLog: 初始化失敗:"+e.Message);
                return;
            }
            base.Start();
        }
    }
}
