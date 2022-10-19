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
    /// Handle api/clusters requests
    /// </summary>
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class ClustersController : Controller
    {
        private static readonly CseLog Logger = new ()
        {
            Name = typeof(ClustersController).FullName,
            ErrorMessage = "ClustersControllerException",
        };

        private static readonly Database Db = new();

        /// <summary>
        /// Returns a dictionary of Clusters
        /// </summary>
        /// <response code="200"></response>
        /// <returns>IActionResult</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, Cluster>))]
        public IActionResult GetClusters()
        {
            Logger.LogInformation("get", "Clusters");

            return Ok(Db.Clusters);
        }

        /// <summary>
        /// Returns a Cluster by ID
        /// </summary>
        /// <param name="clusterId">Cluster ID</param>
        /// <returns>IActionResult</returns>
        [HttpGet("{clusterId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Cluster))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult GetCluster([FromRoute] string clusterId)
        {
            Logger.LogInformation("get", $"Cluster: {clusterId}");

            if (string.IsNullOrEmpty(clusterId) || !Db.Clusters.ContainsKey(clusterId))
            {
                return NotFound("Cluster ID not found");
            }

            return Ok(Db.Clusters[clusterId]);
        }

        /// <summary>
        /// Delete a Cluster by ID
        /// </summary>
        /// <param name="clusterId">Cluster ID</param>
        /// <returns>IActionResult</returns>
        [HttpDelete("{clusterId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Cluster))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult DeleteCluster([FromRoute] string clusterId)
        {
            if (string.IsNullOrEmpty(clusterId) || !Db.Clusters.ContainsKey(clusterId))
            {
                return NotFound("Cluster ID not found");
            }

            Db.DeleteCluster(clusterId);
            Db.Save();

            return Ok();
        }

        /// <summary>
        /// Add a Cluster
        /// </summary>
        /// <param name="cluster">Cluster in json format</param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public IActionResult AddCluster([FromBody] Cluster cluster)
        {
            Db.UpdateCluster(cluster);
            Db.Save();

            return Created($"/api/apps/{cluster.ClusterId}", cluster);
        }

        /// <summary>
        /// Update a Cluster
        /// </summary>
        /// <param name="clusterId">Cluster ID to Update</param>
        /// <param name="cluster">Cluster in json format</param>
        /// <returns>IActionResult</returns>
        [HttpPut("{clusterId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult UpdateCluster([FromRoute] string clusterId, [FromBody] Cluster cluster)
        {
            if (cluster.ClusterId != clusterId)
            {
                return BadRequest("ClusterId doesn't match json ClusterId");
            }

            Db.UpdateCluster(cluster);
            Db.Save();

            return Ok(cluster);
        }
    }
}
