using System;
using System.IO;
using Newtonsoft.Json;

namespace DMPluginTest
{
    public class Class1 : BilibiliDM_PluginFramework.DMPlugin
    {
        string filepath = null;
        string defaultConfig = "{\"format\": \"{name}: {comment}\", \"\"}";
        public Class1()
        {
            this.Connected += Class1_Connected;
            this.Disconnected += Class1_Disconnected;
            this.ReceivedDanmaku += Class1_ReceivedDanmaku;
            this.ReceivedRoomCount += Class1_ReceivedRoomCount;
            this.PluginAuth = "桜野くりこ";
            this.PluginName = "自定义弹幕日志";
            this.PluginCont = "mao.liz.baka@gmail.com";
            this.PluginVer = "v0.0.1";
        }

        public void pushComment(string user, string msg)
        {
            string comment = msg.Replace("{name}", user);
            comment = comment.Replace("{comment}", msg);
            Log(user);
            Log(msg);
            Log(user);
            File.AppendAllText(filepath, comment+"\r\n");
        }

        private void Class1_ReceivedRoomCount(object sender, BilibiliDM_PluginFramework.ReceivedRoomCountArgs e)
        {

        }

        private void Class1_ReceivedDanmaku(object sender, BilibiliDM_PluginFramework.ReceivedDanmakuArgs e)
        {
            if (e.Danmaku.MsgType==BilibiliDM_PluginFramework.MsgTypeEnum.Comment)
                pushComment(e.Danmaku.UserName, e.Danmaku.CommentText);
        }

        private void Class1_Disconnected(object sender, BilibiliDM_PluginFramework.DisconnectEvtArgs e)
        {
            
        }

        private void Class1_Connected(object sender, BilibiliDM_PluginFramework.ConnectedEvtArgs e)
        {
            
        }

        public override void Inited(){
            Start();
            base.Inited();
        }

        public override void Admin()
        {
            //請勿使用任何阻塞方法
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
            string p = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\弹幕姬\\";
            filepath = p+"comment.txt";
            string formatfile = p+"Danmaku2File.cfg";
            if (File.Exists(formatfile))
                fileformat = File.ReadAllText(formatfile);
            else File.WriteAllText(formatfile, fileformat);
            base.Start();
        }
    }
}
