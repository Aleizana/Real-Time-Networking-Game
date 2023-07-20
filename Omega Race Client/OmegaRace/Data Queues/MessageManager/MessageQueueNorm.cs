using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegaRace.Data_Queues.MessageManager
{
    class MessageQueueNorm : MessageQueueBase
    {
        public MessageQueueNorm()
        {
            pInputQueue = new Queue<DataMessage>();
            pOutputQueue = new Queue<DataMessage>();
        }

        public override void ProcessOut()
        {
            MessageQueueManager refMgr = GameSceneCollection.ScenePlay.MsgQueueMgr;

            while (pOutputQueue.Count > 0)
            {
                DataMessage msg = pOutputQueue.Dequeue();
        
                //Perform different actions depending on the sendMsg type
                if (msg.mySendType == DataMessage.msgType.LOCAL_NET)
                {
                    GameSceneCollection.ScenePlay.MessageToServer(msg);
                    refMgr.AddToInputQueue(msg);
                }
                else if (msg.mySendType == DataMessage.msgType.NET)
                {
                    GameSceneCollection.ScenePlay.MessageToServer(msg);
                    msg.ReleaseMsg();
                    msg.released = true;
                }
                else if (msg.mySendType == DataMessage.msgType.LOCAL)
                {
                    refMgr.AddToInputQueue(msg);
                }
            }
        
            //ScreenLog.Add("Net msg count: " + msgcounter);
        }

        public override void ProcessIn()
        {
            while (pInputQueue.Count > 0)
            {
                DataMessage msg = pInputQueue.Dequeue();

                msg.PrintMe();
          
                msg.Execute();

                msg.executed = true;
            }
        }

        public override void Process()
        {
            this.ProcessOut();
            this.ProcessIn();
        }
    }
}
