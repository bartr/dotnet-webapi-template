using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ObjectModel;
using ObjectModel.Model;
using Xunit;

namespace ObjectModel.Tests.Unit
{
    public class TestDatabase
    {
        [Fact]
        public void LoadDatabase()
        {
            Database db1 = new();

            Assert.NotNull(db1);
            Assert.NotNull(db1.Apps);
            Assert.NotNull(db1.Clusters);
            Assert.NotNull(db1.Groups);
            Assert.NotNull(db1.Namespaces);

            Assert.True(db1.Apps.Count == 7);
            Assert.True(db1.Clusters.Count == 18);
            Assert.True(db1.Groups.Count == 14);
            Assert.True(db1.Namespaces.Count == 7);

            Assert.True(db1.Groups["az-central"].GetAllClusters(db1).Count == 3);
            Assert.True(db1.Groups["az-east"].GetAllClusters(db1).Count == 3);
            Assert.True(db1.Groups["az-west"].GetAllClusters(db1).Count == 3);
            Assert.True(db1.Groups["gcp-central"].GetAllClusters(db1).Count == 3);
            Assert.True(db1.Groups["gcp-east"].GetAllClusters(db1).Count == 3);
            Assert.True(db1.Groups["gcp-west"].GetAllClusters(db1).Count == 3);
            Assert.True(db1.Groups["central"].GetAllClusters(db1).Count == 6);
            Assert.True(db1.Groups["east"].GetAllClusters(db1).Count == 6);
            Assert.True(db1.Groups["west"].GetAllClusters(db1).Count == 6);
            Assert.True(db1.Groups["all"].GetAllClusters(db1).Count == 18);
        }

        [Fact]
        public void Delete()
        {
            Database db = new();

            db.DeleteCluster("gcp-central-103");
            db.DeleteCluster("gcp-east-103");
            db.DeleteCluster("gcp-west-103");

            Assert.True(db.Clusters.Count == 15);
            Assert.True(db.Groups["gcp-central"].GetAllClusters(db).Count == 2);
            Assert.True(db.Groups["gcp-east"].GetAllClusters(db).Count == 2);
            Assert.True(db.Groups["gcp-west"].GetAllClusters(db).Count == 2);
            Assert.True(db.Groups["central"].GetAllClusters(db).Count == 5);
            Assert.True(db.Groups["east"].GetAllClusters(db).Count == 5);
            Assert.True(db.Groups["west"].GetAllClusters(db).Count == 5);
            Assert.True(db.Groups["all"].GetAllClusters(db).Count == 15);

            db.DeleteGroup("gcp-central");
            Assert.True(db.Clusters.Count == 15);
            Assert.True(db.Groups["central"].GetAllClusters(db).Count == 3);

            db.DeleteNamespace("imdb");
            Assert.True(db.Namespaces.Count == 6);
            Assert.True(db.Apps.Count == 6);
        }

        [Fact]
        public void Add()
        {
            Database db = new();

            Cluster cl = new()
            {
                ClusterId = "az-central-104",
                Name = "az-central-104",
                Environment = "dev",
                Cores = 32,
                Memory = 128 * 1024,
                GitOpsBranch = "main",
                GitOpsRepo = "retaildevcrews/ist",
                Data = new(),
            };

            cl.Data.Add("region", "central");
            cl.Data.Add("cloud", "az");

            db.UpdateCluster(cl);
            Assert.True(db.Clusters.Count == 19);

            db.Groups["az-central"].Clusters.Add(cl.ClusterId);
            Assert.True(db.Groups["az-central"].GetAllClusters(db).Count == 4);
            Assert.True(db.Groups["central"].GetAllClusters(db).Count == 7);
            Assert.True(db.Groups["all"].GetAllClusters(db).Count == 19);
        }
    }
}
