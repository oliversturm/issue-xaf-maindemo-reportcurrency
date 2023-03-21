using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Filtering;
using DevExpress.Persistent.Base;

namespace MainDemo.Module.BusinessObjects;

[DefaultProperty(nameof(FullName))]
[ImageName("BO_Person")]
public class Person : Party {

    public virtual String FirstName { get; set; }

    public virtual String LastName { get; set; }

    public virtual String MiddleName { get; set; }

    public virtual DateTime? Birthday { get; set; }

    [FieldSize(255)]
    public virtual String Email { get; set; }

    [SearchMemberOptions(SearchMemberMode.Exclude)]
    public String FullName {
        get { return ObjectFormatter.Format(FullNameFormat, this, EmptyEntriesMode.RemoveDelimiterWhenEntryIsEmpty); }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override String DisplayName {
        get { return FullName; }
    }

    public static String FullNameFormat = "{FirstName} {MiddleName} {LastName}";
}
