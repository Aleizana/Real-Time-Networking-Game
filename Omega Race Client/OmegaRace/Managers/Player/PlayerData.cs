using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Common;
using System.Diagnostics;

namespace OmegaRace
{
    public enum Player { Player1, Player2 };
    public class PlayerData
    {
        public Ship ship { get; }
        public int score { get; private set; }
        public int lives { get; private set; }
        public Azul.Color shipColor { get; }
        public Player player { get; }
        public int mineCount { get; private set; }
        public int missileCount { get; private set; }
        public List<Missile> missileList { get; }

        const int MAX_MISSILE = 3;
        const int MAX_MINE = 5;
        const int MAX_LIFE = 3;

        public INPUTAXIS horInputAxis { get; }
        public INPUTAXIS vertInputAxis { get; }
        public INPUTBUTTON fireButton { get; }
        public INPUTBUTTON mineButton { get; }

        public PlayerData(PlayerManager pMgr, Vec2 pos, Azul.Color c, Player p, INPUTAXIS hor, INPUTAXIS ver, INPUTBUTTON fire, INPUTBUTTON mine)
        {
            player = p;
            shipColor = c;
            ship = new Ship(this, pMgr, new Azul.Rect(pos.X, pos.Y, 32, 32), shipColor);
            score = 0;
            lives = MAX_LIFE;
            mineCount = MAX_MINE;
            missileCount = MAX_MISSILE;

            horInputAxis = hor;
            vertInputAxis = ver;
            fireButton = fire;
            mineButton = mine;

            missileList = new List<Missile>();
        }

        public void FireMissile()
        {
            if (missileCount > 0)
            {
                missileCount--;
                Vec2 pos = ship.GetPixelPosition();
                Missile m = new Missile(new Azul.Rect(pos.X, pos.Y, 20, 5), this, shipColor);
                GameManager.AddGameObject(m);
                missileList.Add(m);
                AudioManager.PlaySoundEvent(AUDIO_EVENT.MISSILE_FIRE);
            }
        }
        public void GiveMissile(Missile m)
        {
            if (missileList.Remove(m))
            {
                missileCount++;
            }
        }
        public Missile GetMissileByID(int id)
        {
            Missile m = missileList.Find(mis => mis.getID() == id);
            //Debug.Assert(m != null);

            return m;
        }

        public void LayMine()
        {
            if (mineCount > 0)
            {
                mineCount--;
                Vec2 pos = ship.GetPixelPosition();

                Mine m = new Mine(new Azul.Rect(pos.X, pos.Y, 20, 20), this, shipColor);
                GameManager.AddGameObject(m);
                AudioManager.PlaySoundEvent(AUDIO_EVENT.MINE_LAYED);
            }
        }

        public void GiveMine()
        {
            if (mineCount < MAX_MINE)
            {
                mineCount++;
            }
        }

        public void AddScore()
        {
            score++;
        }

        public void SubtractLife()
        {
            lives--;
        }

        public void ResetLives()
        {
            lives = MAX_LIFE;
        }
    }
}
