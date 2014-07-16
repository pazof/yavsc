<%@ Page Title="Profile" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<yavscModel.RolesAndMembers.Profile>" %>
<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
<title><%=ViewData["UserName"]%> : Profile - <%=YavscHelpers.SiteName%></title>
</asp:Content>
<asp:Content ID="headerContent" ContentPlaceHolderID="header" runat="server">
<h1><%=ViewData["UserName"]%> : Profile - <a href="/"><%=YavscHelpers.SiteName%></a></h1>
<p><%= Html.ActionLink("Changer de mot de passe","ChangePassword", "Account")%></p>
</asp:Content>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<%= Html.ValidationSummary() %>
<% using(Html.BeginForm("UpdateProfile", "Account", FormMethod.Post, new { enctype = "multipart/form-data" })) %>
<% { %>

<table class="layout">
<tr><td align="right">
<%= Html.LabelFor(model => model.Address) %></td><td>
<%= Html.TextBox("Address") %>
<%= Html.ValidationMessage("Address", "*") %></td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.CityAndState) %></td><td>
<%= Html.TextBox("CityAndState") %>
<%= Html.ValidationMessage("CityAndState", "*") %></td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.Country) %></td><td>
<%= Html.TextBox("Country") %>
<%= Html.ValidationMessage("Country", "*") %></td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.WebSite) %></td><td>
<%= Html.TextBox("WebSite") %>
<%= Html.ValidationMessage("WebSite", "*") %></td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.BlogVisible) %></td><td>
<%= Html.CheckBox("BlogVisible") %>
<%= Html.ValidationMessage("BlogVisible", "*") %></td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.BlogTitle) %></td><td>
<%= Html.TextBox("BlogTitle") %>
<%= Html.ValidationMessage("BlogTitle", "*") %></td></tr>
<tr><td align="right">
Avatar   </td><td> <img class="avatar" src="/Blogs/Avatar?user=<%=ViewData["UserName"]%>" alt=""/>
<input type="file" id="AvatarFile" name="AvatarFile"/>
<%= Html.ValidationMessage("AvatarFile", "*") %></td></tr>

</table>
<input type="submit"/>
<% } %>
</asp:Content>
