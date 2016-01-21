<%@ Page Title="Login" Language="C#" Inherits="System.Web.Mvc.ViewPage<LoginModel>" MasterPageFile="~/Models/NoLogin.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div class="panel">

<% using(Html.BeginForm("Login", "Account")) %>
<% { %>
<fieldset>
<legend>Ouverture de session</legend>
<%= Html.ValidationSummary() %>

<%= Html.LabelFor(model => model.UserName) %>
<%= Html.TextBox( "UserName" ) %>
<%= Html.ValidationMessage("UserName", "*") %><br/>

<%= Html.LabelFor(model => model.Password) %>
<%= Html.Password( "Password" ) %>
<%= Html.ValidationMessage("Password", "*") %><br/>

<%= Html.LabelFor(model => model.RememberMe) %>
<%= Html.CheckBox("RememberMe") %>
<%= Html.ValidationMessage("RememberMe", "*") %><br/>
<%= Html.Hidden("returnUrl",ViewData["returnUrl"]) %>
<%= Html.AntiForgeryToken() %>
</fieldset>

<!-- Html.AntiForgeryToken() -->
<input type="submit"/>
<% } %>
<%= Html.TranslatedActionLink("ResetPassword")%>
</div>
<div class="panel">
<%= Html.TranslatedActionLink("S'enregistrer","GetRegister",new {returnUrl=ViewData["returnUrl"]}, new { @class="actionlink" }) %>
</div>
<div class="panel">

<form method="post" action="<%=Url.RouteUrl ("Default", new { controller = "OAuth", action= "External"}) %>">
<fieldset><legend>Authentifiez-vous avec un autre compte :</legend>
<% foreach (var authtype in ViewBag.AuthTypes) { %>
<input type="submit" name="submit.External.<%=authtype.AuthenticationType%>" value="<%=authtype.Caption%>"/>
<% } %>
<input type="hidden" name="returnUrl" value="<%=ViewData["returnUrl"]%>">
</fieldset>
</form>

	</div>
</asp:Content>