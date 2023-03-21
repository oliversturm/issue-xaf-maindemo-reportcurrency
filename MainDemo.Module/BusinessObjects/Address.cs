using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using DevExpress.Persistent.Base;
using System.ComponentModel.DataAnnotations;
using DevExpress.Persistent.BaseImpl.EF;

namespace MainDemo.Module.BusinessObjects;

[DefaultProperty(nameof(FullAddress))]
public class Address : BaseObject {
    private const string defaultFullAddressFormat = "{Country.Name}; {StateProvince}; {City}; {Street}; {ZipPostal}";

    public virtual String Street { get; set; }

    public virtual String City { get; set; }

    public virtual String StateProvince { get; set; }

    public virtual String ZipPostal { get; set; }

    public virtual Country Country { get; set; }

    [InverseProperty(nameof(Employee.Address1)), Browsable(false)]
    public virtual IList<Employee> Parties1 { get; set; } = new ObservableCollection<Employee>();

    [InverseProperty(nameof(Employee.Address2)), Browsable(false)]
    public virtual IList<Employee> Parties2 { get; set; } = new ObservableCollection<Employee>();

    public String FullAddress {
        get { return ObjectFormatter.Format(defaultFullAddressFormat, this, EmptyEntriesMode.RemoveDelimiterWhenEntryIsEmpty); }
    }
}
