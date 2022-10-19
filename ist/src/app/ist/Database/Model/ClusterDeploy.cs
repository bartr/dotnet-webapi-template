// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace ObjectModel.Model
{
    /// <summary>
    /// Cluster Deploy model
    /// </summary>
    public class ClusterDeploy : Cluster
    {
        public double CoresAllocated { get; set; }
        public int MemoryAllocated { get; set; }
        public bool IsDelete { get; set; }
        public Dictionary<string, NamespaceDeploy> Namespaces { get; set; } = new();
    }
}
