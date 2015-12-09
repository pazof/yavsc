<%@ Page Language="C#" Title="Performers" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<PerformerAvailability>>" %>

<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">
 <script type="text/javascript" src="/Scripts/yavsc.user.js"></script>
</asp:Content>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<% foreach (var available in Model) { %>
<div class="panel">
<%= Html.Partial("Performer", available.Profile ) %>
<div class="availability">
<% if (available.DateAvailable) { %>
<%= Html.Translate("ThisPerformerGivesAccessToHisCalendarAndSeemsToBeAvailableThis") %>
<%= available.PerformanceDate.ToString("D") %>
<% } else if (available.Profile.HasCalendar()) { %>
<%= Html.Translate("ThisPerformerGivesAccessToHisCalendarAndItAppearsHeShouldNotBeAvailableThis") %>
<%= available.PerformanceDate.ToString("D") %>
<% } else {%>
<%= Html.Translate("ThisPerformerDoesntGiveAccessToHisCalendar") %>
<% } %>
</div>
</div>
<% } %>
<script>
$(document).ready(function(){
   $("[data-type='user']").user({circles:<%=Ajax.JSonString(ViewData["Circles"])%>});
});
</script>
</asp:Content>
