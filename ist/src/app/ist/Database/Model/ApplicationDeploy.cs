// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace ObjectModel.Model
{
    /// <summary>
    /// Application Deploy model
    /// </summary>
    public class ApplicationDeploy : Application
    {
        public bool? IsDelete { get; set; }
        public List<string> Clusters { get; set; } = new();
    }
}
