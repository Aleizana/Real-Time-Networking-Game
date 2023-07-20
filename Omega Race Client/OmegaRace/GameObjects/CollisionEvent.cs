using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegaRace
{
    public class CollisionEvent
    {
        public static void Action(Fence f, Ship s)
        {
            f.OnHit();
        }
        public static void Action(Missile m, Ship s)
        {
            if (m.GetOwner() != s.GetOwner())
            {
                m.OnHit();
                s.OnHit();
            }
        }

        public static void Action(Fence f, Missile m)
        {
            f.OnHit();
            m.OnHit();  // if not null
        }

        public static void Action(Missile m, FencePost p)
        {
            m.OnHit(); // if not null
        }

        public static void Action(Ship s, Mine m)
        {
            if (m.state == MINE_STATE.ARMED)
            {
                m.OnHit();
                s.OnHit();
            }
        }
    }
}
