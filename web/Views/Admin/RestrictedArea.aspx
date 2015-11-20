<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntryCollection>" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
	Ce contenu est d'accès restreint :<br>
	 &lt;<%= Html.Encode(ViewData["ControllerName"]) %>/<%= Html.Encode(ViewData["ActionName"]) %>&gt;<br>

	 <% if (Roles.IsUserInRole("Admin")) { %>
	 <i>Admin only:</i>
	Ci après les tièrces parties autorisée actuellement :<br>

	Roles autorisés : <%= Html.Encode(ViewData["Roles"]) %><br>
	Utilisateurs autorisés :<%= Html.Encode(ViewData["Users"]) %><br>
	<% } %>
</asp:Content>