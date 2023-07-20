using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Dynamics;
using Box2DX.Collision;
using Box2DX.Common;

namespace OmegaRace
{
    public class PhysicWorld
    {
        World privWorld;

        private static PhysicWorld instance;
        private static PhysicWorld Instance()
        {
            if (instance == null)
            {
                instance = new PhysicWorld();
            }
            return instance;
        }

        public static float PIXELSTOMETERS = 0.02f;
        public static float METERSTOPIXELS = 50.0f;
        public static float DEG_TO_RAD = 0.0174532925f;
        public static float RAD_TO_DEG = 57.2957795147f;

        private ContactManager contactMan;

        private PhysicWorld()
        {
            // Define the size of the world. Simulation will still work
            // if bodies reach the end of the world, but it will be slower.
            AABB worldAABB = new AABB();
            worldAABB.LowerBound.Set(-100.0f, -100.0f);
            worldAABB.UpperBound.Set(100.0f, 100.0f);

            // Define the gravity vector.
            Vec2 gravity = new Vec2(0.0f, 0.0f);

            // Do we want to let bodies sleep?
            bool doSleep = true;

            // Construct a world object, which will hold and simulate the rigid bodies.
            privWorld = new World(worldAABB, gravity, doSleep);

            contactMan = new ContactManager();

            privWorld.SetContactListener(contactMan);

        }

        public static World GetWorld()
        {
            return Instance().privWorld;
        }

        public static void Update()
        {
            // Instruct the world to perform a single step of simulation. It is
            // generally best to keep the time step and iterations fixed.
            Instance().privWorld.Step( TimeManager.GetFrameTime() , 5, 8);

            //ScreenLog.Add("World body Count: {0}", Instance().privWorld.GetBodyCount());
        }

    }
}
