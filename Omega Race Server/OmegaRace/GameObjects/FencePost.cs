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
    public class FencePost : GameObject
    {

        public FencePost(Azul.Rect dRect)
            : base (GAMEOBJECT_TYPE.FENCEPOST, new Azul.Rect(0, 0, 12, 12), dRect, TextureCollection.fencePostTexture, Colors.SlateGray)
        {
            PhysicBody_Data data = new PhysicBody_Data();

            data.position = new Vec2(dRect.x, dRect.y);
            data.size = new Vec2(dRect.width, dRect.height);
            data.active = true;
            data.angle = 0;
            data.shape_type = PHYSICBODY_SHAPE_TYPE.STATIC_BOX;
            CreatePhysicBody(data);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override void Accept(GameObject obj)
        {
            obj.VisitFencePost(this);
        }

        public override void VisitMissile(Missile m)
        {
            CollisionEvent.Action(m, this);
        }

    }
}
