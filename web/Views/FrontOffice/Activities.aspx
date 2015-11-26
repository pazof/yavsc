<%@ Page Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Activity>>" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">

<div id="activities">
<% foreach (var a in Model) { %>
<%= Html.Partial("Activity",a) %>
<% } %>
<aside class="control">
<form method="post" action="DeclareActivity">
<fieldset>
<input type="text" name="meacode" id="meacode" >
<input type="text" name="activityname" id="activityname" >
<input type="text" name="comment" id="comment" >
<input type="button" value="Créer l'activité" id="btncreate" >
</fieldset>
</form>
<script type="text/javascript">
 $(document).ready(function () {
 $('#btncreate').click( function() {
 var activity = { 
 	Title: $('#activityname').val(),
  	Id: $('#meacode').val(),
   	Comment: $('#comment').val(),
    };
 Yavsc.ajax( 'FrontOffice/RegisterActivity', activity, 
    function() {
 var na = $('<p></p>').addClass('activity');
 $.get('FrontOffice/Activity/'+activity.meacode, function(data) {
    na.replaceWith(data);
 	$('#activityname').val('');
 	$('#meacode').val('');
 	$('#comment').val('');
 	na.appendTo('#activities');
    });
 }); } ); });
</script>
</aside>
</div>
</asp:Content>
