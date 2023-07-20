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
    public class Ship : GameObject
    {
        float maxSpeed;
        float maxForce;
        float rotateSpeed;

        Vec2 localFwd;
        Vec2 respawnPos;
        bool respawning;

        PlayerData owner;
        PlayerManager PlMgr;

        //Self-Added
        public PlayerPredictionControlShip predControl;

        public DeadReckoningShip deadReckShip;

        public Ship(PlayerData own, PlayerManager pMgr, Azul.Rect screenRect, Azul.Color color)
            : base (GAMEOBJECT_TYPE.SHIP, new Azul.Rect(0, 0, 32, 32), screenRect, TextureCollection.shipTexture, color)
        {
            PhysicBody_Data data = new PhysicBody_Data();
            data.position = new Vec2(screenRect.x, screenRect.y);
            data.size = new Vec2(screenRect.width, screenRect.height);
            data.active = true;
            data.angle = 0;
            data.shape_type = PHYSICBODY_SHAPE_TYPE.SHIP_MANIFOLD;
            CreatePhysicBody(data);

            owner = own;
            PlMgr = pMgr;
            localFwd = new Vec2(1, 0);

            // maxSpeed is m/s
            maxSpeed = 3;
            maxForce = .3f;
            rotateSpeed = 5.0f;

            respawnPos = data.position;

            //Self-Added
            predControl = new PlayerPredictionControlShip();
            deadReckShip = new DeadReckoningShip();

        }

        public PlayerData GetOwner()
        {
            return owner;
        }

        public override void Update()
        {
            if (respawning == false)
            {
                base.Update();
                LimitSpeed();
            }
            else // needed because we can't change the physical properties during collision processing
            {
                pBody.SetPhysicalPosition(respawnPos);
                pBody.SetPixelVelocity(new Vec2(0, 0));
                respawning = false;
            }
            
        }


        public override void Draw()
        {
            base.Draw();
        }

        public void Move(int vertInput)
        {
            if(vertInput < 0)
            {
                vertInput = 0;
            }
            Vec2 heading = pBody.GetBody().GetWorldVector(localFwd);
            pBody.ApplyForce(heading * vertInput * maxForce, GetPixelPosition());
        }

        public void Rotate(int horzInput)
        {
            pBody.SetAngle(pBody.GetAngleDegs() + (horzInput * -rotateSpeed));
        }

        public void LimitSpeed()
        {
            Vec2 shipVel = pBody.GetPhysicalVelocity();
            float magnitude = shipVel.Length();

            if(magnitude > maxSpeed)
            {
                shipVel.Normalize();
                shipVel *= maxSpeed;
                pBody.GetBody().SetLinearVelocity(shipVel);
            }
        }

        public void Respawn(Vec2 v)
        {
            respawning = true;
            respawnPos = v;
        }

        public Vec2 GetHeading()
        {
            return pBody.GetBody().GetWorldVector(localFwd);
        }

        public void OnHit()
        {
            PlMgr.PlayerKilled(this.owner);
        }

        public override void Accept(GameObject obj)
        {
            obj.VisitShip(this);
        }

        public override void VisitMissile(Missile m)
        {
            CollisionEvent.Action(m, this);
        }
        public override void VisitMine(Mine m)
        {
            CollisionEvent.Action(this, m);
        }

        public override void VisitFence(Fence f)
        {
            CollisionEvent.Action(f, this);
        }
    }

    public class PlayerPredictionControlShip
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

        public PlayerPredictionControlShip()
        {
            velPrime = new Vec2();
            predPos = new Vec2();

            plrInitPred = false;

            timeHold = 0.0f;
            holdAngle = 0.0f;
        }

        public void PredictPos(Ship plrShip)
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
                //Debug.Print("Predicted position: " + holdVecSum.X + ", " + holdVecSum.Y + "/ Velocity: X: " + velPrime.X + ", Y: " + velPrime.Y + "/ PredTime: " + predTime);
              
                //Set the predicted position of the ship
                plrShip.SetPosAndAngle(holdVecSum.X, holdVecSum.Y, holdAngle);
            }
            
        }

        public void Set(Ship plrShip, float newAngle, Vec2 posPrime, float arriveTime)
        {
            //Set the first position received as the pos reference
            if (!plrInitPred)
            {
                predPos = plrShip.GetPixelPosition();
            }

            //Once starting data has been received, checks can be ignored
            plrInitPred = true;

            //Store the angle from the server
            holdAngle = newAngle;

            //t'
            timePrime = arriveTime;

            //v' = (p' - p) / (t' - t)
            ComputeVel(plrShip, posPrime);

            //Predicted position now points to the updated spot
            //new p'
            predPos = posPrime;
        }

        private void ComputeVel(Ship plrShip, Vec2 posPrime)
        {
            Vec2 posNumer;

            //Debug.Print("PosPrime: X: " + posPrime.X + ", Y: " + posPrime.Y + "/ PredPos: " + predPos.X + ", Y: " + predPos.Y);

            //Find the numerator p' - p                   
            //Using previous position to calculate the current one
            posNumer = posPrime - predPos;

            //Find the denominator t' - t
            //In this case, t' is the time of arrival of the message
            float timeCalc = timePrime - timeHold;

            //Debug.Print("Arrival time: " + timePrime + ", Previous Time: " + timeHold);

            //v' = (p' - p) / (t' - t)
            velPrime.X = posNumer.X / timeCalc;
            velPrime.Y = posNumer.Y / timeCalc;

            //Debug.Print("Velocity: X: " + velPrime.X + ", Y: " + velPrime.Y);
        }

        public void RunPredictions(Ship plrShip)
        {
            //Predict movement of player ships every frame
            plrShip.predControl.PredictPos(plrShip);

            //Predict movement of missiles
            foreach (Missile m in plrShip.GetOwner().missileList)
            {
                
                m.predControl.PredictPos(m);
            }
        }
    }

    public class DeadReckoningShip
    {
        //Updated ship position
        //Starts with initial ship position
        Vec2 predPos;

        //Holds the sent velocity of the ship
        Vec2 holdShipVel;

        //Holds the client's angle of the ship
        float holdAngle;

        //Holds the time of pos sent from the server
        float holdServerTime;

        //Ship will need an initial position message before prediction can occur
        public bool initPred { get; set; }

        public DeadReckoningShip()
        {
            initPred = false;
        }

        public void PredictPos(Ship plrShip)
        {
            //Hold the difference in current time from the time of server pos
            //Time "d" obtained from current time - "t" 
            float timeDiff = TimeManager.GetCurrentTime() - holdServerTime;

            //t * v
            Vec2 timeMultVec = timeDiff * holdShipVel;

            //Hold the position of the ship at time T with vel V
            Vec2 posAtTime;

            //p + t * v
            posAtTime = predPos + timeMultVec;

            //Set the position and angle of the ship
            plrShip.SetPosAndAngle(posAtTime.X, posAtTime.Y, holdAngle);
        }

        //Set the position, angle, and speed of the ship at time T when a position update is sent to client
        public void Set(Vec2 newPos, Vec2 newVelocity, float newAngle, float posTime)
        {
            predPos = newPos;
            holdShipVel = newVelocity;
            holdAngle = newAngle;
            holdServerTime = posTime;
            initPred = true;
        }

        public void RunPlayerPredictions(Ship plrShip)
        {
            //If player prediction for the ship has been initialized
            if (initPred)
            {
                plrShip.deadReckShip.PredictPos(plrShip);
            }
            //Predict positions for each missile
            foreach (Missile m in plrShip.GetOwner().missileList)
            {
                m.deadReckMiss.PredictPos(m);
            }
        }
    }

}
