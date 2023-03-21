using DevExpress.ExpressApp;
using MainDemo.Module.BusinessObjects.NonPersistent;

namespace MainDemo.Blazor.Server.API.NonPersistent {
    //Global storage in this example
    public class NonPersistentObjectStorageService {
        public Dictionary<Guid, NonPersistentBaseObject> ObjectsCache { get; } = new();

        public NonPersistentObjectStorageService() {
            CreateObject<CustomNonPersistentObject>("A");
            CreateObject<CustomNonPersistentObject>("B");
            CreateObject<CustomNonPersistentObject>("C");
        }
        private NonPersistentBaseObject CreateObject<T>(string value) where T : NonPersistentBaseObject, new() {
            T result = new T();
            if(result is CustomNonPersistentObject custom) {
                custom.Name = value;
            }
            ObjectsCache.Add(result.Oid, result);
            return result;
        }
    }
}
