<%@ Page Title="Login" Language="C#" Inherits="System.Web.Mvc.ViewPage<LoginModel>" MasterPageFile="~/Models/NoLogin.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div class="panel">
<%= Html.ValidationSummary("Ouverture de session") %>
<% using(Html.BeginForm("Login", "Account")) %>
<% { %>
<%= Html.LabelFor(model => model.UserName) %>
<%= Html.TextBox( "UserName" ) %>
<%= Html.ValidationMessage("UserName", "*") %><br/>

<%= Html.LabelFor(model => model.Password) %>
<%= Html.Password( "Password" ) %>
<%= Html.ValidationMessage("Password", "*") %><br/>

<%= Html.LabelFor(model => model.RememberMe) %>
<%= Html.CheckBox("RememberMe") %>
<%= Html.ValidationMessage("RememberMe", "") %><br/>
<%= Html.Hidden("returnUrl",ViewData["returnUrl"]) %>
<%= Html.AntiForgeryToken() %>
<!-- Html.AntiForgeryToken() -->
<input type="submit"/>
<% } %></div>
<div class="panel">
<%= Html.ActionLink("S'enregistrer","GetRegister",new {returnUrl=ViewData["returnUrl"]}, new { @class="actionlink" }) %>
</div>
<div class="panel">
<a href="<%= Url.RouteUrl ("Default", new { controller = "Google", action= "Login", returnUrl=ViewData["returnUrl"] }) %>" class="actionlink">
	Identification avec un compte Google
	<img src="/App_Themes/images/sign-in-with-google.png" style="max-height:1.5em; max-width:6em;" alt="Google sign in">
	</a>
	</div>
</asp:Content>