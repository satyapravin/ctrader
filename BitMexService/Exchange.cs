using Bitmex.NET;
using Bitmex.NET.Models;
using Bitmex.NET.Dtos;
using Bitmex.NET.Dtos.Socket;
using EmbeddedService;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using ExchangeService.Logging;
using System.Threading;

namespace ExchangeService
{
    public delegate void OnBookChanged(BitmexSocketDataMessage<IEnumerable<OrderBookDto>> response);
    public delegate void OnPosition(BitmexSocketDataMessage<IEnumerable<PositionDto>> response);
    public delegate void OnOrder(BitmexSocketDataMessage<IEnumerable<OrderDto>> response);
    public delegate void OnMargin(BitmexSocketDataMessage<IEnumerable<MarginDto>> response);

    public class Exchange : IEmbeddedService
    {
        System.Threading.EventWaitHandle waitHandle = new System.Threading.AutoResetEvent(false);
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
        private IBitmexApiService bitmexREST = null;
        private IBitmexAuthorization _bitmexAuthorization;
        private IBitmexApiSocketService _bitmexApiSocketService;
        private readonly string apiKey;
        private readonly string apiSecret;
        private readonly bool isLive = false;
        private HashSet<string> subscriptions = new HashSet<string>();
        private OnBookChanged bookChangedHandler = null;
        private OnMargin marginHandler = null;
        private OnOrder orderHandler = null;
        private OnPosition positionHandler = null;

        public ServiceType Service { get { return ServiceType.EXCHANGE; } }
        public Exchange(string apiKey, string apiSecret, bool isLive = false)
        {
            this.isLive = isLive;
            this.apiKey = apiKey;
            this.apiSecret = apiSecret;
        }

        public void Register(List<string> symbols)
        {
            symbols.ForEach(s => subscriptions.Add(s));
        }

        public void SubscribeMarketData(OnBookChanged bookChanged)
        {
            if (bookChangedHandler == null)
                bookChangedHandler = bookChanged;
            else
                bookChangedHandler += bookChanged;
        }

        public void SubscribePositions(OnPosition posHandler)
        {
            if (positionHandler == null)
                positionHandler = posHandler;
            else
                positionHandler += posHandler;
        }

        public void SubscribeOrders(OnOrder handler)
        {
            if (orderHandler == null)
                orderHandler = handler;
            else
                orderHandler += handler;
        }

        public void SubscribeMargin(OnMargin handler)
        {
            if (marginHandler == null)
                marginHandler = handler;
            else
                marginHandler += handler;
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

            _bitmexApiSocketService.Subscribe(BitmetSocketSubscriptions.CreateOrderSubsription(message =>
            {
                    if (orderHandler != null)
                        orderHandler(message);
            }));

            _bitmexApiSocketService.Subscribe(BitmetSocketSubscriptions.CreatePositionSubsription(message =>
            {
                    if (positionHandler != null)
                        positionHandler(message);
            }));

            _bitmexApiSocketService.Subscribe(BitmetSocketSubscriptions.CreateMarginSubscription(message =>
            {
                    if (marginHandler != null)
                        marginHandler(message);
            }));

            _bitmexApiSocketService.Subscribe(BitmetSocketSubscriptions.CreateOrderBookL2_25Subsription(message =>
            {
                if (bookChangedHandler != null)
                {
                    bookChangedHandler(message);
                }
            }));

            bitmexREST = BitmexApiService.CreateDefaultApi(_bitmexAuthorization);
            waitHandle.Set();
            return waitHandle;
        }

        public Task<BitmexApiResult<OrderDto>> Post(OrderPOSTRequestParams posOrderParams)
        {
            return bitmexREST.Execute(BitmexApiUrls.Order.PostOrder, posOrderParams);
        }

        public Task<BitmexApiResult<OrderDto>> Put(OrderPUTRequestParams putOrderParams)
        {
            return bitmexREST.Execute(BitmexApiUrls.Order.PutOrder, putOrderParams);
        }

        public Task<BitmexApiResult<List<OrderDto>>> Delete(OrderDELETERequestParams delOrderParams)
        {
            return bitmexREST.Execute(BitmexApiUrls.Order.DeleteOrder, delOrderParams);
        }

        public bool Stop()
        {
            return true;
        }
    }
}