﻿using Embark;
using Embark.Interaction;
using EmbarkTests._Mocks;
using Xunit;
using System;
using System.IO;
using System.Reflection;

namespace EmbarkTests
{
    
    public class MockDB
    {
        internal static Client SharedRuntimeClient = SetupEnvironmentAndGetTestClient();
        //internal static Client SharedDiskClient;

        internal static Collection RuntimeBasicCollection;
        internal static DataEntryCollection<Sound> RuntimeDataEntryCollection;

        internal static DataEntryCollection<Sound> GetRuntimeCollection(string collectionName)
        {
            return SharedRuntimeClient.GetDataEntryCollection<Sound>(collectionName);
        }

        internal static TestDiskDB GetDiskDB() => new TestDiskDB();

        static Client SetupEnvironmentAndGetTestClient()
        {
            SharedRuntimeClient = Client.GetRuntimeDB();

            RuntimeBasicCollection = SharedRuntimeClient.Basic;
            RuntimeDataEntryCollection = SharedRuntimeClient.GetDataEntryCollection<Sound>("ConventionTests");

            Assert.NotNull(RuntimeBasicCollection);

            return SharedRuntimeClient;
        }

       
    }
}
