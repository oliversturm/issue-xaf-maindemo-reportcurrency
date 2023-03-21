using System;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;

namespace MainDemo.Module.BusinessObjects;

[DefaultProperty(nameof(Text))]
[ImageName("BO_Note")]
public class Note {

    [VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
    public virtual Guid ID { get; set; }

    public virtual String Author { get; set; }

    public virtual DateTime? DateTime { get; set; }

    [FieldSize(FieldSizeAttribute.Unlimited)]
    public virtual String Text { get; set; }
}
