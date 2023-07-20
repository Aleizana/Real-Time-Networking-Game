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

        //Self-added
        public DeadReckoningShip predPlr;

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

            //Self-added
            predPlr = new DeadReckoningShip();
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

    public class DeadReckoningShip
    {
        //Updated ship position
        //Starts with initial ship position
        Vec2 predPos;

        //Holds the sent velocity of the ship
        Vec2 holdShipVel;

        //Holds the client's angle of the ship
        float holdAngle;

        //Holds the time T of the last pos msg sent to the client
        float timeOfMsg;
        //Ship will need an initial position message before prediction can occur
        public bool initPred { get; set; }
        public DeadReckoningShip()
        {
            initPred = false;
        }

        public bool PredictPos(Ship plrShip)
        {
            //Hold the difference in current time from the time of pos msg
            //Time "d" obtained from current time - "t" 
            float timeDiff = TimeManager.GetCurrentTime() - timeOfMsg;

            //t * v
            //Vec2 timeMultVec = TimeManager.GetCurrentTime() * holdShipVel;
            Vec2 timeMultVec = timeDiff * holdShipVel;

            //Hold the position of the ship at time T with vel V
            Vec2 posAtTime;

            //p + t * v
            posAtTime = predPos + timeMultVec;

            //Get the real position of the ship
            Vec2 holdRealPos = plrShip.GetPixelPosition();

            //Calculate the pixel offset from the real ship to the predicted position
            float xPosDiff = holdRealPos.X - posAtTime.X;
            float yPosDiff = holdRealPos.Y - posAtTime.Y;

            //Store the differential value
            float holdDiff = xPosDiff + yPosDiff;

            //If the offset is greater than or equal to 10 pixels, send pos msg
            if (holdDiff >= 5 || holdDiff <= -5)
            {
                return true;
            }
            else
            {
                float realAngle = plrShip.GetAngle_Deg();
                float angleDiff = holdAngle - realAngle;
                
                //If the angle of the client is off by more than 60 degrees, send pos msg
                if (angleDiff >= 60 || angleDiff <= -60)
                {
                   return true;
                }
            }

            //If this is reached, return false and do not send pos msg
            return false;
        }

        //Set the position and speed of the ship at time T when a position update is sent to client
        public void Set(Vec2 newPos, Vec2 newVelocity, float newTime, float newAngle)
        {
            predPos = newPos;
            holdShipVel = newVelocity;
            timeOfMsg = newTime;
            holdAngle = newAngle;
        }

        public void RunPlayerPredictions(Ship plrShip)
        {
            //Ship will need an initial position message before prediction can occur
            if (initPred)
            {
                bool SendPosData = plrShip.predPlr.PredictPos(plrShip);

                //If predicted values are too far off from real position, send a position msg
                if (SendPosData)
                {
                    DataMessage posData;

                    //Determine the player's ship in need of pos msg
                    if (plrShip.GetOwner().player == Player.Player1)
                    {
                        posData = GameSceneCollection.ScenePlay.PoolMgr.Get(DataMessage.dataType.MOVEP1);
                        posData.myDataType = DataMessage.dataType.MOVEP1;
                    }
                    else
                    {
                        posData = GameSceneCollection.ScenePlay.PoolMgr.Get(DataMessage.dataType.MOVEP2);
                        posData.myDataType = DataMessage.dataType.MOVEP2;
                    }

                    //Update send type and add to output queue
                    posData.mySendType = DataMessage.msgType.NET;
                    GameSceneCollection.ScenePlay.MsgQueueMgr.AddToOutputQueue(posData);
                }
            }
        }
    }
}
