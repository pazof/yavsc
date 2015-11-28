<%@ Page Title="Home" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = YavscHelpers.SiteName ; %>
</asp:Content>

<asp:Content ContentPlaceHolderID="overHeaderOne" ID="header1" runat="server">
<h1><%=Html.Encode(Page.Title)%></h1>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div id="activities">
<% foreach (var a in ((Activity[])(ViewData["Activities"]))) { %>
<%= Html.Partial("Activity",a) %>
<% } %>
</div>
</asp:Content>

