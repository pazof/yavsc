<%@ Page Language="C#" Title="Performers" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<PerformerAvailability>>" %>

<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">
 <script type="text/javascript" src="/Scripts/yavsc.user.js"></script>
</asp:Content>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<% foreach (var available in Model) { %>
<div class="panel">
<%= Html.Partial("Performer", available.Profile ) %>
<div class="availability">

<% if (available.DateAvailable) { %><p>
<%= Html.Translate("ThisPerformerGivesAccessToHisCalendarAndSeemsToBeAvailableThis") %>
<%= available.PerformanceDate.ToString("D") %>.</p>
<% } else if (available.Profile.HasCalendar()) { %><p>
<%= Html.Translate("ThisPerformerGivesAccessToHisCalendarAndItAppearsHeShouldNotBeAvailableThis") %>
<%= available.PerformanceDate.ToString("D") %>.</p>
<% } else {%><p>
<%= Html.Translate("ThisPerformerDoesntGiveAccessToHisCalendar") %></p>
<% } %>

<% if (User.Identity!=null && User.Identity.IsAuthenticated) { %>

<form data-ajax-action="Basket/Create" method="post" >
<input type="hidden" name="productref" value="main">
<input type="hidden" name="clientname" value="<%= ViewBag.ClientName %>">
<input type="hidden" name="type" value="Yavsc.Model.FrontOffice.NominativeSimpleBookingQuery">
<input type="hidden" name="MEACode" value="<%=ViewBag.SimpleBookingQuery.MEACode%>" >
<input type="hidden" name="Need" value="<%=ViewBag.SimpleBookingQuery.Need%>" >
<input type="hidden" name="PreferedDate" value="<%=ViewBag.SimpleBookingQuery.PreferedDate%>" >
<input type="hidden" name="PerformerName" value="<%=available.Profile.UserName%>" >
<input type="submit" name="submit" class="actionlink" id="btnAskFAE" value="<%=Html.Translate("AskForAnEstimate")%>">
</form>
<div id="AskForAnEstimateResult"></div>

<% } else { %>
<p><%= Html.Translate("YouNeedToBeAuthenticatedIOToContact") %></p>
<p>
<a href="<%= Url.RouteUrl("Default", new { controller = "Account", action = "Login", returnUrl=Request.Url.PathAndQuery}) %>" class="link" accesskey = "C">
<i class="fa fa-sign-in"></i> Connexion
</a></p>

<% } %>

</div>
</div>
<% } %>
<script>
function receiveCmdRef(response) {
var msg = 'Votre commande a été enregistrée sous le numéro '+
 response.CommandId+' <br>'+
  (response.NotifiedOnMobile?'Une notification a été poussée sur le mobile du préstataire.<br>':'')+
  (response.EmailSent?'Un e-mail lui a été envoyé.':'');
 Yavsc.notice (msg); 
	}
$(document).ready(function(){
   $('form').submit (
   function(e){ 
    e.preventDefault();
    var jstxta = [];
    var action = $(this).data("ajax-action");
    $(this).children('input').each(function(i,elt){
    	jstxta.push ('"'+elt.name+'":'+'"'+elt.value+'"') ;
    });
    var jstxt = "{"+jstxta.join(', ')+"}";
    var data = JSON.parse(jstxt);
    Yavsc.ajax(action,data,receiveCmdRef);
   });
   $("[data-type='user']").user({circles:<%=Ajax.JSonString(ViewData["Circles"])%>});
});
</script>
</asp:Content>
