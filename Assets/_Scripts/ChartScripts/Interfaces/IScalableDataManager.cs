using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Chart
{
    public interface IScalableDataManager: IChartDataManager 
    {
        DateTime WorkBeginTime { get; }
        DateTime WorkEndTime { get; }

        event Action WorkFlowChanged;
        void SetWorkDataRange(DateTime fromTime, DateTime toTime);
        void SetWorkDataRange(int startFluctuationNumber, int loadFluctuationCount);
        void ResetWorkDataRange();
        bool AddTimeStep();
    }
}
