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
    public enum PARTICLE_EVENT
    {
        EXPLOSION,
        FENCE_HIT
    }


    public class ParticleSpawner
    {
        private static ParticleSpawner instance = null;
        private static ParticleSpawner Instance()
        {
            if (instance == null)
            {
                instance = new ParticleSpawner();
            }
            return instance;
        }

        private ParticleSpawner()
        {

        }

        public static void SpawnParticleEvent(PARTICLE_EVENT Event_Type, GameObject Sender)
        {
            AnimationParticle part = GetParticle(Event_Type, Sender);
            part.StartAnimation(0,0);
        }

        public static AnimationParticle GetParticle(PARTICLE_EVENT Event_Type, GameObject Sender)
        {
            AnimationParticle part = null;

            ParticleSpawner inst = Instance();
            switch (Event_Type)
            {
                case PARTICLE_EVENT.EXPLOSION:
                    part = inst.SpawnExplosionParticle(Sender);
                    break;
                case PARTICLE_EVENT.FENCE_HIT:
                    part = inst.SpawnFenceParticle(Sender as Fence);
                    break;
            }

            return part;
        }

        AnimationParticle SpawnExplosionParticle(GameObject obj)
        {
            Vec2 pos = obj.GetPixelPosition();
            Azul.Rect dest = new Azul.Rect(pos.X, pos.Y, 30, 30);
            return new AnimationParticle(new Azul.Rect(0, 0, 86, 70), dest, TextureCollection.explosionText, Colors.Red);
        }

        AnimationParticle SpawnFenceParticle(Fence obj)
        {
            Vec2 pos = obj.GetPixelPosition();
            Azul.Rect source = new Azul.Rect(0, 0, 6, 209);
            Azul.Color color = Colors.DarkYellow;
            Azul.Rect dest = new Azul.Rect(pos.X, pos.Y, 1, 1);

            Azul.Rect objDimensions = obj.getDestRect();
            objDimensions = new Azul.Rect(objDimensions.x, objDimensions.y, objDimensions.width, objDimensions.height - 6);


            Azul.Sprite s1 = new Azul.Sprite(TextureCollection.fenceText1, source, objDimensions, color);
            Azul.Sprite s2 = new Azul.Sprite(TextureCollection.fenceText2, source, objDimensions, color);
            Azul.Sprite s3 = new Azul.Sprite(TextureCollection.fenceText3, source, objDimensions, color);
            Azul.Sprite s4 = new Azul.Sprite(TextureCollection.fenceText4, source, objDimensions, color);
            Azul.Sprite s5 = new Azul.Sprite(TextureCollection.fenceText5, source, objDimensions, color);
            Azul.Sprite s6 = new Azul.Sprite(TextureCollection.fenceText6, source, objDimensions, color);
            Azul.Sprite s7 = new Azul.Sprite(TextureCollection.fenceText7, source, objDimensions, color);

            s1.angle = obj.GetAngle_Rad();
            s2.angle = obj.GetAngle_Rad();
            s3.angle = obj.GetAngle_Rad();
            s4.angle = obj.GetAngle_Rad();
            s5.angle = obj.GetAngle_Rad();
            s6.angle = obj.GetAngle_Rad();
            s7.angle = obj.GetAngle_Rad();

            List<Azul.Sprite> anim = new List<Azul.Sprite>();
            anim.Add(s1);
            anim.Add(s2);
            anim.Add(s3);
            anim.Add(s4);
            anim.Add(s5);
            anim.Add(s6);
            anim.Add(s7);

            AnimationParticle part = new AnimationParticle(source, dest, TextureCollection.fenceText1, color);
            part.setAnimation(anim);

            return part;
        }



    }
}
