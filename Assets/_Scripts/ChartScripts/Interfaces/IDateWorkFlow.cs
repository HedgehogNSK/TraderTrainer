using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Chart
{
    public interface IDateWorkFlow 
    {
        void SetWorkDataRange(DateTime fromTime, DateTime toTime);
        void SetWorkDataRange(int startFluctuationNumber, int loadFluctuationCount);
        void ResetWorkDataRange();
        bool AddTimeStep();
    }
}
