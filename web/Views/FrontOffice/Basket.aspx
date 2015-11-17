<%@ Page Title="Basket" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<CommandSet>" %>


<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = Title +" "+ Model.Count+" article(s)"; %>
</asp:Content>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">

<% if (Model.Count>0) { %>
<ul>
<% foreach (Command cmd in Model.Values) { %>
	<li>
	<%= cmd.Id %>
	<%= cmd.CreationDate %>

	<%= cmd.Status %>
	<%= cmd.ProductRef %>
	<ul>
	<% if (cmd.Parameters!=null)
	foreach (string key in cmd.Parameters.Keys) { %>
		<li><%=key%>: <%=cmd.Parameters[key]%></li>
	<% } %>
	</ul>
	</li>
<% } %>
	</ul>
<% } %>


 <ul><li>
 <%= Html.ActionLink("Catalog","Catalog" ) %>
 </li><li>
 <%= Html.ActionLink("Estimates","Estimates" ) %>
 </li></ul>
</asp:Content>