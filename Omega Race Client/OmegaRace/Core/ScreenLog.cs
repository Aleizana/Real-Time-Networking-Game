using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace OmegaRace
{
    public class ScreenLog
    {
        private static ScreenLog instance = null;
        static ScreenLog Instance()
        {
            if (instance == null)
            {
                instance = new ScreenLog();
            }
            return instance;
        }

        struct LogData
        {
            public string msg;
            public Azul.Color col;
        }

        Stack<LogData> LogDataPool;
        List<LogData> msgList;
        SpriteFont logfont;
        Azul.Color dftCol = Colors.DarkGray2;

        int anchorx = 15;  // some day make this window relative
        int anchory = 485;

        ScreenLog()
        {
            msgList = new List<LogData>();
            LogDataPool = new Stack<LogData>();
            logfont = TextureCollection.ScreenLogFont;

        }

        public static SpriteFont GetSpriteFont() { return Instance().logfont; }
        public static void Add(Azul.Color c, string formattedMsg, params object[] values)
        {
            LogData d;
            if (Instance().LogDataPool.Count == 0)
                d = new LogData();
            else
                d = Instance().LogDataPool.Pop();

            d.msg = String.Format(formattedMsg, values);
            d.col = c;
            Instance().msgList.Add(d);
        }
        public static void Add(string formattedMsg, params object[] values)
        {
            Add(Instance().dftCol, formattedMsg, values);
        }
        public static void Display() { Instance().pDisplay(); }
        void pDisplay()
        {
            int x = anchorx;
            int y = anchory;
            int ydelta = logfont.GetFontHeight();

            foreach (LogData d in Instance().msgList)
            {
                logfont.Render(d.msg, x, y, d.col);
                y -= ydelta;

                LogDataPool.Push(d);
            }

            Instance().msgList.Clear();
        }

    }

}
