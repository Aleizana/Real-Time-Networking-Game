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

namespace OmegaRace
{

    [Serializable]
    public class DataMessage
    {
        //Time that the message was executed
        public float myProcTime;

        //For Cristian's Algorithm in Time Sync
        public float time_Zero;
        public float time_Server;
        public float time_One;

        //Button press values
        public int horzInput;
        public int vertInput;

        //Boolean to determine whether lay mine or fire missile actions were pressed
        public bool layMyMine = false;
        public bool sendMissile = false;

        //For collision detection from the server
        public GameObject gObjA;
        public GameObject gObjB;
        public int ID_objA;
        public int ID_objB;

        //Missile ID for pos data
        public int misID1 = 0;
        public int misID2 = 0;
        public int misID3 = 0;

        //Number of missiles received from the server
        public int numMisServer = 0;

        //Positional data for the missiles of both players
        public Vec2 pos1;
        public Vec2 pos2;
        public Vec2 pos3;
        public float angle1;
        public float angle2;
        public float angle3;
        public Vec2 misSpeed1;
        public Vec2 misSpeed2;
        public Vec2 misSpeed3;

        //Positional data for the ships of both players
        public Vec2 shipPos;
        public float shipAngle;
        public Vec2 shipSpeed;

        //For prediction of ship/missile position
        public float arrivalTime;

        //Controls Lidgren delivery type
        public NetDeliveryMethod myDelivType;

        //Controls the channel that a message is sent through
        public int myPortNum;

        //To know whether or not an object was cleared
        public bool released = false;

        //bool to know if a message has been executed yet so that it
        //does not get released
        public bool executed = false;

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

        //Locate player manager
        protected PlayerManager plrMgr = GameSceneCollection.ScenePlay.PlayerMgr;

        //Locate object pool
        protected ObjectPool poolRef = GameSceneCollection.ScenePlay.PoolMgr;

        public virtual void Serialize(ref BinaryWriter writer)
        {
            
        }

        public virtual void SerializeRecord(ref BinaryWriter writer)
        {

        }

        public static DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            //Read the enum data type value of the message
            dataType myDataType = (dataType)reader.ReadByte();

            //Make sure this gets deleted!!!!
            DataMessage storeData;

            switch (myDataType)
            {
                case dataType.MOVEP1:
                    storeData = P1MoveRotMsg.Deserialize(ref reader, objPool);
                    break;
                case dataType.MOVEP2:
                    storeData = P2MoveRotMsg.Deserialize(ref reader, objPool);
                    break;
                case dataType.MISSILEP1:
                    storeData = P1FireMissile.Deserialize(ref reader, objPool);
                    break;
                case dataType.MISSILEP2:
                    storeData = P2FireMissile.Deserialize(ref reader, objPool);
                    break;
                case dataType.MINEP1:
                    storeData = P1LayMine.Deserialize(ref reader, objPool);
                    break;
                case dataType.MINEP2:
                    storeData = P2LayMine.Deserialize(ref reader, objPool);
                    break;
                case dataType.COLLIDE:
                    storeData = CollideMsg.Deserialize(ref reader, objPool);
                    break;
                case dataType.TIME:
                    storeData = TimeMsg.Deserialize(ref reader, objPool);
                    break;
                default:
                    return null;
            }

            storeData.released = false;
            return storeData;
        }

        public void UpdateMissiles(PlayerData plrRef)
        {
            //For holding missile by ID
            Missile updatePos;

            //If at least one missile is present
            if (numMisServer >= 1)
            {
                //Check to make sure missile exists
                updatePos = plrRef.GetMissileByID(misID1);

                if (updatePos != null)
                {
                    //Update pos of missile
                    updatePos.SetPosAndAngle(pos1.X, pos1.Y, angle1);
                }
            }

            if (numMisServer >= 2)
            {
                updatePos = plrRef.GetMissileByID(misID2);

                if (updatePos != null)
                {
                    updatePos.SetPosAndAngle(pos2.X, pos2.Y, angle2);
                }
            }

            if (numMisServer >= 3)
            {
                updatePos = plrRef.GetMissileByID(misID3);

                if (updatePos != null)
                {
                    updatePos.SetPosAndAngle(pos3.X, pos3.Y, angle3);
                }
            }
        }

        public void UpdateMissilesDeadReck(PlayerData plrRef)
        {
            //For holding missile by ID
            Missile updatePos;

            //If at least one missile is present
            if (numMisServer >= 1)
            {
                //Check to make sure missile exists
                updatePos = plrRef.GetMissileByID(misID1);

                if (updatePos != null)
                {
                    //Set prediction data for dead reckoning on missile
                    updatePos.deadReckMiss.Set(updatePos, pos1, misSpeed1, angle1, arrivalTime);

                    //Update pos of missile
                    updatePos.SetPosAndAngle(pos1.X, pos1.Y, angle1);
                }
            }

            if (numMisServer >= 2)
            {
                updatePos = plrRef.GetMissileByID(misID2);

                if (updatePos != null)
                {
                    //Set prediction data for dead reckoning on missile
                    updatePos.deadReckMiss.Set(updatePos, pos2, misSpeed2, angle2, arrivalTime);

                    updatePos.SetPosAndAngle(pos2.X, pos2.Y, angle2);
                }
            }

            if (numMisServer >= 3)
            {
                updatePos = plrRef.GetMissileByID(misID3);

                if (updatePos != null)
                {
                    //Set prediction data for dead reckoning on missile
                    updatePos.deadReckMiss.Set(updatePos, pos3, misSpeed3, angle3, arrivalTime);

                    updatePos.SetPosAndAngle(pos3.X, pos3.Y, angle3);
                }
            }
        }

        public void UpdateMissilesClientPred(PlayerData plrRef)
        {
            //For holding missile by ID
            Missile updatePos;

            //If at least one missile is present
            if (numMisServer >= 1)
            {
                //Check to make sure missile exists
                updatePos = plrRef.GetMissileByID(misID1);

                if (updatePos != null)
                {
                    //Update client side prediction values for missile
                    updatePos.predControl.Set(updatePos, angle1, pos1, arrivalTime);

                    //Update pos of missile
                    updatePos.SetPosAndAngle(pos1.X, pos1.Y, angle1);
                }
            }

            if (numMisServer >= 2)
            {
                updatePos = plrRef.GetMissileByID(misID2);

                if (updatePos != null)
                {
                    //Update client side prediction values for missile
                    updatePos.predControl.Set(updatePos, angle2, pos2, arrivalTime);

                    updatePos.SetPosAndAngle(pos2.X, pos2.Y, angle2);
                }
            }

            if (numMisServer >= 3)
            {
                updatePos = plrRef.GetMissileByID(misID3);

                if (updatePos != null)
                {
                    //Update client side prediction values for missile
                    updatePos.predControl.Set(updatePos, angle3, pos3, arrivalTime);

                    updatePos.SetPosAndAngle(pos3.X, pos3.Y, angle3);
                }
            }
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

    [Serializable]
    public class P1FireMissile : DataMessage
    {
        //Serialize the sending of player 1's missile
        public override void Serialize(ref BinaryWriter writer)
        {
            writer.Write((byte)dataType.MISSILEP1);

            writer.Write(sendMissile);

            //Fire missile message will be sent in port 0 with reliable ordered settings
            myDelivType = NetDeliveryMethod.ReliableOrdered;
            myPortNum = 0;
        }

        //Serialize the sending of player 1's missile for recording/playback
        public override void SerializeRecord(ref BinaryWriter writer)
        {
            writer.Write((byte)dataType.MISSILEP1);

            writer.Write(sendMissile);

            if (sendMissile)
            {
                //Write the time of execution of msg
                writer.Write(arrivalTime);

                //Dead-reckoning will send missile position data once along with missile fire message
                if (GameSceneCollection.ScenePlay.ServerSidePrediction)
                {
                    if (numMisServer >= 1)
                    {
                        //So deserialize knows how many missiles there are by true/false marks
                        writer.Write(true);
                        writer.Write(misID1);
                        writer.Write(pos1.X);
                        writer.Write(pos1.Y);
                        writer.Write(angle1);
                        writer.Write(misSpeed1.X);
                        writer.Write(misSpeed1.Y);
                    }

                    if (numMisServer >= 2)
                    {
                        //So deserialize knows how many missiles there are by true/false marks
                        writer.Write(true);
                        writer.Write(misID2);
                        writer.Write(pos2.X);
                        writer.Write(pos2.Y);
                        writer.Write(angle2);
                        writer.Write(misSpeed2.X);
                        writer.Write(misSpeed2.Y);
                    }

                    if (numMisServer >= 3)
                    {
                        //So deserialize knows how many missiles there are by true/false marks
                        writer.Write(true);
                        writer.Write(misID3);
                        writer.Write(pos3.X);
                        writer.Write(pos3.Y);
                        writer.Write(angle3);
                        writer.Write(misSpeed3.X);
                        writer.Write(misSpeed3.Y);
                    }

                    //Mark the end of missile processing
                    writer.Write(false);
                }
                else
                {
                    //Write move data if can fire missile
                    writer.Write(shipPos.X);
                    writer.Write(shipPos.Y);
                    writer.Write(shipAngle);
                }
            }
        }

        //Only the boolean of whether or not a missile was sent needs to be read
        public static new DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            DataMessage newMis = objPool.Get(DataMessage.dataType.MISSILEP1);

            //Set data type
            newMis.myDataType = dataType.MISSILEP1;

            //Read if can fire missile
            newMis.sendMissile = reader.ReadBoolean();

            //If missile can be fired, read ship pos data
            if (newMis.sendMissile)
            {
                //Read time of execution
                newMis.arrivalTime = reader.ReadSingle();

                //Dead-reckoning will send missile position data once along with missile fire message
                if (GameSceneCollection.ScenePlay.ServerSidePrediction)
                {
                    //Read if there is missile pos information following
                    bool readMiss = reader.ReadBoolean();

                    //To increment through missiles
                    int count = 1;

                    //When there is no more data to be read, readMiss will find "false" and stop execution
                    while (readMiss)
                    {
                        if (count == 1)
                        {
                            newMis.misID1 = reader.ReadInt32();
                            newMis.pos1.X = reader.ReadSingle();
                            newMis.pos1.Y = reader.ReadSingle();
                            newMis.angle1 = reader.ReadSingle();
                            newMis.misSpeed1.X = reader.ReadSingle();
                            newMis.misSpeed1.Y = reader.ReadSingle();
                        }
                        else if (count == 2)
                        {
                            newMis.misID2 = reader.ReadInt32();
                            newMis.pos2.X = reader.ReadSingle();
                            newMis.pos2.Y = reader.ReadSingle();
                            newMis.angle2 = reader.ReadSingle();
                            newMis.misSpeed2.X = reader.ReadSingle();
                            newMis.misSpeed2.Y = reader.ReadSingle();
                        }
                        else if (count == 3)
                        {
                            newMis.misID3 = reader.ReadInt32();
                            newMis.pos3.X = reader.ReadSingle();
                            newMis.pos3.Y = reader.ReadSingle();
                            newMis.angle3 = reader.ReadSingle();
                            newMis.misSpeed3.X = reader.ReadSingle();
                            newMis.misSpeed3.Y = reader.ReadSingle();
                        }

                        //Read if there is missile pos information following
                        readMiss = reader.ReadBoolean();
                    }

                    //Store the number of missiles read
                    newMis.numMisServer = count;
                }
                //Regular processing will use ship position to verify missile location
                else
                {
                    newMis.shipPos.X = reader.ReadSingle();
                    newMis.shipPos.Y = reader.ReadSingle();
                    newMis.shipAngle = reader.ReadSingle();
                }

                
            }

            return newMis;
        }

        //Putting the responsibility of managing missile count on the server
        public override void Execute()
        {
            //Debug.Print("Missile b4 fire: " + plrMgr.P1Data.missileList.Count);

            //If sendMissile becomes false for whatever reason while a missile is supposed to have fired, the angle NOT being 0.0f will allow us to fire
            if (sendMissile || shipAngle != 0.0f || (shipPos.X != 0.0f && shipPos.Y != 0.0f))
            {
                //Check for client side prediction
                if (GameSceneCollection.ScenePlay.ClientSidePrediction)
                {
                    //Adjust prediction data to match ship fire pos
                    plrMgr.P1Data.ship.predControl.Set(plrMgr.P1Data.ship, shipAngle, shipPos, arrivalTime);
                    
                    //Make sure that ship is firing in the same pos for both players
                    plrMgr.P1Data.ship.SetPosAndAngle(shipPos.X, shipPos.Y, shipAngle);
                }

                //Fire a missile for P1
                plrMgr.P1Data.FireMissile();

                //Check for dead reckoning and set missile prediction data if true
                if (GameSceneCollection.ScenePlay.ServerSidePrediction)
                {
                    UpdateMissilesDeadReck(plrMgr.P1Data);
                }
            }
        }

        //Release an object back into the pool to be reused
        public override void ReleaseMsg()
        {
            sendMissile = false;

            shipPos.X = 0.0f;
            shipPos.Y = 0.0f;
            shipAngle = 0.0f;
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

            //Fire missile message will be sent in port 2 with unreliable sequenced settings
            myDelivType = NetDeliveryMethod.ReliableOrdered;
            myPortNum = 0;
        }

        //Serialize the sending of player 2's missile
        public override void SerializeRecord(ref BinaryWriter writer)
        {
            writer.Write((byte)dataType.MISSILEP2);

            //Assuring that send missile be written correctly
            if (shipAngle != 0.0f || (shipPos.X != 0.0f && shipPos.Y != 0.0f))
            {
                sendMissile = true;
            }

            writer.Write(sendMissile);

            if (sendMissile)
            {
                //Write the time of execution of msg
                writer.Write(arrivalTime);

                //Dead-reckoning will send missile position data once along with missile fire message
                if (GameSceneCollection.ScenePlay.ServerSidePrediction)
                {
                    if (numMisServer >= 1)
                    {
                        //So deserialize knows how many missiles there are by true/false marks
                        writer.Write(true);
                        writer.Write(misID1);
                        writer.Write(pos1.X);
                        writer.Write(pos1.Y);
                        writer.Write(angle1);
                        writer.Write(misSpeed1.X);
                        writer.Write(misSpeed1.Y);
                    }

                    if (numMisServer >= 2)
                    {
                        //So deserialize knows how many missiles there are by true/false marks
                        writer.Write(true);
                        writer.Write(misID2);
                        writer.Write(pos2.X);
                        writer.Write(pos2.Y);
                        writer.Write(angle2);
                        writer.Write(misSpeed2.X);
                        writer.Write(misSpeed2.Y);
                    }

                    if (numMisServer >= 3)
                    {
                        //So deserialize knows how many missiles there are by true/false marks
                        writer.Write(true);
                        writer.Write(misID3);
                        writer.Write(pos3.X);
                        writer.Write(pos3.Y);
                        writer.Write(angle3);
                        writer.Write(misSpeed3.X);
                        writer.Write(misSpeed3.Y);
                    }

                    //Mark the end of missile processing
                    writer.Write(false);
                }
                else
                {
                    //Write move data if can fire missile
                    writer.Write(shipPos.X);
                    writer.Write(shipPos.Y);
                    writer.Write(shipAngle);
                }
            }
        }

        //Only the boolean of whether or not a missile was sent needs to be read
        public static new DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            DataMessage newMis = objPool.Get(DataMessage.dataType.MISSILEP2);

            //Set data type
            newMis.myDataType = dataType.MISSILEP2;

            //Read if can fire missile
            newMis.sendMissile = reader.ReadBoolean();

            //If missile can be fired, read ship pos data
            if (newMis.sendMissile)
            {
                //Read time of execution
                newMis.arrivalTime = reader.ReadSingle();

                //Dead-reckoning will send missile position data once along with missile fire message
                if (GameSceneCollection.ScenePlay.ServerSidePrediction)
                {
                    //Read if there is missile pos information following
                    bool readMiss = reader.ReadBoolean();

                    //To increment through missiles
                    int count = 1;

                    //When there is no more data to be read, readMiss will find "false" and stop execution
                    while (readMiss)
                    {
                        if (count == 1)
                        {
                            newMis.misID1 = reader.ReadInt32();
                            newMis.pos1.X = reader.ReadSingle();
                            newMis.pos1.Y = reader.ReadSingle();
                            newMis.angle1 = reader.ReadSingle();
                            newMis.misSpeed1.X = reader.ReadSingle();
                            newMis.misSpeed1.Y = reader.ReadSingle();
                        }
                        else if (count == 2)
                        {
                            newMis.misID2 = reader.ReadInt32();
                            newMis.pos2.X = reader.ReadSingle();
                            newMis.pos2.Y = reader.ReadSingle();
                            newMis.angle2 = reader.ReadSingle();
                            newMis.misSpeed2.X = reader.ReadSingle();
                            newMis.misSpeed2.Y = reader.ReadSingle();
                        }
                        else if (count == 3)
                        {
                            newMis.misID3 = reader.ReadInt32();
                            newMis.pos3.X = reader.ReadSingle();
                            newMis.pos3.Y = reader.ReadSingle();
                            newMis.angle3 = reader.ReadSingle();
                            newMis.misSpeed3.X = reader.ReadSingle();
                            newMis.misSpeed3.Y = reader.ReadSingle();
                        }

                        //Read if there is missile pos information following
                        readMiss = reader.ReadBoolean();

                        count++;
                    }

                    //Store the number of missiles read
                    newMis.numMisServer = count;
                }
                //Regular processing will use ship position to verify missile location
                else
                {
                    newMis.shipPos.X = reader.ReadSingle();
                    newMis.shipPos.Y = reader.ReadSingle();
                    newMis.shipAngle = reader.ReadSingle();
                }


            }

            return newMis;
        }

        //Putting the responsibility of managing missile count on the server
        public override void Execute()
        {
            //If sendMissile becomes false for whatever reason while a missile is supposed to have fired, the angle NOT being 0.0f will allow us to fire
            if (sendMissile || shipAngle != 0.0f || (shipPos.X != 0.0f && shipPos.Y != 0.0f))
            {
                //Check for client side prediction
                if (GameSceneCollection.ScenePlay.ClientSidePrediction)
                {
                    //Adjust prediction data to match ship fire pos
                    plrMgr.P2Data.ship.predControl.Set(plrMgr.P2Data.ship, shipAngle, shipPos, arrivalTime);

                    //Make sure that ship is firing in the same pos for both players
                    plrMgr.P2Data.ship.SetPosAndAngle(shipPos.X, shipPos.Y, shipAngle);
                }

                //Fire a missile for P2
                plrMgr.P2Data.FireMissile();

                //Check for dead reckoning and set missile prediction data if true
                if (GameSceneCollection.ScenePlay.ServerSidePrediction)
                {
                    UpdateMissilesDeadReck(plrMgr.P2Data);
                }
            }

        }

        //Release an object back into the pool to be reused
        public override void ReleaseMsg()
        {
            sendMissile = false;

            shipPos.X = 0;
            shipPos.Y = 0;
            shipAngle = 0;
        }

        public override void PrintMe()
        {
            Debug.Print("Event Msg: (" + mySendType + ", P2FIREMISSILE" + ") Player 2 (sent on frame " + GameManager.GetFrameCount() + ")");
        }
    }

    [Serializable]
    public class P1MoveRotMsg : DataMessage
    {
        //Serialize the movement inputs of player 1
        public override void Serialize(ref BinaryWriter writer)
        {
            //Write the type of message being sent
            writer.Write((byte)dataType.MOVEP1);

            //Move
            writer.Write(vertInput);

            //Rotate
            writer.Write(horzInput);

            //Different send types based on how networking runs
            if (GameSceneCollection.ScenePlay.ServerSidePrediction || GameSceneCollection.ScenePlay.ClientSidePrediction)
            {
                //Ship pos will be sent in port 0 with reliable sequenced settings
                myDelivType = NetDeliveryMethod.ReliableOrdered;
                myPortNum = 0;
            }
            else
            {
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

            //Write time for prediction
            writer.Write(arrivalTime);

            //Write move data
            writer.Write(shipPos.X);
            writer.Write(shipPos.Y);
            writer.Write(shipAngle);

            //Write ship speed if dead reckoning is on, missile pos is not required
            if (GameSceneCollection.ScenePlay.ServerSidePrediction)
            {
                writer.Write(shipSpeed.X);
                writer.Write(shipSpeed.Y);
            }
            //Write missile pos otherwise
            else
            {
                //MISSILE POSITION
                //Store the number of missiles present from the server
                writer.Write(numMisServer);

                //If the number of missiles are present, then read and store the ID and the positional data
                if (numMisServer >= 1)
                {
                    writer.Write(misID1);
                    writer.Write(pos1.X);
                    writer.Write(pos1.Y);
                    writer.Write(angle1);
                }

                if (numMisServer >= 2)
                {
                    writer.Write(misID2);
                    writer.Write(pos2.X);
                    writer.Write(pos2.Y);
                    writer.Write(angle2);
                }

                if (numMisServer >= 3)
                {
                    writer.Write(misID3);
                    writer.Write(pos3.X);
                    writer.Write(pos3.Y);
                    writer.Write(angle3);
                }
            }
        }

        //store the position of player 1's ship from the server
        public static new DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            DataMessage output = objPool.Get(DataMessage.dataType.MOVEP1);

            //SHIP POS
            //Set data type
            output.myDataType = dataType.MOVEP1;

            //Read the time at which the ship moved
            output.arrivalTime = reader.ReadSingle();

            output.shipPos.X = reader.ReadSingle();
            output.shipPos.Y = reader.ReadSingle();

            output.shipAngle = reader.ReadSingle();

            //If dead reckoning is on, missile pos is not sent with ship pos and read ship speed instead
            if (GameSceneCollection.ScenePlay.ServerSidePrediction)
            {
                output.shipSpeed.X = reader.ReadSingle();
                output.shipSpeed.Y = reader.ReadSingle();
                GameSceneCollection.ScenePlay.PlayerMgr.P1Data.ship.deadReckShip.initPred = true;
            }
            //Deserialize missile pos if not
            else
            {
                //MISSILE POS
                //Store the number of missiles present from the server
                output.numMisServer = reader.ReadInt32();

                //If the number of missiles are present, then read and store the ID and the positional data
                if (output.numMisServer >= 1)
                {
                    output.misID1 = reader.ReadInt32();

                    output.pos1.X = reader.ReadSingle();
                    output.pos1.Y = reader.ReadSingle();
                    output.angle1 = reader.ReadSingle();
                }

                if (output.numMisServer >= 2)
                {
                    output.misID2 = reader.ReadInt32();

                    output.pos2.X = reader.ReadSingle();
                    output.pos2.Y = reader.ReadSingle();
                    output.angle2 = reader.ReadSingle();
                }

                if (output.numMisServer >= 3)
                {
                    output.misID3 = reader.ReadInt32();

                    output.pos3.X = reader.ReadSingle();
                    output.pos3.Y = reader.ReadSingle();
                    output.angle3 = reader.ReadSingle();
                }
            }

            return output;
        }

        //Change the position of player 1's ship using pos from server
        public override void Execute()
        {
            //If client side prediction is turned on
            if (GameSceneCollection.ScenePlay.ClientSidePrediction)
            {
                //Set prediction data for client side prediction
                plrMgr.P1Data.ship.predControl.Set(plrMgr.P1Data.ship, shipAngle, shipPos, arrivalTime);

                //Update the position of the ship
                plrMgr.P1Data.ship.SetPosAndAngle(shipPos.X, shipPos.Y, shipAngle);

                //Update prediction data for missiles using client side prediction
                UpdateMissilesClientPred(plrMgr.P1Data);
            }
            else if (GameSceneCollection.ScenePlay.ServerSidePrediction)
            {
                //Set prediction data for dead reckoning
                plrMgr.P1Data.ship.deadReckShip.Set(shipPos, shipSpeed, shipAngle, arrivalTime);
                
                //Set the speed of the ship given from the server
                plrMgr.P1Data.ship.SetPixelVelocity(shipSpeed);

                //Update the position of the ship
                plrMgr.P1Data.ship.SetPosAndAngle(shipPos.X, shipPos.Y, shipAngle);
            }
            else
            {
                //Update the position of the ship
                plrMgr.P1Data.ship.SetPosAndAngle(shipPos.X, shipPos.Y, shipAngle);

                //Update position of missiles with no prediction
                UpdateMissiles(plrMgr.P1Data);
            }
        }
        

        public override void ReleaseMsg()
        {
            shipPos.X = 0.0f;
            shipPos.Y = 0.0f;
            shipAngle = 0.0f;
            horzInput = 0;
            vertInput = 0;

            pos1.X = 0.0f;
            pos2.X = 0.0f;
            pos3.X = 0.0f;
            pos1.Y = 0.0f;
            pos2.Y = 0.0f;
            pos3.Y = 0.0f;
            angle1 = 0.0f;
            angle2 = 0.0f;
            angle3 = 0.0f;

            misID1 = 0;
            misID3 = 0;
            misID3 = 0;

            numMisServer = 0;
        }

        public override void PrintMe()
        {
            Debug.Print("Event Msg: (" + mySendType + ", P1MOVE/MISSILE" + ") Player 1 (sent on frame " + GameManager.GetFrameCount() + ")");

            int count = 0;

            Debug.Print("NUM MISSILES: " + numMisServer);

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
        //Serialize the movement inputs of player 2, never used
        public override void Serialize(ref BinaryWriter writer)
        {
            //Move
            writer.Write(vertInput);

            //Rotate
            writer.Write(horzInput);

            //Ship pos will be sent in port 0 with unreliable sequenced settings
            myDelivType = NetDeliveryMethod.UnreliableSequenced;
            myPortNum = 1;
        }

        //Serialize inputs for the record/playback file
        public override void SerializeRecord(ref BinaryWriter writer)
        {
            //Write the type of message being sent
            writer.Write((byte)dataType.MOVEP2);

            //Write time for prediction
            writer.Write(arrivalTime);

            //Write move data
            writer.Write(shipPos.X);
            writer.Write(shipPos.Y);
            writer.Write(shipAngle);

            //Write ship speed if dead reckoning is on, missile pos is not required
            if (GameSceneCollection.ScenePlay.ServerSidePrediction)
            {
                writer.Write(shipSpeed.X);
                writer.Write(shipSpeed.Y);
            }
            //Write missile pos otherwise
            else
            {
                //MISSILE POSITION
                //Store the number of missiles present from the server
                writer.Write(numMisServer);

                //If the number of missiles are present, then read and store the ID and the positional data
                if (numMisServer >= 1)
                {
                    writer.Write(misID1);
                    writer.Write(pos1.X);
                    writer.Write(pos1.Y);
                    writer.Write(angle1);
                }

                if (numMisServer >= 2)
                {
                    writer.Write(misID2);
                    writer.Write(pos2.X);
                    writer.Write(pos2.Y);
                    writer.Write(angle2);
                }

                if (numMisServer >= 3)
                {
                    writer.Write(misID3);
                    writer.Write(pos3.X);
                    writer.Write(pos3.Y);
                    writer.Write(angle3);
                }
            }
        }

        //store the position of player 2's ship from the server
        public static new DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            DataMessage output = objPool.Get(DataMessage.dataType.MOVEP2);

            //SHIP POS
            //Set data type
            output.myDataType = dataType.MOVEP2;

            //Read the time at which the ship moved
            output.arrivalTime = reader.ReadSingle();

            output.shipPos.X = reader.ReadSingle();
            output.shipPos.Y = reader.ReadSingle();

            output.shipAngle = reader.ReadSingle();

            //If dead reckoning is on, missile pos is not sent with ship pos and read ship speed instead
            if (GameSceneCollection.ScenePlay.ServerSidePrediction)
            {
                output.shipSpeed.X = reader.ReadSingle();
                output.shipSpeed.Y = reader.ReadSingle();
                GameSceneCollection.ScenePlay.PlayerMgr.P2Data.ship.deadReckShip.initPred = true;
            }
            //Deserialize missile pos if not
            else
            {
                //MISSILE POS
                //Store the number of missiles present from the server
                output.numMisServer = reader.ReadInt32();

                //If the number of missiles are present, then read and store the ID and the positional data
                if (output.numMisServer >= 1)
                {
                    output.misID1 = reader.ReadInt32();

                    output.pos1.X = reader.ReadSingle();
                    output.pos1.Y = reader.ReadSingle();
                    output.angle1 = reader.ReadSingle();
                }

                if (output.numMisServer >= 2)
                {
                    output.misID2 = reader.ReadInt32();

                    output.pos2.X = reader.ReadSingle();
                    output.pos2.Y = reader.ReadSingle();
                    output.angle2 = reader.ReadSingle();
                }

                if (output.numMisServer >= 3)
                {
                    output.misID3 = reader.ReadInt32();

                    output.pos3.X = reader.ReadSingle();
                    output.pos3.Y = reader.ReadSingle();
                    output.angle3 = reader.ReadSingle();
                }
            }


            return output;
        }

        //Change the position of player 2's ship using pos from server
        public override void Execute()
        {
            //If client side prediction is turned on
            if (GameSceneCollection.ScenePlay.ClientSidePrediction)
            {
                //Set prediction data for client side prediction
                plrMgr.P2Data.ship.predControl.Set(plrMgr.P2Data.ship, shipAngle, shipPos, arrivalTime);

                //Update the position of the ship
                plrMgr.P2Data.ship.SetPosAndAngle(shipPos.X, shipPos.Y, shipAngle);

                //Update prediction data for missiles using client side prediction
                UpdateMissilesClientPred(plrMgr.P2Data);
            }
            else if (GameSceneCollection.ScenePlay.ServerSidePrediction)
            {
                //Set prediction data for dead reckoning
                plrMgr.P2Data.ship.deadReckShip.Set(shipPos, shipSpeed, shipAngle, arrivalTime);

                //Set the speed of the ship given from the server
                plrMgr.P2Data.ship.SetPixelVelocity(shipSpeed);

                //Update the position of the ship
                plrMgr.P2Data.ship.SetPosAndAngle(shipPos.X, shipPos.Y, shipAngle);
            }
            else
            {
                //Update the position of the ship
                plrMgr.P2Data.ship.SetPosAndAngle(shipPos.X, shipPos.Y, shipAngle);

                //Update position of missiles with no prediction
                UpdateMissiles(plrMgr.P2Data);
            }
        }

        public override void ReleaseMsg()
        {
            shipPos.X = 0.0f;
            shipPos.Y = 0.0f;
            shipAngle = 0.0f;
            horzInput = 0;
            vertInput = 0;

            pos1.X = 0.0f;
            pos2.X = 0.0f;
            pos3.X = 0.0f;
            pos1.Y = 0.0f;
            pos2.Y = 0.0f;
            pos3.Y = 0.0f;
            angle1 = 0.0f;
            angle2 = 0.0f;
            angle3 = 0.0f;

            misID1 = 0;
            misID3 = 0;
            misID3 = 0;

            numMisServer = 0;
        }

        public override void PrintMe()
        {
            Debug.Print("Event Msg: (" + mySendType + ", P2MOVE/MISSILE" + ") Player 2 (sent on frame " + GameManager.GetFrameCount() + ")");

            int count = 0;

            Debug.Print("NUM MISSILES: " + numMisServer);

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
        //Serialize laying of a mine
        public override void Serialize(ref BinaryWriter writer)
        {
            writer.Write((byte)dataType.MINEP1);

            layMyMine = true;

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

        //Putting responsibility of mine storage on the server
        public override void Execute()
        {
            if (layMyMine)
            {
                plrMgr.P1Data.LayMine();
            }
            
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
        //Serialize laying of a mine
        public override void Serialize(ref BinaryWriter writer)
        {
            writer.Write((byte)dataType.MINEP2);

            layMyMine = true;

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

        //Putting responsibility of mine storage on the server
        public override void Execute()
        {
            if (layMyMine)
            {
                plrMgr.P2Data.LayMine();
            }
            
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
        

        //Serialize the collision data
        public override void SerializeRecord(ref BinaryWriter writer)
        {
            //Write the type of message being sent
            writer.Write((byte)dataType.COLLIDE);

            //Write the position where the mine was laid
            writer.Write(gObjA.getID());

            //Write an invalid ID if the object is null
            if (gObjB == null)
            {
                writer.Write(-1);
            }
            else
            {
                writer.Write(gObjB.getID());
            }

            
        }

        //Store the visitor pattern from the server
        public static new DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            //Find the game msgPool using the IDS
            DataMessage newColl = GameSceneCollection.ScenePlay.PoolMgr.Get(DataMessage.dataType.COLLIDE);

            //Set data type
            newColl.myDataType = dataType.COLLIDE;

            //Read the ID's of the msgPool from the buffer
            newColl.ID_objA = reader.ReadInt32();
            newColl.ID_objB = reader.ReadInt32();

            //Store game object references by ID
            newColl.gObjA = GameManager.Find(newColl.ID_objA);
            newColl.gObjB = GameManager.Find(newColl.ID_objB);

            return newColl;
        }

        //Process collision
        public override void Execute()
        {
            if (gObjA != null && gObjB != null)
            {
                gObjA.Accept(gObjB);
            }
            //Attempt to find object again by searching for it's ID
            else
            {
                FindMissile(GameManager.Find(ID_objB));
            }
        }

        //Release an object back into the pool to be reused
        public override void ReleaseMsg()
        {
            GameSceneCollection.ScenePlay.PoolMgr.BackToStack(this);

            released = true;
        }

        public override void PrintMe()
        {
            if (gObjB == null)
            {
                Debug.Print("NULL MESSAGE FOUND");
            }

            Debug.Print("Event Msg: (" + mySendType + ", " + myDataType + ") " + ID_objA + " vs " + ID_objB + " (sent on frame " + GameManager.GetFrameCount() + ")");
        }

        //Function that attempts to check if a null message is a missile
        //If it's null, drop execution
        private void FindMissile(GameObject findMissile)
        {
            if (gObjA != null && findMissile != null)
            {
                gObjA.Accept(findMissile);
            }

        }
    }

    public class TimeMsg : DataMessage
    {
        public TimeMsg()
        {
            this.mySendType = msgType.LOCAL;

            myDataType = dataType.TIME;
        }

        //Serialize the collision data
        public override void Serialize(ref BinaryWriter writer)
        {
            //Write the type of message being sent
            writer.Write((byte)dataType.TIME);

            //Mark the current time of server
            time_Zero = TimeManager.GetCurrentTime();

            //Need to have it set to reliable unordered as in order to start the game we need synced time. Can not have packet loss.
            myDelivType = NetDeliveryMethod.ReliableUnordered;
            myPortNum = 0;
        }

        //Serialize the collision data
        public override void SerializeRecord(ref BinaryWriter writer)
        {
            //Write the type of message being sent
            writer.Write((byte)dataType.TIME);

            //Write the time received from the server
            writer.Write(time_Server);
        }

        //Store the visitor pattern from the file for playback
        public static new DataMessage Deserialize(ref BinaryReader reader, ObjectPool objPool)
        {
            //Find the game msgPool using the IDS
            DataMessage newTime = GameSceneCollection.ScenePlay.PoolMgr.Get(dataType.TIME);

            //Mark time of receival and local time of server
            newTime.time_One = TimeManager.GetCurrentTime();
            newTime.time_Server = reader.ReadSingle();

            //Set data type
            newTime.myDataType = dataType.TIME;

            return newTime;
        }

        //Use Cristian's algorithm to syncronize time with server
        public override void Execute()
        {
            TimeManager.SetTime(time_Server + ((time_One - time_Zero) / 2));
        }

        //Release an object back into the pool to be reused
        public override void ReleaseMsg()
        {
           time_Zero = 0.0f;
           time_One = 0.0f;
           time_Server = 0.0f;
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
};

