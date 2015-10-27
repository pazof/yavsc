<%@ Page Title="Liste des rôles" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/AppAdmin.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
Roles:
 <ul>
       <%foreach (string rolename in (string[]) Model){ %>

       <li><%=rolename%> <% if (Roles.IsUserInRole("Admin")) { %>
	 <%= Html.ActionLink("Supprimer","RemoveRole", new { rolename = rolename }, new { @class="actionlink" } ) %>	
<% } %></li>
	<% } %>
	
	</ul>
	<% if (Roles.IsUserInRole("Admin")) { %>
	 <%= Html.ActionLink("Ajouter un rôle","AddRole", null, new { @class="actionlink" } ) %>	
<% } %>
</asp:Content>


