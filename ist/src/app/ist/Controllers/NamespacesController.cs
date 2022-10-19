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
    /// Handle api/namespaces requests
    /// </summary>
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class NamespacesController : Controller
    {
        private static readonly CseLog Logger = new ()
        {
            Name = typeof(ClustersController).FullName,
            ErrorMessage = "NamespacesControllerException",
        };

        /// <summary>
        /// Returns a dictionary of Namespace documents
        /// </summary>
        /// <response code="200"></response>
        /// <returns>IActionResult</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, Namespace>))]
        public IActionResult GetNamespaces()
        {
            Logger.LogInformation("get", "Namespaces");

            Database db = new();

            return Ok(db.Namespaces);
        }

        /// <summary>
        /// Returns a Namespace by ID
        /// </summary>
        /// <param name="namespaceId">Namespace ID</param>
        /// <returns>IActionResult</returns>
        [HttpGet("{namespaceId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Namespace))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult GetNamespace([FromRoute] string namespaceId)
        {
            Database db = new();

            if (string.IsNullOrEmpty(namespaceId) || !db.Namespaces.ContainsKey(namespaceId))
            {
                return NotFound("Namespace ID not found");
            }

            return Ok(db.Namespaces[namespaceId]);
        }

        /// <summary>
        /// Delete a Namespace by ID
        /// </summary>
        /// <param name="namespaceId">Namespace ID</param>
        /// <returns>IActionResult</returns>
        [HttpDelete("{namespaceId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Namespace))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult DeleteNamespace([FromRoute] string namespaceId)
        {
            Database db = new();

            if (string.IsNullOrEmpty(namespaceId) || !db.Namespaces.ContainsKey(namespaceId))
            {
                return NotFound("Namespace ID not found");
            }

            db.DeleteNamespace(namespaceId);
            db.Save();

            return Ok();
        }

        /// <summary>
        /// Add a Namespace
        /// </summary>
        /// <param name="ns">Namespace in json format</param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public IActionResult AddNamespace([FromBody] Namespace ns)
        {
            Database db = new();

            db.UpdateNamespace(ns);
            db.Save();

            return Created($"/api/apps/{ns.NamespaceId}", ns);
        }

        /// <summary>
        /// Update a Namespace
        /// </summary>
        /// <param name="namespaceId">Namespace ID to Update</param>
        /// <param name="ns">Namespace in json format</param>
        /// <returns>IActionResult</returns>
        [HttpPut("{namespaceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult UpdateNamespace([FromRoute] string namespaceId, [FromBody] Namespace ns)
        {
            Database db = new();

            if (ns.NamespaceId != namespaceId)
            {
                return BadRequest("NamespaceId must match json namespaceId");
            }

            db.UpdateNamespace(ns);
            db.Save();

            return Ok(ns);
        }
    }
}
