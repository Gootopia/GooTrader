﻿/* Copyright (C) 2018 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IBApi;

namespace IBSampleApp.messages
{
    class TickPriceMessage : MarketDataMessage
    {
        private double price;
        private TickAttrib attribs;

        public TickPriceMessage(int requestId, int field, double price, TickAttrib attribs)
            : base(requestId, field)
        {
            this.price = price;
            this.attribs = attribs;
        }

        public TickAttrib Attribs
        {
            get { return attribs; }
            set { attribs = value; }
        }
        public double Price
        {
            get { return price; }
            set { price = value; }
        }

    }
}
