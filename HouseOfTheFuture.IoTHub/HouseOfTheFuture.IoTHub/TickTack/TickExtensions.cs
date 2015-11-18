﻿// Code generated by Microsoft (R) AutoRest Code Generator 0.9.7.0
// Changes may cause incorrect behavior and will be lost if the code is regenerated.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using TickTack;
using TickTack.Models;

namespace TickTack
{
    public static partial class TickExtensions
    {
        /// <param name='operations'>
        /// Reference to the TickTack.ITick.
        /// </param>
        /// <param name='request'>
        /// Required.
        /// </param>
        public static object Post(this ITick operations, PostRequest request)
        {
            return Task.Factory.StartNew((object s) => 
            {
                return ((ITick)s).PostAsync(request);
            }
            , operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }
        
        /// <param name='operations'>
        /// Reference to the TickTack.ITick.
        /// </param>
        /// <param name='request'>
        /// Required.
        /// </param>
        /// <param name='cancellationToken'>
        /// Cancellation token.
        /// </param>
        public static async Task<object> PostAsync(this ITick operations, PostRequest request, CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            Microsoft.Rest.HttpOperationResponse<object> result = await operations.PostWithOperationResponseAsync(request, cancellationToken).ConfigureAwait(false);
            return result.Body;
        }
    }
}