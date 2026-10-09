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
        vm.Users.Add(new Yavsc.Abstract.Identity.UserInfo { UserId = "1", UserName = "alice", FullName = "Alice Example" });
        vm.Users.Add(new Yavsc.Abstract.Identity.UserInfo { UserId = "2", UserName = "bob", FullName = "Bob Jones" });
        vm.Users.Add(new Yavsc.Abstract.Identity.UserInfo { UserId = "3", UserName = "carol", FullName = "Carol Smith" });

        vm.SearchText = "ali";

        Assert.Single(vm.FilteredUsers);
        Assert.Equal("alice", vm.FilteredUsers[0].UserName);
    }

    [Fact]
    public void Administration_page_filters_users_by_full_name()
    {
        var vm = new AdministrationPageViewModel();
        vm.Users.Add(new Yavsc.Abstract.Identity.UserInfo { UserId = "1", UserName = "alice", FullName = "Alice Example" });
        vm.Users.Add(new Yavsc.Abstract.Identity.UserInfo { UserId = "2", UserName = "bob", FullName = "Bob Jones" });

        vm.SearchText = "example";

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
        Assert.False(vm.SaveProfileCommand.CanExecute(null));
    }

    [Fact]
    public void UserProfilePage_builds_profile_payload_for_calendar_and_bank_values()
    {
        var vm = new UserProfilePageViewModel
        {
            UserName = "alice.new",
            FullName = "Alice Example",
            Address = "14 rue de l’Érable",
            DedicatedGoogleCalendar = "calendar-123",
            BankInfoSummary = "FR14 2004 1010 0505 0001 3M02 606"
        };

        var payload = vm.BuildProfileUpdateRequest();

        Assert.Equal("alice.new", payload.UserName);
        Assert.Equal("calendar-123", payload.GoogleCalendarId);
        Assert.Null(payload.BankInfoSummary);
    }

    [Fact]
    public void UserProfilePage_turns_placeholder_values_into_empty_payload_fields()
    {
        var vm = new UserProfilePageViewModel
        {
            UserName = "alice.new",
            FullName = "Alice Example",
            Address = "14 rue de l’Érable",
            DedicatedGoogleCalendar = "Non configuré",
            BankInfoSummary = "Aucune information bancaire"
        };

        var payload = vm.BuildProfileUpdateRequest();

        Assert.Equal(string.Empty, payload.GoogleCalendarId);
        Assert.Null(payload.BankInfoSummary);
    }

    [Fact]
    public void PerformerConfigurationPage_preserves_current_user_id_in_save_payload()
    {
        var vm = new PerformerConfigurationPageViewModel
        {
            PerformerId = "user-42",
            UserName = "alice",
            ExerciseCountryCode = "fr",
            SIREN = "123456789",
            Website = "https://example.com",
            Active = true,
            AcceptNotifications = true,
            AcceptPublicContact = true,
            UseGeoLocalizationToReduceDistanceWithClients = true
        };

        var payload = vm.BuildPerformerProfileSettings();

        Assert.Equal("user-42", payload.PerformerId);
        Assert.Equal("alice", payload.UserName);
        Assert.Equal("fr", payload.ExerciseCountryCode);
    }
}
