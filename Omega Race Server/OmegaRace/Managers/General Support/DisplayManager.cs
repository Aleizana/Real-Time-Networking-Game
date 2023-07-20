using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegaRace
{
    public class DisplayManager
    {
        Azul.Sprite p1LifeDisplay;
        Azul.Sprite p2LifeDisplay;

        Azul.Sprite p2MineDisplay;
        Azul.Sprite p1MineDisplay;

        public DisplayManager()
        {
            p1LifeDisplay = new Azul.Sprite(TextureCollection.shipTexture, new Azul.Rect(0, 0, 32, 32), new Azul.Rect(0, 0, 32, 32), Colors.Green);
            p1LifeDisplay.angle = 90 * PhysicWorld.DEG_TO_RAD;
            p2LifeDisplay = new Azul.Sprite(TextureCollection.shipTexture, new Azul.Rect(0, 0, 32, 32), new Azul.Rect(0, 0, 32, 32), Colors.ColdBlue);
            p2LifeDisplay.angle = 90 * PhysicWorld.DEG_TO_RAD;

            p1MineDisplay = new Azul.Sprite(TextureCollection.mineTexture, new Azul.Rect(0, 0, 12, 12), new Azul.Rect(0, 0, 20, 20), Colors.Green);
            p2MineDisplay = new Azul.Sprite(TextureCollection.mineTexture, new Azul.Rect(0, 0, 12, 12), new Azul.Rect(0, 0, 20, 20), Colors.ColdBlue);

        }

        public void DisplayHUD(PlayerData p1, PlayerData p2)
        {
            ShowPlayerStats(p1, "P1", 245, 220, p1LifeDisplay, p1MineDisplay);
            ShowPlayerStats(p2, "P2", 455, 220, p2LifeDisplay, p2MineDisplay);
        }

        void ShowPlayerStats( PlayerData player, string label, int xpos, int ypos, Azul.Sprite lifeDisplay, Azul.Sprite mineDisplay)
        {
            TextureCollection.scoreFont.Render( label + " WINS: " + player.score, xpos, ypos);

            for (int i = 0; i < player.lives; i++)
            {
                lifeDisplay.x = xpos + 14 + (i * 36);
                lifeDisplay.y = ypos + 65;
                lifeDisplay.Update();
                lifeDisplay.Render();
            }

            for (int i = 0; i < player.mineCount; i++)
            {
                mineDisplay.x = xpos - 10 + (i * 30);
                mineDisplay.y = ypos + 30;
                mineDisplay.Update();
                mineDisplay.Render();
            }
        }
    }
}
