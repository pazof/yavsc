<%@ Page Title="Basket" Language="C#"  Inherits="System.Web.Mvc.ViewPage<Basket>" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = Title +" "+ Model.Count+" article(s)"; %>
</asp:Content>


<asp:Content ContentPlaceHolderID="MainContent" ID="mainContent" runat="server">

<ul>
<% foreach (Commande cmd in Model.Values) { %>
	<li>
	<%= cmd.Id %>
	<%= cmd.CreationDate %>

	<%= cmd.Status %>
	<%= cmd.ProductRef %>
	<ul>
	<% foreach (string key in cmd.Parameters.Keys) { %>
		<li><%=key%>: <%=cmd.Parameters[key]%></li>
	<% } %>
	</ul>
	</li>
<% } %>
	</ul>
</asp:Content>
