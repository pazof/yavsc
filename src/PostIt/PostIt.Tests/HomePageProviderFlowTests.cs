using Yavsc.Abstract.Identity;
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

    [Fact]
    public void HomePage_exposes_profile_command()
    {
        var vm = new HomePageViewModel();

        Assert.NotNull(vm.OpenProfile);
        Assert.True(vm.OpenProfile.CanExecute(null));
    }

    [Fact]
    public void HomePage_exposes_performer_configuration_command()
    {
        var vm = new HomePageViewModel();

        Assert.NotNull(vm.OpenPerformerConfiguration);
        Assert.True(vm.OpenPerformerConfiguration.CanExecute(null));
    }

    [Fact]
    public void HomePage_exposes_administration_command()
    {
        var vm = new HomePageViewModel();

        Assert.NotNull(vm.OpenAdministration);
        Assert.True(vm.OpenAdministration.CanExecute(null));
    }

    [Fact]
    public void Administration_page_filters_users_by_search_text()
    {
        var vm = new AdministrationPageViewModel();
        vm.Users.Add(new Yavsc.Abstract.Identity.UserInfo { UserId = "1", UserName = "alice" });
        vm.Users.Add(new Yavsc.Abstract.Identity.UserInfo { UserId = "2", UserName = "bob" });
        vm.Users.Add(new Yavsc.Abstract.Identity.UserInfo { UserId = "3", UserName = "carol" });

        vm.SearchText = "ali";

        Assert.Single(vm.FilteredUsers);
        Assert.Equal("alice", vm.FilteredUsers[0].UserName);
    }

    [Fact]
    public void UserProfilePage_exposes_full_name_and_address_fields()
    {
        var vm = new UserProfilePageViewModel();

        Assert.Equal("—", vm.FullName);
        Assert.Equal("—", vm.Address);
        Assert.Equal("Non configuré", vm.DedicatedGoogleCalendar);
        Assert.Equal("Aucune information bancaire", vm.BankInfoSummary);
        Assert.NotNull(vm.RefreshCommand);
    }

    [Fact]
    public void ProfileUpdateRequest_allows_username_updates()
    {
        var request = new ProfileUpdateRequest
        {
            UserName = "alice.new"
        };

        Assert.Equal("alice.new", request.UserName);
    }

    [Fact]
    public void UserProfilePage_can_save_profile_values()
    {
        var vm = new UserProfilePageViewModel
        {
            UserName = "alice.new",
            FullName = "Alice Example",
            Address = "14 rue de l’Érable",
            DedicatedGoogleCalendar = "calendar-123",
            BankInfoSummary = "FR14 2004 1010 0505 0001 3M02 606"
        };

        Assert.NotNull(vm.SaveProfileCommand);
        Assert.True(vm.SaveProfileCommand.CanExecute(null));
    }
}
