// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using CseLabs.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ObjectModel.Model;

namespace ObjectModel
{
    /// <summary>
    /// Handle deployment requests
    /// </summary>
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class DeployController : Controller
    {
        private static readonly CseLog Logger = new ()
        {
            Name = typeof(DeployController).FullName,
            ErrorMessage = "DeployControllerException",
        };

        /// <summary>
        /// Returns a dictionary of deployed Clusters, Namespaces, and Apps
        /// </summary>
        /// <response code="200"></response>
        /// <returns>IActionResult</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, ClusterDeploy>))]
        public IActionResult GetDeployModel()
        {
            Logger.LogInformation("get", "Deploy");

            return Ok(new Database().DeployModel);
        }
    }
}
