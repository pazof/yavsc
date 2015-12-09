<%@ Page Title="Liste des administrateurs" Language="C#" Inherits="System.Web.Mvc.ViewPage<NewAdminModel>" MasterPageFile="~/Models/AppAdmin.master" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div>

<table>
<% foreach (string u in (string[])ViewData["admins"]) { %>
<tr><td>
<%= u %> </td><td>
<%= Html.TranslatedActionLink("Remove","RemoveFromRole",
new { username = u, rolename="Admin", returnUrl = Request.Url.PathAndQuery })%>

</td></tr>

<% } %>
</table>
</div>
<div>
<h2>Ajout d'un administrateur
</h2>
<p><%= Html.ValidationSummary() %> </p>

<% using ( Html.BeginForm("Admin", "Admin") ) {  %>

<%= Html.LabelFor(model => model.UserName) %> : 
<%= Html.DropDownListFor(model => model.UserName, (List<SelectListItem>)ViewData["useritems"] ) %>
<%= Html.ValidationMessage("UserName", "*") %>

<input type="submit"/>
     <% } %>
  
     </div>
</asp:Content>

