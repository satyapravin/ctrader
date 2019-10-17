using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Executions;
using Bitmex.Client.Websocket.Responses.Margins;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Websockets;
using EmbeddedService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BitMex
{
    public delegate void OnBookChanged(BookResponse response);
    public delegate void OnExecution(ExecutionResponse respone);
    public delegate void OnPosition(PositionResponse response);
    public delegate void OnOrder(OrderResponse response);
    public delegate void OnMargin(MarginResponse response);

    public class BitMexService : IEmbeddedService, IDisposable
    {
        private BitMEXApi bitmexREST = null;
        private BitmexWebsocketClient client = null;
        private BitmexWebsocketCommunicator communicator = null;
        private readonly string apiKey;
        private readonly string apiSecret;
        private readonly bool isLive = false;
        private HashSet<string> subscriptions = new HashSet<string>();
        private int webSocketRemainingRate = 10;
        private int apiRemainingRate = 10;
        private OnBookChanged bookChangedHandler = null;
        private OnExecution executionHandler = null;
        private OnMargin marginHandler = null;
        private OnOrder orderHandler = null;
        private OnPosition positionHandler = null;

        public BitMexService(string apiKey, string apiSecret, bool isLive = false)
        {
            this.isLive = isLive;
            this.apiKey = apiKey;
            this.apiSecret = apiSecret;
        }

        public void Register(List<string> symbols)
        {
            symbols.ForEach(s => subscriptions.Add(s));
        }

        public string Query(string method, string function, Dictionary<string, object> param = null, bool auth = false, bool json = false)
        {
            return bitmexREST.Query(method, function, param, auth, json);
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

        public void SubscribeExecutions(OnExecution handler)
        {
            if (executionHandler == null)
                executionHandler = handler;
            else
                executionHandler += handler;
        }

        public async void Start()
        {
            var url = Bitmex.Client.Websocket.BitmexValues.ApiWebsocketTestnetUrl;

            if (this.isLive)
                url = Bitmex.Client.Websocket.BitmexValues.ApiWebsocketUrl;


            communicator = new BitmexWebsocketCommunicator(url);

            client = new BitmexWebsocketClient(communicator);
            await client.Authenticate(apiKey, apiSecret);

            client.Streams.OrderStream.Subscribe(order =>
            {
                  if (orderHandler != null)
                      orderHandler(order);
            });

            client.Streams.PositionStream.Subscribe(position =>
            {
                if (positionHandler != null)
                {
                    positionHandler(position);
                }

            });

            client.Streams.InfoStream.Subscribe(sinfo =>
            {
                string key = "remaining";

                if (sinfo.Limit.ContainsKey(key))
                {
                    webSocketRemainingRate = (int)sinfo.Limit[key];
                    if (webSocketRemainingRate < 3)
                    {
                        communicator.ReconnectTimeoutMs = 60 * 1000;
                    }
                    else
                    {
                        communicator.ReconnectTimeoutMs = 2000;
                    }
                }
                else
                    throw new ApplicationException("remaining websocket rate limit not found");
            });

            client.Streams.MarginStream.Subscribe(margin =>
            {
                if (marginHandler != null)
                {
                    marginHandler(margin);
                }
            });

            client.Streams.AuthenticationStream.Subscribe(auth =>
            {
                if (auth.Success)
                    SubscribeAll();
                else
                    throw new ApplicationException("Authentication failed");
            });

            client.Streams.BookStream.Subscribe(book =>
            {
                if (bookChangedHandler != null)
                {
                    bookChangedHandler(book);
                }
            });

            await communicator.Start();
            bitmexREST = new BitMEXApi(apiKey, apiSecret, this.isLive);
        }


        public bool Stop()
        {
            Dispose();
            return true;
        }

        public void Dispose()
        {
            client.Dispose();
            communicator.Dispose();
        }

        private async void SubscribeAll()
        {
            await client.Send(new MarginSubscribeRequest());
            subscriptions.ToList().ForEach(async x => await client.Send(new BookSubscribeRequest(x)));
            //await client.Send(new ExecutionSubscribeRequest());
            await client.Send(new OrderSubscribeRequest());
            await client.Send(new PositionSubscribeRequest());
        }
    }
}