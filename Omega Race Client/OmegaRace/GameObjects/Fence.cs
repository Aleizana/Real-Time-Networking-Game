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
    public class Fence : GameObject
    {
        AnimationParticle animPart;
        public Fence(Azul.Rect dRect, float angle)
            : base(GAMEOBJECT_TYPE.FENCE, new Azul.Rect(0, 0, 6, 209), dRect, TextureCollection.fenceTexture, Colors.DarkSlateGray)
        {
            PhysicBody_Data data = new PhysicBody_Data();

            data.position = new Vec2(dRect.x, dRect.y);
            data.size = new Vec2(dRect.width, dRect.height);
            data.angle = angle;
            data.shape_type = PHYSICBODY_SHAPE_TYPE.STATIC_BOX;
            CreatePhysicBody(data);

            animPart = ParticleSpawner.GetParticle(PARTICLE_EVENT.FENCE_HIT, this);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Draw()
        {
            base.Draw();
        }

        public void OnHit()
        {
            AudioManager.PlaySoundEvent(AUDIO_EVENT.FENCE_HIT);
            animPart.StartAnimation(pSprite.x, pSprite.y);
        }

        public override void Accept(GameObject obj)
        {
            obj.VisitFence(this);
        }

        public override void VisitMissile(Missile m)
        {
            CollisionEvent.Action(this, m);
        }

        public override void VisitShip(Ship s)
        {
            CollisionEvent.Action(this, s);
        }

    }
}
