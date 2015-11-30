<%@ Page Language="C#" Title="Performers" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<PerformerAvailability>>" %>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<% foreach (var available in Model) { %>
<div class="usercard">
<%= Html.Partial("Performer", available.Profile ) %><br>
<% if (available.DateAvailable) {
%>
<%= Html.Translate("ThisPerformerGivesAccessToHisCalendarAndSeemsToBeAvailable") %>
<% 
}
else if (available.Profile.HasCalendar()) {
%>
<%= Html.Translate("ThisPerformerGivesAccessToHisCalendarAndItAppearsHeShouldNotBeAvailable") %>
<% 
} else {
%>
<%= Html.Translate("ThisPerformerDoesntGiveAccessToHisCalendar") %>


<% 
} %></div><% } %>
</asp:Content>
