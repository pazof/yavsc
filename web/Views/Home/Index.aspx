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
<div>
<div data-type="background" data-speed="10" style="width:100%; height:10em; background: url(<%=Ajax.JString(a.Photo)%>) 50% 50% repeat fixed;" >
</div>
<h1><a href="<%= Url.RouteUrl("Default", new { controller="FrontOffice", action="Booking", id=a.Id }) %>"><%=Html.Encode(a.Title)%></a></h1>
<i>(<%=Html.Encode(a.Id)%>)</i>
<p>
<%=Html.Markdown(a.Comment)%>
</p></div>
<% } %>
</div>
<%= Html.Partial("TagPanel",ViewData["Accueil"]) %>
</asp:Content>

