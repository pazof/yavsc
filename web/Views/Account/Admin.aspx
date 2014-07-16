<%@ Page Title="Administration" Language="C#" Inherits="System.Web.Mvc.ViewPage<NewAdminModel>" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">	
<h2>Liste des administrateurs </h2>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div>

<table>
<% foreach (string u in (string[])ViewData["admins"]) { %>
<tr><td>
<%= u %> </td><td><%= Html.ActionLink("Remove","RemoveFromRole",new { username = u, rolename="Admin", returnUrl = Request.Url.PathAndQuery })%>
</td></tr>

<% } %>
</table>
</div>
<div>
<h2>Ajout d'un administrateur
</h2>
<p><%= Html.ValidationSummary() %> </p>

<% using ( Html.BeginForm("Admin", "Account") ) {  %>

<%= Html.LabelFor(model => model.UserName) %> : 
<%= Html.DropDownListFor(model => model.UserName, (List<SelectListItem>)ViewData["useritems"] ) %>
<%= Html.ValidationMessage("UserName", "*") %>

<input type="submit"/>
     <% } %>
  
     </div>
</asp:Content>

