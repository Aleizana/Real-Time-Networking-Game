using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace OmegaRace
{
    public class TimeManager
    {
        private static TimeManager instance = null;

        private static TimeManager Instance()
        {
            if (instance == null)
            {
                instance = new TimeManager();
            }
            return instance;
        }


        float prevTime;
        float currentTime;
        float frameTime;
        float timeOffset;

        public static void Initialize(float now)
        {
            TimeManager inst = Instance();
            inst.prevTime = now;
            inst.currentTime = now;
            inst.frameTime = 0;
            inst.timeOffset = 0;
        }

        public static void SetTime(float newtime)
        {
            instance.timeOffset += newtime - instance.currentTime;

            // Adjust other vars to stay consistent.
            instance.prevTime += instance.timeOffset;
            instance.currentTime = newtime;
        }

        static public void Update(float now)
        {
            TimeManager inst = Instance();

            inst.prevTime = inst.currentTime;
            inst.currentTime = now + inst.timeOffset;
            inst.frameTime = inst.currentTime - inst.prevTime;
        }

        public static float GetCurrentTime() { return Instance().currentTime; }
        public static float GetFrameTime() { return Instance().frameTime; }

    }
}
