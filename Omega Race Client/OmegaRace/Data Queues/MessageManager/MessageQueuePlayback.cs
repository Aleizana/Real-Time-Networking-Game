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

                //Process messages until reaching the end of a batch, which is marked with a zero
                while (batchNum != 0)
                {
                    msg = DataMessage.Deserialize(ref reader, GameSceneCollection.ScenePlay.PoolMgr);

                    GameSceneCollection.ScenePlay.MsgQueueMgr.AddToInputQueue(msg);

                    batchNum = reader.ReadInt32();
                }
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
            }
        }

        public override void ProcessIn()
        {
            while (pInputQueue.Count > 0)
            {
                DataMessage msg = pInputQueue.Dequeue();

                msg.PrintMe();

                //Process message actions
                msg.Execute();
            }
        }

        public override void Process()
        {
            this.ProcessOut();

            //Read data messages from the file
            this.ProcessPlaybackMsg();

            //Process data messages
            this.ProcessIn();
        }
    }
}
