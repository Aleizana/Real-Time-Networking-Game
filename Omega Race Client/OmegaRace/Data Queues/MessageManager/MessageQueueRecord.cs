using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OmegaRace.Data_Queues.MessageManager
{
    class MessageQueueRecord : MessageQueueBase
    {
        FileStream myStream;
        BinaryWriter writer;

        public MessageQueueRecord(string file)
        {
            myStream = new FileStream("../bin/Debug" + file, FileMode.Create);
            writer = new BinaryWriter(myStream);

            pInputQueue = new Queue<DataMessage>();
            pOutputQueue = new Queue<DataMessage>();
        }

        //Process out and process in can happen within the derived classes
        public override void ProcessOut()
        {
            while (pOutputQueue.Count > 0)
            {
                DataMessage msg = pOutputQueue.Dequeue();

                //Perform different actions depending on the sendMsg type
                if (msg.mySendType == DataMessage.msgType.LOCAL_NET)
                {
                    GameSceneCollection.ScenePlay.MessageToServer(msg);
                
                    GameSceneCollection.ScenePlay.MsgQueueMgr.AddToInputQueue(msg);
                }
                else if (msg.mySendType == DataMessage.msgType.NET)
                {
                    GameSceneCollection.ScenePlay.MessageToServer(msg);
                    msg.ReleaseMsg();
                    msg.released = true;
                }
                else if (msg.mySendType == DataMessage.msgType.LOCAL)
                {
                    GameSceneCollection.ScenePlay.MsgQueueMgr.AddToInputQueue(msg);
                }
            }

            //ScreenLog.Add("Net msg count: " + msgcounter);
        }

        public override void ProcessIn()
        {
            while (pInputQueue.Count > 0)
            {
                DataMessage msg = pInputQueue.Dequeue();

                //Record message to file stream
                RecordToFile(msg);

                //Process message actions
                msg.Execute();
               
                msg.executed = true;
            }

            //End the batch of messages with a signifying 0
            EndBatch(0);
        }

        public override void Process()
        {
            this.ProcessOut();
            this.ProcessIn();
        }

        private void RecordToFile(DataMessage msg)
        {
            //Mark messages as NOT being the last message
            writer.Write(1);

            //Write data to the file
            myStream.Flush();

            //Serialize the message data
            msg.SerializeRecord(ref writer);

            //Write data to the file
            myStream.Flush();
        }

        public override void EndBatch(int num)
        {
            writer.Write(num);
            myStream.Flush();
        }
    }
}
