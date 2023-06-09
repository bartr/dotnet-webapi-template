// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace CSApp.Controllers;

/// <summary>
/// Handle benchmark requests
/// </summary>
[Route("csapp/api/v1/[controller]")]
public class BenchmarkController : Controller
{
    private static string benchmarkData;

    /// <summary>
    /// Returns a string value of benchmark data
    /// </summary>
    /// <param name="size">size of return</param>
    /// <response code="200"></response>
    /// <returns>IActionResult</returns>
    [HttpGet("{size}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDataAsync([FromRoute] long size)
    {
        // validate size
        if (size < 1)
        {
            Dictionary<string, object> errors = new()
            {
                { "statusCode", 400 },
                { "error", "size must be > 0" },
            };

            return BadRequest(errors);
        }

        if (size > 1024 * 1024)
        {
            Dictionary<string, object> errors = new()
            {
                { "statusCode", 400 },
                { "error", $"size must be <= 1 MB ({1024 * 1024})" },
            };

            return BadRequest(errors);
        }

        // return exact byte size
        return Ok(await GetBenchmarkDataAsync((int)size));
    }

    // get benchmark data
    private static async Task<string> GetBenchmarkDataAsync(int size)
    {
        // create the string
        if (string.IsNullOrEmpty(benchmarkData))
        {
            benchmarkData = "0123456789ABCDEF";

            // 1 MB
            while (benchmarkData.Length < 1024 * 1024)
            {
                benchmarkData += benchmarkData;
            }
        }

        // return a slice based on size
        return await Task<string>.Factory.StartNew(() =>
        {
            return benchmarkData[0..size];
        }).ConfigureAwait(false);
    }
}
