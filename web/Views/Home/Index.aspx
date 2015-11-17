<%@ Page Title="Home" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = YavscHelpers.SiteName ; %>
</asp:Content>

<asp:Content ContentPlaceHolderID="overHeaderOne" ID="header1" runat="server">
<h1><%=Html.Encode(Page.Title)%></h1>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<%= Html.Partial("TagPanel",ViewData["Accueil"]) %>
<%= Html.Partial("TagPanel",ViewData["Yavsc"]) %>
<%= Html.Partial("TagPanel",ViewData["Événements"]) %>
<%= Html.Partial("TagPanel",ViewData["Mentions légales"]) %>
</asp:Content>

