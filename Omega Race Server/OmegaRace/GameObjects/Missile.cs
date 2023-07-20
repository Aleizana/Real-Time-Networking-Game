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
    public class Missile : GameObject
    {
        PlayerData owner;
        float MaxForce;
        AnimationParticle animPart;

        //Self-added for dead-reckoning
        public bool posSent { get; set; }

        public Missile(Azul.Rect destRect, PlayerData missileOwner, Azul.Color color)
            : base(GAMEOBJECT_TYPE.MISSILE, new Azul.Rect(0, 0, 24, 6), destRect, TextureCollection.missileTexture, color)
        {
            PhysicBody_Data data = new PhysicBody_Data();
            data.position = new Vec2(destRect.x, destRect.y);
            data.size = new Vec2(destRect.width, destRect.height);
            data.isSensor = true;
            data.angle = 0;
            data.shape_type = PHYSICBODY_SHAPE_TYPE.DYNAMIC_BOX;
            CreatePhysicBody(data);

            owner = missileOwner;
            MaxForce = 17;

            pBody.SetAngle(owner.ship.GetAngle_Deg());
            pBody.ApplyForce(owner.ship.GetHeading() * MaxForce, GetPixelPosition());

            animPart = ParticleSpawner.GetParticle(PARTICLE_EVENT.EXPLOSION, this);

            //Self-added for dead-reckoning
            posSent = false;
        }

        public PlayerData GetOwner()
        {
            return owner;
        }

        public void OnHit()
        {
            owner.GiveMissile(this);
            GameManager.DestroyObject(this);
        }

        public override void Destroy()
        {
            AudioManager.PlaySoundEvent(AUDIO_EVENT.MISSILE_HIT);
            animPart.StartAnimation(pSprite.x, pSprite.y);
            base.Destroy();
        }

        public override void Accept(GameObject obj)
        {
            obj.VisitMissile(this);
        }

        public override void VisitFence(Fence f)
        {
            CollisionEvent.Action(f, this);
        }

        public override void VisitFencePost(FencePost fp)
        {
            CollisionEvent.Action(this, fp);
        }

        public override void VisitShip(Ship s)
        {
            CollisionEvent.Action(this, s);
        }
    }
}
