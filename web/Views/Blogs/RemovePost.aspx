﻿<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntry>" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<%= Html.ValidationSummary() %>
<% using (Html.BeginForm("RemovePost","Blogs")) { %>

<%= Html.LabelFor(model => model.Title) %> :
<%= Html.TextBox( "Title" ) %>
<%= Html.ValidationMessage("Title", "*") %><br/>
Identifiant du post : <%= ViewData["pid"] %><br/>
<span class="preview"><%= Html.Markdown(Model.Content) %></span>
<label for="confirm">supprimer le billet</label>
<%= Html.CheckBox( "confirm" ) %>
<%= Html.ValidationMessage("AgreeToRemove", "*") %>
<%= Html.Hidden("pid") %>
<%= Html.Hidden("returnUrl") %>
 <input type="submit"/>
 <% } %>

</asp:Content>


