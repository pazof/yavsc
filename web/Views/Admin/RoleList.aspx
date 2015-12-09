<%@ Page Title="Liste des rôles" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/AppAdmin.master" %>


<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
Roles:
 <ul>
       <%foreach (string rolename in (string[]) Model){ %>

       <li><%=Html.TranslatedActionLink(rolename,"UsersInRole", new { rolename = rolename }, new { @class="actionlink" } )%> <% if (Roles.IsUserInRole("Admin")) { %>
	 <%= Html.TranslatedActionLink("Supprimer","RemoveRole", new { rolename = rolename }, new { @class="actionlink" } ) %>	
<% } %></li>
	<% } %>
	
	</ul>
	<% if (Roles.IsUserInRole("Admin")) { %>
	 <%= Html.TranslatedActionLink("Ajouter un rôle","AddRole", null, new { @class="actionlink" } ) %>	
<% } %>
</asp:Content>


