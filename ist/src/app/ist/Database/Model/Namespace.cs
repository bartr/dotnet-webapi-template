// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace ObjectModel.Model
{
    /// <summary>
    /// Namespace model
    /// </summary>
    public class Namespace
    {
        public string NamespaceId { get; set; }
        public string Name { get; set; }
        public string Environment { get; set; }
        public double CpuLimit { get; set; }
        public int MemoryLimit { get; set; }
        public int? Take { get; set; }
        public List<SubGroup> SubGroups { get; set; } = new();
        public Dictionary<string, object> Data { get; set; } = new();
    }
}
