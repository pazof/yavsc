<%@ Page Title="Booking" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<SimpleBookingQuery>" %>
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
<% using ( Html.BeginForm("Booking") ) { %>
<%= Html.Hidden("MAECode") %>
  <fieldset>
<legend>Préferences musicales</legend>
  <%= Html.LabelFor(model=>model.Needs) %>:
  <ul>
  <% foreach (var need in Model.Needs) { %>
  <li><%= need.Name %> <%= Html.Partial("RateSkillControl",need)%></li>
  <% } %>
  </ul>
  <%= Html.ValidationMessageFor(model=>model.Needs) %>
 </fieldset>

  <fieldset>
<legend>Date de l'événement</legend>
Intervention souhaitée le 
  <input type="text" id="PreferedDate" name="PreferedDate" class="start date" value="<%=Model.PreferedDate.ToString("yyyy/MM/dd")%>">
  <%= Html.ValidationMessageFor( model=>model.PreferedDate ) %>
 </fieldset>
  <script>
  $(document).ready(function(){
  $('[data-type="rate-site-skill"]').rate({target: 'FrontOffice/RateSkill'});
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

