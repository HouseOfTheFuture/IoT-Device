﻿// Code generated by Microsoft (R) AutoRest Code Generator 0.9.7.0
// Changes may cause incorrect behavior and will be lost if the code is regenerated.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using TickTack.Models;

namespace TickTack
{
    public partial interface IServiceLocator
    {
        /// <param name='cancellationToken'>
        /// Cancellation token.
        /// </param>
        Task<HttpOperationResponse<ServiceLocations>> GetWithOperationResponseAsync(CancellationToken cancellationToken = default(System.Threading.CancellationToken));
    }
}
