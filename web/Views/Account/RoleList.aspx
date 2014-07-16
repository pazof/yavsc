<%@ Page Title="Role list" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">
</asp:Content>
<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">	
<h2>Liste des rôles</h2>
</asp:Content>
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


