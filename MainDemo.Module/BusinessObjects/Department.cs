using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;

namespace MainDemo.Module.BusinessObjects;

[DefaultClassOptions]
[DefaultProperty(nameof(Department.Title))]
[RuleCriteria("Department_PositionsIsNotEmpty", DefaultContexts.Save, "Positions.Count > 0", CustomMessageTemplate = "The Department must contain at least one position.")]
[RuleCriteria("Department_EmployeesIsNotEmpty", DefaultContexts.Save, "Employees.Count > 0", CustomMessageTemplate = "The Department must contain at least one employee.")]
public class Department : BaseObject {

    [RuleRequiredField]
    public virtual string Title { get; set; }

    public virtual string Office { get; set; }

    public virtual IList<Position> Positions { get; set; } = new ObservableCollection<Position>();

    public virtual IList<Employee> Employees { get; set; } = new ObservableCollection<Employee>();

    public virtual string Location { get; set; }

    [StringLength(4096)]
    public virtual string Description { get; set; }

    [DataSourceProperty("Employees", DataSourcePropertyIsNullMode.SelectAll)]
    [RuleRequiredField]
    public virtual Employee DepartmentHead { get; set; }
}
