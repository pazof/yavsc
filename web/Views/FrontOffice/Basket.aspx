<%@ Page Title="Basket" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<CommandSet>" %>


<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = Title +" "+ Model.Count+" article(s)"; %>
</asp:Content>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">

<% if (Model.Count>0) { %>
<ul>
<% foreach (Command cmd in Model.Values) { %>
	<li><div>
	<%= cmd.Id %>/<%= cmd.CreationDate %>/<%= cmd.Status %>
	<%= cmd.ProductRef %></div>
	<div>
	<%= Html.Partial(cmd.GetType().Name,cmd) %>
	</div>
	</li>
<% } %>
	</ul>
<% } %>
 <ul><li>
 <%= Html.TranslatedActionLink("Catalog","Catalog" ) %>
 </li><li>
 <%= Html.TranslatedActionLink("Estimates","Estimates" ) %>
 </li></ul>
</asp:Content>