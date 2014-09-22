﻿<%@ Page Title="Register" Language="C#" Inherits="Yavsc.RegisterPage" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">	

</asp:Content>
<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">	
<h2> Créez votre compte utilisateur <%= Html.Encode(YavscHelpers.SiteName) %> </h2>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<%= Html.ValidationSummary() %>
<% using(Html.BeginForm("Register", "Account")) %>
<% { %>
<table class="layout">
<tr><td align="right">
<%= Html.LabelFor(model => model.UserName) %>
</td><td>
<%= Html.TextBox( "UserName" ) %>
<%= Html.ValidationMessage("UserName", "*") %></td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.Password) %>
</td><td>
<%= Html.Password( "Password" ) %>
<%= Html.ValidationMessage("Password", "*") %>
</td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.ConfirmPassword) %>
</td><td>
   <%= Html.Password( "ConfirmPassword" ) %>
<%= Html.ValidationMessage("ConfirmPassword", "*") %>
</td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.Email) %>
</td><td>
<%= Html.TextBox( "Email" ) %>
<%= Html.ValidationMessage("Email", "*") %>
</td></tr>
</table>
<br/>
<%= Html.Hidden("returnUrl",ViewData["returnUrl"]) %>

<input type="submit"/>
<% } %>
</asp:Content>

