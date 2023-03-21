using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;

namespace MainDemo.Module.BusinessObjects.NonPersistent {
    [DomainComponent]
    public class CustomNonPersistentObject : NonPersistentBaseObject {
        public string Name { get; set; }
    }
}
