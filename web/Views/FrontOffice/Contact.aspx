<%@  Page Title="Contact" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div class="bigpanel">
<p>
<%= Html.Translate("ContactThisPerformer") %>
</p>
<% using (Html.BeginForm("Contact", "Home")) { %>
<fieldset style="width:100%">
<legend>Message</legend>
<p>
<%= Html.Label("email",LocalizedText.email) %>:
<%= Html.ValidationMessage("email") %><br/>
<%= Html.TextBox("email") %>
</p>
<p>
<%= Html.Label("reason",LocalizedText.reason) %>:
<%= Html.ValidationMessage("reason") %><br/>
<%= Html.TextBox("reason") %>
</p>
<p>
<%= Html.Label("body",LocalizedText.body) %>:
<%= Html.ValidationMessage("body") %><br/>
<%= Html.TextArea("body",new {@rows="25"}) %>
</p>
</fieldset>
<input type="submit" value="<%=Html.Translate("Submit")%>">
<% } %>
</div>
</asp:Content>