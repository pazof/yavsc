﻿<%@ Page Title="Login" Language="C#" Inherits="System.Web.Mvc.ViewPage<LoginModel>" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<div>
<%= Html.ValidationSummary("Ouverture de session") %>
<% using(Html.BeginForm("DoLogin", "Account")) %>
<% { %>
<%= Html.LabelFor(model => model.UserName) %>
<%= Html.TextBox( "UserName" ) %>
<%= Html.ValidationMessage("UserName", "*") %><br/>

<%= Html.LabelFor(model => model.Password) %>
<%= Html.Password( "Password" ) %>
<%= Html.ValidationMessage("Password", "*") %><br/>

<label for="RememberMe">Se souvenir du mot de passe:</label>
<%= Html.CheckBox("RememberMe") %>
<%= Html.ValidationMessage("RememberMe", "") %><br/>
<%= Html.Hidden("returnUrl",ViewData["returnUrl"]) %>
<!-- Html.AntiForgeryToken() -->
<input type="submit"/>
<% } %>
</div>
<div>
<%= Html.ActionLink("S'enregistrer","Register",new {returnUrl=ViewData["returnUrl"]}) %></div>
</asp:Content>