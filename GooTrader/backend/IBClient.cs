/* Copyright (C) 2013 Interactive Brokers LLC. All rights reserved.  This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */
using System;
using System.Collections.Generic;
using IBApi;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;

namespace GooTrader
{
    public class IBClient : EWrapper
    {
        private EClientSocket clientSocket;
        private int nextOrderId;
        private int clientId;
  
        public Task<Contract> ResolveContractAsync(int conId, string refExch)
        {
            var reqId = new Random(DateTime.Now.Millisecond).Next();
            var resolveResult = new TaskCompletionSource<Contract>();
            var resolveContract_Error = new Action<int, int, string, Exception>((id, code, msg, ex) =>
                {
                    if (reqId != id)
                        return;

                    resolveResult.SetResult(null);
                });
            var resolveContract = new Action<int, ContractDetails>((id, details) =>
                {
                    if (id == reqId) 
                        resolveResult.SetResult(details.Summary);
                });
            var contractDetailsEnd = new Action<int>(id =>
            {
                if (reqId == id && !resolveResult.Task.IsCompleted)
                    resolveResult.SetResult(null);
            });

            var tmpError = Error;
            var tmpContractDetails = ContractDetails;
            var tmpContractDetailsEnd = ContractDetailsEnd;

            Error = resolveContract_Error;
            ContractDetails = resolveContract;
            ContractDetailsEnd = contractDetailsEnd;

            resolveResult.Task.ContinueWith(t => {
                Error = tmpError;
                ContractDetails = tmpContractDetails;
                ContractDetailsEnd = tmpContractDetailsEnd; 
            });

            ClientSocket.reqContractDetails(reqId, new Contract() { ConId = conId, Exchange = refExch });

            return resolveResult.Task;
        }

        public Task<Contract[]> ResolveContractAsync(string secType, string symbol, string currency, string exchange)
        {
            var reqId = new Random(DateTime.Now.Millisecond).Next();
            var res = new TaskCompletionSource<Contract[]>();
            var contractList = new List<Contract>();
            var resolveContract_Error = new Action<int, int, string, Exception>((id, code, msg, ex) =>
                {
                    if (reqId != id)
                        return;

                    res.SetResult(new Contract[0]);
                });
            var contractDetails = new Action<int, ContractDetails>((id, details) =>
                {
                    if (reqId != id)
                        return;

                    contractList.Add(details.Summary);
                });
            var contractDetailsEnd = new Action<int>(id =>
                {
                    if (reqId == id)
                        res.SetResult(contractList.ToArray());
                });

            var tmpError = Error;
            var tmpContractDetails = ContractDetails;
            var tmpContractDetailsEnd = ContractDetailsEnd;

            Error = resolveContract_Error;
            ContractDetails = contractDetails;
            ContractDetailsEnd = contractDetailsEnd;

            res.Task.ContinueWith(t => {
                Error = tmpError;
                ContractDetails = tmpContractDetails;
                ContractDetailsEnd = tmpContractDetailsEnd; 
            });

            ClientSocket.reqContractDetails(reqId, new Contract() { SecType = secType, Symbol = symbol, Currency = currency, Exchange = exchange });

            return res.Task;
        }

        #region Properties
        public int ClientId
        {
            get { return clientId; }
            set { clientId = value; }
        }

        public IBClient(EReaderMonitorSignal signal)
        {
            clientSocket = new EClientSocket(this, signal);
        }

        public EClientSocket ClientSocket
        {
            get { return clientSocket; }
            private set { clientSocket = value; }
        }

        public int NextOrderId
        {
            // Auto increment to always have a valid request id.
            get { return nextOrderId++; }
            set { nextOrderId = value; }
        }
        #endregion Properties

        #region Actions
        public event Action<int, int, string, Exception> Error;
        public event Action ConnectionClosed;
        public event Action<long> CurrentTime;
        public event Action<int, int, double, int> TickPrice;
        public event Action<int, int, int> TickSize;
        public event Action<int, int, string> TickString;
        public event Action<int, int, double> TickGeneric;
        public event Action<int, int, double, string, double, int, string, double, double> TickEFP;
        public event Action<int> TickSnapshotEnd;
        public event Action<int> NextValidId;
        public event Action<int, UnderComp> DeltaNeutralValidation;
        public event Action<string> ManagedAccounts;
        public event Action<int, int, double, double, double, double, double, double, double, double> TickOptionCommunication;
        public event Action<int, string, string, string, string> AccountSummary;
        public event Action<int> AccountSummaryEnd;
        public event Action<string, string, string, string> UpdateAccountValue;
        public event Action<Contract, double, double, double, double, double, double, string> UpdatePortfolio;
        public event Action<string> UpdateAccountTime;
        public event Action<string> AccountDownloadEnd;
        public event Action<int, string, double, double, double, int, int, double, int, string> OrderStatus;
        public event Action<int, Contract, Order, OrderState> OpenOrder;
        public event Action OpenOrderEnd;
        public event Action<int, ContractDetails> ContractDetails;
        public event Action<int> ContractDetailsEnd;
        public event Action<int, Contract, Execution> ExecDetails;
        public event Action<int> ExecDetailsEnd;
        public event Action<CommissionReport> CommissionReport;
        public event Action<int, string> FundamentalData;
        public event Action<int, string, double, double, double, double, int, int, double, bool> HistoricalData;
        public event Action<int, string, string> HistoricalDataEnd;
        public event Action<int, int> MarketDataType;
        public event Action<int, int, int, int, double, int> UpdateMktDepth;
        public event Action<int, int, string, int, int, double, int> UpdateMktDepthL2;
        public event Action<int, int, String, String> UpdateNewsBulletin;
        public event Action<string, Contract, double, double> Position;
        public event Action PositionEnd;
        public event Action<int, long, double, double, double, double, long, double, int> RealtimeBar;
        public event Action<string> ScannerParameters;
        public event Action<int, int, ContractDetails, string, string, string, string> ScannerData;
        public event Action<int> ScannerDataEnd;
        public event Action<int, string> ReceiveFA;
        public event Action<int, ContractDetails> BondContractDetails;
        public event Action<string> VerifyMessageAPI;
        public event Action<bool, string> VerifyCompleted;
        public event Action<string, string> VerifyAndAuthMessageAPI;
        public event Action<bool, string> VerifyAndAuthCompleted;
        public event Action<int, string> DisplayGroupList;
        public event Action<int, string> DisplayGroupUpdated;
        public event Action<int, string, string, Contract, double, double> PositionMulti;
        public event Action<int> PositionMultiEnd;
        public event Action<int, string, string, string, string, string> AccountUpdateMulti;
        public event Action<int> AccountUpdateMultiEnd;
        public event Action<int, string, int, string, string, HashSet<string>, HashSet<double>> SecurityDefinitionOptionParameter;
        public event Action<int> SecurityDefinitionOptionParameterEnd;
        public event Action<int, SoftDollarTier[]> SoftDollarTiers;
        #endregion Actions

        #region Wrapper Functions
        void EWrapper.error(Exception e)
        {
            var tmp = Error;
            if (tmp != null)
                tmp(0, 0, null, e);
        }

        void EWrapper.error(string str)
        {
            var tmp = Error;

            if (tmp != null)
                tmp(0, 0, str, null);
        }

        void EWrapper.error(int id, int errorCode, string errorMsg)
        {
            var tmp = Error;

            if (tmp != null)
                tmp(id, errorCode, errorMsg, null);
        }

        void EWrapper.connectionClosed()
        {
            var tmp = ConnectionClosed;

            if (tmp != null)
                tmp();
        }

        void EWrapper.currentTime(long time)
        {
            var tmp = CurrentTime;

            if (tmp != null)
                tmp(time);
        }

        void EWrapper.tickPrice(int tickerId, int field, double price, int canAutoExecute)
        {
            var tmp = TickPrice;

            if (tmp != null)
                tmp(tickerId, field, price, canAutoExecute);
        }

        void EWrapper.tickSize(int tickerId, int field, int size)
        {
            var tmp = TickSize;

            if (tmp != null)
                tmp(tickerId, field, size);
        }

        void EWrapper.tickString(int tickerId, int tickType, string value)
        {
            var tmp = TickString;

            if (tmp != null)
                tmp(tickerId, tickType, value);
        }

        void EWrapper.tickGeneric(int tickerId, int field, double value)
        {
            var tmp = TickGeneric;

            if (tmp != null)
                tmp(tickerId, field, value);            
        }

        void EWrapper.tickEFP(int tickerId, int tickType, double basisPoints, string formattedBasisPoints, double impliedFuture, int holdDays, string futureLastTradeDate, double dividendImpact, double dividendsToLastTradeDate)
        {
            var tmp = TickEFP;

            if (tmp != null)
                tmp(tickerId, tickType, basisPoints, formattedBasisPoints, impliedFuture, holdDays, futureLastTradeDate, dividendImpact, dividendsToLastTradeDate);
        }

        void EWrapper.tickSnapshotEnd(int tickerId)
        {
            var tmp = TickSnapshotEnd;

            if (tmp != null)
                tmp(tickerId);            
        }

        void EWrapper.nextValidId(int orderId)
        {
            var tmp = NextValidId;

            if (tmp != null)
                tmp(orderId);

            NextOrderId = orderId;
        }

        void EWrapper.deltaNeutralValidation(int reqId, UnderComp underComp)
        {
            var tmp = DeltaNeutralValidation;

            if (tmp != null)
                tmp(reqId, underComp);            
        }

        void EWrapper.managedAccounts(string accountsList)
        {
            var tmp = ManagedAccounts;

            if (tmp != null)
                tmp(accountsList);            
        }

        void EWrapper.tickOptionComputation(int tickerId, int field, double impliedVolatility, double delta, double optPrice, double pvDividend, double gamma, double vega, double theta, double undPrice)
        {
            var tmp = TickOptionCommunication;

            if (tmp != null)
                tmp(tickerId, field, impliedVolatility, delta, optPrice, pvDividend, gamma, vega, theta, undPrice);
        }

        void EWrapper.accountSummary(int reqId, string account, string tag, string value, string currency)
        {
            var tmp = AccountSummary;

            if (tmp != null)
                tmp(reqId, account, tag, value, currency);
        }

        void EWrapper.accountSummaryEnd(int reqId)
        {
            var tmp = AccountSummaryEnd;

            if (tmp != null)
                tmp(reqId);
        }

        void EWrapper.updateAccountValue(string key, string value, string currency, string accountName)
        {
            var tmp = UpdateAccountValue;

            if (tmp != null)
                tmp(key, value, currency, accountName);
        }

        void EWrapper.updatePortfolio(Contract contract, double position, double marketPrice, double marketValue, double averageCost, double unrealisedPNL, double realisedPNL, string accountName)
        {
            var tmp = UpdatePortfolio;

            if (tmp != null)
                tmp(contract, position, marketPrice, marketValue, averageCost, unrealisedPNL, realisedPNL, accountName);
        }

        void EWrapper.updateAccountTime(string timestamp)
        {
            var tmp = UpdateAccountTime;

            if (tmp != null)
                tmp(timestamp);
        }

        void EWrapper.accountDownloadEnd(string account)
        {
            var tmp = AccountDownloadEnd;

            if (tmp != null)
                tmp(account);
        }

        void EWrapper.orderStatus(int orderId, string status, double filled, double remaining, double avgFillPrice, int permId, int parentId, double lastFillPrice, int clientId, string whyHeld)
        {
            var tmp = OrderStatus;

            if (tmp != null)
                tmp(orderId, status, filled, remaining, avgFillPrice, permId, parentId, lastFillPrice, clientId, whyHeld);
        }

        void EWrapper.openOrder(int orderId, Contract contract, Order order, OrderState orderState)
        {
            var tmp = OpenOrder;

            if (tmp != null)
                tmp(orderId, contract, order, orderState);
        }

        void EWrapper.openOrderEnd()
        {
            var tmp = OpenOrderEnd;

            if (tmp != null)
                tmp();
        }

        void EWrapper.contractDetails(int reqId, ContractDetails contractDetails)
        {
            var tmp = ContractDetails;

            if (tmp != null)
                tmp(reqId, contractDetails);
        }

        void EWrapper.contractDetailsEnd(int reqId)
        {
            var tmp = ContractDetailsEnd;

            if (tmp != null)
                tmp(reqId);
        }

        void EWrapper.execDetails(int reqId, Contract contract, Execution execution)
        {
            var tmp = ExecDetails;

            if (tmp != null)
                tmp(reqId, contract, execution);
        }

        void EWrapper.execDetailsEnd(int reqId)
        {
            var tmp = ExecDetailsEnd;

            if (tmp != null)
                tmp(reqId);
        }

        void EWrapper.commissionReport(CommissionReport commissionReport)
        {
            var tmp = CommissionReport;

            if (tmp != null)
                tmp(commissionReport);
        }
        
        void EWrapper.fundamentalData(int reqId, string data)
        {
            var tmp = FundamentalData;

            if (tmp != null)
                tmp(reqId, data);
        }

        void EWrapper.historicalData(int reqId, string date, double open, double high, double low, double close, int volume, int count, double WAP, bool hasGaps)
        {
            var tmp = HistoricalData;

            if (tmp != null)
                tmp(reqId, date, open, high, low, close, volume, count, WAP, hasGaps);
        }

        void EWrapper.historicalDataEnd(int reqId, string startDate, string endDate)
        {
            var tmp = HistoricalDataEnd;

            if (tmp != null)
                tmp(reqId, startDate, endDate);
        }

        void EWrapper.marketDataType(int reqId, int marketDataType)
        {
            var tmp = MarketDataType;

            if (tmp != null)
                tmp(reqId, marketDataType);
        }

        void EWrapper.updateMktDepth(int tickerId, int position, int operation, int side, double price, int size)
        {
            var tmp = UpdateMktDepth;

            if (tmp != null)
                tmp(tickerId, position, operation, side, price, size);
        }

        void EWrapper.updateMktDepthL2(int tickerId, int position, string marketMaker, int operation, int side, double price, int size)
        {
            var tmp = UpdateMktDepthL2;

            if (tmp != null)
                tmp(tickerId, position, marketMaker, operation, side, price, size);
        }

        void EWrapper.updateNewsBulletin(int msgId, int msgType, String message, String origExchange)
        {
            var tmp = UpdateNewsBulletin;

            if (tmp != null)
                tmp(msgId, msgType, message, origExchange);
        }

        void EWrapper.position(string account, Contract contract, double pos, double avgCost)
        {
            var tmp = Position;

            if (tmp != null)
                tmp(account, contract, pos, avgCost);
        }

        void EWrapper.positionEnd()
        {
            var tmp = PositionEnd;

            if (tmp != null)
                tmp();            
        }

        void EWrapper.realtimeBar(int reqId, long time, double open, double high, double low, double close, long volume, double WAP, int count)
        {
            var tmp = RealtimeBar;

            if (tmp != null)
                tmp(reqId, time, open, high, low, close, volume, WAP, count);
        }

        void EWrapper.scannerParameters(string xml)
        {
            var tmp = ScannerParameters;

            if (tmp != null)
                tmp(xml);
        }

        void EWrapper.scannerData(int reqId, int rank, ContractDetails contractDetails, string distance, string benchmark, string projection, string legsStr)
        {
            var tmp = ScannerData;

            if (tmp != null)
                tmp(reqId, rank, contractDetails, distance, benchmark, projection, legsStr);
        }

        void EWrapper.scannerDataEnd(int reqId)
        {
            var tmp = ScannerDataEnd;

            if (tmp != null)
                tmp(reqId);
        }

        void EWrapper.receiveFA(int faDataType, string faXmlData)
        {
            var tmp = ReceiveFA;

            if (tmp != null)
                tmp(faDataType, faXmlData);
        }

        void EWrapper.bondContractDetails(int requestId, ContractDetails contractDetails)
        {
            var tmp = BondContractDetails;

            if (tmp != null)
                tmp(requestId, contractDetails);
        }

        void EWrapper.verifyMessageAPI(string apiData)
        {
            var tmp = VerifyMessageAPI;

            if (tmp != null)
                tmp(apiData);
        }

        void EWrapper.verifyCompleted(bool isSuccessful, string errorText)
        {
            var tmp = VerifyCompleted;

            if (tmp != null)
                tmp(isSuccessful, errorText);
        }

        void EWrapper.verifyAndAuthMessageAPI(string apiData, string xyzChallenge)
        {
            var tmp = VerifyAndAuthMessageAPI;

            if (tmp != null)
                tmp(apiData, xyzChallenge);
        }

        void EWrapper.verifyAndAuthCompleted(bool isSuccessful, string errorText)
        {
            var tmp = VerifyAndAuthCompleted;

            if (tmp != null)
                tmp(isSuccessful, errorText);            
        }

        void EWrapper.displayGroupList(int reqId, string groups)
        {
            var tmp = DisplayGroupList;

            if (tmp != null)
                tmp(reqId, groups);
        }

        void EWrapper.displayGroupUpdated(int reqId, string contractInfo)
        {
            var tmp = DisplayGroupUpdated;

            if (tmp != null)
                tmp(reqId, contractInfo);
        }

        void EWrapper.connectAck()
        {
            if (ClientSocket.AsyncEConnect)
                ClientSocket.startApi();
        }

        void EWrapper.positionMulti(int reqId, string account, string modelCode, Contract contract, double pos, double avgCost)
        {
            var tmp = PositionMulti;

            if (tmp != null)
                tmp(reqId, account, modelCode, contract, pos, avgCost);
        }

        void EWrapper.positionMultiEnd(int reqId)
        {
            var tmp = PositionMultiEnd;

            if (tmp != null)
                tmp(reqId);
        }

        void EWrapper.accountUpdateMulti(int reqId, string account, string modelCode, string key, string value, string currency)
        {
            var tmp = AccountUpdateMulti;

            if (tmp != null)
                tmp(reqId, account, modelCode, key, value, currency);
        }

        void EWrapper.accountUpdateMultiEnd(int reqId)
        {
            var tmp = AccountUpdateMultiEnd;

            if (tmp != null)
                tmp(reqId);
        }

        void EWrapper.securityDefinitionOptionParameter(int reqId, string exchange, int underlyingConId, string tradingClass, string multiplier, HashSet<string> expirations, HashSet<double> strikes)
        {
            var tmp = SecurityDefinitionOptionParameter;

            if (tmp != null)
                tmp(reqId, exchange, underlyingConId, tradingClass, multiplier, expirations, strikes);
        }

        void EWrapper.securityDefinitionOptionParameterEnd(int reqId)
        {
            var tmp = SecurityDefinitionOptionParameterEnd;

            if (tmp != null)
                tmp(reqId);
        }

        void EWrapper.softDollarTiers(int reqId, SoftDollarTier[] tiers)
        {
            var tmp = SoftDollarTiers;

            if (tmp != null)
                tmp(reqId, tiers);
        }
        #endregion Wrapper Functions
    }
}
