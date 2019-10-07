# USER STORIES

# LIST OF ABBREVIATIONS
    - TA = Trading Algorithm => Any automated set of rules which can submit orders
    - TC = Trading Client => The client app which interfaces to the broker application (TWS)
    - TI = Trading Instrument => Any trading contract
    - TR = Trader => Person who oversees the system

## Trading Instrument (TI)
As a TI:
1.  I want to be able to receive real-time quotes from the TC so I can generate related data (bars, indicators, etc.).
2.  I want to be able to designate a proxy for the long and short side of my desired instrument so I can lower my exposure (SPY vs. ES)
3.  I want to be able to compute indicators from real-time data so TA can utilize this information
4.  I want to be able to download and store historical data so I can compute indicators over longer periods.

## Trading Algorithm (TA)
As a TA:
1.  I want to receive data from a TI so I can generate trading signals.
2.  I want to submit orders with the TC so I can place trades.
3.  I want to receive updates on orders I have placed from the TC so I can process trading signals.
4.  I want to be able to maintain my state during system interruptions.
5.  I want to be able to log my closed trades in order to track performance.

## Trading Client (TC)
As a TC:
1.  I want to be able to automatically connect to the broker platform so I can recover from interruptions due to PC crash or network outage.

## Trader (TR)
As a TR:
1.  I want to have access to a GUI so so I can monitor trades/system status.
2.  I want to be able to receive updates remotely so I do not have to be at the PC.
3.  I want to be able to enable/disable a TA remotely so I can override a system.
4.  I want to be able to define position limits which are automatically enforced on TA so that I can control risk

