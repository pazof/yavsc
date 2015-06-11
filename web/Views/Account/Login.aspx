<%@ Page Title="Login" Language="C#" Inherits="System.Web.Mvc.ViewPage<LoginModel>" MasterPageFile="~/Models/NoLogin.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
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
<!-- Html.AntiForgeryToken() -->
<input type="submit"/>
<% } %>

<%= Html.ActionLink("S'enregistrer","Register",new {returnUrl=ViewData["returnUrl"]}, new { @class="actionlink" }) %>

<a href="<%=Request.Url.Scheme + "://" + Request.Url.Authority + "/Google/Login"%>?returnUrl=<%=ViewData["returnUrl"]==null?Request.Url.PathAndQuery:(string)ViewData["returnUrl"]%>" class="actionlink">
	Identification avec un compte Google
	<img src="/images/sign-in-with-google.png" style="max-height:1.5em; max-width:6em;" alt="Google sign in">
	</a>

</asp:Content>