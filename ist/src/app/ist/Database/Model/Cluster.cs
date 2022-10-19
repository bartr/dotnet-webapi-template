// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace ObjectModel.Model
{
    /// <summary>
    /// Cluster model
    /// </summary>
    public class Cluster
    {
        public string ClusterId { get; set; }
        public string Name { get; set; }
        public string Environment { get; set; }
        public double Cores { get; set; }
        public int Memory { get; set; }
        public string GitOpsRepo { get; set; }
        public string GitOpsBranch { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
    }
}
