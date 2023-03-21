﻿using System.Data;
using System.Drawing;
using System.Security.Cryptography;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Updating;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EFCore.AuditTrail;
using DevExpress.Xpo;
using MainDemo.Module.BusinessObjects;

namespace MainDemo.Module.DatabaseUpdate;
public class Updater : ModuleUpdater {
    public Updater(IObjectSpace objectSpace, Version currentDBVersion) : base(objectSpace, currentDBVersion) { }


    private Employee FindTellitson() => FindEmployee("Mary", "Tellitson");
    private Employee FindJanete() => FindEmployee("Janete", "Limeira");
    private Employee FindEmployee(string firstName, string lastName) => ObjectSpace.FirstOrDefault<Employee>(e => e.FirstName == firstName && e.LastName == lastName);
    private Position FindPosition(string positionTitle) => ObjectSpace.FirstOrDefault<Position>(p => p.Title == positionTitle, true);
    private Department FindDepartment(string departmentTitle) => ObjectSpace.FirstOrDefault<Department>(d => d.Title == departmentTitle, true);

    public override void UpdateDatabaseAfterUpdateSchema() {
        base.UpdateDatabaseAfterUpdateSchema();

        UpdateStatus("CreateEmployees", "", "Creating employees, departments and positions in the database...");

        ObjectSpace.CommitChanges();
        try {
            CreateDepartments();
            CreateEmployees();
        }
        catch(Exception e) {
            Tracing.Tracer.LogText("Cannot initialize employees, departments and positions from the XML file.");
            Tracing.Tracer.LogError(e);
        }
        InitEmployeeLocation();


        UpdateStatus("CreatePayments", "", "Creating payments, resumes and scheduler events in the database...");
        IList<Employee> topTenEmployees = ObjectSpace.GetObjects<Employee>();
        ObjectSpace.SetCollectionSorting(topTenEmployees, new SortProperty[] { new SortProperty("LastName", DevExpress.Xpo.DB.SortingDirection.Ascending) });
        ObjectSpace.SetTopReturnedObjectsCount(topTenEmployees, 10);
        string[] notes = {
                "works with customers until their problems are resolved and often goes an extra step to help upset customers be completely surprised by how far we will go to satisfy customers",
                "is very good at making team members feel included. The inclusion has improved the team's productivity dramatically",
                "is very good at sharing knowledge and information during a problem to increase the chance it will be resolved quickly",
                "actively elicits feedback from customers and works to resolve their problems",
                "creates an inclusive work environment where everyone feels they are a part of the team",
                "consistently keeps up on new trends in the industry and applies these new practices to every day work",
                "is clearly not a short term thinker - the ability to set short and long term business goals is a great asset to the company",
                "seems to want to achieve all of the goals in the last few weeks before annual performance review time, but does not consistently work towards the goals throughout the year",
                "does not yet delegate effectively and has a tendency to be overloaded with tasks which should be handed off to subordinates",
                "to be discussed with the top management..."
            };
        for(int i = 0; i < topTenEmployees.Count; i++) {
            Employee employee = topTenEmployees[i];
            if(ObjectSpace.FirstOrDefault<Paycheck>(p => p.Employee == employee) == null) {
                PayrollSampleDataGenerator.GenerateEmployeePaychecks(ObjectSpace, employee);
                ObjectSpace.CommitChanges();
            }
            Resume resume = ObjectSpace.FirstOrDefault<Resume>(r => r.Employee == employee);
            if(resume == null) {
                resume = ObjectSpace.CreateObject<Resume>();
                var file = ObjectSpace.CreateObject<DevExpress.Persistent.BaseImpl.EF.FileData>();
                try {
                    Stream stream = FindEmployeeResume(employee);
                    if(stream != null) {
                        file.LoadFromStream(string.Format("{0}.pdf", employee.FullName), stream);
                    }
                }
                catch(Exception e) {
                    Tracing.Tracer.LogText("Cannot initialize FileData for the employee {0}.", employee.FullName);
                    Tracing.Tracer.LogError(e);
                }
                resume.File = file;
                resume.Employee = employee;
            }
            Employee reviewerEmployee = i < 5 ? FindTellitson() : FindJanete();
            Note note = ObjectSpace.FirstOrDefault<Note>(n => n.Text.Contains(employee.FullName));
            if(note == null) {
                note = ObjectSpace.CreateObject<Note>();
                note.Author = reviewerEmployee.FullName;
                note.Text = string.Format("<span style='color:#000000;font-family:Tahoma;font-size:8pt;'><b>{0}</b> \r\n{1}</span>", employee.FullName, notes[i]);
                note.DateTime = DateTime.Now.AddDays(i * (-1));
            }
#if !EASYTEST
            var appointment = ObjectSpace.FirstOrDefault<DevExpress.Persistent.BaseImpl.EF.Event>(a => a.Subject.Contains(employee.FullName));
            if(appointment == null) {
                appointment = ObjectSpace.CreateObject<DevExpress.Persistent.BaseImpl.EF.Event>();
                appointment.Subject = string.Format("{0} - performance review", employee.FullName);
                appointment.Description = string.Format("{0} \r\n{1}", employee.FullName, notes[i]);

                appointment.StartOn = note.DateTime?.AddDays(5).AddHours(12);
                appointment.EndOn = appointment.StartOn?.AddHours(2);
                appointment.Location = "101";
                appointment.AllDay = false;
                appointment.Status = 0;
                appointment.Label = i % 2 == 0 ? 2 : 5;
                var reviewerEmployeeResource = ObjectSpace.FirstOrDefault<DevExpress.Persistent.BaseImpl.EF.Resource>(r => r.Caption.Contains(reviewerEmployee.FullName));
                if(reviewerEmployeeResource == null) {
                    reviewerEmployeeResource = ObjectSpace.CreateObject<DevExpress.Persistent.BaseImpl.EF.Resource>();
                    reviewerEmployeeResource.Caption = reviewerEmployee.FullName;
                    reviewerEmployeeResource.Color = reviewerEmployee == FindTellitson() ? Color.AliceBlue : Color.LightCoral;
                }
                appointment.Resources.Add(reviewerEmployeeResource);
            }
#endif
        }

        ObjectSpace.CommitChanges();

        UpdateStatus("CreateTasks", "", "Creating demo tasks in the database...");

#if EASYTEST
            Employee employeeMary = FindTellitson();
            Employee employeeJohn = FindEmployee("John", "Nilsen");
            if(ObjectSpace.FirstOrDefault<DemoTask>(d => d.Subject == "Review reports") == null) {
                DemoTask task = ObjectSpace.CreateObject<DemoTask>();
                task.Subject = "Review reports";
                task.AssignedTo = employeeJohn;
                task.StartDate = DateTime.Parse("May 03, 2008");
                task.DueDate = DateTime.Parse("September 06, 2008");
                task.Status = BusinessObjects.TaskStatus.InProgress;
                task.Priority = Priority.High;
                task.EstimatedWorkHours = 60;
                task.Description = "Analyse the reports and assign new tasks to employees.";
            }

            if(ObjectSpace.FirstOrDefault<DemoTask>(d => d.Subject == "Fix breakfast") == null) {
                DemoTask task = ObjectSpace.CreateObject<DemoTask>();
                task.Subject = "Fix breakfast";
                task.AssignedTo = employeeMary;
                task.StartDate = DateTime.Parse("May 03, 2008");
                task.DueDate = DateTime.Parse("May 04, 2008");
                task.Status = BusinessObjects.TaskStatus.Completed;
                task.Priority = BusinessObjects.Priority.Low;
                task.EstimatedWorkHours = 1;
                task.ActualWorkHours = 3;
                task.Description = "The Development Department - by 9 a.m.\r\nThe R&QA Department - by 10 a.m.";
            }
            if(ObjectSpace.FirstOrDefault<DemoTask>(d => d.Subject == "Task1") == null) {
                DemoTask task = ObjectSpace.CreateObject<DemoTask>();
                task.Subject = "Task1";
                task.AssignedTo = employeeJohn;
                task.StartDate = DateTime.Parse("June 03, 2008");
                task.DueDate = DateTime.Parse("June 06, 2008");
                task.Status = BusinessObjects.TaskStatus.Completed;
                task.Priority = Priority.High;
                task.EstimatedWorkHours = 10;
                task.ActualWorkHours = 15;
                task.Description = "A task designed specially to demonstrate the PivotChart module. Switch to the Reports navigation group to view the generated analysis.";
            }
            if(ObjectSpace.FirstOrDefault<DemoTask>(d => d.Subject == "Task2") == null) {
                DemoTask task = ObjectSpace.CreateObject<DemoTask>();
                task.Subject = "Task2";
                task.AssignedTo = employeeJohn;
                task.StartDate = DateTime.Parse("July 03, 2008");
                task.DueDate = DateTime.Parse("July 06, 2008");
                task.Status = BusinessObjects.TaskStatus.Completed;
                task.Priority = Priority.Low;
                task.EstimatedWorkHours = 8;
                task.ActualWorkHours = 16;
                task.Description = "A task designed specially to demonstrate the PivotChart module. Switch to the Reports navigation group to view the generated analysis.";
            }
#endif
#if !EASYTEST
        IList<Employee> employees = ObjectSpace.GetObjects<Employee>();
        IList<DemoTask> taskList = GenerateTask(employees);
        if(taskList.Count > 0) {
            Random rndGenerator = new Random();
            foreach(Employee employee in employees) {
                if(taskList.Count == 1) {
                    employee.Tasks.Add(taskList[0]);
                }
                else if(taskList.Count == 2) {
                    employee.Tasks.Add(taskList[0]);
                    employee.Tasks.Add(taskList[1]);
                }
                else {
                    int index = rndGenerator.Next(1, taskList.Count - 2);
                    employee.Tasks.Add(taskList[index]);
                    employee.Tasks.Add(taskList[index - 1]);
                    employee.Tasks.Add(taskList[index + 1]);
                }
            }
        }
#endif
        UpdateStatus("CreateAnalysis", "", "Creating analysis reports in the database...");
        CreateDataToBeAnalysed();
        UpdateStatus("CreateSecurityData", "", "Creating users and roles in the database...");

        #region Create Users for the Complex Security Strategy
        // If a user named 'Sam' doesn't exist in the database, create this user
        ApplicationUser user1 = ObjectSpace.FirstOrDefault<ApplicationUser>(u => u.UserName == "Sam");
        if(user1 == null) {
            user1 = ObjectSpace.CreateObject<ApplicationUser>();
            user1.UserName = "Sam";
            // Set a password if the standard authentication type is used
            user1.SetPassword("");

            // Set default photo for users
            var andreaDeville = FindEmployee("Andrea", "Deville");
            user1.Photo.MediaData = andreaDeville?.Photo;

            // The UserLoginInfo object requires a user object Id (Oid).
            // Commit the user object to the database before you create a UserLoginInfo object. This will correctly initialize the user key property.
            ObjectSpace.CommitChanges(); //This line persists created object(s).
            ((ISecurityUserWithLoginInfo)user1).CreateUserLoginInfo(SecurityDefaults.PasswordAuthentication, ObjectSpace.GetKeyValueAsString(user1));
        }
        // If a user named 'John' doesn't exist in the database, create this user
        ApplicationUser user2 = ObjectSpace.FirstOrDefault<ApplicationUser>(u => u.UserName == "John");
        if(user2 == null) {
            user2 = ObjectSpace.CreateObject<ApplicationUser>();
            user2.UserName = "John";
            // Set a password if the standard authentication type is used
            user2.SetPassword("");

            // Set default photo for users
            var johnNilsen = FindEmployee("John", "Nilsen");
            user2.Photo.MediaData = johnNilsen?.Photo;

            // The UserLoginInfo object requires a user object Id (Oid).
            // Commit the user object to the database before you create a UserLoginInfo object. This will correctly initialize the user key property.
            ObjectSpace.CommitChanges(); //This line persists created object(s).
            ((ISecurityUserWithLoginInfo)user2).CreateUserLoginInfo(SecurityDefaults.PasswordAuthentication, ObjectSpace.GetKeyValueAsString(user2));
        }



        // If a role with the Administrators name doesn't exist in the database, create this role
        PermissionPolicyRole adminRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administrators");
        if(adminRole == null) {
            adminRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
            adminRole.Name = "Administrators";
        }
        adminRole.IsAdministrative = true;

        // If a role with the Users name doesn't exist in the database, create this role
        PermissionPolicyRole userRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Users");
        if(userRole == null) {
            userRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
            userRole.Name = "Users";
            userRole.PermissionPolicy = SecurityPermissionPolicy.AllowAllByDefault;
            userRole.AddNavigationPermission("Application/NavigationItems/Items/Default/Items/PermissionPolicyRole_ListView", SecurityPermissionState.Deny);
            userRole.AddNavigationPermission("Application/NavigationItems/Items/Default/Items/ApplicationUser_ListView", SecurityPermissionState.Deny);
            userRole.AddTypePermission<PermissionPolicyRole>(SecurityOperations.FullAccess, SecurityPermissionState.Deny);
            userRole.AddTypePermission<PermissionPolicyUser>(SecurityOperations.FullAccess, SecurityPermissionState.Deny);
            userRole.AddObjectPermissionFromLambda<PermissionPolicyUser>(SecurityOperations.ReadOnlyAccess, cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            userRole.AddMemberPermissionFromLambda<PermissionPolicyUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", null, SecurityPermissionState.Allow);
            userRole.AddMemberPermissionFromLambda<PermissionPolicyUser>(SecurityOperations.Write, "StoredPassword", null, SecurityPermissionState.Allow);
            userRole.AddTypePermission<PermissionPolicyTypePermissionObject>("Write;Delete;Create", SecurityPermissionState.Deny);
            userRole.AddTypePermission<PermissionPolicyMemberPermissionsObject>("Write;Delete;Create", SecurityPermissionState.Deny);
            userRole.AddTypePermission<PermissionPolicyObjectPermissionsObject>("Write;Delete;Create", SecurityPermissionState.Deny);
            userRole.AddTypePermission<PermissionPolicyNavigationPermissionObject>("Write;Delete;Create", SecurityPermissionState.Deny);
            userRole.AddTypePermission<PermissionPolicyActionPermissionObject>("Write;Delete;Create", SecurityPermissionState.Deny);
            userRole.AddObjectPermissionFromLambda<AuditDataItemPersistent>(SecurityOperations.Read, a => a.UserObject.Key != CurrentUserIdOperator.CurrentUserId().ToString(), SecurityPermissionState.Deny);
        }

        // Add the Administrators role to the user1
        user1.Roles.Add(adminRole);
        // Add the Users role to the user2
        user2.Roles.Add(userRole);
        #endregion

        ObjectSpace.CommitChanges();
    }
    private void CreateDataToBeAnalysed() {
        var taskAnalysis1 = ObjectSpace.FirstOrDefault<DevExpress.Persistent.BaseImpl.EF.Analysis>(a => a.Name == "Completed tasks");
        if(taskAnalysis1 == null) {
            taskAnalysis1 = ObjectSpace.CreateObject<DevExpress.Persistent.BaseImpl.EF.Analysis>();
            taskAnalysis1.Name = "Completed tasks";
            taskAnalysis1.ObjectTypeName = typeof(DemoTask).FullName;
            taskAnalysis1.Criteria = "[Status] = ##Enum#MainDemo.Module.BusinessObjects.TaskStatus,Completed#";
        }
        var taskAnalysis2 = ObjectSpace.FirstOrDefault<DevExpress.Persistent.BaseImpl.EF.Analysis>(a => a.Name == "Estimated and actual work comparison");
        if(taskAnalysis2 == null) {
            taskAnalysis2 = ObjectSpace.CreateObject<DevExpress.Persistent.BaseImpl.EF.Analysis>();
            taskAnalysis2.Name = "Estimated and actual work comparison";
            taskAnalysis2.ObjectTypeName = typeof(DemoTask).FullName;
        }
    }
    private void CreateDepartments() {
        DataTable departmentsTable = GetDepartmentsDataTable();
        DataTable employeesTable = GetEmployeesDataTable();
        IList<Department> result = new List<Department>();
        foreach(DataRow departmentData in departmentsTable.Rows) {
            string departmentTitle = Convert.ToString(departmentData["Title"]);
            Department department = FindDepartment(departmentTitle);
            if(department == null) {
                department = ObjectSpace.CreateObject<Department>();
                department.Title = departmentTitle;
                department.Office = Convert.ToString(departmentData["Office"]);
                department.Location = Convert.ToString(departmentData["Location"]);
                department.Description = Convert.ToString(departmentData["Description"]);
                result.Add(department);
            }
        }
        ObjectSpace.CommitChanges();
        foreach(Department department in result) {
            DataRow departmentData = departmentsTable.Rows.OfType<DataRow>().First(p => Convert.ToString(p["Title"]) == department.Title);
            string departmentHeadId = Convert.ToString(departmentData["DepartmentHead"]);
            DataRow employee = employeesTable.Select($"Id = '{departmentHeadId}'").FirstOrDefault();
            if(employee != null) {
                department.DepartmentHead = GetEmployee(employee);
            }
        }

        ObjectSpace.CommitChanges();
    }
    private void CreateEmployees() {
        DataTable employeesTable = GetEmployeesDataTable();
        foreach(DataRow employee in employeesTable.Rows) {
            GetEmployee(employee);
        }
        ObjectSpace.CommitChanges();
    }
    private Employee GetEmployee(DataRow employeeRow) {
        string email = Convert.ToString(employeeRow["EmailAddress"]);
        Employee employee = ObjectSpace.FirstOrDefault<Employee>(e => e.Email == email, true);
        if(employee == null) {
            employee = ObjectSpace.CreateObject<Employee>();
            employee.Email = email;
            employee.FirstName = Convert.ToString(employeeRow["FirstName"]);
            employee.LastName = Convert.ToString(employeeRow["LastName"]);
            employee.Birthday = Convert.ToDateTime(employeeRow["BirthDate"]);
            employee.Photo = Convert.FromBase64String(Convert.ToString(employeeRow["ImageData"]));
            string titleOfCourtesyText = Convert.ToString(employeeRow["Title"]).ToLower();
            if(!string.IsNullOrEmpty(titleOfCourtesyText)) {
                titleOfCourtesyText = titleOfCourtesyText.Replace(".", "");
                TitleOfCourtesy titleOfCourtesy;
                if(Enum.TryParse<TitleOfCourtesy>(titleOfCourtesyText, true, out titleOfCourtesy)) {
                    employee.TitleOfCourtesy = titleOfCourtesy;
                }
            }
            BusinessObjects.PhoneNumber phoneNumber = ObjectSpace.CreateObject<BusinessObjects.PhoneNumber>();
            phoneNumber.Party = employee;
            phoneNumber.Number = Convert.ToString(employeeRow["Phone"]);
            phoneNumber.PhoneType = "Work";
            Address address = ObjectSpace.CreateObject<Address>();
            employee.Address1 = address;
            address.ZipPostal = Convert.ToString(employeeRow["PostalCode"]);
            address.Street = Convert.ToString(employeeRow["AddressLine1"]);
            address.City = Convert.ToString(employeeRow["City"]);
            address.StateProvince = Convert.ToString(employeeRow["StateProvinceName"]);
            string countryName = Convert.ToString(employeeRow["CountryRegionName"]);
            Country country = ObjectSpace.FirstOrDefault<Country>(c => c.Name == countryName, true);
            if(country == null) {
                country = ObjectSpace.CreateObject<Country>();
                country.Name = countryName;
            }
            address.Country = country;

            string departmentTitle = Convert.ToString(employeeRow["GroupName"]);
            employee.Department = FindDepartment(departmentTitle);

            string positionTitle = Convert.ToString(employeeRow["JobTitle"]);
            Position position = FindPosition(positionTitle);
            if(position == null) {
                position = ObjectSpace.CreateObject<Position>();
                position.Title = positionTitle;
                if(employee.Department != null) {
                    position.Departments.Add(employee.Department);
                    employee.Department.Positions.Add(position);
                }
            }
            employee.Position = position;
        }
        return employee;
    }
    private IList<DemoTask> GenerateTask(IList<Employee> employees) {
        Random rndGenerator = new Random();
        List<DemoTask> taskList = new List<DemoTask>();
        DataTable tasksTable = GetTasksDataTable();
        foreach(DataRow taskRow in tasksTable.Rows) {
            string taskSubject = UpdateContent(Convert.ToString(taskRow["Subject"]));
            if(ObjectSpace.FirstOrDefault<DemoTask>(t => t.Subject == taskSubject) == null) {
                DemoTask task = ObjectSpace.CreateObject<DemoTask>();
                task.Subject = taskSubject;
                task.Description = UpdateContent(Convert.ToString(taskRow["Description"]));
                int rndStatus = rndGenerator.Next(0, 5);
                task.Status = (BusinessObjects.TaskStatus)rndStatus;
                task.DueDate = DateTime.Now.AddHours((90 - rndStatus * 9) + 24).Date;
                task.EstimatedWorkHours = rndGenerator.Next(10, 20);
                if(task.Status == BusinessObjects.TaskStatus.WaitingForSomeoneElse ||
                   task.Status == BusinessObjects.TaskStatus.Completed ||
                   task.Status == BusinessObjects.TaskStatus.InProgress) {
                    task.StartDate = DateTime.Now.AddHours(-rndGenerator.Next(720)).Date;
                    task.ActualWorkHours = rndGenerator.Next(task.EstimatedWorkHours - 10, task.EstimatedWorkHours + 10);
                }
                task.DueDate = DateTime.Now.AddHours((90 - rndStatus * 9) + 24).Date;
                task.AssignedTo = employees[rndGenerator.Next(0, employees.Count)];
                task.Priority = (BusinessObjects.Priority)rndGenerator.Next(3);
                taskList.Add(task);
            }
        }
        return taskList;
    }
    private Stream FindEmployeeResume(Employee employee) {
        string shortName = employee.FirstName + "_" + employee.LastName + ".pdf";
        string embeddedResourceName = Array.Find<string>(this.GetType().Assembly.GetManifestResourceNames(), (s) => { return s.Contains(shortName); });
        return this.GetType().Assembly.GetManifestResourceStream(embeddedResourceName);
    }
    private bool LocationIsEmpty(Employee employee) {
        return employee.Location == null || employee.Location.Employee == null || (employee.Location.Latitude == 0 && employee.Location.Longitude == 0);
    }
    private void InitEmployeeLocation() {
        Employee employeeMary = FindTellitson();
        if(employeeMary != null && LocationIsEmpty(employeeMary)) {
            if(employeeMary.Location == null)
                employeeMary.Location = ObjectSpace.CreateObject<Location>();

            employeeMary.Location.Employee = employeeMary;
            employeeMary.Location.Latitude = 40.620610;
            employeeMary.Location.Longitude = -73.935242;
        }

        Employee employeeJohn = FindEmployee("John", "Nilsen");
        if(employeeJohn != null && LocationIsEmpty(employeeJohn)) {
            if(employeeJohn.Location == null)
                employeeJohn.Location = ObjectSpace.CreateObject<Location>();

            employeeJohn.Location.Employee = employeeJohn;
            employeeJohn.Location.Latitude = 40.711510;
            employeeJohn.Location.Longitude = -73.845252;
        }

        Employee employeeJanete = FindJanete();
        if(employeeJanete != null && LocationIsEmpty(employeeJanete)) {
            if(employeeJanete.Location == null)
                employeeJanete.Location = ObjectSpace.CreateObject<Location>();

            employeeJanete.Location.Employee = employeeJanete;
            employeeJanete.Location.Latitude = 40.710410;
            employeeJanete.Location.Longitude = -73.963262;
        }

        Employee employeeKarl = FindEmployee("Karl", "Jablonski");
        if(employeeKarl != null && LocationIsEmpty(employeeKarl)) {
            if(employeeKarl.Manager == null) {
                employeeKarl.Manager = employeeJanete;
            }
            if(employeeKarl.Location == null)
                employeeKarl.Location = ObjectSpace.CreateObject<Location>();

            employeeKarl.Location.Employee = employeeKarl;
            employeeKarl.Location.Latitude = 40.792613;
            employeeKarl.Location.Longitude = -73.925142;
        }

        ObjectSpace.CommitChanges();
    }

    private DataTable employeesDataTable = null;
    private DataTable GetEmployeesDataTable() {
        if(employeesDataTable == null) {
            string shortName = "EmployeesWithPhoto.xml";
            Stream stream = GetResourceByName(shortName);
            DataSet ds = new DataSet();
            ds.ReadXml(stream);
            employeesDataTable = ds.Tables["Employee"];
        }
        return employeesDataTable;
    }
    private DataTable GetTasksDataTable() {
        string shortName = "Tasks.xml";
        Stream stream = GetResourceByName(shortName);
        DataSet ds = new DataSet();
        ds.ReadXml(stream);
        return ds.Tables["Task"];
    }
    private DataTable GetDepartmentsDataTable() {
        string shortName = "Departments.xml";
        Stream stream = GetResourceByName(shortName);
        DataSet ds = new DataSet();
        ds.ReadXml(stream);
        return ds.Tables["Department"];
    }
    private Stream GetResourceByName(string shortName) {
        string embeddedResourceName = Array.Find<string>(this.GetType().Assembly.GetManifestResourceNames(), (s) => { return s.Contains(shortName); });
        Stream stream = this.GetType().Assembly.GetManifestResourceStream(embeddedResourceName);
        if(stream == null) {
            throw new Exception(string.Format("Cannot read data from the {0} file!", shortName));
        }
        return stream;
    }
    private string UpdateContent(string content) {
        if(!string.IsNullOrEmpty(content)) {
            return content.Replace("LAST_YEAR", (DateTime.Now.Year - 1).ToString()).Replace("CURRENT_YEAR", DateTime.Now.Year.ToString());
        }
        return content;
    }
}
class PayrollSampleDataGenerator {
    public static void GenerateEmployeePaychecks(DevExpress.ExpressApp.IObjectSpace objectSpace, Employee employee) {
        int year = DateTime.Now.Year - 1;
        var taxRates = new double[] { 0.20, 0.22, 0.30 };
        var hoursBaseline = new int[] { 35, 30 };
        var rand = new Random();
        var payRate = rand.Next(10, 12) + rand.Next(0, 3) + rand.Next(5, 9);
        var overtimePayRate = rand.Next(15, 25) + rand.Next(5, 5);
        var taxRated = taxRates[rand.Next(0, 3)];
        var hours = rand.Next(0, 5) == 0 ? hoursBaseline[rand.Next(0, 1)] : 40;
        for(int month = 1; month <= 12; month++) {
            GeneratePaycheck(objectSpace, employee, rand, payRate, overtimePayRate, taxRated, hours,
                2 * month - 1, new DateTime(day: 1, month: month, year: year), new DateTime(day: 15, month: month, year: year));
            GeneratePaycheck(objectSpace, employee, rand, payRate, overtimePayRate, taxRated, hours,
                2 * month, new DateTime(day: 16, month: month, year: year), new DateTime(day: DateTime.DaysInMonth(year, month), month: month, year: year));
        }
    }

    private static void GeneratePaycheck(IObjectSpace objectSpace, Employee employee, Random rand, int payRate, int overtimePayRate, double taxRated, int hours, int period, DateTime start, DateTime end) {
        var paycheck = objectSpace.CreateObject<Paycheck>();
        if(rand.Next(0, 5) == 0) {
            hours -= rand.Next(0, 2) + rand.Next(0, 2) + rand.Next(0, 2);
        }
        int overtimeHours = 0;
        if(rand.Next(0, 5) == 0) {
            overtimeHours += rand.Next(2, 5) + rand.Next(1, 6);
        }
        paycheck.PayPeriod = period;
        paycheck.PayPeriodStart = start;
        paycheck.PayPeriodEnd = end;
        paycheck.PaymentDate = end;
        paycheck.Employee = employee;
        paycheck.PayRate = payRate;
        paycheck.Hours = hours;
        paycheck.OvertimePayRate = overtimePayRate;
        paycheck.OvertimeHours = overtimeHours;
        paycheck.TaxRate = taxRated;
    }
}
