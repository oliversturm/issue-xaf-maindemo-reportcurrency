using DevExpress.ExpressApp.DC;

namespace Demos.Data {
    [DomainComponent]
    public class UseSQLAlternativeInfo {
        public string SQLIssue { get; set; }
        public string Alternative { get; set;}
        public string Restrictions { get; set; }
    }
}
