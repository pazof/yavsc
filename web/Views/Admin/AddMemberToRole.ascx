<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<%= Html.ValidationSummary() %>
<% using(Ajax.BeginForm()) 
 { %>
Nom du rôle : 
<%= Html.TextBox( "RoleName" ) %>
<%= Html.ValidationMessage("RoleName", "*") %><br/>
<input class="actionlink" type="submit"/>
<% } %>

