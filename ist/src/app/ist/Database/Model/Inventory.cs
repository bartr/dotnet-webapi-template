// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace ObjectModel.Model
{
    /// <summary>
    /// Container for json serialization
    /// </summary>
    public class Inventory
    {
        public Dictionary<string, Application> Apps { get; set; } = new();
        public Dictionary<string, Cluster> Clusters { get; set; } = new();
        public Dictionary<string, Namespace> Namespaces { get; set; } = new();
        public Dictionary<string, Group> Groups { get; set; } = new();
    }
}
