// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace ObjectModel.Model
{
    /// <summary>
    /// Application model
    /// </summary>
    public class Application
    {
        public string AppId { get; set; }
        public string Environment { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
        public double CpuLimit { get; set; }
        public int MemoryLimit { get; set; }
        public string Repo { get; set; }
        public int? Take { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
    }
}
