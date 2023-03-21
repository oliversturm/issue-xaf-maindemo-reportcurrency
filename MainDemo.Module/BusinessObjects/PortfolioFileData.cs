using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;

namespace MainDemo.Module.BusinessObjects;

[ImageName("BO_FileAttachment")]
public class PortfolioFileData : BaseObject {

    [ExpandObjectMembers(ExpandObjectMembers.Never), RuleRequiredField("PortfolioFileDataRule", "Save", "File should be assigned")]
    public virtual FileData File { get; set; }

    [VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
    public virtual int DocumentType_Int { get; set; }

    public virtual Resume Resume { get; set; }

    [NotMapped]
    public DocumentType DocumentType {
        get { return (DocumentType)DocumentType_Int; }
        set { DocumentType_Int = (int)value; }
    }

    public override void OnCreated() {
        DocumentType = DocumentType.Unknown;
    }
}

public enum DocumentType {
    SourceCode = 1,
    Tests = 2,
    Documentation = 3,
    Diagrams = 4,
    Screenshots = 5,
    Unknown = 6
}
