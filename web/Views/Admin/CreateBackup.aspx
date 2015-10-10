<%@ Page Title="Backup creation" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<DataAccess>" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<%= Html.ValidationSummary("CreateBackup","Admin") %>
<% using (Html.BeginForm("CreateBackup")) { %>

<%= Html.LabelFor(model => model.Host) %>:
<%= Html.TextBox( "Host" ) %>
<%= Html.ValidationMessage("Host", "*") %><br/>
<%= Html.LabelFor(model => model.Port) %>:
<%= Html.TextBox( "Port" ) %>
<%= Html.ValidationMessage("Port", "*") %><br/>
<%= Html.LabelFor(model => model.DbName) %>:
<%= Html.TextBox( "DbName" ) %>
<%= Html.ValidationMessage("DbName", "*") %><br/>
<%= Html.LabelFor(model => model.DbUser) %>:
<%= Html.TextBox( "DbUser" ) %>
<%= Html.ValidationMessage("DbUser", "*") %><br/>
<%= Html.LabelFor(model => model.Password) %>:
<%= Html.Password( "Password" ) %>
<%= Html.ValidationMessage("Password", "*") %><br/>
<input type="submit"/>
<% } %>
</asp:Content>
