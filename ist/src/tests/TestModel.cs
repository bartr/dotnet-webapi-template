using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using ObjectModel;
using ObjectModel.Model;
using Xunit;

namespace ObjectModel.Tests.Unit
{
    public class TestModel
    {
        [Fact]
        public void TestApp()
        {
            Application a1 = new()
            {
                AppId = "1",
                Name = "App1",
                Namespace = "app1",
                CpuLimit = 1,
                Data = new(),
                Environment = "dev",
                MemoryLimit = 1024,
                Repo = "retaildevcrews/ist",
                Take = null,
            };

            Assert.NotNull(a1);
        }

        [Fact]
        public void TestCluster()
        {
            Cluster c1 = new()
            {
                ClusterId = "1",
                Name = "Cluster1",
                Cores = 32,
                Data = new(),
                Environment = "dev",
                Memory = 128 * 1024,
                GitOpsRepo = "retaildevcrews/ist",
                GitOpsBranch = "main",
            };

            Assert.NotNull(c1);
        }

        [Fact]
        public void TestGroup()
        {
        }

        [Fact]
        public void TestNamespace()
        {
        }

        [Fact]
        public void TestSubGroup()
        {
        }
    }
}
