using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace OmegaRace
{
    public class GameSceneTitlePage : IGameScene
    {
        Azul.Sprite titlePageSpr;
        public GameSceneTitlePage()
        {
            titlePageSpr = new Azul.Sprite( TextureCollection.titleScreenText, new Azul.Rect(0, 0, 800, 480), new Azul.Rect(400, 250, 800, 500));
            titlePageSpr.Update();
        }

        void IGameScene.Enter()
        {
            Program.AdjustWindow();

            AudioManager.PlaySoundEvent(AUDIO_EVENT.MINE_DESPAWN);
        }
        void IGameScene.Update()
        {
            if ( InputManager.GetButtonDown(INPUTBUTTON.START) )
            {
                GameManager.ChangeState(GameSceneCollection.ScenePlay);
            }
        }
        void IGameScene.Draw()
        {
            titlePageSpr.Render();
        }
        void IGameScene.Leave() { }
    }
}
