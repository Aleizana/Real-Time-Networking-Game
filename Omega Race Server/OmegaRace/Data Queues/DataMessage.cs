using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;

using Box2DX.Common;
using Lidgren.Network;

namespace OmegaRace
{

    [Serializable]
    public class DataMessage
    {
        //Time that the message was executed
        public float myProcTime;

        //Variable to hold current time to be sent
        public float sendTime;

        //Button press values
        public int horzInput = 0;
        public int vertInput = 0;

        //Hold the ID of a missile when fired
        public int holdMisID = 0;

        //For sending timing of position to client
        public float posTime;

        //Boolean to determine whether lay mine or fire missile actions were pressed
        public bool layMyMine = false;
        public bool sendMissile = false;

        //For collision detection from the server
        public GameObject gObjA;
        public GameObject gObjB;

        //Positional data for the missiles of both players
        public Vec2 pos1;
        public Vec2 pos2;
        public Vec2 pos3;
        public float angle1;
        public float angle2;
        public float angle3;

        //For sending ship pos when a missile is fired
        public Vec2 shipPosMissile;
        public float shipAngleMissile;

        //Controls Lidgren delivery type
        public NetDeliveryMethod myDelivType;

        //Controls the channel that a message is sent through
        public int myPortNum;

        //Types to signify where certain data needs to be sent
        public enum msgType
        {
            LOCAL, LOCAL_NET, NET, DEFAULT
        };

        public msgType mySendType;

        //Types to represent the type of data being sent
        public enum dataType
        {
            MISSILEP1, MINEP1, MOVEP1,
            MISSILEP2, MINEP2, MOVEP2,
            TIME, COLLIDE
        }

        public dataType myDataType;

        // Locate player manager
        protected PlayerManager plrMgr = GameSceneCollection.ScenePlay.PlayerMgr;

        //Locate object pool
        protected ObjectPool poolRef = GameSceneCollection.ScenePlay.PoolMgr;

        public virtual void Serialize(ref BinaryWriter writer)
        {

        }

        public virtual void SerializeRecord(ref BinaryWriter writer)
        {

        }

        //public void SerializeShipDeadReckoning(ref BinaryWriter writer, PlayerData plrRef)
        //{
        //    //Data type will be assigned in GameScenePlay as either MOVEP1 or MOVEP2
        //    writer.Write((byte)myDataType);
        //
        //    //Write the time at which the ship moved
        //    writer.Write(posTime);
        //
        //    //Store the position of the ship after movement
        //    Vec2 shipPos = plrRef.ship.GetPixelPosition();
        //    float shipAngle = plrRef.ship.GetAngle_Deg();
        //    Vec2 shipSpeed = plrRef.ship.GetPixelVelocity();
        //
        //    //Write move data
        //    writer.Write(shipPos.X);
        //    writer.Write(shipPos.Y);
        //    writer.Write(shipAngle);
        //    writer.Write(shipSpeed.X);
        //    writer.Write(shipSpeed.Y);
        //}

        //Attempting upcasting first, if fails, change to individual allocation
        public static DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            //Read the enum data type value of the message
            dataType myDataType = (dataType)reader.ReadByte();

            DataMessage storeData;

            //These are the only types we can receive/SHOULD from the client
            switch (myDataType)
            {
                case dataType.MOVEP1:
                    storeData = P1MoveRotMsg.Deserialize(ref reader, objPool);
                    break;
                case dataType.MISSILEP1:
                    storeData = P1FireMissile.Deserialize(ref reader, objPool);
                    break;
                case dataType.MINEP1:
                    storeData = P1LayMine.Deserialize(ref reader, objPool);
                    break;
                case dataType.TIME:
                    storeData = TimeMsg.Deserialize(ref reader, objPool);
                    break;
                //For playback only
                case dataType.MOVEP2:
                    storeData = P2MoveRotMsg.Deserialize(ref reader, objPool);
                    break;
                case dataType.MISSILEP2:
                    storeData = P2FireMissile.Deserialize(ref reader, objPool);
                    break;
                case dataType.MINEP2:
                    storeData = P2LayMine.Deserialize(ref reader, objPool);
                    break;
                case dataType.COLLIDE:
                    storeData = CollideMsg.Deserialize(ref reader, objPool);
                    break;
                default:
                    return null;
            }

            return storeData;
        }

        public virtual void Execute()
        {
           
        }

        //Release an object back into the pool to be reused
        public virtual void ReleaseMsg()
        {
            mySendType = msgType.DEFAULT;
        }

        public virtual void PrintMe()
        {
            
        }

        public virtual void SetCollide(GameObject _objA, GameObject _objB)
        {

        }
    }

    [Serializable]
    public class P1FireMissile : DataMessage
    {
        //Serialize the sending of player 1's missile
        public override void Serialize(ref BinaryWriter writer)
        {
            writer.Write((byte)dataType.MISSILEP1);

            writer.Write(sendMissile);

            //If a missile was fired
            if (sendMissile)
            {
                //Write the time of execution of the message
                writer.Write(posTime);

                //Dead reckoning will send missile information along with the fire missile message
                if (GameSceneCollection.ScenePlay.DeadReckoningOn)
                {
                    //Iterate through each missile and find the missile just fired
                    foreach (Missile m in plrMgr.P1Data.missileList)
                    {
                        //Only send each missile position once
                        if (!m.posSent)
                        {
                            //Write "true" to signify that there is another missile to be read
                            writer.Write(true);

                            Vec2 misPos = m.GetPixelPosition();
                            float misAngle = m.GetAngle_Deg();
                            Vec2 misSpeed = m.GetPixelVelocity();

                            //Write the ID of the missile so the client can find it 
                            writer.Write(m.getID());

                            //Write the data of the missile
                            writer.Write(misPos.X);
                            writer.Write(misPos.Y);
                            writer.Write(misAngle);
                            writer.Write(misSpeed.X);
                            writer.Write(misSpeed.Y);

                            //Only send each missile position once
                            m.posSent = true;
                        }
                    }

                    //Write "false" to signify that there are no more missiles to be read
                    writer.Write(false);
                }
                //Regular processing will send ship position along with the missile fire message
                else
                {
                    writer.Write(shipPosMissile.X);
                    writer.Write(shipPosMissile.Y);
                    writer.Write(shipAngleMissile);
                }
            }
            

            //Fire missile message will be sent in port 0 with reliable ordered settings
            myDelivType = NetDeliveryMethod.ReliableOrdered;
            myPortNum = 0;
        }

        //Serialize the sending of player 1's missile for recording/playback
        public override void SerializeRecord(ref BinaryWriter writer)
        {
            writer.Write((byte)dataType.MISSILEP1);

            writer.Write(sendMissile);
        }

        //Only the boolean of whether or not a missile was sent needs to be read
        public static new DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            DataMessage newMis = objPool.Get(DataMessage.dataType.MISSILEP1);

            //Set data type
            newMis.myDataType = dataType.MISSILEP1;

            newMis.sendMissile = reader.ReadBoolean();

            return newMis;
        }

        public override void Execute()
        {
            if (plrMgr.P1Data.missileList.Count != 3)
            {
                //Store the position of the ship when missile is fired
                shipPosMissile = plrMgr.P1Data.ship.GetPixelPosition();
                shipAngleMissile = plrMgr.P1Data.ship.GetAngle_Deg();

                //Store time of execution
                posTime = TimeManager.GetCurrentTime();

                //Fire missile
                plrMgr.P1Data.FireMissile();

                sendMissile = true;
            }
        }

        //Release an object back into the pool to be reused
        public override void ReleaseMsg()
        {
            sendMissile = false;
        }

        public override void PrintMe()
        {
            Debug.Print("Event Msg: (" + mySendType + ", P1FIREMISSILE" + ") Player 1 (sent on frame " + GameManager.GetFrameCount() + ")");
        }
    }

    [Serializable]
    public class P2FireMissile : DataMessage
    {
        //Serialize the sending of player 2's missile, never used
        public override void Serialize(ref BinaryWriter writer)
        {
            writer.Write((byte)dataType.MISSILEP2);

            writer.Write(sendMissile);

            //If a missile was fired
            if (sendMissile)
            {
                //Write the time of execution of the message
                writer.Write(posTime);

                //Dead reckoning will send missile information along with the fire missile message
                if (GameSceneCollection.ScenePlay.DeadReckoningOn)
                {
                    //Iterate through each missile and find the missile just fired
                    foreach (Missile m in plrMgr.P2Data.missileList)
                    {
                        //Only send each missile position once
                        if (!m.posSent)
                        {
                            //Write "true" to signify that there is another missile to be read
                            writer.Write(true);

                            Vec2 misPos = m.GetPixelPosition();
                            float misAngle = m.GetAngle_Deg();
                            Vec2 misSpeed = m.GetPixelVelocity();

                            //Write the ID of the missile so the client can find it 
                            writer.Write(m.getID());

                            //Write the data of the missile
                            writer.Write(misPos.X);
                            writer.Write(misPos.Y);
                            writer.Write(misAngle);
                            writer.Write(misSpeed.X);
                            writer.Write(misSpeed.Y);

                            //Only send each missile position once
                            m.posSent = true;
                        }
                    }

                    //Write "false" to signify that there are no more missiles to be read
                    writer.Write(false);
                }
                //Regular processing will send ship position along with the missile fire message
                else
                {
                    writer.Write(shipPosMissile.X);
                    writer.Write(shipPosMissile.Y);
                    writer.Write(shipAngleMissile);
                }
            }


            //Fire missile message will be sent in port 2 with reliable ordered settings
            myDelivType = NetDeliveryMethod.ReliableOrdered;
            myPortNum = 0;
        }

        //Serialize the sending of player 2's missile, never used
        public override void SerializeRecord(ref BinaryWriter writer)
        {
            writer.Write((byte)dataType.MISSILEP2);

            writer.Write(sendMissile);
        }

        //Only the boolean of whether or not a missile was sent needs to be read
        public static new DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            DataMessage newMis = objPool.Get(DataMessage.dataType.MISSILEP2);

            //Set data type
            newMis.myDataType = dataType.MISSILEP2;

            newMis.sendMissile = reader.ReadBoolean();

            return newMis;
        }

        //Putting the responsibility of managing missile count on the server
        public override void Execute()
        {
            //Store the position of the ship when missile is fired
            shipPosMissile = plrMgr.P2Data.ship.GetPixelPosition();
            shipAngleMissile = plrMgr.P2Data.ship.GetAngle_Deg();

            //Store time of execution
            posTime = TimeManager.GetCurrentTime();

            //Fire missile
            plrMgr.P2Data.FireMissile();

            sendMissile = true;
        }

        //Release an object back into the pool to be reused
        public override void ReleaseMsg()
        {
            sendMissile = false;
        }

        public override void PrintMe()
        {
            Debug.Print("Event Msg: (" + mySendType + ", P2FIREMISSILE" + ") Player 2 (sent on frame " + GameManager.GetFrameCount() + ")");
        }
    }

    [Serializable]
    public class P1MoveRotMsg : DataMessage
    {
        //Serialize position of the ship
        public override void Serialize(ref BinaryWriter writer)
        {
            //Store the position of the ship after movement
            Vec2 pos = plrMgr.P1Data.ship.GetPixelPosition();
            float angle = plrMgr.P1Data.ship.GetAngle_Deg();

            //Write the type of message being sent
            writer.Write((byte)dataType.MOVEP1);

            //Write the time at which the ship moved
            writer.Write(posTime);

            //Write move data
            writer.Write(pos.X);
            writer.Write(pos.Y);
            writer.Write(angle);

            //if dead-reckoning is on, write speed of ship to msg
            if (GameSceneCollection.ScenePlay.DeadReckoningOn)
            {
                Vec2 speed = plrMgr.P1Data.ship.GetPixelVelocity();
                writer.Write(speed.X);
                writer.Write(speed.Y);

                //Ship pos will be sent in port 0 with reliable sequenced settings
                //myDelivType = NetDeliveryMethod.ReliableSequenced;
                myDelivType = NetDeliveryMethod.ReliableOrdered;
                myPortNum = 0;

                //Set prediction data of ship for dead-reckoning
                plrMgr.P1Data.ship.predPlr.Set(pos, speed, posTime, angle);
            }
            else
            {
                //MISSILE POS
                //Store the number of missiles present
                int count = plrMgr.P1Data.missileList.Count;

                //Write the number of missiles present
                writer.Write(count);

                //Iterate through each missile and store their data
                foreach (Missile m in plrMgr.P1Data.missileList)
                {
                    pos = m.GetPixelPosition();
                    angle = m.GetAngle_Deg();

                    //Write the ID of the missile so the client can find it 
                    writer.Write(m.getID());

                    writer.Write(pos.X);
                    writer.Write(pos.Y);
                    writer.Write(angle);
                }

                //Ship pos will be sent in port 0 with unreliable sequenced settings
                myDelivType = NetDeliveryMethod.UnreliableSequenced;
                myPortNum = 0;
            }
        }

        //Serialize inputs for the record/playback file
        public override void SerializeRecord(ref BinaryWriter writer)
        {
            //Write the type of message being sent
            writer.Write((byte)dataType.MOVEP1);

            //Write move data
            //Move
            writer.Write(vertInput);

            //Rotate
            writer.Write(horzInput);
        }

        //Store the button inputs of the client
        public static new DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            DataMessage output = objPool.Get(DataMessage.dataType.MOVEP1);

            //Set data type
            output.myDataType = dataType.MOVEP1;

            //Move
            output.vertInput = reader.ReadInt32();

            //Rotate
            output.horzInput = reader.ReadInt32();

            return output;
        }

        //Can move the ship using regular methods in our own execute as the server, and will write the position to the buffer
        public override void Execute()
        {
            posTime = TimeManager.GetCurrentTime();

            plrMgr.P1Data.ship.Move(vertInput);
            plrMgr.P1Data.ship.Rotate(horzInput);

            //Set the prediction data that the client will be using initially
            if (GameSceneCollection.ScenePlay.DeadReckoningOn)
            {
                //plrMgr.P1Data.ship.predPlr.Set(plrMgr.P1Data.ship.GetPixelPosition(), plrMgr.P1Data.ship.GetPixelVelocity(), posTime, plrMgr.P1Data.ship.GetAngle_Deg());
                if (!plrMgr.P1Data.ship.predPlr.initPred)
                {
                    plrMgr.P1Data.ship.predPlr.Set(plrMgr.P1Data.ship.GetPixelPosition(), plrMgr.P1Data.ship.GetPixelVelocity(), posTime, plrMgr.P1Data.ship.GetAngle_Deg());
                }
            }
        }

        //Release an object back into the pool to be reused
        public override void ReleaseMsg()
        {

        }

        public override void PrintMe()
        {
            Debug.Print("Event Msg: (" + mySendType + ", P1MOVE/MISSILE" + ") Player 1 (sent on frame " + GameManager.GetFrameCount() + ")");

            int count = 0;

            foreach (Missile m in GameSceneCollection.ScenePlay.PlayerMgr.P1Data.missileList)
            {
                if (count == 0)
                {
                    Debug.Print("Missile 1 ID: " + m.getID());
                }
                else if (count == 1)
                {
                    Debug.Print("Missile 2 ID: " + m.getID());
                }
                else if (count == 2)
                {
                    Debug.Print("Missile 3 ID: " + m.getID());
                }

                count++;
            }
        }
    }

    [Serializable]
    public class P2MoveRotMsg : DataMessage
    {
        //Serialize position of the ship
        public override void Serialize(ref BinaryWriter writer)
        {
            //Store the position of the ship after movement
            Vec2 pos = plrMgr.P2Data.ship.GetPixelPosition();
            float angle = plrMgr.P2Data.ship.GetAngle_Deg();

            //Write the type of message being sent
            writer.Write((byte)dataType.MOVEP2);

            //Write the time at which the ship moved
            writer.Write(posTime);

            //Write move data
            writer.Write(pos.X);
            writer.Write(pos.Y);
            writer.Write(angle);

            //if dead-reckoning is on, write speed of ship to msg
            if (GameSceneCollection.ScenePlay.DeadReckoningOn)
            {
                Vec2 speed = plrMgr.P2Data.ship.GetPixelVelocity();
                writer.Write(speed.X);
                writer.Write(speed.Y);

                //Ship pos will be sent in port 0 with reliable sequenced settings
                //myDelivType = NetDeliveryMethod.ReliableSequenced;
                myDelivType = NetDeliveryMethod.ReliableOrdered;
                myPortNum = 1;

                //Set prediction data of ship for dead-reckoning
                plrMgr.P2Data.ship.predPlr.Set(pos, speed, posTime, angle);
            }
            else
            {
                //MISSILE POS
                //Store the number of missiles present
                int count = plrMgr.P2Data.missileList.Count;

                //Write the number of missiles present
                writer.Write(count);

                //Iterate through each missile and store their data
                foreach (Missile m in plrMgr.P2Data.missileList)
                {
                    pos = m.GetPixelPosition();
                    angle = m.GetAngle_Deg();

                    //Write the ID of the missile so the client can find it 
                    writer.Write(m.getID());

                    writer.Write(pos.X);
                    writer.Write(pos.Y);
                    writer.Write(angle);
                }

                //Ship pos will be sent in port 0 with unreliable sequenced settings
                myDelivType = NetDeliveryMethod.UnreliableSequenced;
                myPortNum = 0;
            }
        }

        //Serialize inputs for the record/playback file
        public override void SerializeRecord(ref BinaryWriter writer)
        {
            //Write the type of message being sent
            writer.Write((byte)dataType.MOVEP2);

            //Write move data
            //Move
            writer.Write(vertInput);

            //Rotate
            writer.Write(horzInput);
        }

        //Store the button inputs of the client
        public static new DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            DataMessage output = objPool.Get(DataMessage.dataType.MOVEP2);

            //Set data type
            output.myDataType = dataType.MOVEP2;

            //Move
            output.vertInput = reader.ReadInt32();

            //Rotate
            output.horzInput = reader.ReadInt32();

            return output;
        }

        //Can move the ship using regular methods in our own execute as the server, and will write the position to the buffer
        public override void Execute()
        {
            posTime = TimeManager.GetCurrentTime();

            plrMgr.P2Data.ship.Move(vertInput);
            plrMgr.P2Data.ship.Rotate(horzInput);

            //Set the prediction data that the client will be using initially
            if (GameSceneCollection.ScenePlay.DeadReckoningOn)
            {
                if (!plrMgr.P2Data.ship.predPlr.initPred)
                {
                    plrMgr.P2Data.ship.predPlr.Set(plrMgr.P2Data.ship.GetPixelPosition(), plrMgr.P1Data.ship.GetPixelVelocity(), posTime, plrMgr.P1Data.ship.GetAngle_Deg());
                }
            }
        }

        //Release an object back into the pool to be reused
        public override void ReleaseMsg()
        {

        }

        public override void PrintMe()
        {
            Debug.Print("Event Msg: (" + mySendType + ", P2MOVE/MISSILE" + ") Player 2 (sent on frame " + GameManager.GetFrameCount() + ")");

            int count = 0;

            foreach (Missile m in GameSceneCollection.ScenePlay.PlayerMgr.P2Data.missileList)
            {
                if (count == 0)
                {
                    Debug.Print("Missile 1 ID: " + m.getID());
                }
                else if (count == 1)
                {
                    Debug.Print("Missile 2 ID: " + m.getID());
                }
                else if (count == 2)
                {
                    Debug.Print("Missile 3 ID: " + m.getID());
                }

                count++;
            }
        }
    }

    public class P1LayMine : DataMessage
    {
        //Serialize the laying of player 1's mine
        public override void Serialize(ref BinaryWriter writer)
        {
            //Write the type of message being sent
            writer.Write((byte)dataType.MINEP1);

            //Write the notice that a mine was laid
            writer.Write(layMyMine);

            //Lay mine message will be sent in port 2 with unreliable sequenced settings
            myDelivType = NetDeliveryMethod.ReliableUnordered;
            myPortNum = 0;
        }

        //Serialize the laying of player 1's mine for record/playback from file
        public override void SerializeRecord(ref BinaryWriter writer)
        {
            //Write the type of message being sent
            writer.Write((byte)dataType.MINEP1);

            //Write the notice that a mine was laid
            writer.Write(layMyMine);
        }

        //Store the button inputs of the client
        public static new DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            DataMessage newMine = objPool.Get(DataMessage.dataType.MINEP1);

            //Set data type
            newMine.myDataType = dataType.MINEP1;

            newMine.layMyMine = reader.ReadBoolean();

            return newMine;
        }

        public override void Execute()
        {
            plrMgr.P1Data.LayMine();
        }

        //Release an object back into the pool to be reused
        public override void ReleaseMsg()
        {
            layMyMine = false;
        }

        public override void PrintMe()
        {
            Debug.Print("Event Msg: (" + mySendType + ", " + myDataType + ") Player 1 (sent on frame " + GameManager.GetFrameCount() + ")");
        }
    }

    public class P2LayMine : DataMessage
    {
        //Serialize the laying of player 2's mine
        public override void Serialize(ref BinaryWriter writer)
        {
            //Write the type of message being sent
            writer.Write((byte)dataType.MINEP2);

            //Write the notice that a mine was laid
            writer.Write(layMyMine);

            //Lay mine message will be sent in port 2 with unreliable sequenced settings
            myDelivType = NetDeliveryMethod.ReliableUnordered;
            myPortNum = 0;
        }

        //Serialize the laying of player 2's mine for record/playback from file
        public override void SerializeRecord(ref BinaryWriter writer)
        {
            //Write the type of message being sent
            writer.Write((byte)dataType.MINEP2);

            //Write the notice that a mine was laid
            writer.Write(layMyMine);
        }

        //Store the button inputs of the client
        public static new DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            DataMessage newMine = objPool.Get(DataMessage.dataType.MINEP2);

            //Set data type
            newMine.myDataType = dataType.MINEP2;

            newMine.layMyMine = reader.ReadBoolean();

            return newMine;
        }

        public override void Execute()
        {
            plrMgr.P2Data.LayMine();
        }

        //Release an object back into the pool to be reused
        public override void ReleaseMsg()
        {
            layMyMine = false;
        }

        public override void PrintMe()
        {
            Debug.Print("Event Msg: (" + mySendType + ", " + myDataType + ") Player 2 (sent on frame " + GameManager.GetFrameCount() + ")");
        }
    }

    public class CollideMsg : DataMessage
    {


        public CollideMsg(GameObject _objA, GameObject _objB)
        {
            gObjA = _objA;
            gObjB = _objB;

            this.mySendType = msgType.LOCAL_NET;

            myDataType = dataType.COLLIDE;
        }

        public override void SetCollide(GameObject _objA, GameObject _objB)
        {
            gObjA = _objA;
            gObjB = _objB;
        }

        //Serialize the collision data
        public override void Serialize(ref BinaryWriter writer)
        {
            //Write the type of message being sent
            writer.Write((byte)dataType.COLLIDE);

            //Write the position where the mine was laid
            writer.Write(gObjA.getID());
            writer.Write(gObjB.getID());

            //Collide message will be sent in it's own channel so that other messages do not interfere with the ordering
            myDelivType = NetDeliveryMethod.ReliableOrdered;
            myPortNum = 0;
        }

        //Serialize the collision data
        public override void SerializeRecord(ref BinaryWriter writer)
        {
            //Write the type of message being sent
            writer.Write((byte)dataType.COLLIDE);

            //Write the position where the mine was laid
            writer.Write(gObjA.getID());
            writer.Write(gObjB.getID());
        }

        //Store the visitor pattern from the file for playback
        public static new DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            //Read the ID's of the objects from the buffer
            int ID_objA = reader.ReadInt32();
            int ID_objB = reader.ReadInt32();

            //Store game object references by ID
            GameObject _ObjA = GameManager.Find(ID_objA);
            GameObject _ObjB = GameManager.Find(ID_objB);

            //Find the game objects using the IDS
            DataMessage newColl = GameSceneCollection.ScenePlay.PoolMgr.Get(_ObjA, _ObjB);

            //Set data type
            newColl.myDataType = dataType.COLLIDE;

            return newColl;
        }


        public override void Execute()
        {
            if (gObjA != null && gObjB != null)
            {
                gObjA.Accept(gObjB);
            }
            
        }

        //Release an object back into the pool to be reused
        public override void ReleaseMsg()
        {

            gObjA = null;
            gObjB = null;

            GameSceneCollection.ScenePlay.PoolMgr.BackToStack(this);
        }

        public override void PrintMe()
        {
            if (gObjA != null && gObjB != null)
            {
                Debug.Print("Event Msg: (" + mySendType + ", " + myDataType + ") " + gObjA.getID() + " vs " + gObjB.getID() + " (sent on frame " + GameManager.GetFrameCount() + ")");
            }

            
        }
    }

    public class TimeMsg : DataMessage
    {
        public TimeMsg()
        {
            this.mySendType = msgType.NET;

            myDataType = dataType.TIME;
        }

        //Serialize the collision data
        public override void Serialize(ref BinaryWriter writer)
        {
            //Write the type of message being sent
            writer.Write((byte)dataType.TIME);

            //Mark the current time of server
            sendTime = TimeManager.GetCurrentTime();

            //Write the current time
            writer.Write(sendTime);

            //Need to have it set to reliable unordered as in order to start the game we need synced time. Can not have packet loss.
            myDelivType = NetDeliveryMethod.ReliableUnordered;
            myPortNum = 0;
        }

        //Serialize the collision data
        public override void SerializeRecord(ref BinaryWriter writer)
        {
            //Write the type of message being sent
            writer.Write((byte)dataType.TIME);
        }

        //Store the visitor pattern from the file for playback
        public static new DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            //Find the game objects using the IDS
            DataMessage newTime = GameSceneCollection.ScenePlay.PoolMgr.Get(dataType.TIME);

            //Set data type
            newTime.myDataType = dataType.TIME;

            //Set send type to only be sent to the client
            newTime.mySendType = msgType.NET; 

            return newTime;
        }


        public override void Execute()
        {
            
        }

        //Release an object back into the pool to be reused
        public override void ReleaseMsg()
        {
            sendTime = 0.0f;
        }
    }

    public class ObjectPool
    {
        private DataMessage[] objects;

        private Stack<DataMessage> collStack;

        public ObjectPool()
        {
            // Initially nothing in the pool
            //Pool indices corresspond to enum values
            objects = new DataMessage[7];

            collStack = new Stack<DataMessage>();
        }

        //For player input message types
        public DataMessage Get(DataMessage.dataType myData)
        {
            // If empty, make a new DataMessage
            // Otherwise, get one from the pool
            return objects[(int)myData] == null ? findType(myData) : objects[(int)myData];
        }

        //For collision messages, enum index 8
        public DataMessage Get(GameObject gObjA, GameObject gObjB)
        {
            // If empty, make a new DataMessage
            // Otherwise, get one from the stack
            DataMessage storeData;

            if (collStack.Count == 0)
            {
                storeData = new CollideMsg(gObjA, gObjB);
            }
            else
            {
                storeData = collStack.Pop();
                storeData.SetCollide(gObjA, gObjB);
            }

            return storeData;
        }

        //Decide which message to init depending on type
        public DataMessage findType(DataMessage.dataType myData)
        {
            DataMessage storeData;

            switch (myData)
            {
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
            objects[(int)myData] = storeData;

            return storeData;
        }

        //Return a collide message back to the stack
        public void BackToStack(DataMessage collMsg)
        {
            collStack.Push(collMsg);
        }
    }
}

