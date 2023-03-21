using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace MainDemo.Module.BusinessObjects;

[DefaultClassOptions]
[ImageName("BO_Resume")]
public class Resume : BaseObject {

    [Aggregated]
    public virtual IList<PortfolioFileData> Portfolio { get; set; } = new ObservableCollection<PortfolioFileData>();

    public virtual Employee Employee { get; set; }

    [Aggregated, ExpandObjectMembers(ExpandObjectMembers.Never)]
    public virtual FileData File { get; set; }
}

