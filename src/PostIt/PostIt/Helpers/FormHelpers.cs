using System;
using PostIt.ViewModels;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;

namespace PostIt.Helpers;

public static class FormHelpers
{
    public static BillingCommandPageViewModel
    CreateCommandPageViewModel(
        this CommandFormSummary form,
        ActivityInfo activity,
        ActivityUserDisplayItem performer,
        BillingApiClient billingClient)
    {

        string formVMName = form.ActionName + "ViewModel";

        string formOnActivityVMName = activity.Code + formVMName;

        var vmType = Type.GetType(formOnActivityVMName);
        if (vmType == null)
        {
            vmType = Type.GetType(formVMName);
        }
        if (vmType == null)
        {
            throw new InvalidOperationException($"Cannot find type '{formOnActivityVMName}' or '{formVMName}'");
        }
        if (!typeof(BillingCommandPageViewModel).IsAssignableFrom(vmType))
        {
            throw new InvalidOperationException($"The type '{formOnActivityVMName}' or '{formVMName}' is not a BillingCommandPageViewModel");
        }

        var vm = Activator.CreateInstance(vmType);

        if (vm == null)
        {
            throw new InvalidOperationException($"Cannot create instance of '{formOnActivityVMName}' or '{formVMName}'");
        }

        return vm as BillingCommandPageViewModel ?? throw new InvalidOperationException($"The type '{formOnActivityVMName}' or '{formVMName}' is not a BillingCommandPageViewModel");
    }
}
