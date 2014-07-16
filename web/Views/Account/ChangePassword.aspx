<%@ Page Title="Change your Password" Language="C#" Inherits="System.Web.Mvc.ViewPage<ChangePasswordModel>" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">
	
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<%= Html.ValidationSummary("Modification de mot de passe") %>
<% using(Html.BeginForm("ChangePassword", "Account")) { %>
<label for="UserName">User Name:</label>
<%= Html.TextBox( "UserName" ) %>
<%= Html.ValidationMessage("UserName", "*") %><br/>
<label for="OldPassword">Old password:</label>
   <%= Html.Password( "OldPassword" ) %>
<%= Html.ValidationMessage("OldPassword", "*") %><br/>
<label for="NewPassword">New password:</label>
   <%= Html.Password( "NewPassword" ) %>
<%= Html.ValidationMessage("NewPassword", "*") %><br/>
<label for="ConfirmPassword">Confirm password:</label>
   <%= Html.Password( "ConfirmPassword" ) %>
<%= Html.ValidationMessage("ConfirmPassword", "*") %>
<input type="submit"/>
<% } %>

</asp:Content>


