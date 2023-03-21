using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace MainDemo.Module.BusinessObjects;

[DefaultProperty(nameof(Subject))]
public class Task : BaseObject {

    public virtual DateTime? DateCompleted { get; set; }

    public virtual String Subject { get; set; }

    [FieldSize(FieldSizeAttribute.Unlimited)]
    public virtual String Description { get; set; }

    public virtual DateTime? DueDate { get; set; }

    public virtual DateTime? StartDate { get; set; }

    [Browsable(false)]
    [NotMapped]
    public int Status_Int { get; set; }

    public virtual int PercentCompleted { get; set; }

    public virtual Party AssignedTo { get; set; }

    private TaskStatus status;

    [Column(nameof(Status_Int))]
    public virtual TaskStatus Status {
        get { return status; }
        set {
            status = value;
            if(isLoaded) {
                if(value == TaskStatus.Completed) {
                    DateCompleted = DateTime.Now;
                }
                else {
                    DateCompleted = null;
                }
            }
        }
    }

    [Action(ImageName = "State_Task_Completed")]
    public void MarkCompleted() {
        Status = TaskStatus.Completed;
    }

    private bool isLoaded = false;
    public override void OnLoaded() {
        isLoaded = true;
    }
}

public enum TaskStatus {
    [ImageName("State_Task_NotStarted")]
    NotStarted,
    [ImageName("State_Task_InProgress")]
    InProgress,
    [ImageName("State_Task_WaitingForSomeoneElse")]
    WaitingForSomeoneElse,
    [ImageName("State_Task_Deferred")]
    Deferred,
    [ImageName("State_Task_Completed")]
    Completed
}
