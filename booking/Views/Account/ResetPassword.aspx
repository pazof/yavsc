<%@ Page Title="Reset your Password" Language="C#" Inherits="System.Web.Mvc.ViewPage<LostPasswordModel>" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<%= Html.ValidationSummary("Modification de mot de passe") %>

<% using(Html.BeginForm("ResetPassword", "Account")) { %>
Enter one of the following : <br/>
<ul><li>
<label for="UserName">Your user name (login):</label>
<%= Html.TextBox( "UserName" ) %>
<%= Html.ValidationMessage("UserName", "*") %></li>
<li>
<label for="Email">The e-mail address you used to register here:</label>
<%= Html.TextBox( "Email" ) %>
<%= Html.ValidationMessage("Email", "*") %>
</li>
</ul>
Then, hit the following button: 
<input type="submit" value="I lost my password!"/> <br/>
A message will be sent to you, containning a link that you'll can use to reset your password.
<% } %>
</asp:Content>


