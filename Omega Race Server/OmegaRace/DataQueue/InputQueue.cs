using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegaRace
{
    class InputQueue
    {
        private static InputQueue instance = null;
        public static InputQueue Instance()
        {
            if (instance == null)
            {
                instance = new InputQueue();
            }
            return instance;
        }

        Queue<DataMessage> pInputQueue;

        private InputQueue()
        {
            pInputQueue = new Queue<DataMessage>();
        }

        public static void AddToQueue(DataMessage msg)
        {
            instance.pInputQueue.Enqueue(msg);
        }

        public static void Process()
        {
            while (instance.pInputQueue.Count > 0)
            {
                DataMessage msg = instance.pInputQueue.Dequeue();

                GameManager.RecieveMessage(msg);
            }
        }
    }
}
