using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace OmegaRace.Data_Queues.MessageManager
{
    class MessageQueuePlayback : MessageQueueBase
    {
        BinaryReader reader;

        public MessageQueuePlayback(string file)
        {
            reader = new BinaryReader(new FileStream("../bin/Debug" + file, FileMode.Open));

            pInputQueue = new Queue<DataMessage>();
            pOutputQueue = new Queue<DataMessage>();
        }

        public void ProcessPlaybackMsg()
        {
            //If we have bytes to read, read them
            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                int batchNum = reader.ReadInt32();

                DataMessage msg;

                //Process messages until reaching the end of a batch
                while (batchNum != 0)
                {
                    msg = DataMessage.Deserialize(ref reader, GameSceneCollection.ScenePlay.PoolMgr);

                    GameSceneCollection.ScenePlay.MsgQueueMgr.AddToInputQueue(msg);

                    batchNum = reader.ReadInt32();
                }

                //Add the final message of the batch to the input queue and process
                msg = DataMessage.Deserialize(ref reader, GameSceneCollection.ScenePlay.PoolMgr);

                GameSceneCollection.ScenePlay.MsgQueueMgr.AddToInputQueue(msg);
            }
            else
            {
                //Pause system indefinitely at the end of the playback
                while(true)
                {
                    Thread.Sleep(5000);
                }
            }
            

            
        }

        //Process out and process in can happen within the derived classes
        public override void ProcessOut()
        {
            while (pOutputQueue.Count > 0)
            {
                DataMessage msg = pOutputQueue.Dequeue();

                if (msg.mySendType == DataMessage.msgType.NET)
                {
                    msg.ReleaseMsg();
                }
            }
        }

        public override void ProcessIn(bool processMoves)
        {
            while (pInputQueue.Count > 0)
            {
                DataMessage msg = pInputQueue.Dequeue();

                if (msg.mySendType == DataMessage.msgType.DEFAULT)
                {
                    msg.ReleaseMsg();
                }
                else
                {
                    //Print the contents of the data message
                    msg.PrintMe();

                    //Process message actions
                    msg.Execute();
                }
            }
        }

        public override void Process(bool processMoves)
        {
            if (processMoves == false)
            {
                this.ProcessOut();

                //Read data messages from the file
                this.ProcessPlaybackMsg();

                //Process data messages
                this.ProcessIn(processMoves);
            }
            
        }
    }
}
