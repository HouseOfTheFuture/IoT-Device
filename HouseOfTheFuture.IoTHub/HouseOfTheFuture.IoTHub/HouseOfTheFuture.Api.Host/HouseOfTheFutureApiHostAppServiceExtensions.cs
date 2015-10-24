using System;
using System.Net.Http;
using Microsoft.Azure.AppService;

namespace HouseOfTheFuture.IoTHub
{
    public static class HouseOfTheFutureApiHostAppServiceExtensions
    {
        public static HouseOfTheFutureApiHost CreateHouseOfTheFutureApiHost(this IAppServiceClient client)
        {
            return new HouseOfTheFutureApiHost(client.CreateHandler());
        }

        public static HouseOfTheFutureApiHost CreateHouseOfTheFutureApiHost(this IAppServiceClient client, params DelegatingHandler[] handlers)
        {
            return new HouseOfTheFutureApiHost(client.CreateHandler(handlers));
        }

        public static HouseOfTheFutureApiHost CreateHouseOfTheFutureApiHost(this IAppServiceClient client, Uri uri, params DelegatingHandler[] handlers)
        {
            return new HouseOfTheFutureApiHost(uri, client.CreateHandler(handlers));
        }

        public static HouseOfTheFutureApiHost CreateHouseOfTheFutureApiHost(this IAppServiceClient client, HttpClientHandler rootHandler, params DelegatingHandler[] handlers)
        {
            return new HouseOfTheFutureApiHost(rootHandler, client.CreateHandler(handlers));
        }
    }
}
