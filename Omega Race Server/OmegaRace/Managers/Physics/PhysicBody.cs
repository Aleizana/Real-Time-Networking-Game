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
    public enum PHYSICBODY_SHAPE_TYPE
    {
        NULL,
        DYNAMIC_BOX,
        DYNAMIC_CIRCLE,
        STATIC_BOX,
        STATIC_CIRCLE,
        SHIP_MANIFOLD
    };

    public struct PhysicBody_Data
    {

        public bool active;
        public Vec2 position;
        //Vec2 localPosition = Vect(0.0f, 0.0f, 0.0f);
        public Vec2 size;
        public float radius;
        public float linearDamping;
        public float angularDamping;
        public float angle;
        public PHYSICBODY_SHAPE_TYPE shape_type;
        public bool isSensor;
        //public ushort CategoryBits;
        //public ushort MaskBits;
    };


    public class PhysicBody
    {
        Body pBody;

        public PhysicBody(PhysicBody_Data _data, GameObject gameObject)
        {

            float xMeters = PhysicWorld.PIXELSTOMETERS * _data.position.X;
            float yMeters = PhysicWorld.PIXELSTOMETERS * _data.position.Y;

            float sizeXMeters = PhysicWorld.PIXELSTOMETERS * _data.size.X;
            float sizeYMeters = PhysicWorld.PIXELSTOMETERS * _data.size.Y;

            BodyDef bodyDef = new BodyDef();
            

            Debug.Assert(_data.shape_type != PHYSICBODY_SHAPE_TYPE.NULL);

            if (_data.shape_type == PHYSICBODY_SHAPE_TYPE.DYNAMIC_BOX)
            {
                bodyDef.Position.Set(xMeters, yMeters);
                pBody = PhysicWorld.GetWorld().CreateBody(bodyDef);
                pBody.AllowSleeping(false);
                pBody.SetFixedRotation(true);
                pBody.SetAngle(_data.angle * PhysicWorld.DEG_TO_RAD);

                PolygonDef shapeDef = new PolygonDef();
                shapeDef.SetAsBox(sizeXMeters/2, sizeYMeters/2);
                
                shapeDef.Density = 1.0f;
                shapeDef.Friction = 0.0f;
                shapeDef.Restitution = 0.9f;
                
                shapeDef.UserData = gameObject;
                shapeDef.IsSensor = _data.isSensor;

                pBody.CreateFixture(shapeDef);
                
            }
            else if(_data.shape_type == PHYSICBODY_SHAPE_TYPE.DYNAMIC_CIRCLE)
            {
                bodyDef.Position.Set(xMeters, yMeters);
                pBody = PhysicWorld.GetWorld().CreateBody(bodyDef);

                pBody.SetAngle(_data.angle * PhysicWorld.DEG_TO_RAD);

                CircleDef circleDef = new CircleDef();
                circleDef.Radius = _data.radius * PhysicWorld.PIXELSTOMETERS;
                circleDef.Density = 1.0f;
                circleDef.Friction = 0.0f;
                circleDef.Restitution = 0.9f;
                circleDef.LocalPosition = new Vec2(0f, 0f);
                circleDef.UserData = gameObject;
                circleDef.IsSensor = _data.isSensor;

                pBody.CreateFixture(circleDef);
            }
            else if(_data.shape_type == PHYSICBODY_SHAPE_TYPE.STATIC_BOX)
            {
                bodyDef.Position.Set(xMeters, yMeters);
                pBody = PhysicWorld.GetWorld().CreateBody(bodyDef);
                pBody.AllowSleeping(false);
                

                pBody.SetAngle(_data.angle * PhysicWorld.DEG_TO_RAD);


                PolygonDef shapeDef = new PolygonDef();
                shapeDef.SetAsBox(sizeXMeters / 2, sizeYMeters / 2);
                shapeDef.UserData = gameObject;
                shapeDef.IsSensor = _data.isSensor;
                //shapeDef.Density = 1.0f;
                shapeDef.Friction = 0.0f;
                shapeDef.Restitution = 0.9f;

                pBody.CreateFixture(shapeDef);
            }
            else if(_data.shape_type == PHYSICBODY_SHAPE_TYPE.SHIP_MANIFOLD)
            {
                bodyDef.Position.Set(xMeters, yMeters);
                pBody = PhysicWorld.GetWorld().CreateBody(bodyDef);
                pBody.AllowSleeping(false);
                pBody.SetFixedRotation(true);

                pBody.SetAngle(_data.angle * PhysicWorld.DEG_TO_RAD);

                PolygonDef shapeDef = new PolygonDef();
                shapeDef.VertexCount = 3;
                shapeDef.Vertices[0] = new Vec2(sizeXMeters/2,0);
                shapeDef.Vertices[1] = new Vec2(-sizeXMeters/2, sizeYMeters/2);
                shapeDef.Vertices[2] = new Vec2(-sizeXMeters/2, -sizeYMeters/2);
                
                shapeDef.UserData = gameObject;
                shapeDef.IsSensor = _data.isSensor;
                shapeDef.Density = 1.0f;
                shapeDef.Friction = 0.0f;
                shapeDef.Restitution = 0.5f;

                pBody.CreateFixture(shapeDef);
            }
            else
            {
                Debug.Assert(false, "You did not set the shape type");
            }

            pBody.SetUserData(this);
            pBody.SetMassFromShapes();
        }

        public Vec2 GetPixelPosition()
        {
            Vec2 pos = pBody.GetPosition();

            return (pos * PhysicWorld.METERSTOPIXELS);
        }

        public Vec2 GetPhysicalPosition()
        {
            return pBody.GetPosition();
        }

        // Sets position of the body 
        //      parameter: position in pixel cordinates
        public void SetPhysicalPosition(Vec2 pixelPos)
        {
            pixelPos *= PhysicWorld.PIXELSTOMETERS;

            pBody.SetPosition(pixelPos);
        }


        public void ApplyForce(Vec2 pixelForce, Vec2 pixelPoint)
        {
            pixelForce *= PhysicWorld.PIXELSTOMETERS;
            pixelPoint *= PhysicWorld.PIXELSTOMETERS;

            pBody.ApplyImpulse(pixelForce, pBody.GetWorldCenter());

        }

        public void SetPixelVelocity(Vec2 pix_vel)
        {
            pix_vel *= PhysicWorld.PIXELSTOMETERS;
            pBody.SetLinearVelocity(pix_vel);
        }

        public void SetAngularVelocity(float pix_vel)
        {
            pix_vel *= PhysicWorld.PIXELSTOMETERS;
            pBody.SetAngularVelocity(pix_vel);
        }

        // Sets Angle of body in Degrees
        public void SetAngle(float degrees)
        {
            pBody.SetAngle(degrees * PhysicWorld.DEG_TO_RAD);
        }


        public float GetAngleRads()
        {
            return pBody.GetAngle();
        }

        public float GetAngleDegs()
        {
            return pBody.GetAngle() * PhysicWorld.RAD_TO_DEG;
        }

        public Vec2 GetPhysicalVelocity()
        {
            return pBody.GetLinearVelocity();
        }

        public Body GetBody()
        {
            return pBody;
        }
    }
}
