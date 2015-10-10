<%@ Page Title="Init db" Language="C#" MasterPageFile="~/Models/NoLogin.master" Inherits="System.Web.Mvc.ViewPage<DataAccess>" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<%= Html.ValidationSummary("Init a new data base") %>
<% using (Html.BeginForm("InitDb","Admin")) { %>
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
<label for="doInit">Executer le script de création de la base: </label><input type="checkbox" name="doInit" id="doInit" >
<input type="submit"/>
<% } %>
</asp:Content>
