using System;
using PostIt.ViewModels;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;

namespace PostIt.Helpers;

public static class FormHelpers
{
    public static BillingCommandPageViewModel?
    CreateCommandPageViewModel(
        this CommandFormSummary form,
        ActivityInfo activity,
        ActivityUserDisplayItem performer,
        BillingApiClient billingClient)
    {

        string namespacePrefix = typeof(PostIt.ViewModels.Commands.RdvViewModel).Namespace + ".";

        string formVMName =   form.ActionName + "ViewModel";

        string formOnActivityVMName =  activity.Code + formVMName + "ViewModel";

        var vmType = Type.GetType(namespacePrefix +formOnActivityVMName);
        if (vmType == null)
        {
            vmType = Type.GetType(namespacePrefix + formVMName);
        }
        if (vmType == null)
        {
            Console.Error.WriteLine(
                $"! Cannot find type '{formOnActivityVMName}' or '{formVMName}'");
            return null;
        }
        if (!typeof(BillingCommandPageViewModel).IsAssignableFrom(vmType))
        {
            Console.Error.WriteLine($"! The type '{formOnActivityVMName}' or '{formVMName}' is not a BillingCommandPageViewModel");
            return null;
        }

        var vm = Activator.CreateInstance(vmType, activity, performer, form, billingClient);

        if (vm == null)
        {
            throw new InvalidOperationException($"Cannot create instance of '{formOnActivityVMName}' or '{formVMName}'");
        }

        return vm as BillingCommandPageViewModel ?? throw new InvalidOperationException($"The type '{formOnActivityVMName}' or '{formVMName}' is not a BillingCommandPageViewModel");
    }
}
