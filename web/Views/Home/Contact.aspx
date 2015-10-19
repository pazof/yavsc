<%@  Page Title="Contact" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<% using (Html.BeginForm("Contact", "Home")) { %>
<fieldset>
<legend>Message</legend>
<p>
<%= Html.Label("email") %>:
<%= Html.ValidationMessage("email") %><br/>
<%= Html.TextBox("email") %>
</p>
<p>
<%= Html.Label("reason") %>:
<%= Html.ValidationMessage("reason") %><br/>
<%= Html.TextBox("reason") %>
</p>
<p>
<%= Html.Label("body") %>:
<%= Html.ValidationMessage("body") %><br/>
<%= Html.TextArea("body") %>
</p>
</fieldset>
<input type="submit">
<% } %>

</asp:Content>