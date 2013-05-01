using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSAPI;
using System.IO;
using System.Collections.Generic;

namespace CSAPITests
{
    [TestClass]
    public class CSAPITests
    {
        private StreamWriter consoleOutStream;

        [TestInitialize]
        public void Setup()
        {
            consoleOutStream = new StreamWriter("../../CSAPITests.log");
            Console.SetOut(consoleOutStream);
        }

        [TestCleanup]
        public void Teardown()
        {
            consoleOutStream.Close();
        }

        [TestMethod]
        public void TestCheckKeys()
        {
            // good credentials
            var goodCredentials = ReadCredentialsFromFile("..\\..\\Credentials.txt");
            Assert.IsNotNull(goodCredentials);

            var api = new CSAPILowLevel(goodCredentials.ApiKey, goodCredentials.ApiId);

            try
            {
                Assert.IsTrue(api.CheckKeys());
            }
            catch (ApiException e)
            {
                Assert.Fail("We shouldn't get ApiException: " + e.Message);
            }

            try
            {
                var task = api.CheckKeysAsync();
                task.Wait();
                Assert.IsTrue(task.Result);
            }
            catch (ApiException e)
            {
                Assert.Fail("We shouldn't get ApiException: " + e.Message);
            }

            // bad credentials
            var badCredentials = ReadCredentialsFromFile("..\\..\\BadCredentials.txt");
            Assert.IsNotNull(badCredentials);

            api = new CSAPILowLevel(badCredentials.ApiKey, badCredentials.ApiId);
            try
            {
                api.CheckKeys();
                Assert.Fail("CheckKeys should throw ApiException");
            }
            catch (ApiException e)
            {
                // we should get here
                Console.WriteLine(e.Message);
            }

            try
            {
                var task = api.CheckKeysAsync();
                task.Wait();
                Assert.Fail("CheckKeysAsync should throw ApiException");
            }
            catch (AggregateException e)
            {
                Assert.IsTrue(e.InnerExceptions.Any(ex => ex is ApiException));
                Console.WriteLine(e.InnerException.Message);
            }
        }

        [TestMethod]
        public void TestHighLevel()
        {
            var goodCredentials = ReadCredentialsFromFile("..\\..\\Credentials.txt");
            Assert.IsNotNull(goodCredentials);
            var api = new CSAPIHighLevel(goodCredentials.ApiKey, goodCredentials.ApiId);

            var envStatusList = api.GetEnvironmentStatusList();
            foreach (var env in envStatusList)
            {
                Console.WriteLine(env.envId);
            }
        }

        [TestMethod]
        public void TestHighLevelAsync()
        {
            var goodCredentials = ReadCredentialsFromFile("..\\..\\Credentials.txt");
            Assert.IsNotNull(goodCredentials);
            var api = new CSAPIHighLevel(goodCredentials.ApiKey, goodCredentials.ApiId);

            var envStatusList = api.GetEnvironmentStatusList();
            
            foreach (var env in envStatusList)
            {
                try
                {
                    var t = api.ResumeEnvironmentAsync(env);
                    t.Wait();
                    Console.WriteLine("Resumed env {0}", env.envId);
                }
                catch (AggregateException e)
                {
                    Assert.IsTrue(e.InnerException is ApiException);
                    var apiException = e.InnerException as ApiException;
                    Assert.IsTrue(apiException.ApiResponse != null, String.Format("We got Bad API Exception: {0}", apiException.Message));
                    
                    Console.WriteLine("Not expected to fail resuming env {0}.\nException message: {1}", env.envId, apiException.Message);
                }

                try
                {
                    env.envId = "NOTEXIST";
                    var t = api.ResumeEnvironmentAsync(env);
                    t.Wait();
                    Console.WriteLine("Resumed env {0}", env.envId);
                }
                catch (AggregateException e)
                {
                    Assert.IsTrue(e.InnerException is ApiException);
                    var apiException = e.InnerException as ApiException;
                    Assert.IsTrue(apiException.ApiResponse != null, String.Format("We got Bad API Exception: {0}", apiException.Message));

                    Console.WriteLine("Expected to fail resuming non existing env {0}.\nException message: {1}", env.envId, apiException.Message);
                }
            }

            envStatusList.Add(new EnvStatus());
        }

        [TestMethod]
        public void TestTemplates()
        {
            var goodCredentials = ReadCredentialsFromFile("..\\..\\Credentials.txt");
            Assert.IsNotNull(goodCredentials);

            var api = new CSAPILowLevel(goodCredentials.ApiKey, goodCredentials.ApiId);
            Assert.IsTrue(api.CheckKeys());

            var highlevel = new CSAPIHighLevel(goodCredentials.ApiKey, goodCredentials.ApiId);
            var templatesList = highlevel.ListTemplates();

            var categoriesLists = (from t in templatesList select t.categories).ToList();
            var categories = new HashSet<string>();
            foreach (var cList in categoriesLists)
            {
                if (cList != null)
                    foreach (var item in cList)
                        if (item != null)
                            categories.Add((String)item);
            }
            Console.WriteLine(String.Join("\n", categories));

            Console.WriteLine("number of templates = " + templatesList.Count.ToString());
            var envList = highlevel.ListEnvironments();
            foreach (var env in envList)
                Console.WriteLine(env.name + "\t" + env.envId);
            foreach (var tmp in templatesList)
                if (tmp.memory_size_mb < 1024)
                    Console.WriteLine(tmp.name + "\t" + tmp.id);
        }

        [TestMethod]
        public void TestCheckCloudFolders()
        {
            var goodCredentials = ReadCredentialsFromFile("..\\..\\Credentials.txt");
            Assert.IsNotNull(goodCredentials);
            var api = new CSAPIHighLevel(goodCredentials.ApiKey, goodCredentials.ApiId);

            var cloudFoldersInfo = api.GetCloudFoldersInfo();
            Console.WriteLine("CloudFolder status -");
            Console.WriteLine("host: " + cloudFoldersInfo.host);
            Console.WriteLine("user: " + cloudFoldersInfo.user);
            Console.WriteLine("password: " + cloudFoldersInfo.password);
        }

        private Credentials ReadCredentialsFromFile(String fileName)
        {
            String Id = "";
            String Key = "";

            try
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    String line = sr.ReadLine();
                    Id = line.Split('=')[1];

                    line = sr.ReadLine();
                    Key = line.Split('=')[1];
                }
            }
            catch (Exception)
            {
                return null;
            }

            return new Credentials { ApiId = Id, ApiKey = Key };
        }
    }
}
