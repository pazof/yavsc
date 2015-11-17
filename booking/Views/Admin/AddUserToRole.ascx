<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%= Html.ValidationSummary() %>
<% using(Ajax.BeginForm("AddUserToRole", "Admin", new AjaxOptions() { UpdateTargetId = "roleaddedresult" })) 
 { %>
 <fieldset>
 <div id="roleaddedresult"></div>
<label for="UserName" >Utilisateur : </label><input type="text" name="UserName" id="UserName">
<%= Html.ValidationMessage("UserName", "*") %><br/>
<label for="RoleName" >Nom du rôle : </label>
<input type="text" name="RoleName" id="RoleName">
<%= Html.ValidationMessage("RoleName", "*") %><br/>
<%= Ajax.ActionLink("AddUserToRole", "AddUserToRole", new { RoleName = "Admin" } , new AjaxOptions() { UpdateTargetId = "roleaddedresult" }) %>
 </fieldset>
<% } %>