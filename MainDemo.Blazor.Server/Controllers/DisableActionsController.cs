using DevExpress.ExpressApp;
using MainDemo.Module.Controllers;

namespace MainDemo.Blazor.Server.Controllers;
public class DisableActionsController : ViewController {
    protected override void OnActivated() {
        base.OnActivated();
        DeactivateController<PopupNotesController>();
#if !DEBUG
            DeactivateController<DevExpress.ExpressApp.SystemModule.ObjectMethodActionsViewController>();
            DeactivateController<TaskActionsController>();
#endif
        if(View is ListView) {
            Frame.GetController<DevExpress.ExpressApp.ViewVariantsModule.ChangeVariantController>()?.ChangeVariantAction.Active.SetItemValue("BlazorTemporary", false);
        }
    }
    private void DeactivateController<T>() where T : Controller {
        Frame.GetController<T>()?.Active.SetItemValue("BlazorTemporary", false);
    }
    private void ActivateController<T>() where T : Controller {
        Frame.GetController<T>()?.Active.SetItemValue("BlazorTemporary", true);
    }
}
