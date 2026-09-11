using PostIt.ViewModels;

namespace PostIt.Tests;

public class HomePageProviderFlowTests
{
    [Fact]
    public void HomePage_exposes_provider_requests_command()
    {
        var vm = new HomePageViewModel();

        Assert.NotNull(vm.OpenProviderRequests);
        Assert.True(vm.OpenProviderRequests.CanExecute(null));
    }
}
