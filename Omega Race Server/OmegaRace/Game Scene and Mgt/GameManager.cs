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
    public class GameManager
    {
        private static GameManager instance = null;
        private static GameManager Instance()
        {
            if (instance == null)
            {
                instance = new GameManager();
            }
            return instance;
        }

        List<GameObject> destroyList;
        List<GameObject> gameObjList;

        GameSceneFSM gameSceneFSM;

        public int frameCount = 0;
        public static int GetFrameCount() { return Instance().frameCount; }

        private GameManager()
        {
            destroyList = new List<GameObject>();
            gameObjList = new List<GameObject>();

            gameSceneFSM = new GameSceneFSM();

        }

        public static void SetMode()
        {
           Instance().gameSceneFSM.ChangeTo(GameSceneCollection.SceneTitlePage);
        }

        public static void ChangeState(IGameScene st)
        {
            Instance().gameSceneFSM.ChangeTo(st);
        }

        public static void Update()
        {
            Instance().pUpdate();
        }

        private void pUpdate()
        {
            //Debug.WriteLine("Frame #{0} --------------------", frameCount);


            gameSceneFSM.TransitionState();
            gameSceneFSM.Update();

            for (int i = gameObjList.Count - 1; i >= 0; i--)
            {
                gameObjList[i].Update();
            }

            CleanUp();

            frameCount++;
        }

        public static void Draw()
        {
            Instance().pDraw();
        }

        private void pDraw()
        {
            gameSceneFSM.Draw();


            for (int i = 0; i < gameObjList.Count; i++)
            {
                gameObjList[i].Draw();

                /* Show GamObjectID on screen                 
                ScreenLog.GetSpriteFont().Render("" + gameObjList[i].getID(), (int)gameObjList[i].GetPixelPosition().X, (int)gameObjList[i].GetPixelPosition().Y);
                //*/
            }
        }

        public static GameObject Find(int id)
        {
            GameObject toReturn = null;

            foreach (GameObject obj in Instance().gameObjList)
            {
                if (obj.getID() == id)
                {
                    toReturn = obj;
                    break;
                }
            }

            return toReturn;
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

        public static void DestroyObject(GameObject obj)
        {
            if (obj.isAlive()) // Make sure to add once only
            {
                obj.setAlive(false);
                Instance().destroyList.Add(obj);
            }
        }
    }
}
