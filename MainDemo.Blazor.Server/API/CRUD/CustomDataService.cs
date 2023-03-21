using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.WebApi.Services;
using DevExpress.Persistent.Validation;

namespace MainDemo.WebApi.Services {
    public class CustomDataService : DataService {
        readonly IValidator validator;
        public CustomDataService(IObjectSpaceFactory objectSpaceFactory, ITypesInfo typesInfo, IValidator validator) : base(objectSpaceFactory, typesInfo) {
            this.validator = validator;
        }

        protected override IObjectSpace CreateObjectSpace(Type objectType) {
            IObjectSpace objectSpace = base.CreateObjectSpace(objectType);
            objectSpace.Committing += ObjectSpace_Committing;
            return objectSpace;
        }

        private void ObjectSpace_Committing(object? sender, System.ComponentModel.CancelEventArgs e) {
            IObjectSpace os = (IObjectSpace)sender!;
            var validationResult = validator.RuleSet.ValidateAllTargets((IObjectSpace)sender!, os.ModifiedObjects, DefaultContexts.Save);
            if(validationResult.ValidationOutcome == ValidationOutcome.Error) {
                throw new ValidationException(validationResult);
            }
        }
    }
}
