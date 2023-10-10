// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Kic.Middleware;

// logging codes
public enum LogCodes
{
    Success = 0,
    AppStarting,
    AppShuttingDown,
    AppShutdown,
    HealthcheckStart,
    HealthcheckComplete,
    WebListening,
    DatabaseConnected,
    DataServiceConnected,
    NoResults,
    HealthcheckRetry,
    Probe = 42,
    Ok = 200,
    Created = 201,
    NoContent = 204,
    BadRequest = 400,
    NotFound = 404,
    Exception = 500,
    AbnormalAppShutdown = 601,
    AppException,
    InvalidConnectionString,
    DatabaseConnectionFailed,
    DataServiceConnectionFailed,
    HealthcheckFailed,
}
