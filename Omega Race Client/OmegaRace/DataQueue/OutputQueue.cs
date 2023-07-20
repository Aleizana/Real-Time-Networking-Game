using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegaRace
{
    class OutputQueue
    {
        private static OutputQueue instance = null;
        public static OutputQueue Instance()
        {
            if (instance == null)
            {
                instance = new OutputQueue();
            }
            return instance;
        }

        Queue<DataMessage> pOutputQueue;

        private OutputQueue()
        {
            pOutputQueue = new Queue<DataMessage>();
        }

        public static void AddToQueue(DataMessage msg)
        {
            instance.pOutputQueue.Enqueue(msg);
        }

        public static void Process()
        {
            while (instance.pOutputQueue.Count > 0)
            {
                DataMessage msg = instance.pOutputQueue.Dequeue();

                InputQueue.AddToQueue(msg);

            }


        }


    }
}
