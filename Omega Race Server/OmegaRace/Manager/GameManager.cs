using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Box2DX.Dynamics;
using Box2DX.Collision;
using Box2DX.Common;

namespace OmegaRace
{
    public enum GAME_STATE
    {
        PLAY
    }

    public class GameManager 
    {
        private static GameManager instance = null;
        public static GameManager Instance()
        {
            if (instance == null)
            {
                instance = new GameManager();
            }
            return instance;
        }

        List<GameObject> destroyList;
        List<GameObject> gameObjList;

        public Ship player1;
        public Ship player2;

        public int p2Score;
        public int p1Score;

        GameManager_UI gamManUI;

        private GameManager()
        {
            destroyList = new List<GameObject>();
            gameObjList = new List<GameObject>();

            gamManUI = new GameManager_UI();
        }

        public static void Start()
        {
            LoadLevel_Helper.LoadLevel();
        }


        public static void Update(float gameTime)
        {
            GameManager inst = Instance();
            
            inst.pUpdate();
        }

        public static void Draw()
        {
            GameManager inst = Instance();
            
            inst.pDraw();
        }

        public GameObject Find(int id)
        {
            GameObject toReturn = null;

            foreach (GameObject obj in gameObjList)
            {
                if (obj.getID() == id)
                {
                    toReturn = obj;
                    break;
                }
            }

            return toReturn;
        }
        public static void RecieveMessage(DataMessage msg)
        {
            int p1H = msg.horzInput;
            int p1V = msg.vertInput;

            instance.player1.Rotate(p1H);
            instance.player1.Move(p1V);

        }
        

        private void pUpdate()
        {
            //**** Player 1: for demonstration, processing its movement through messaging queues
            int p1_H = InputManager.GetAxis(INPUTAXIS.HORIZONTAL_P1);
            int p1_V = InputManager.GetAxis(INPUTAXIS.VERTICAL_P1);
            //player1.Rotate(p1_H);
            //player1.Move(p1_V);
            DataMessage msg1 = new DataMessage();
            msg1.horzInput = p1_H;
            msg1.vertInput = p1_V;
            OutputQueue.AddToQueue(msg1);
            //******************************

            //**** Player 2: For demonstration, processing its movement through direct method calls
            int p2_H = InputManager.GetAxis(INPUTAXIS.HORIZONTAL_P2);
            int p2_V = InputManager.GetAxis(INPUTAXIS.VERTICAL_P2);
            player2.Rotate(p2_H);
            player2.Move(p2_V);
            //******************************

            if (InputManager.GetButtonDown(INPUTBUTTON.P1_FIRE))
            {
                GameManager.FireMissile(player1);
            }
            
            if (InputManager.GetButtonDown(INPUTBUTTON.P2_FIRE))
            {
                GameManager.FireMissile(player2);
            }


            //**** General engine operations. No touchy!
            for (int i = gameObjList.Count - 1; i >= 0; i--)
            {
                gameObjList[i].Update();
            }
            gamManUI.Update(); // Note: Game UI (score display) is not processed as a game object
        }
        
        private void pDraw()
        {
            player1.Draw();
            player2.Draw();

            for (int i = 0; i < gameObjList.Count; i++)
            {
                gameObjList[i].Draw();
            }

            gamManUI.Draw();
        }
        


        public static void PlayerKilled(Ship s)
        {
            Instance().pPlayerKilled(s);
        }
        

        void pPlayerKilled(Ship shipKilled)
        {

            // Player 1 is Killed
            if(player1.getID() == shipKilled.getID())
            {
                p2Score++;

                player1.Respawn(new Vec2(400, 100));
                player2.Respawn(new Vec2(400, 400));
            }
            // Player 2 is Killed
            else if (player2.getID() == shipKilled.getID())
            {
                p1Score++;
                player1.Respawn(new Vec2(400, 100));
                player2.Respawn(new Vec2(400, 400));
                  
            }
        }

        public static void MissileDestroyed(Missile m)
        {
            GameManager inst = Instance();

            if (m.GetOwnerID() == inst.player1.getID())
            {
                inst.player1.GiveMissile();
            }
            else if (m.GetOwnerID() == inst.player2.getID())
            {
                inst.player2.GiveMissile();
            }
        }

        public static void FireMissile(Ship ship)
        {
            if (ship.UseMissile())
            {
                ship.Update();
                Vec2 pos = ship.GetWorldPosition();
                Vec2 direction = ship.GetHeading();
                Missile m = new Missile(new Azul.Rect(pos.X, pos.Y, 20, 5), ship.getID(), direction, ship.getColor());
                Instance().gameObjList.Add(m);
                AudioManager.PlaySoundEvent(AUDIO_EVENT.MISSILE_FIRE);
            }
        }

        

        public static void AddGameObject(GameObject obj)
        {
            Instance().gameObjList.Add(obj);
        }

        public static void CleanUp()
        {
            foreach (GameObject obj in Instance().destroyList)
            {
                Instance().gameObjList.Remove(obj);
                obj.Destroy();
            }

            Instance().destroyList.Clear();
        }
        
        public void DestroyAll()
        {
            foreach(GameObject obj in gameObjList)
            {
                destroyList.Add(obj);
            }
            gameObjList.Clear();
        }
            
        public static void DestroyObject(GameObject obj)
        {
            obj.setAlive(false);
            Instance().destroyList.Add(obj);
        }
        
        
    }
}
