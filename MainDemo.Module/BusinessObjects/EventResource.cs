using DevExpress.Persistent.BaseImpl.EF;

namespace MainDemo.Module.BusinessObjects;

public class EventResource : BaseObject {
    public virtual Event Event { get; set; }
    public virtual Resource Resource { get; set; }
}
