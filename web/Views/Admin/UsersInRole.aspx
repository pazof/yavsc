<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" Title="UsersInRoleOf" MasterPageFile="~/Models/AppAdmin.master"  %>
<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% ViewState["orgtitle"] =  string.Format ( LocalizedText.UsersInRole , Html.Encode(ViewData["RoleName"])); %>
<% Page.Title = ViewState["orgtitle"] + " - " + YavscHelpers.SiteName; %>

</asp:Content>
<asp:Content ContentPlaceHolderID="overHeaderOne" ID="header1" runat="server">
<h1><a href="<%= Url.RouteUrl("Default", new { action = "UsersInRole", RoleName = ViewData["RoleName"] } )%>">
<%=ViewState["orgtitle"]%></a>
- <a href="<%= Url.RouteUrl("Default",new {controller="Home" }) %>"><%= YavscHelpers.SiteName %></a>
</h1>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<ul class="editablelist userlist">
 <%foreach (string username in (string[]) ViewData["UsersInRole"]){ %>
 <li><%= username %></li>
 <% } %>
 </ul>
<%= Html.Partial("AddUserToRole") %>
</asp:Content>