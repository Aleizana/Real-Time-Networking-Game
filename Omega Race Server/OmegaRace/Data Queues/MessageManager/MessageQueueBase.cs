﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OmegaRace.Data_Queues.MessageManager
{
    class MessageQueueBase
    {
        //Put these into individual classes
        public Queue<DataMessage> pInputQueue;
        public Queue<DataMessage> pOutputQueue;

        //Process out and process in can happen within the derived classes
        public virtual void ProcessOut()
        {
           
        }

        public virtual void ProcessIn(bool processMoves)
        {
           
        }

        public virtual void Process(bool processMoves)
        {
            this.ProcessOut();
            this.ProcessIn(processMoves);
        }
    }
}
