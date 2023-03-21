using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using MainDemo.Blazor.Server.Controllers;

namespace MainDemo.Blazor.Server;

public sealed class MainDemoBlazorModule : ModuleBase {
    public MainDemoBlazorModule() {
        Description = "Blazor Main Demo module";
    }

    public override void Setup(XafApplication application) {
        base.Setup(application);
        application.CreateCustomLogonWindowControllers += Application_CreateCustomLogonWindowControllers;
    }

    private void Application_CreateCustomLogonWindowControllers(object? sender, CreateCustomLogonWindowControllersEventArgs e) {
        e.Controllers.Add(Application.CreateController<LogonParametersViewController>());
    }

    protected override IEnumerable<Type> GetDeclaredExportedTypes() {
        return Type.EmptyTypes;
    }

    public override void CustomizeTypesInfo(ITypesInfo typesInfo) {
        base.CustomizeTypesInfo(typesInfo);
    }
}
