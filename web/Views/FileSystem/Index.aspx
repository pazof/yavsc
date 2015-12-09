<%@ Page Title="Files" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<System.IO.FileInfo[]>" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<h1> Index 
 </h1>
<ul>
<% foreach (System.IO.FileInfo fi in Model) { %>
<li>	<%= Html.TranslatedActionLink(fi.Name,"Details",new {id = fi.Name}) %> </li>
<% } %>
</ul>

<%= Html.TranslatedActionLink("Ajouter des fichiers","Create") %>
</asp:Content>
