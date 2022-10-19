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
    /// Handle api/apps requests
    /// </summary>
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class AppsController : Controller
    {
        private static readonly CseLog Logger = new ()
        {
            Name = typeof(ClustersController).FullName,
            ErrorMessage = "AppsControllerException",
        };

        private static readonly Database Db = new();

        /// <summary>
        /// Dictionary of all Apps
        /// </summary>
        /// <returns>IActionResult</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, Application>))]
        public IActionResult GetApps()
        {
            Logger.LogInformation("get", "Apps");
            return Ok(Db.Apps);
        }

        /// <summary>
        /// Returns an Application by ID
        /// </summary>
        /// <param name="appId">App ID</param>
        /// <returns>IActionResult</returns>
        [HttpGet("{appId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Application))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult GetApp([FromRoute] string appId)
        {
            Logger.LogInformation("get", $"App: {appId}");

            if (string.IsNullOrEmpty(appId) || !Db.Apps.ContainsKey(appId))
            {
                Logger.LogWarning("get-notfound", $"App: {appId}");

                return NotFound("App ID not found");
            }

            return Ok(Db.Apps[appId]);
        }

        /// <summary>
        /// Delete an Application by ID
        /// </summary>
        /// <param name="appId">App ID</param>
        /// <returns>IActionResult</returns>
        [HttpDelete("{appId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Application))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult DeleteApp([FromRoute] string appId)
        {
            Logger.LogInformation("delete", $"App: {appId}");

            if (string.IsNullOrEmpty(appId) || !Db.Apps.ContainsKey(appId))
            {
                Logger.LogWarning("delete-notfound", $"App: {appId}");

                return NotFound("App ID not found");
            }

            Db.DeleteApp(appId);
            Db.Save();

            return Ok();
        }

        /// <summary>
        /// Add an Application
        /// </summary>
        /// <param name="app">App in json format</param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public IActionResult AddApp([FromBody] Application app)
        {
            Logger.LogInformation("post", "App");

            Db.UpdateApp(app);
            Db.Save();

            return Created($"/api/apps/{app.AppId}", app);
        }

        /// <summary>
        /// Update an Application
        /// </summary>
        /// <param name="appId">App ID to Update</param>
        /// <param name="app">App in json format</param>
        /// <returns>IActionResult</returns>
        [HttpPut("{appId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult UpdateApp([FromRoute] string appId, [FromBody] Application app)
        {
            Logger.LogInformation("put", $"App");

            if (app.AppId != appId)
            {
                return BadRequest("AppId doesn't match json AppId");
            }

            Db.UpdateApp(app);
            Db.Save();

            return Ok(app);
        }
    }
}
