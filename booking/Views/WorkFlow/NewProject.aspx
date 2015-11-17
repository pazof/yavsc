<%@ Page Title="Nouveau projet" Language="C#" Inherits="System.Web.Mvc.ViewPage<WorkFlow.NewProjectModel>" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div>
<%= Html.ValidationSummary("Nouveau projet") %>
<% using ( Html.BeginForm("NewProject", "WorkFlow") ) {  %>
<%= Html.LabelFor(model => model.Name) %> : 
<%= Html.TextBox( "Name" ) %>
<%= Html.ValidationMessage("Name", "*") %><br/>
<%= Html.LabelFor(model => model.Manager) %> : 
<%= Html.TextBox( "Manager" ) %>
<%= Html.ValidationMessage("Manager", "*") %><br/>
<%= Html.LabelFor(model => model.Description) %> : 
<%= Html.TextBox( "Description" ) %>
<%= Html.ValidationMessage("Description", "*") %><br/>
<input type="submit"/>
<% } %>
</div>
</asp:Content>


