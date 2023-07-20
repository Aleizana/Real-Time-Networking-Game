using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Lidgren.Network;
using OmegaRace.Managers.NetworkManager;

namespace OmegaRace
{
    public class GameScenePlay : IGameScene
    {
        public PlayerManager PlayerMgr { get; }
        public MessageQueueManager MsgQueueMgr { get; private set; }

        DisplayManager DisplayMgr;

        NetworkManagerServer NetworkMgr;

        //Create an object pool to store data messages and upcast different kinds
        public ObjectPool PoolMgr;

        //Switch to activate dead reckoning predictions
        public bool DeadReckoningOn { get; private set; }

        public GameScenePlay()
        {
            PlayerMgr = new PlayerManager();
            DisplayMgr = new DisplayManager();

            //Create an object pool to store data messages and upcast different kinds
            PoolMgr = new ObjectPool();

            NetworkMgr = new NetworkManagerServer(14240);

            //Switch to activate dead reckoning predictions
            //DeadReckoningOn = false;
            DeadReckoningOn = true;

            MsgQueueMgr = new MessageQueueManager(MessageQueueManager.Mode.NORMAL);
            //MsgQueueMgr = new MessageQueueManager(MessageQueueManager.Mode.RECORD, "GameLog.txt");
            //MsgQueueMgr = new MessageQueueManager(MessageQueueManager.Mode.PLAYBACK, "GameLog.txt");
        }

        public void MessageFromClient(NetIncomingMessage im)
        {
            byte[] bytes = im.ReadBytes(im.LengthBytes);
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            DataMessage dataMSG = DataMessage.Deserialize(ref reader, PoolMgr);

            Debug.Print("Rcv Msg:");

            dataMSG.PrintMe();

            MsgQueueMgr.AddToInputQueue(dataMSG);
            //Debug.WriteLine("Move received " + movedata.xdelta + ", " + movedata.ydelta);
        }


        public void MessageToClient(DataMessage data)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            data.Serialize(ref writer);

            //NetworkMgr.SendMessage(stream.ToArray());
            NetworkMgr.SendMessage(stream.ToArray(), data.myDelivType, data.myPortNum);
        }

        void IGameScene.Enter()
        {
            LoadLevel();

            //Set time to zero when the game actually starts
            TimeManager.SetTime(0);
        }
        void IGameScene.Update()
        {
            //Print frame and time at start of processing
            Debug.Print("<Frame " + GameManager.GetFrameCount() + ", Time: " + TimeManager.GetCurrentTime() + ">");

            // First, update the physics engine
            PhysicWorld.Update();

            //Receive player 1 move data
            NetworkMgr.ProcessIncoming(this);

            //Process initial client actions and execute P2 actions to be sent back to the client
            MsgQueueMgr.Process(true);

            //Send both player movements to the client
            MsgQueueMgr.Process(false);

            // Process reactions to inputs for Player 2
            int p2_H = InputManager.GetAxis(INPUTAXIS.HORIZONTAL_P2);
            int p2_V = InputManager.GetAxis(INPUTAXIS.VERTICAL_P2);
            
            //Create message for sending move data
            DataMessage moveP2 = PoolMgr.Get(DataMessage.dataType.MOVEP2);
            moveP2.vertInput = p2_V;
            moveP2.horzInput = p2_H;
            
            //Set msg to be sent locally, and THEN sent on net in 2nd processing
            moveP2.mySendType = DataMessage.msgType.LOCAL;

            //Set data type
            moveP2.myDataType = DataMessage.dataType.MOVEP2;
            
            //Add to the output queue to be processed later
            MsgQueueMgr.AddToOutputQueue(moveP2);
            
            //If the buttons are clicked, then we send knowledge of this to the queue and process it from there
            if (InputManager.GetButtonDown(INPUTBUTTON.P2_FIRE))
            {
                if (PlayerMgr.P2Data.missileCount != 0)
                {
                    DataMessage missileP2 = PoolMgr.Get(DataMessage.dataType.MISSILEP2);
                    missileP2.myDataType = DataMessage.dataType.MISSILEP2;
                    missileP2.mySendType = DataMessage.msgType.LOCAL;
                    MsgQueueMgr.AddToOutputQueue(missileP2);
                }
                
            }
            if (InputManager.GetButtonDown(INPUTBUTTON.P2_LAYMINE))
            {
                if (PlayerMgr.P2Data.mineCount != 0)
                {
                    DataMessage mineP2 = PoolMgr.Get(DataMessage.dataType.MINEP2);
                    mineP2.myDataType = DataMessage.dataType.MINEP2;
                    mineP2.layMyMine = true;
                    mineP2.mySendType = DataMessage.msgType.LOCAL;
                    MsgQueueMgr.AddToOutputQueue(mineP2);
                }
            }
            
            //Prediction for dead reckoning
            if (DeadReckoningOn)
            {
                //Perform dead reckoning prediction on player 1 ship and missiles
                //Adds pos msg to output queue if prediction is off
                PlayerMgr.P1Data.ship.predPlr.RunPlayerPredictions(PlayerMgr.P1Data.ship);

                //Perform dead reckoning prediction on player 2 ship and missiles
                PlayerMgr.P2Data.ship.predPlr.RunPlayerPredictions(PlayerMgr.P2Data.ship);
            }
           

            ScreenLog.Add(string.Format("Game Time: {0:0.0}", TimeManager.GetCurrentTime()));

            /* Screen log example
            ScreenLog.Add(string.Format("Frame Time: {0:0.0}", 1 / TimeManager.GetFrameTime()));
            ScreenLog.Add(Colors.DarkKhaki, string.Format("P1 ammo: {0}", PlayerMgr.P1Data.missileCount));
            ScreenLog.Add(Colors.Orchid, string.Format("P2 ammo: {0}", PlayerMgr.P2Data.missileCount));
            //*/
        }
        void IGameScene.Draw()
        {
            DisplayMgr.DisplayHUD(PlayerMgr.P1Data, PlayerMgr.P2Data);
        }
        void IGameScene.Leave()
        {
            
        }

        void LoadLevel()
        {
            GameManager.AddGameObject(PlayerMgr.P1Data.ship);
            GameManager.AddGameObject(PlayerMgr.P2Data.ship);

            // Fence OutsideBox

            GameManager.AddGameObject(new Fence(new Azul.Rect(100, 5, 8, 200), 90));
            GameManager.AddGameObject(new Fence(new Azul.Rect(300, 5, 8, 200), 90));
            GameManager.AddGameObject(new Fence(new Azul.Rect(500, 5, 8, 200), 90));
            GameManager.AddGameObject(new Fence(new Azul.Rect(700, 5, 8, 200), 90));
  
            GameManager.AddGameObject(new Fence(new Azul.Rect(100, 495, 8, 200), 90));
            GameManager.AddGameObject(new Fence(new Azul.Rect(300, 495, 8, 200), 90));
            GameManager.AddGameObject(new Fence(new Azul.Rect(500, 495, 8, 200), 90));
            GameManager.AddGameObject(new Fence(new Azul.Rect(700, 495, 8, 200), 90));

            GameManager.AddGameObject(new Fence(new Azul.Rect(5, 125, 8, 250), 0));
            GameManager.AddGameObject(new Fence(new Azul.Rect(5, 375, 8, 250), 0));
            GameManager.AddGameObject(new Fence(new Azul.Rect(795, 125, 8, 250), 0));
            GameManager.AddGameObject(new Fence(new Azul.Rect(795, 375, 8, 250), 0));

            // Fence InsideBox
            GameManager.AddGameObject(new Fence(new Azul.Rect(300, 170, 10, 200), 90));
            GameManager.AddGameObject(new Fence(new Azul.Rect(500, 170, 10, 200), 90));
            GameManager.AddGameObject(new Fence(new Azul.Rect(300, 330, 10, 200), 90));
            GameManager.AddGameObject(new Fence(new Azul.Rect(500, 330, 10, 200), 90));

            GameManager.AddGameObject(new Fence(new Azul.Rect(200, 250, 10, 160), 0));
            GameManager.AddGameObject(new Fence(new Azul.Rect(600, 250, 10, 160), 0));


            // OutsideBox
            GameManager.AddGameObject(new FencePost(new Azul.Rect(5, 5, 10, 10)));
            GameManager.AddGameObject(new FencePost(new Azul.Rect(200, 5, 10, 10)));
            GameManager.AddGameObject(new FencePost(new Azul.Rect(400, 5, 10, 10)));
            GameManager.AddGameObject(new FencePost(new Azul.Rect(600, 5, 10, 10)));
            GameManager.AddGameObject(new FencePost(new Azul.Rect(800 - 5, 5, 10, 10)));

            GameManager.AddGameObject(new FencePost(new Azul.Rect(0 + 5, 495, 10, 10)));
            GameManager.AddGameObject(new FencePost(new Azul.Rect(200, 495, 10, 10)));
            GameManager.AddGameObject(new FencePost(new Azul.Rect(400, 495, 10, 10)));
            GameManager.AddGameObject(new FencePost(new Azul.Rect(600, 495, 10, 10)));
            GameManager.AddGameObject(new FencePost(new Azul.Rect(800 - 5, 495, 10, 10)));

            GameManager.AddGameObject(new FencePost(new Azul.Rect(5, 250, 10, 10)));
            GameManager.AddGameObject(new FencePost(new Azul.Rect(795, 250, 10, 10)));

            // InsideBox

            GameManager.AddGameObject(new FencePost(new Azul.Rect(200, 170, 10, 10)));
            GameManager.AddGameObject(new FencePost(new Azul.Rect(400, 170, 10, 10)));
            GameManager.AddGameObject(new FencePost(new Azul.Rect(600, 170, 10, 10)));
            GameManager.AddGameObject(new FencePost(new Azul.Rect(200, 330, 10, 10)));
            GameManager.AddGameObject(new FencePost(new Azul.Rect(400, 330, 10, 10)));
            GameManager.AddGameObject(new FencePost(new Azul.Rect(600, 330, 10, 10)));
        }
    }
}
