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

        //Self-Added
        public PlayerPredictionControlMissile predControl;

        public DeadReckoningMissile deadReckMiss;

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

            //Self-Added
            predControl = new PlayerPredictionControlMissile();
            deadReckMiss = new DeadReckoningMissile();
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

    public class PlayerPredictionControlMissile
    {
        //To know whether prediction data has been initially updated
        bool plrInitPred;

        //t' (t-prime)
        float timePrime;

        //Holds t' for following predictions
        //Start at 0 to align with start time of the game
        float timeHold;

        //v' (velocity-prime)
        Vec2 velPrime;

        //Updated ship position
        //Starts with initial ship position
        Vec2 predPos;

        //Stores the last angle of the ship received from the server
        float holdAngle;

        bool setVel;

        public PlayerPredictionControlMissile()
        {
            velPrime = new Vec2();
            predPos = new Vec2();

            plrInitPred = false;
            setVel = false;

            timeHold = 0.0f;
            holdAngle = 0.0f;

            timePrime = TimeManager.GetCurrentTime();
        }

        public void PredictPos(Missile plrMissile)
        {
            if (plrInitPred)
            {
                //Store current time for future calculations
                timeHold = TimeManager.GetCurrentTime();

                //(current time - t')
                float predTime = timeHold - timePrime;

                if (predTime < 0)
                {
                    predTime *= -1;
                }

                Vec2 vel_time;

                //(current time - t') * v'
                vel_time = predTime * velPrime;

                //Variable to hold the sum between vectors
                Vec2 holdVecSum;

                //p' + (current time - t') * v'
                holdVecSum = predPos + vel_time;

                //Updates the position using the pos from the server and
                //continues to use that old position to calculate new positioning.
                
                //Set the predicted position of the ship
                plrMissile.SetPosAndAngle(holdVecSum.X, holdVecSum.Y, holdAngle);
            }
        }

        public void Set(Missile plrMissile, float newAngle, Vec2 posPrime, float arriveTime)
        {
            //Set the first position received as the pos reference
            if (!plrInitPred)
            {
                predPos = plrMissile.GetPixelPosition();

                //Once starting data has been received, checks can be ignored
                plrInitPred = true;
            }

            //Store the angle from the server
            holdAngle = newAngle;

            //t'
            timePrime = arriveTime;

            if (!setVel)
            {
                //v' = (p' - p) / (t' - t)
                ComputeVel(plrMissile, posPrime);
            }
            

            //Predicted position now points to the updated spot
            //new p'
            predPos = posPrime;
        }

        private void ComputeVel(Missile plrMissile, Vec2 posPrime)
        {
            Vec2 posNumer;

            //Find the numerator p' - p                   
            //Using previous position to calculate the current one
            posNumer = posPrime - predPos;

            //Find the denominator t' - t
            //In this case, t' is the time of arrival of the message
            float timeCalc = timePrime - timeHold;

            //Debug.Print("Missile Arrival time: " + timePrime + ", Previous Time: " + timeHold);

            //v' = (p' - p) / (t' - t)
            velPrime.X = posNumer.X / timeCalc;
            velPrime.Y = posNumer.Y / timeCalc;

            setVel = true;

            //Debug.Print("Missile Velocity: X: " + velPrime.X + ", Y: " + velPrime.Y);
        }
    }

    public class DeadReckoningMissile
    {
        //Updated ship position
        //Starts with initial ship position
        Vec2 predPos;

        //Holds the sent velocity of the ship
        Vec2 holdMissVel;

        //Holds the client's angle of the ship
        float holdAngle;

        //Holds the time of pos sent from the server
        float holdServerTime;

        //Ship will need an initial position message before prediction can occur
        public bool initPred { get; set; }

        public DeadReckoningMissile()
        {
            initPred = false;
            holdMissVel = new Vec2(0.0f);
        }

        public void PredictPos(Missile plrMiss)
        {
            //Once initial pos data has been received for the missile, can predict pos
            if (initPred)
            {
                //Hold the difference in current time from the time of server pos
                //Time "d" obtained from current time - "t" 
                float timeDiff = TimeManager.GetCurrentTime() - holdServerTime;

                //t * v
                Vec2 timeMultVec = timeDiff * holdMissVel;

                //Hold the position of the missile at time T with vel V
                Vec2 posAtTime;

                //p + t * v
                posAtTime = predPos + timeMultVec;

                //Set the position and angle of the missile
                plrMiss.SetPosAndAngle(posAtTime.X, posAtTime.Y, holdAngle);
                
            }
            
        }

        //Set the position, angle, and speed of the ship at time T when a position update is sent to client
        public void Set(Missile plrMiss, Vec2 newPos, Vec2 newVelocity, float newAngle, float posTime)
        {
            predPos = newPos;
            holdMissVel = newVelocity;
            holdAngle = newAngle;
            holdServerTime = posTime;

            //Now that position is set, can do predictions
            initPred = true;
        }
    }
}
