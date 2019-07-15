﻿/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBSampleApp.messages
{
    class HistoricalDataEndMessage
    {
        private int requestId;
        private string startDate;
        private string endDate;

        public string StartDate
        {
            get { return startDate; }
            set { startDate = value; }
        }

        public int RequestId
        {
            get { return requestId; }
            set { requestId = value; }
        }
        
        public string EndDate
        {
            get { return endDate; }
            set { endDate = value; }
        }

        public HistoricalDataEndMessage(int requestId, string startDate, string endDate)
        {
            RequestId = requestId;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
