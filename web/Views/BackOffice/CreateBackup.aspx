<%@ Page Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<DataAccess>" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<%= Html.ValidationSummary("CreateBackup") %>
<% using (Html.BeginForm("CreateBackup","BackOffice")) { %>

<%= Html.LabelFor(model => model.Host) %>:
<%= Html.TextBox( "Host" ) %>
<%= Html.ValidationMessage("Host", "*") %><br/>
<%= Html.LabelFor(model => model.Port) %>:
<%= Html.TextBox( "Port" ) %>
<%= Html.ValidationMessage("Port", "*") %><br/>
<%= Html.LabelFor(model => model.Dbname) %>:
<%= Html.TextBox( "Dbname" ) %>
<%= Html.ValidationMessage("Dbname", "*") %><br/>
<%= Html.LabelFor(model => model.Dbuser) %>:
<%= Html.TextBox( "Dbuser" ) %>
<%= Html.ValidationMessage("Dbuser", "*") %><br/>
<%= Html.LabelFor(model => model.Password) %>:
<%= Html.Password( "Password" ) %>
<%= Html.ValidationMessage("Password", "*") %><br/>
<input type="submit"/>
<% } %>
</asp:Content>
