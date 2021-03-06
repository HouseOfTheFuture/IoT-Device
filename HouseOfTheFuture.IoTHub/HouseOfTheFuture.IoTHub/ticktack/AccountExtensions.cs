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
    public static partial class AccountExtensions
    {
        /// <param name='operations'>
        /// Reference to the TickTack.IAccount.
        /// </param>
        /// <param name='register'>
        /// Required.
        /// </param>
        public static string Post(this IAccount operations, RegistrationDto register)
        {
            return Task.Factory.StartNew((object s) => 
            {
                return ((IAccount)s).PostAsync(register);
            }
            , operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }
        
        /// <param name='operations'>
        /// Reference to the TickTack.IAccount.
        /// </param>
        /// <param name='register'>
        /// Required.
        /// </param>
        /// <param name='cancellationToken'>
        /// Cancellation token.
        /// </param>
        public static async Task<string> PostAsync(this IAccount operations, RegistrationDto register, CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            Microsoft.Rest.HttpOperationResponse<string> result = await operations.PostWithOperationResponseAsync(register, cancellationToken).ConfigureAwait(false);
            return result.Body;
        }
    }
}
