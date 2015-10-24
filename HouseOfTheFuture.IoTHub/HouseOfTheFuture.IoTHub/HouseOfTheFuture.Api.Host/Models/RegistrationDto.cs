﻿// Code generated by Microsoft (R) AutoRest Code Generator 0.9.7.0
// Changes may cause incorrect behavior and will be lost if the code is regenerated.

using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace HouseOfTheFuture.IoTHub.Models
{
    public partial class RegistrationDto
    {
        private string _password;
        
        /// <summary>
        /// Required.
        /// </summary>
        public string Password
        {
            get { return this._password; }
            set { this._password = value; }
        }
        
        private string _username;
        
        /// <summary>
        /// Required.
        /// </summary>
        public string Username
        {
            get { return this._username; }
            set { this._username = value; }
        }
        
        /// <summary>
        /// Initializes a new instance of the RegistrationDto class.
        /// </summary>
        public RegistrationDto()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the RegistrationDto class with
        /// required arguments.
        /// </summary>
        public RegistrationDto(string username, string password)
            : this()
        {
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            this.Username = username;
            this.Password = password;
        }
        
        /// <summary>
        /// Serialize the object
        /// </summary>
        /// <returns>
        /// Returns the json model for the type RegistrationDto
        /// </returns>
        public virtual JToken SerializeJson(JToken outputObject)
        {
            if (outputObject == null)
            {
                outputObject = new JObject();
            }
            if (this.Password == null)
            {
                throw new ArgumentNullException("Password");
            }
            if (this.Username == null)
            {
                throw new ArgumentNullException("Username");
            }
            if (this.Password != null)
            {
                outputObject["password"] = this.Password;
            }
            if (this.Username != null)
            {
                outputObject["username"] = this.Username;
            }
            return outputObject;
        }
    }
}
