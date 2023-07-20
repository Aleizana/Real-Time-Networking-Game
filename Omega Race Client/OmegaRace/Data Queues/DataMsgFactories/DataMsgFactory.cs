using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;

//For usage of Vec2
using Box2DX.Common;

//For net enum 
using Lidgren.Network;

namespace OmegaRace.Data_Queues.DataMsgFactories
{
    [Serializable]
    class DataMsgFactory
    {

        public virtual void Serialize(ref BinaryWriter writer)
        {

        }

        public virtual void SerializeRecord(ref BinaryWriter writer)
        {

        }

        public static DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            return null;
        }

        public virtual void Execute()
        {


        }

        //Release an object back into the pool to be reused
        public virtual void ReleaseMsg()
        {

        }

        public virtual void PrintMe()
        {

        }
    }

    public class ObjectPool
    {
        private DataMessage[] msgPool;

        private Stack<DataMessage> collStack;

        public ObjectPool()
        {
            // Initially nothing in the pool
            //Pool indices corresspond to enum values
            msgPool = new DataMessage[8];

            //Stack to hold multiple collision messages
            collStack = new Stack<DataMessage>();
        }

        //Returns an object of the given data type using enum as an index
        public DataMessage Get(DataMessage.dataType myData)
        {
            // If empty, make a new DataMessage
            // Otherwise, get one from the pool
            return msgPool[(int)myData] == null ? findType(myData) : msgPool[(int)myData];
        }

        //Decide which message to init depending on type
        public DataMessage findType(DataMessage.dataType myData)
        {
            DataMessage storeData;

            switch (myData)
            {
                case DataMessage.dataType.COLLIDE:
                    //If an object is available, use it, otherwise make a new one
                    if (collStack.Count == 0)
                    {
                        storeData = new CollideMsg();
                    }
                    else
                    {
                        storeData = collStack.Pop();
                    }

                    return storeData;


                case DataMessage.dataType.MOVEP1:
                    storeData = new P1MoveRotMsg();
                    break;
                case DataMessage.dataType.MOVEP2:
                    storeData = new P2MoveRotMsg();
                    break;
                case DataMessage.dataType.MISSILEP1:
                    storeData = new P1FireMissile();
                    break;
                case DataMessage.dataType.MISSILEP2:
                    storeData = new P2FireMissile();
                    break;
                case DataMessage.dataType.MINEP1:
                    storeData = new P1LayMine();
                    break;
                case DataMessage.dataType.MINEP2:
                    storeData = new P2LayMine();
                    break;
                case DataMessage.dataType.TIME:
                    storeData = new TimeMsg();
                    break;
                default:
                    return null;
            }

            //Can use the enum value to store the data in the correct spot
            msgPool[(int)myData] = storeData;

            return storeData;
        }

        public void ReleaseAll()
        {
            foreach (DataMessage msg in msgPool)
            {
                if (msg != null && msg.released == false && msg.executed)
                {
                    msg.ReleaseMsg();
                }
            }
        }

        //Return a collide message back to the stack
        public void BackToStack(DataMessage collMsg)
        {
            collStack.Push(collMsg);
        }
    }
}
