// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace CSApp
{
    /// <summary>
    /// Application secrets
    /// </summary>
    public class Secrets
    {
        public string Volume { get; set; }

        /// <summary>
        /// Get the secrets from the k8s volume
        /// </summary>
        /// <param name="volume">k8s volume name</param>
        /// <returns>Secrets or null</returns>
        public Dictionary<string, string> GetSecretsFromVolume()
        {
            if (string.IsNullOrWhiteSpace(Volume))
            {
                throw new ArgumentNullException(nameof(Volume));
            }

            return GetAllSecrets();
        }

        public string GetSecretFromFile(string key)
        {
            if (string.IsNullOrWhiteSpace(Volume))
            {
                throw new Exception("Volume is empty");
            }

            // thow exception if volume doesn't exist
            if (!Directory.Exists(Volume))
            {
                throw new Exception($"Volume does not exist: {Volume}");
            }

            string path = Path.Combine(Volume, key);

            if (!File.Exists(path))
            {
                throw new Exception($"Secret not found: {key}");
            }

            return File.ReadAllText(path).Trim();
        }

        // get all secrets in secrets volume
        private Dictionary<string, string> GetAllSecrets()
        {
            Dictionary<string, string> secrets = new ();

            foreach (string key in Directory.EnumerateFiles(Volume))
            {
                string value = File.ReadAllText(key).Trim();
                secrets.Add(Path.GetFileName(key), value);
            }

            return secrets;
        }

        // basic validation of Cosmos values
        //private void ValidateCosmosSecrets(Secrets sec)
        //{
        //    if (sec == null)
        //    {
        //        throw new Exception($"Unable to read secrets from volume: {volume}");
        //    }

        //    if (string.IsNullOrWhiteSpace(sec.CosmosCollection))
        //    {
        //        throw new Exception($"CosmosCollection cannot be empty");
        //    }

        //    if (string.IsNullOrWhiteSpace(sec.CosmosDatabase))
        //    {
        //        throw new Exception($"CosmosDatabase cannot be empty");
        //    }

        //    if (string.IsNullOrWhiteSpace(sec.CosmosKey))
        //    {
        //        throw new Exception($"CosmosKey cannot be empty");
        //    }

        //    if (string.IsNullOrWhiteSpace(sec.CosmosServer))
        //    {
        //        throw new Exception($"CosmosUrl cannot be empty");
        //    }

        //    if (!sec.CosmosServer.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
        //        !sec.CosmosServer.Contains(".documents.azure.com", StringComparison.OrdinalIgnoreCase))
        //    {
        //        throw new Exception($"Invalid value for CosmosUrl: {sec.CosmosServer}");
        //    }

        //    if (sec.CosmosKey.Length < 64)
        //    {
        //        throw new Exception($"Invalid value for CosmosKey: {sec.CosmosKey}");
        //    }
        //}
    }
}
