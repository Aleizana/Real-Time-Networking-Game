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

                //Print contents of msg to debug window
                msg.PrintMe();

                //Perform different actions depending on the sendMsg type
                if (msg.mySendType == DataMessage.msgType.LOCAL_NET)
                {
                    GameSceneCollection.ScenePlay.MessageToClient(msg);
                
                    GameSceneCollection.ScenePlay.MsgQueueMgr.AddToInputQueue(msg);
                }
                else if (msg.mySendType == DataMessage.msgType.NET)
                {
                    GameSceneCollection.ScenePlay.MessageToClient(msg);
                    msg.ReleaseMsg();
                }
                else if (msg.mySendType == DataMessage.msgType.LOCAL)
                {
                    GameSceneCollection.ScenePlay.MsgQueueMgr.AddToInputQueue(msg);
                }
            }

            //ScreenLog.Add("Net msg count: " + msgcounter);
        }

        public override void ProcessIn(bool processMoves)
        {
            while (pInputQueue.Count > 0)
            {
                DataMessage msg = pInputQueue.Dequeue();

                //Record message to file stream
                RecordToFile(msg);

                //Process message actions
                msg.Execute();

                //Don't send back fire missile message if false
                if (msg.myDataType == DataMessage.dataType.MISSILEP1 && msg.sendMissile == false)
                {
                    msg.ReleaseMsg();
                }
                //If processing the initial client data, add it back to the output queue to be sent back to the client
                else if (processMoves && msg.myDataType != DataMessage.dataType.COLLIDE)
                {
                    //Branch for dead reckoning consideration
                    //Only mines and fire msgs need be sent back immediately, move and missile are handled in GameScenePlay 
                    if (GameSceneCollection.ScenePlay.DeadReckoningOn)
                    {
                        HandlePrediction(msg, processMoves);
                    }
                    //For non-dead reckoning
                    else
                    {
                        msg.mySendType = DataMessage.msgType.NET;
                        GameSceneCollection.ScenePlay.MsgQueueMgr.AddToOutputQueue(msg);
                    }
                    
                }
                else
                {
                    msg.ReleaseMsg();
                }
            }
        }

        public override void Process(bool processMoves)
        {
            this.ProcessOut();
            this.ProcessIn(processMoves);
        }

        private void RecordToFile(DataMessage msg)
        {
            //If the message is the last in the input queue, mark it as the last of a given batch of messages
            if (pInputQueue.Count == 0)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(1);
            }

            //Write data to the file
            myStream.Flush();

            //Serialize the message data
            msg.SerializeRecord(ref writer);

            //Write data to the file
            myStream.Flush();
        }

        private void HandlePrediction(DataMessage msg, bool processMoves)
        {
            //After the first position message for player ships has been sent, further movement messages will not be returned to the client in message queue
            if ((!GameSceneCollection.ScenePlay.PlayerMgr.P1Data.ship.predPlr.initPred && msg.myDataType == DataMessage.dataType.MOVEP1)
                || (!GameSceneCollection.ScenePlay.PlayerMgr.P2Data.ship.predPlr.initPred && msg.myDataType == DataMessage.dataType.MOVEP2))
            {
                msg.mySendType = DataMessage.msgType.NET;
                GameSceneCollection.ScenePlay.MsgQueueMgr.AddToOutputQueue(msg);

                //Differentiate between player 1 and player 2 move messages
                if (msg.myDataType == DataMessage.dataType.MOVEP1)
                {
                    //Mark the first position message as being sent
                    GameSceneCollection.ScenePlay.PlayerMgr.P1Data.ship.predPlr.initPred = true;
                }
                else
                {
                    //Mark the first position message as being sent
                    GameSceneCollection.ScenePlay.PlayerMgr.P2Data.ship.predPlr.initPred = true;
                }

            }
            //Future messages will only be recycled if they are non-move related messages
            else if (msg.myDataType != DataMessage.dataType.MOVEP1 && msg.myDataType != DataMessage.dataType.MOVEP2)
            {
                msg.mySendType = DataMessage.msgType.NET;
                GameSceneCollection.ScenePlay.MsgQueueMgr.AddToOutputQueue(msg);
            }
            //Release message if it is a move message
            else
            {
                msg.ReleaseMsg();
            }
        }
    }
}
