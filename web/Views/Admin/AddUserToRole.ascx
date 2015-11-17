<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%= Html.ValidationSummary() %>
<% using(Html.BeginForm("AddUserToRole", "Admin")) 
 { %>
 <fieldset>
 <div id="roleaddedresult"></div>
 <% if (ViewData ["UserName"] != null) { %>
<%= Html.Hidden("RoleName",ViewData ["RoleName"]) %>
<% } else { %>
<div>
<label for="UserName" >Utilisateur : </label><input type="text" name="UserName" id="UserName">
<%= Html.ValidationMessage("UserName", "*") %>
</div>
<% } %>

<% if (ViewData ["RoleName"] != null) { %>
<%= Html.Hidden("RoleName",ViewData ["RoleName"]) %>
<% } else { %>
<div>
<label for="RoleName" >Nom du rôle : </label>
<input type="text" name="RoleName" id="RoleName">
<%= Html.ValidationMessage("RoleName", "*") %>
</div>
<% } %>

<%= Html.Hidden("ReturnUrl", Request.Url.PathAndQuery ) %>
<input type="submit">
 </fieldset>
<% } %>