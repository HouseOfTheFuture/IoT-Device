using System;
using System.Net.Http;
using Microsoft.Azure.AppService;

namespace HouseOfTheFuture.IoTHub.Host
{
    public static class TicktackAppServiceExtensions
    {
        public static Ticktack CreateTicktack(this IAppServiceClient client)
        {
            return new Ticktack(client.CreateHandler());
        }

        public static Ticktack CreateTicktack(this IAppServiceClient client, params DelegatingHandler[] handlers)
        {
            return new Ticktack(client.CreateHandler(handlers));
        }

        public static Ticktack CreateTicktack(this IAppServiceClient client, Uri uri, params DelegatingHandler[] handlers)
        {
            return new Ticktack(uri, client.CreateHandler(handlers));
        }

        public static Ticktack CreateTicktack(this IAppServiceClient client, HttpClientHandler rootHandler, params DelegatingHandler[] handlers)
        {
            return new Ticktack(rootHandler, client.CreateHandler(handlers));
        }
    }
}
