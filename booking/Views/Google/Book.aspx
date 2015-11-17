<%@ Page Title="Booking" Language="C#" Inherits="System.Web.Mvc.ViewPage<BookQuery>" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">
 <link rel="stylesheet" type="text/css" href="/App_Themes/jquery.timepicker.css" />
 <script type="text/javascript" src="/Scripts/globalize/globalize.js"></script>
 <script type="text/javascript" src="/Scripts/jquery.mousewheel.js"></script>
 <script type="text/javascript" src="/Scripts/globalize/cultures/globalize.culture.fr.js"></script>
 <script type="text/javascript" src="/Scripts/datepicker-fr.js"></script>
 <script type="text/javascript" src="/Scripts/datepicker-en-GB.js"></script>
 <script type="text/javascript" src="/Scripts/jquery.timepicker.min.js"></script>
 <script type="text/javascript" src="/Scripts/datepair.js"></script>
 <script type="text/javascript" src="/Scripts/jquery.datepair.min.js"></script>
</asp:Content>

 <asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<% using ( Html.BeginForm("Book","Google") ) { %>
<div id="book" >Date d'intervention :
Intervention souhaitée entre le 
  <input type="text" id="StartDate" name="StartDate" class="start date" value="<%=Model.StartDate.ToString("yyyy/MM/dd")%>">
  <%= Html.ValidationMessageFor( model=>model.StartDate ) %>
et le 
  <input type="text" id="EndDate" name="EndDate" class="end date" value="<%=Model.StartDate.ToString("yyyy/MM/dd")%>">
  <%= Html.ValidationMessageFor(model=>model.EndDate) %>
 <br>
 Heure et durée d'intervention souhaitée
<%= Html.LabelFor(model=>model.StartHour) %>
 <input type="text" id="StartHour" name="StartHour" class="start time" value="<%=Model.StartHour%>">
  <%= Html.ValidationMessageFor(model=>model.StartHour) %>

<%= Html.LabelFor(model=>model.EndHour) %>
  <input type="text" id="EndHour" name="EndHour" class="end time" value="<%=Model.EndHour%>">
  <%= Html.ValidationMessageFor(model=>model.EndHour) %>
  </div>
  <fieldset>
<legend>Intervenant</legend>
  <%= Html.LabelFor(model=>model.Role) %>:
  <%= Html.TextBoxFor(model=>model.Role) %>
  <%= Html.ValidationMessageFor(model=>model.Role) %>
 <br>
  <%= Html.LabelFor(model=>model.Person) %>:
  <%= Html.TextBoxFor(model=>model.Person) %>
  <%= Html.ValidationMessageFor(model=>model.Person) %>
 </fieldset>
  <script>
  $(document).ready(function(){
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

   $('#book .time').timepicker(tpconfig);
   $('#book .date').datepicker(dpconfig);
   $('#book').datepair();
   });
</script>

<input type="submit">
<% } %>
</asp:Content>
