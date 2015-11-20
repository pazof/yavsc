<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" Title="UsersInRoleOf" MasterPageFile="~/Models/AppAdmin.master"  %>
<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% ViewState["orgtitle"] =  string.Format ( LocalizedText.UsersInRole , Html.Encode(ViewData["RoleName"])); %>
<% Page.Title = ViewState["orgtitle"] + " - " + YavscHelpers.SiteName; %>
</asp:Content>
<asp:Content ContentPlaceHolderID="head" ID="head1" runat="server">
<script src="<%=Url.Content("~/Scripts/yavsc.admin.js")%>"></script>
<script>
 $(document).ready(function(){ 
  $('[data-type="userlist"]').children().each(function() { 
    $(this).decharger({roleName: <%= YavscAjaxHelper.QuoteJavascriptString((string)ViewData["RoleName"]) %>});
  });
 });
 </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="overHeaderOne" ID="header1" runat="server">
<h1><a href="<%= Url.RouteUrl("Default", new { action = "UsersInRole", RoleName = ViewData["RoleName"] } )%>">
<%=ViewState["orgtitle"]%></a>
- <a href="<%= Url.RouteUrl("Default",new {controller="Home" }) %>"><%= YavscHelpers.SiteName %></a>
</h1>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<ul class="editablelist" data-role="<%=ViewData["RoleName"]%>" data-type="userlist">

 <%foreach (string username in (string[]) ViewData["UsersInRole"]){ %>
 <li><%= username %></li>
 <% } %>
 </ul>
<%= Html.Partial("AddUserToRole") %>
</asp:Content>