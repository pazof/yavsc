<%@  Page Title="Contact" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<PerformerContact>" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div class="bigpanel">

<p>
<%= Html.Translate("ContactAPerformer") %>
</p>
<% using (Html.BeginForm("Send", "FrontOffice")) { %>
<%= Html.Translate("Performer") %> : <%= Html.Encode(Model.Performer) %>
<%= Html.Hidden("Performer") %>
<br>
<fieldset style="width:100%">
<legend>Message</legend>
<p>
<%= Html.Label("reason",LocalizedText.reason) %>:
<%= Html.ValidationMessage("reason") %><br/>
<%= Html.TextBox("reason",null, new { @style="width:100%", @placeholder=LocalizedText.PleaseFillInAReason } ) %>
</p>
<p>
<%= Html.Label("body",LocalizedText.body) %>:
<%= Html.ValidationMessage("body") %><br/>
<%= Html.TextArea("body",new {@rows="25", @style="width:100%",  @placeholder=LocalizedText.PleaseFillInABody }) %>
</p>
</fieldset>
<input type="submit" value="<%=Html.Translate("Submit")%>">
<% } %>
</div>
</asp:Content>