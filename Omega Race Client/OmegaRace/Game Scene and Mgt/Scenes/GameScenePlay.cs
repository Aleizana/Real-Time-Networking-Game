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

        NetworkManager NetworkMgr;

        //Create an object pool to store data messages and upcast different kinds
        public ObjectPool PoolMgr;

        //True = client side prediction on | false = client side prediction off
        public bool ClientSidePrediction { get; private set; }

        //True = server side prediction on | false = server side prediction off
        public bool ServerSidePrediction { get; private set; }


        public GameScenePlay()
        {
            PlayerMgr = new PlayerManager();
            DisplayMgr = new DisplayManager();

            //Create an object pool to store data messages and upcast different kinds
            PoolMgr = new ObjectPool();

            //Start connection to server
            NetworkMgr = new NetworkManager("localhost", 14240);

            //Set client side prediction
            ClientSidePrediction = false;
            //ClientSidePrediction = true;

            //Set server side prediction
            //ServerSidePrediction = false;
            ServerSidePrediction = true;

            MsgQueueMgr = new MessageQueueManager(MessageQueueManager.Mode.NORMAL);
            //MsgQueueMgr = new MessageQueueManager(MessageQueueManager.Mode.RECORD, "GameLog.txt");
            //MsgQueueMgr = new MessageQueueManager(MessageQueueManager.Mode.PLAYBACK, "GameLog.txt");
        }

        public void MessageFromServer(NetIncomingMessage im)
        {
            byte[] bytes = im.ReadBytes(im.LengthBytes);
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            
            if (bytes.Length != 0)
            {
                DataMessage dataMSG = DataMessage.Deserialize(ref reader, PoolMgr);

                Debug.Print("Rcv Msg:");

                dataMSG.PrintMe();

                MsgQueueMgr.AddToInputQueue(dataMSG);

                //Debug.WriteLine("Move received " + movedata.xdelta + ", "+ movedata.ydelta);
            }

        }

        public void MessageToServer(DataMessage data)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            data.Serialize(ref writer);

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

            //First, update the physics engine
            //PhysicWorld.Update();

            //Send P1 movements to the server
            MsgQueueMgr.Process();
            
            //Receive all game events from the server
            NetworkMgr.ProcessIncoming(this);
            
            //Process all player movements + game events
            MsgQueueMgr.Process();
            
            //So as to not interfere with objects that need several updates,
            //we release anything unreleased at the end of a frame
            PoolMgr.ReleaseAll();
            
            // Process reactions to inputs for Player 1
            int p1_H = InputManager.GetAxis(INPUTAXIS.HORIZONTAL_P1);
            int p1_V = InputManager.GetAxis(INPUTAXIS.VERTICAL_P1);
            
            //Create message for sending move data
            DataMessage moveP1 = PoolMgr.Get(DataMessage.dataType.MOVEP1);
            moveP1.vertInput = p1_V;
            moveP1.horzInput = p1_H;
            
            //Set msg type to be sent both locally and net
            moveP1.mySendType = DataMessage.msgType.NET;
            
            //Add to the output queue to be processed later
            MsgQueueMgr.AddToOutputQueue(moveP1);
            
            //If the buttons are clicked, then we send knowledge of this to the queue and process it from there
            if (InputManager.GetButtonDown(INPUTBUTTON.P1_FIRE))
            {
                if (PlayerMgr.P1Data.missileCount != 0)
                {
                    DataMessage missileP1 = PoolMgr.Get(DataMessage.dataType.MISSILEP1);
                    missileP1.mySendType = DataMessage.msgType.NET;
                    missileP1.myDataType = DataMessage.dataType.MISSILEP1;
                    MsgQueueMgr.AddToOutputQueue(missileP1);
                }
            }
            if (InputManager.GetButtonDown(INPUTBUTTON.P1_LAYMINE))
            {
                if (PlayerMgr.P1Data.mineCount != 0)
                {
                    DataMessage mineP1 = PoolMgr.Get(DataMessage.dataType.MINEP1);
                    mineP1.layMyMine = true;
                    mineP1.mySendType = DataMessage.msgType.NET;
                    MsgQueueMgr.AddToOutputQueue(mineP1);
                }
                
            }
            
            //Client side prediction code only
            if (ClientSidePrediction)
            {
                //Predict movement of player ships every frame
                PlayerMgr.P1Data.ship.predControl.RunPredictions(PlayerMgr.P1Data.ship);
                PlayerMgr.P2Data.ship.predControl.RunPredictions(PlayerMgr.P2Data.ship);
            }
            
            //Dead reckoning only
            if (ServerSidePrediction)
            {
                PlayerMgr.P1Data.ship.deadReckShip.RunPlayerPredictions(PlayerMgr.P1Data.ship);
                PlayerMgr.P2Data.ship.deadReckShip.RunPlayerPredictions(PlayerMgr.P2Data.ship);
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
