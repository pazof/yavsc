<%@ Page Title="Restore" Language="C#" MasterPageFile="~/Models/AppAdmin.master" Inherits="System.Web.Mvc.ViewPage<DataAccess>" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<%= Html.ValidationSummary("Restore a database backup") %>
<% using (Html.BeginForm("Restore","Admin")) { %>

<% string [] bcfiles = (string[]) ViewData["Backups"]; %>
<select name="backupName">
<% foreach (string s in bcfiles)
{
%>
<option value="<%=s%>"><%=s%></option>
<%
}
%>
</select>
<label for="dataOnly">Data only :</label> 
<%= Html.CheckBox("dataOnly")%>

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
