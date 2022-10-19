// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace ObjectModel.Model
{
    /// <summary>
    /// Namespace deploy model
    /// </summary>
    public class NamespaceDeploy : Namespace
    {
        public double CpuAllocated { get; set; }
        public int MemoryAllocated { get; set; }
        public bool IsDelete { get; set; }
        public List<string> Clusters { get; set; } = new();
        public Dictionary<string, ApplicationDeploy> Apps { get; set; } = new();
    }
}
