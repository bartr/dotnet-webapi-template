// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace ObjectModel.Model
{
    /// <summary>
    /// Group model
    /// </summary>
    public class Group
    {
        public string GroupId { get; set; }
        public string Name { get; set; }
        public string Environment { get; set; }
        public List<string> Clusters { get; set; } = new();
        public List<SubGroup> SubGroups { get; set; } = new();
        public Dictionary<string, object> Data { get; set; } = new();

        public List<string> GetAllClusters(Database db)
        {
            List<string> groups = new();

            Clusters ??= new();

            SubGroups ??= new();

            groups.AddRange(Clusters);

            foreach (SubGroup sg in SubGroups)
            {
                List<string> clusters = sg.GetAllClusters(db);

                foreach (string cl in clusters)
                {
                    if (!groups.Contains(cl))
                    {
                        groups.Add(cl);
                    }
                }
            }

            return groups;
        }
    }
}
