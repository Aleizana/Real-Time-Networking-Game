using OmegaRace.Data_Queues.DataMsgFactories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegaRace.Data_Queues.DataMsgDerived
{
    class PlrMoveFactory : DataMsgFactory
    {
        public void UpdateMissiles(PlayerData plrRef)
        {
            //For holding missile by ID
            Missile updatePos;

            //If at least one missile is present
            if (numMisServer >= 1)
            {
                //Check to make sure missile exists
                updatePos = plrRef.GetMissileByID(misID1);

                if (updatePos != null)
                {
                    //Update pos of missile
                    updatePos.SetPosAndAngle(pos1.X, pos1.Y, angle1);
                }
            }

            if (numMisServer >= 2)
            {
                updatePos = plrRef.GetMissileByID(misID2);

                if (updatePos != null)
                {
                    updatePos.SetPosAndAngle(pos2.X, pos2.Y, angle2);
                }
            }

            if (numMisServer >= 3)
            {
                updatePos = plrRef.GetMissileByID(misID3);

                if (updatePos != null)
                {
                    updatePos.SetPosAndAngle(pos3.X, pos3.Y, angle3);
                }
            }
        }

        public void UpdateMissilesDeadReck(PlayerData plrRef)
        {
            //For holding missile by ID
            Missile updatePos;

            //If at least one missile is present
            if (numMisServer >= 1)
            {
                //Check to make sure missile exists
                updatePos = plrRef.GetMissileByID(misID1);

                if (updatePos != null)
                {
                    //Set prediction data for dead reckoning on missile
                    updatePos.deadReckMiss.Set(updatePos, pos1, misSpeed1, angle1, arrivalTime);

                    //Update pos of missile
                    updatePos.SetPosAndAngle(pos1.X, pos1.Y, angle1);
                }
            }

            if (numMisServer >= 2)
            {
                updatePos = plrRef.GetMissileByID(misID2);

                if (updatePos != null)
                {
                    //Set prediction data for dead reckoning on missile
                    updatePos.deadReckMiss.Set(updatePos, pos2, misSpeed2, angle2, arrivalTime);

                    updatePos.SetPosAndAngle(pos2.X, pos2.Y, angle2);
                }
            }

            if (numMisServer >= 3)
            {
                updatePos = plrRef.GetMissileByID(misID3);

                if (updatePos != null)
                {
                    //Set prediction data for dead reckoning on missile
                    updatePos.deadReckMiss.Set(updatePos, pos3, misSpeed3, angle3, arrivalTime);

                    updatePos.SetPosAndAngle(pos3.X, pos3.Y, angle3);
                }
            }
        }

        public void UpdateMissilesClientPred(PlayerData plrRef)
        {
            //For holding missile by ID
            Missile updatePos;

            //If at least one missile is present
            if (numMisServer >= 1)
            {
                //Check to make sure missile exists
                updatePos = plrRef.GetMissileByID(misID1);

                if (updatePos != null)
                {
                    //Update client side prediction values for missile
                    updatePos.predControl.Set(updatePos, angle1, pos1, arrivalTime);

                    //Update pos of missile
                    updatePos.SetPosAndAngle(pos1.X, pos1.Y, angle1);
                }
            }

            if (numMisServer >= 2)
            {
                updatePos = plrRef.GetMissileByID(misID2);

                if (updatePos != null)
                {
                    //Update client side prediction values for missile
                    updatePos.predControl.Set(updatePos, angle2, pos2, arrivalTime);

                    updatePos.SetPosAndAngle(pos2.X, pos2.Y, angle2);
                }
            }

            if (numMisServer >= 3)
            {
                updatePos = plrRef.GetMissileByID(misID3);

                if (updatePos != null)
                {
                    //Update client side prediction values for missile
                    updatePos.predControl.Set(updatePos, angle3, pos3, arrivalTime);

                    updatePos.SetPosAndAngle(pos3.X, pos3.Y, angle3);
                }
            }
        }
    }
}
