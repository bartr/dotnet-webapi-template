// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ObjectModel.Model;

namespace ObjectModel
{
    /// <summary>
    /// Database class (singleton)
    /// </summary>
    public sealed class Database
    {
        public static readonly string DataPath = Path.Combine("ist", "data");
        private readonly Inventory inventory;
        private readonly DeployModel deployModel;

        public Database()
        {
            // can throw an exception

            inventory = JsonSerializer.Deserialize<Inventory>(File.ReadAllText(Path.Join(DataPath, "object-model.json")));
            inventory.Apps ??= new();
            inventory.Clusters ??= new();
            inventory.Groups ??= new();
            inventory.Namespaces ??= new();

            deployModel = JsonSerializer.Deserialize<DeployModel>(File.ReadAllText(Path.Join(DataPath, "deploy-model.json")));
        }

        public static JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

            //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
        };

        public DeployModel DeployModel
        {
            get
            {
                return deployModel;
            }
        }

        public Dictionary<string, Application> Apps
        {
            get
            {
                if (inventory == null || inventory.Apps == null)
                {
                    return new();
                }

                return inventory.Apps;
            }
        }

        public Dictionary<string, Cluster> Clusters
        {
            get
            {
                if (inventory == null || inventory.Clusters == null)
                {
                    return new();
                }

                return inventory.Clusters;
            }
        }

        public Dictionary<string, Namespace> Namespaces
        {
            get
            {
                if (inventory == null || inventory.Namespaces == null)
                {
                    return new();
                }

                return inventory.Namespaces;
            }
        }

        public Dictionary<string, Group> Groups
        {
            get
            {
                if (inventory == null || inventory.Groups == null)
                {
                    return new();
                }

                return inventory.Groups;
            }
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(DataPath, JsonSerializer.Serialize(inventory, JsonOptions));
            }
            catch (Exception ex)
            {
                // todo - structured error
                Console.WriteLine($"Database.Save.Exception: {ex.Message}");
            }
        }

        public void UpdateApp(Application app)
        {
            inventory.Apps[app.AppId] = app;
        }

        public void UpdateCluster(Cluster c)
        {
            inventory.Clusters[c.ClusterId] = c;
        }

        public void UpdateNamespace(Namespace ns)
        {
            inventory.Namespaces[ns.NamespaceId] = ns;
        }

        public void UpdateGroup(Group g)
        {
            inventory.Groups[g.GroupId] = g;
        }

        public void DeleteApp(string appId)
        {
            if (inventory.Apps.ContainsKey(appId))
            {
                inventory.Apps.Remove(appId);
            }
        }

        public void DeleteCluster(string clusterId)
        {
            if (inventory.Clusters.ContainsKey(clusterId))
            {
                inventory.Clusters.Remove(clusterId);
            }

            foreach (Group g in inventory.Groups.Values)
            {
                if (g.Clusters != null && g.Clusters.Contains(clusterId))
                {
                    g.Clusters.Remove(clusterId);
                }
            }
        }

        public void DeleteNamespace(string namespaceId)
        {
            if (inventory.Namespaces.ContainsKey(namespaceId))
            {
                inventory.Namespaces.Remove(namespaceId);
            }

            foreach (string k in Apps.Keys)
            {
                if (Apps[k].Namespace == namespaceId)
                {
                    Apps.Remove(k);
                }
            }
        }

        public void DeleteGroup(string groupId)
        {
            if (inventory.Groups.ContainsKey(groupId))
            {
                inventory.Groups.Remove(groupId);
            }

            foreach (string k in Groups.Keys)
            {
                List<SubGroup> groups = Groups[k].SubGroups;

                if (groups != null && groups.Count > 0)
                {
                    for (int i = groups.Count - 1; i >= 0; i--)
                    {
                        if (groups[i].GroupId == groupId)
                        {
                            Groups[k].SubGroups.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}
