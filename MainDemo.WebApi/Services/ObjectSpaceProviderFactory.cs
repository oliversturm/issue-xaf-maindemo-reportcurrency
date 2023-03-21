using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;

namespace MainDemo.WebApi.Core;

public sealed class ObjectSpaceProviderFactory : IObjectSpaceProviderFactory
{
  readonly ITypesInfo typesInfo;
  readonly IDbContextFactory<MainDemo.Module.BusinessObjects.MainDemoDbContext> dbFactory;

  public ObjectSpaceProviderFactory(ITypesInfo typesInfo, IDbContextFactory<MainDemo.Module.BusinessObjects.MainDemoDbContext> dbFactory)
  {
    this.typesInfo = typesInfo;
    this.dbFactory = dbFactory;
  }

  IEnumerable<IObjectSpaceProvider> IObjectSpaceProviderFactory.CreateObjectSpaceProviders()
  {
    yield return new EFCoreObjectSpaceProvider<MainDemo.Module.BusinessObjects.MainDemoDbContext>(dbFactory, typesInfo);
    yield return new NonPersistentObjectSpaceProvider(typesInfo, null);
  }
}
