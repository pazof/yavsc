<%@ Page Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<Yavsc.FileInfoCollection>" %>
<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="headerContent" ContentPlaceHolderID="header" runat="server">
</asp:Content>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<h1> Index of <%=ViewData["UserName"] %>'s files  (<%= Html.Encode(Model.Count) %>) </h1>
<ul>
<% foreach (System.IO.FileInfo fi in Model) { %>
<li>	<%= Html.ActionLink(fi.Name,"Details",new {id = fi.Name}) %> </li>
<% } %>
</ul>

<%= Html.ActionLink("Ajouter des fichiers","Create") %>
</asp:Content>
