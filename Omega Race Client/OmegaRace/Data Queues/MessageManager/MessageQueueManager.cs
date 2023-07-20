using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

using OmegaRace.Managers.NetworkManager;
using OmegaRace.Data_Queues.MessageManager;

namespace OmegaRace
{
    public class MessageQueueManager 
    {
        public enum Mode { NORMAL, RECORD, PLAYBACK };

        MessageQueueBase msgMgr;

        public MessageQueueManager(Mode m = Mode.NORMAL, string file = "")
        {
            Debug.Assert((m == Mode.NORMAL && file == "") || (m != Mode.NORMAL && file != ""));

            switch (m)
            {
                case Mode.NORMAL:
                    msgMgr = new MessageQueueNorm();
                    break;
                case Mode.RECORD:
                    msgMgr = new MessageQueueRecord(file);
                    break;
                case Mode.PLAYBACK:
                    msgMgr = new MessageQueuePlayback(file);
                    break;
                default:
                    throw new InvalidDataException("Message queue mode not initialized.");
            }
        }

        public void AddToInputQueue(DataMessage msg)
        {
            msgMgr.pInputQueue.Enqueue(msg);
        }

        public void AddToOutputQueue(DataMessage msg)
        {
            //Frame count can be added to data message here
            msgMgr.pOutputQueue.Enqueue(msg);
        }

        public void EndFileBatch(int num)
        {
            msgMgr.EndBatch(num);
        }

        public void Process()
        {
            msgMgr.Process();
        }
    }
}
