<%@ Page Title="Booking" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<SimpleBookingQuery>" %>

<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = Html.Translate("BookingTitle"+Model.MEACode) + " - " + YavscHelpers.SiteName; %>
</asp:Content>

<asp:Content ContentPlaceHolderID="overHeaderOne" ID="header1" runat="server">
<h1>
<a href="<%=Url.RouteUrl("FrontOffice",new {action="Booking", MEACode=Model.MEACode })%>">
<img href="<%= ViewData["Photo"] %>" alt="">
<%=Html.Translate("BookingTitle"+Model.MEACode)%>
</a>
- <a href="<%= Url.RouteUrl("Default",new {controller="Home" }) %>"><%= YavscHelpers.SiteName %></a>
</h1>
</asp:Content>

<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">
 <link rel="stylesheet" type="text/css" href="/App_Themes/jquery.timepicker.css" />
 <script type="text/javascript" src="/Scripts/globalize/globalize.js"></script>
 <script type="text/javascript" src="/Scripts/jquery.mousewheel.js"></script>
 <script type="text/javascript" src="/Scripts/globalize/cultures/globalize.culture.fr.js"></script>
 <script type="text/javascript" src="/Scripts/datepicker-fr.js"></script>
 <script type="text/javascript" src="/Scripts/datepicker-en-GB.js"></script>
 <script type="text/javascript" src="/Scripts/jquery.timepicker.min.js"></script>
</asp:Content>

 <asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<% using ( Html.BeginForm( "Booking", "FrontOffice", new { MEACode = Model.MEACode }) ) { %>
<%= Html.ValidationSummary() %>
<%= Html.Hidden("MEACode") %>
  <fieldset>
<legend><%= Html.Translate("YourNeed") %></legend>
  <input type="hidden" name="Needs" id="Needs" value="">
  <ul>
  <% foreach (var need in (SkillEntity[])(ViewData["Needs"])) { %>
  <li><%= need.Name %> <%= Html.Partial("RateSkillControl", need)%></li>
  <% } %>
  </ul>
  <%= Html.ValidationMessageFor(model=>model.Needs) %>
 </fieldset>
  <fieldset>
<legend><%= Html.Translate("PerformanceDate") %></legend>
Intervention souhaitée le 
  <input type="text" id="PreferedDate" name="PreferedDate" class="start date" value="<%=Model.PreferedDate.ToString("yyyy/MM/dd")%>">
  <%= Html.ValidationMessageFor( model=>model.PreferedDate ) %>
 </fieldset>
  <script>
  $(document).ready(function(){
  var needs = <%= Ajax.JSonString((SkillEntity[])(ViewData["Needs"])) %>;
  var fneeds = needs.map( function (need) { 
    return need.Id+' '+need.Rate; } );

    fneeds.forEach(function(elt) { console.log(Yavsc.dumpprops(elt)) } );
  $('#Needs').val(fneeds);
  $('[data-type="rate-site-skill"]').rate({jsTarget: function (rating)
  {
  // console.log(Yavsc.dumpprops(rating)); 
  return true;
  }
  });
  var tpconfig = { 
  'timeFormat': 'H:i',
  'showDuration': true,
  'disableTimeRanges': [
        ['17:01pm', '24:01pm'],
        ['0am', '9am']
   ]};
   $.datepicker.setDefaults($.datepicker.regional[ "fr" ] );
   var dpconfig = {  
   'format': 'yy/mm/dd',
   'autoclose': true } ;

   // $('#PreferedHour').timepicker(tpconfig);
   $('#PreferedDate').datepicker(dpconfig);
 
   });
</script>

<input type="submit">
<% } %>
</asp:Content>

