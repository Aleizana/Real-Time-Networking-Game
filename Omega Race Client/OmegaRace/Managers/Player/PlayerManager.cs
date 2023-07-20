using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Common;

namespace OmegaRace
{
    public class PlayerManager
    {
        public PlayerData P1Data { get; }
        public PlayerData P2Data { get; }

        int midline;
        Vec2 SpawnTop;
        Vec2 SpawnBottom;
        public PlayerManager()
        {
            midline = 250;
            SpawnTop = new Vec2(400, 400);
            SpawnBottom = new Vec2(400, 100);

            P1Data = new PlayerData(this, SpawnBottom, Colors.Green, Player.Player1,
                                    INPUTAXIS.HORIZONTAL_P1, INPUTAXIS.VERTICAL_P1,
                                    INPUTBUTTON.P1_FIRE, INPUTBUTTON.P1_LAYMINE);

            P2Data = new PlayerData(this, SpawnTop, Colors.ColdBlue, Player.Player2,
                                    INPUTAXIS.HORIZONTAL_P2, INPUTAXIS.VERTICAL_P2,
                                    INPUTBUTTON.P2_FIRE, INPUTBUTTON.P2_LAYMINE);
        }

        public void PlayerKilled(PlayerData playerKilled)
        {
            // Identify who is killed and who is alive
            PlayerData playerAlive = P1Data;
            if (playerAlive == playerKilled)
            {
                playerAlive = P2Data;
            }

            // Adjust lives
            playerKilled.SubtractLife();

            // Respawn the killed player away from the live one
            if (playerAlive.ship.GetPixelPosition().Y > midline)
            {
                playerKilled.ship.Respawn(SpawnBottom);
            }
            else
            {
                playerKilled.ship.Respawn(SpawnTop);
            }

            // If this was the end of a round, reset everything
            if (playerKilled.lives <= 0)
            {
                playerAlive.AddScore();
                playerKilled.ResetLives();
                playerAlive.ResetLives();

                if (playerAlive.ship.GetPixelPosition().Y > midline)
                {
                    playerAlive.ship.Respawn(SpawnTop);
                }
                else
                {
                    playerAlive.ship.Respawn(SpawnBottom);
                }
            }
        }
    }
}
