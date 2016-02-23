using System;
using System.IO;
using System.Linq;
using Bonobo.Git.Server.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.MembershipTests
{
    [TestClass]
    public class ADBackendStoreTest
    {
        private ADBackendStore<StorableClass> _store;

        [TestInitialize]
        public void Initialise()
        {
            Directory.Delete(Path.Combine(Path.GetTempPath(), "BonoboTestStore"), true);
            _store = MakeStore();
        }

        private static ADBackendStore<StorableClass> MakeStore()
        {
            return new ADBackendStore<StorableClass>(Path.GetTempPath(), "BonoboTestStore");
        }

        [TestMethod]
        public void NewStoreIsEmpty()
        {
            Assert.AreEqual(0, _store.Count());
        }

        [TestMethod]
        public void ItemCanBeAddedToStore()
        {
            _store.Add(new StorableClass());
            Assert.AreEqual(1, _store.Count());
        }

        [TestMethod]
        public void ItemCanBeAddedToStoreAndRetrieved()
        {
            _store.Add(new StorableClass { Name = "Hello" });
            var retrieved = _store.Single();
            Assert.AreEqual("Hello", retrieved.Name);
        }

        [TestMethod]
        public void ItemCanBeAddedToStoreAndRetrievedViaDisk()
        {
            _store.Add(new StorableClass { Name = "Hello" });

            var loadStore = MakeStore();
            var retrieved = loadStore.Single();
            Assert.AreEqual("Hello", retrieved.Name);
        }

        [TestMethod]
        public void ItemCanBeRetrievedByKey()
        {
            var id = Guid.NewGuid();
            _store.Add(new StorableClass { Id = id, Name = "Hello" });
            var retrieved = _store[id];
            Assert.AreEqual(id, retrieved.Id);
        }

        [TestMethod]
        public void ItemCanBeUpdated()
        {
            var id = Guid.NewGuid();
            var item = new StorableClass { Id = id, Name = "Hello" };
            _store.Add(item);

            var updatedItem = new StorableClass { Id = id, Name = "Hello" };
            updatedItem.Name = "Goodbye";
            _store.Update(updatedItem);
            var retrieved = _store[id];
            Assert.AreEqual(id, retrieved.Id);
            Assert.AreEqual("Goodbye", retrieved.Name);
        }

        [TestMethod]
        public void ItemCanBeDeletedById()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            _store.Add(new StorableClass { Id = id1, Name = "Hello1" });
            _store.Add(new StorableClass { Id = id2, Name = "Hello2" });
            _store.Add(new StorableClass { Id = id3, Name = "Hello3" });

            Assert.IsTrue(_store.Remove(id2));

            Assert.AreEqual(2, _store.Count());
            Assert.AreEqual("Hello1", _store[id1].Name);
            Assert.AreEqual("Hello3", _store[id3].Name);
        }

        [TestMethod]
        public void ItemCanBeDeletedByItem()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var item1 = new StorableClass { Id = id1, Name = "Hello1" };
            _store.Add(item1);
            _store.Add(new StorableClass { Id = id2, Name = "Hello2" });

            Assert.IsTrue(_store.Remove(item1));

            Assert.AreEqual(1, _store.Count());
            Assert.AreEqual("Hello2", _store[id2].Name);
        }

        [TestMethod]
        public void AddOrUpdateWillAddItemIfItDoesntExist()
        {
            var item1 = new StorableClass { Id = Guid.NewGuid(), Name = "Hello1" };
            _store.AddOrUpdate(item1);

            Assert.AreEqual(1, _store.Count());
            Assert.AreEqual("Hello1", _store.First().Name);
        }

        [TestMethod]
        public void AddOrUpdateWillUpdateItemIfItDoesExist()
        {
            var item1 = new StorableClass { Id = Guid.NewGuid(), Name = "Hello" };
            _store.Add(item1);

            var changedItem = new StorableClass { Id = item1.Id, Name = "Goodbye" };

            _store.AddOrUpdate(changedItem);

            Assert.AreEqual(1, _store.Count());
            Assert.AreEqual("Goodbye", _store.First().Name);
        }

        class StorableClass : INameProperty
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string DisplayName { get { return Name; } }
        }
    }
}