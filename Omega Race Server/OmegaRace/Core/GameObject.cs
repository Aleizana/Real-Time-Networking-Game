using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Dynamics;
using Box2DX.Collision;
using Box2DX.Common;
using System.Diagnostics;

namespace OmegaRace
{
    public enum GAMEOBJECT_TYPE
    {
        NULL,
        SHIP,
        MISSILE,
        FENCEPOST,
        FENCE,
        MINE,
        PARTICLE
    }

    public class GameObject : Visitor
    {       
        // Each object has a type
        public GAMEOBJECT_TYPE type;

        // Reference for the Sprite
        protected Azul.Sprite pSprite;

        // Reference for the ScreenRect (i.e. Position and Size)
        protected Azul.Rect pScreenRect;

        // Refernce for the Color (tints the sprite)
        protected Azul.Color color;

        // World angle of the object (Degrees)
        protected float angle;

        // Reference to Physics Body
        protected PhysicBody pBody;
        
        // Every GameObject has a unique ID number.
        static int IDNUM;
        int id;
        
        bool alive;
        

        public GameObject(GAMEOBJECT_TYPE _type, Azul.Rect textureRect, Azul.Rect screenRect, Azul.Texture text, Azul.Color c)
        {
            type = _type;
            color = c;
            pSprite = new Azul.Sprite(text, textureRect, screenRect, color);
            pScreenRect = screenRect;
            id = IDNUM++;
            alive = true;
        }

        public int getID()
        {
            return id;
        }

        public Azul.Rect getDestRect()
        {
            return pScreenRect;
        }

        public virtual void Update()
        {
            if (pBody != null)
            {
                pushPhysics();
            }

            pSprite.x = pScreenRect.x;
            pSprite.y = pScreenRect.y;
            pSprite.Update();
        }

        public bool isAlive()
        {
            return alive;
        }
        public void setAlive(bool b)
        {
            alive = b;
        }

        public void CreatePhysicBody(PhysicBody_Data _data)
        {
            pBody = new PhysicBody(_data, this);
        }

        public virtual void Draw()
        {           
            pSprite.Render();
        }

        public Vec2 GetPixelPosition()
        {
            return new Vec2(pSprite.x, pSprite.y);
        }

        public void SetPosAndAngle(float x, float y, float ang)
        {
            pBody.SetPhysicalPosition(new Vec2(x, y));
            pBody.SetAngle(ang);
        }

        public void SetPixelVelocity(Vec2 v)
        {
            pBody.SetPixelVelocity(v);
        }

        public Vec2 GetPixelVelocity()
        {
            return pBody.GetPhysicalVelocity() * PhysicWorld.METERSTOPIXELS;
        }

        public Vec2 GetPhysicalVelocity()
        {
            return pBody.GetPhysicalVelocity();
        }

        public float GetAngle_Deg()
        {
            return pBody.GetAngleDegs();
        }
        public float GetAngle_Rad()
        {
            return pBody.GetAngleRads();
        }

        void pushPhysics()
        {
            Vec2 bodyPos = pBody.GetPixelPosition();
            pSprite.angle = pBody.GetAngleRads();

            pScreenRect.x = bodyPos.X;
            pScreenRect.y = bodyPos.Y;
        }

        public virtual void Destroy()
        {
            //pSprite = null;

            if (pBody != null)
            {
                Body b = pBody.GetBody();
                if (b != null)
                {
                    PhysicWorld.GetWorld().DestroyBody(b);
                    //Debug.WriteLine("GameObject {0}'s physic body destroyed", id);
                }
            }
            pBody = null;

        }
    }
}
