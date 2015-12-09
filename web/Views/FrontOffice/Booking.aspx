<%@ Page Title="Booking" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<SimpleBookingQuery>" %>

<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = Html.Encode(ViewBag.Activity.Title) + " - " + YavscHelpers.SiteName; %>
</asp:Content>

<asp:Content ContentPlaceHolderID="overHeaderOne" ID="header1" runat="server">
<h1><%=Html.Encode(ViewBag.Activity.Title) %> - <a href="<%= Url.RouteUrl("Default",new {controller="Home" }) %>"><%= YavscHelpers.SiteName %></a>
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
<%= Html.Translate("Si vous le voulez, vous pouvez détailler ici votre demande, en matière de compétences attendues:") %>
<div>
  <input type="hidden" name="Need" id="Need" value="">
  <ul >
  <% if (ViewData ["Needs"]!=null)
   foreach (var need in (SkillEntity[])(ViewData ["Needs"])) { %>
  <li class="skillname"><%= need.Name %> <%= Html.Partial("RateSkillControl", need)%></li>
  <% } %>
  </ul>
  <%= Html.ValidationMessageFor(model=>model.Need) %>
  </div>
 </fieldset>
  <fieldset>
<legend><%= Html.Translate("PerformanceDate") %></legend>
<%= Html.Translate("Indiquez ici la date souhaitée pour la prestation.") %>
<div>
Intervention souhaitée le : <input type="text" id="PreferedDate" name="PreferedDate" class="start date" style="z-index: 99 !important;" value="<%=Model.PreferedDate.ToString("yyyy/MM/dd")%>">
  <%= Html.ValidationMessageFor( model=>model.PreferedDate ) %>
   </div>
 </fieldset>
  <script>
  $(document).ready(function(){
  var needs = <%= Ajax.JSonString((SkillEntity[])(ViewData ["Needs"])) %>;
  var fneeds = [];
  needs.forEach(function(elt) { fneeds.push (''+elt.Id+' '+elt.Rate); } );
  if (needs.length>0) $('#Need').val(fneeds); else $('#Need').val('none');
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
   $('#PreferedDate').datepicker(dpconfig).zIndex(4);
 
   });
</script>

<input type="submit" value="<%=Html.Translate("Search")%>" class="fullwidth actionlink">
<% } %>
</asp:Content>

