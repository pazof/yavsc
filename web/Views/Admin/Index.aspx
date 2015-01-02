<%@ Page Title="Admin" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<div class="panel">

<ul><li>
<%= Html.ActionLink("Backups","Backups") %>
</li>
<li><%= Html.ActionLink("Restaurations", "Restore") %></li>
<li><%= Html.ActionLink("Create backup","CreateBackup") %></li>
<li><%= Html.ActionLink("Remove user", "RemoveUser") %></li>
<li><%= Html.ActionLink("Remove role", "RemoveRoleQuery") %></li>
<li><%= Html.ActionLink("User list", "UserList") %></li>
<li><%= Html.ActionLink("Role list", "RoleList") %></li>
</ul>
</div>

</asp:Content>

