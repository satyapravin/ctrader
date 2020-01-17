using Bitmex.NET;
using Bitmex.NET.Models;
using Bitmex.NET.Dtos;
using Bitmex.NET.Dtos.Socket;
using EmbeddedService;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Threading;
using log4net;
using System.Collections.Concurrent;

namespace ExchangeService
{
    public class Exchange : IEmbeddedService
    {
        readonly System.Threading.EventWaitHandle waitHandle = new System.Threading.AutoResetEvent(false);
        private static readonly ILog Log = log4net.LogManager.GetLogger(typeof(Exchange));
        private IBitmexApiService bitmexREST = null;
        private IBitmexAuthorization _bitmexAuthorization;
        private IBitmexApiSocketService _bitmexApiSocketService;
        private readonly string apiKey;
        private readonly string apiSecret;
        private readonly bool isLive = false;

        public readonly MDS marketDataSystem = new MDS();
        
        public ServiceType Service { get { return ServiceType.EXCHANGE; } }
        public Exchange(string apiKey, string apiSecret, bool isLive = false)
        {
            this.isLive = isLive;
            this.apiKey = apiKey;
            this.apiSecret = apiSecret;
        }

        public WaitHandle Start()
        {
            var env = BitmexEnvironment.Test;

            if (this.isLive)
                env = BitmexEnvironment.Prod;

            _bitmexAuthorization = new BitmexAuthorization { BitmexEnvironment = env };
            _bitmexAuthorization.Key = apiKey;
            _bitmexAuthorization.Secret = apiSecret;
            _bitmexApiSocketService = BitmexApiSocketService.CreateDefaultApi(_bitmexAuthorization);

            if (!_bitmexApiSocketService.Connect())
            {
                Log.Error("Failed to connect to bimex websocket");
                throw new ApplicationException("Failed to connect to bitmex websocket");
            }

            if (marketDataSystem.GetSubscriptions().Length > 0)
            {
                _bitmexApiSocketService.Subscribe(BitmexSocketSubscriptions.CreateOrderBook10Subsription(message =>
                {
                    marketDataSystem.Post(message.Data);
                }, marketDataSystem.GetSubscriptions()));
            }

            if (marketDataSystem.GetInstrs().Length > 0)
            {
                _bitmexApiSocketService.Subscribe(BitmexSocketSubscriptions.CreateInstrumentSubsription(message =>
                {
                    marketDataSystem.Post(message.Data);
                }, marketDataSystem.GetInstrs()));
            }
            bitmexREST = BitmexApiService.CreateDefaultApi(_bitmexAuthorization);
            marketDataSystem.Start();
            waitHandle.Set();
            return waitHandle;
        }

        public Task<BitmexApiResult<List<PositionDto>>> GetPositions(PositionGETRequestParams posParams)
        {
            return bitmexREST.Execute(BitmexApiUrls.Position.GetPosition, posParams);
        }

        public Task<BitmexApiResult<MarginDto>> GetMargin(UserMarginGETRequestParams marginParams)
        {
            return bitmexREST.Execute(BitmexApiUrls.User.GetUserMargin, marginParams);
        }

        public Task<BitmexApiResult<List<OrderDto>>> GetOrder(OrderGETRequestParams orderGetParams)
        {
            return bitmexREST.Execute(BitmexApiUrls.Order.GetOrder, orderGetParams);
        }
        
        public Task<BitmexApiResult<OrderDto>> PostOrder(OrderPOSTRequestParams posOrderParams)
        {
            return bitmexREST.Execute(BitmexApiUrls.Order.PostOrder, posOrderParams);
        }

        public Task<BitmexApiResult<OrderDto>> PutOrder(OrderPUTRequestParams putOrderParams)
        {
            return bitmexREST.Execute(BitmexApiUrls.Order.PutOrder, putOrderParams);
        }

        public Task<BitmexApiResult<List<OrderDto>>> DeleteOrder(OrderDELETERequestParams delOrderParams)
        {
            return bitmexREST.Execute(BitmexApiUrls.Order.DeleteOrder, delOrderParams);
        }

        public bool Stop()
        {
            marketDataSystem.Stop();
            return true;
        }
    }
}