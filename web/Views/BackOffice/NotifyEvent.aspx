<%@ Page Title="Notify" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<EventPub>" %>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<% using( Html.BeginForm("NotifyEvent")) { %>

<%= Html.ValidationSummary() %>
<%= Html.LabelFor(model => model.Title) %>
<%= Html.TextBox("Title") %>
<%= Html.ValidationMessage("Title", "*") %><br>

<%= Html.LabelFor(model => model.Description) %>
<%= Html.TextArea("Description") %>
<%= Html.ValidationMessage("Description", "*") %><br>

<%= Html.LabelFor(model => model.CircleIds) %>
<%= Html.ListBox("CircleIds") %>
<%= Html.ValidationMessage("CircleIds", "*") %>

<input type="submit">
<% } %>

</asp:Content>
