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
    /// Handle benchmark requests
    /// </summary>
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class GroupsController : Controller
    {
        private static readonly CseLog Logger = new ()
        {
            Name = typeof(ClustersController).FullName,
            ErrorMessage = "GroupControllerException",
        };

        private static readonly Database Db = new();

        /// <summary>
        /// Returns all Cluster documents
        /// </summary>
        /// <response code="200">text/plain of size</response>
        /// <returns>IActionResult</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, Group>))]
        public IActionResult GetGroups()
        {
            Logger.LogInformation("get", "Groups");

            return Ok(Db.Groups);
        }

        /// <summary>
        /// Returns a Group by ID
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns>IActionResult</returns>
        [HttpGet("{groupId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Group))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult GetGroupById([FromRoute] string groupId)
        {
            if (string.IsNullOrEmpty(groupId) || !Db.Groups.ContainsKey(groupId))
            {
                return NotFound("Group ID not found");
            }

            return Ok(Db.Groups[groupId]);
        }

        /// <summary>
        /// Add a Group
        /// </summary>
        /// <param name="group">Group in json format</param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public IActionResult AddGroup([FromBody] Group group)
        {
            Db.UpdateGroup(group);
            Db.Save();

            return Created($"/api/groups/{group.GroupId}", group);
        }

        /// <summary>
        /// Update a Group
        /// </summary>
        /// <param name="groupId">Group ID to Update</param>
        /// <param name="group">Group in json format</param>
        /// <returns>IActionResult</returns>
        [HttpPut("groupId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult UpdateGroup([FromRoute] string groupId, [FromBody] Group group)
        {
            if (group.GroupId != groupId)
            {
                return BadRequest("GroupId must match json groupId");
            }

            Db.UpdateGroup(group);
            Db.Save();

            return Ok(group);
        }

        /// <summary>
        /// Delete a Group by ID
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns>IActionResult</returns>
        [HttpDelete("{groupId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Application))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult DeleteGroup([FromRoute] string groupId)
        {
            if (string.IsNullOrEmpty(groupId) || !Db.Groups.ContainsKey(groupId))
            {
                return NotFound("Group ID not found");
            }

            Db.DeleteGroup(groupId);
            Db.Save();

            return Ok();
        }
    }
}
