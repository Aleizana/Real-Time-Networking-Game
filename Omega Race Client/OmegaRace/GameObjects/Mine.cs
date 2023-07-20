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
    public enum MINE_STATE
    {
        LAYED,
        ARMED
    }

    public class Mine : GameObject
    {
        PlayerData owner;

        Azul.Sprite mineRed;
        bool drawMineRed;

        public MINE_STATE state;
        float armTime;
        float cancelTime;
        float animTime;
        float animfreq;

        AnimationParticle animPart;

        public Mine(Azul.Rect destRect, PlayerData mineOwner, Azul.Color color)
            : base(GAMEOBJECT_TYPE.MINE, new Azul.Rect(0, 0, 12, 12), destRect, TextureCollection.mineTexture, color)
        {
            PhysicBody_Data data = new PhysicBody_Data();
            data.position = new Vec2(destRect.x, destRect.y);
            data.size = new Vec2(destRect.width, destRect.height);
            data.isSensor = true;
            data.angle = 0;
            data.shape_type = PHYSICBODY_SHAPE_TYPE.DYNAMIC_BOX;
            CreatePhysicBody(data);

            owner = mineOwner;

            mineRed = new Azul.Sprite(TextureCollection.mineTexture2, new Azul.Rect(0, 0, 12, 12), destRect, Colors.Red);
            mineRed.Update();

            drawMineRed = false;
            animfreq = .1f;
            armTime = TimeManager.GetCurrentTime() + 2;

            state = MINE_STATE.LAYED;

            animPart = ParticleSpawner.GetParticle(PARTICLE_EVENT.EXPLOSION, this);
        }

        public override void Update()
        {
            base.Update();

            switch (state)
            {
                case MINE_STATE.LAYED:
                    LayedUpdate();
                    break;
                case MINE_STATE.ARMED:
                    ArmedUpdate();
                    break;
            }

        }

        public override void Draw()
        {
            switch (state)
            {
                case MINE_STATE.LAYED:
                    LayedDraw();
                    break;
                case MINE_STATE.ARMED:
                    ArmedDraw();
                    break;
            }

        }

        private void LayedUpdate()
        {
            float now = TimeManager.GetCurrentTime();
            if (now > armTime)
            {
                state = MINE_STATE.ARMED;
                cancelTime = now + 5;
                animTime = now + animfreq;
                AudioManager.PlaySoundEvent(AUDIO_EVENT.MINE_ARMED);
            }
        }

        private void LayedDraw()
        {
            base.Draw();
        }

        private void ArmedUpdate()
        {
            float now = TimeManager.GetCurrentTime();

            if (now > cancelTime)
            {
                OnHit(); // explode the mine and return it
            }

            if (now > animTime) // cheesy way to animate the red part
            {
                drawMineRed = !drawMineRed;
                animTime = now + animfreq;
            }
        }

        private void ArmedDraw()
        {
            if (drawMineRed)
            {
                mineRed.Render();
            }

            base.Draw();
        }

        public PlayerData GetOwner()
        {
            return owner;
        }

        public void OnHit()
        {
            animPart.StartAnimation(pSprite.x, pSprite.y);
            owner.GiveMine();
            AudioManager.PlaySoundEvent(AUDIO_EVENT.MINE_DESPAWN);
            GameManager.DestroyObject(this);
        }

        public override void Accept(GameObject obj)
        {
            obj.VisitMine(this);
        }
        public override void VisitMissile(Missile m)
        {
        }
        public override void VisitShip(Ship s)
        {
            CollisionEvent.Action(s, this);
        }
    }
}
