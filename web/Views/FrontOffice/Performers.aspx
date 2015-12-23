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

<form action="/api/Basket/Create" method="POST">
<div id="AskForAnEstimateResult"></div>
<input type="hidden" name="productref" value="main">
<input  type="hidden" name="clientname" value="<%= ViewBag.ClientName %>">
<input type="hidden" name="type" value="Yavsc.Model.FrontOffice.NominativeSimpleBookingQuery">
<input type="hidden" name="MEACode" value="<%=ViewBag.SimpleBookingQuery.MEACode%>" >
<input type="hidden" name="Need" value="<%=ViewBag.SimpleBookingQuery.Need%>" >
<input type="hidden" name="PreferedDate" value="<%=ViewBag.SimpleBookingQuery.PreferedDate%>" >
<input type="hidden" name="PerformerName" value="<%=available.Profile.UserName%>" >
<input type="submit" class="actionlink" value="<%= Html.Translate("AskForAnEstimate") %>">
</form>

</div>
</div>
<% } %>
<script>
$(document).ready(function(){
   $("[data-type='user']").user({circles:<%=Ajax.JSonString(ViewData["Circles"])%>});
});
</script>
</asp:Content>
