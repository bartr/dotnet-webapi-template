// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace ObjectModel.Model
{
    /// <summary>
    /// SubGroup model
    /// </summary>
    public class SubGroup
    {
        public string GroupId { get; set; }
        public int? Take { get; set; }

        public List<string> GetAllClusters(Database db)
        {
            List<string> clusters = new();

            if (db.Groups.ContainsKey(GroupId))
            {
                clusters = db.Groups[GroupId].GetAllClusters(db);

                if (Take != null && Take > 0)
                {
                    clusters.RemoveRange((int)Take, clusters.Count - (int)Take);
                }
            }

            return clusters;
        }
    }
}
