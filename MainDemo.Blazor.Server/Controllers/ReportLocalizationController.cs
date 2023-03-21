using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.ExpressApp.ReportsV2.Blazor;
using Microsoft.JSInterop;

namespace MainDemo.Blazor.Server.Controllers;

public class ReportLocalizationController : ViewController<DetailView> {
    private BlazorApplication BlazorApplication => (BlazorApplication)Application;
    private IXafCultureInfoService CultureInfoService => BlazorApplication.ServiceProvider.GetRequiredService<IXafCultureInfoService>();
    private IJSRuntime JSRuntime => BlazorApplication.ServiceProvider.GetRequiredService<IJSRuntime>();

    protected override void OnActivated() {
        base.OnActivated();
        View.CustomizeViewItemControl<ReportViewerViewItem>(this, CustomizeReportViewer);
        View.CustomizeViewItemControl<ReportDesignerViewItem>(this, CustomizeReportDesigner);
    }
    private async void CustomizeReportDesigner(ReportDesignerViewItem propertyEditor) {
        var adapter = (ReportDesignerViewItem.DxReportDesignerAdapter)propertyEditor.Control;
        await JSRuntime.InvokeVoidAsync("ReportingLocalization.setCurrentCulture", CultureInfoService.CurrentCulture.Name);
        adapter.CallbacksModel.CustomizeLocalization = "ReportingLocalization.onCustomizeLocalization";
    }
    private async void CustomizeReportViewer(ReportViewerViewItem propertyEditor) {
        var adapter = (ReportViewerViewItem.DxDocumentViewerAdapter)propertyEditor.Control;
        await JSRuntime.InvokeVoidAsync("ReportingLocalization.setCurrentCulture", CultureInfoService.CurrentCulture.Name);
        adapter.CallbacksModel.CustomizeLocalization = "ReportingLocalization.onCustomizeLocalization";
    }
}
