using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace MainDemo.Module.BusinessObjects;

[DefaultProperty(nameof(DisplayName))]
public abstract class Party : BaseObject {

    [ImageEditor]
    public virtual Byte[] Photo { get; set; }

    [Aggregated]
    public virtual IList<PhoneNumber> PhoneNumbers { get; set; } = new ObservableCollection<PhoneNumber>();

    [Aggregated, ExpandObjectMembers(ExpandObjectMembers.Never)]
    public virtual Address Address1 { get; set; }

    [Aggregated, ExpandObjectMembers(ExpandObjectMembers.Never)]
    public virtual Address Address2 { get; set; }

    [NotMapped]
    public abstract String DisplayName { get; }

    public override String ToString() {
        return DisplayName;
    }
}
