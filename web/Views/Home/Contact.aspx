<%@  Page Title="Contact" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div class="panel">
<p>
Directeur : Paul Schneider<br>
Adresse postale : 2 Boulevard Aristide Briand<br>
Tél. : +33 (0) 9 80 90 36 42<br>
Mobile : +33 (0) 6 51 14 15 64<br>
SIREN : 803 851 674<br>
SIRET : 803 851 674 00017<br>
Activité Principalement Exercée (APE) : 5829C Édition de logiciels applicatifs<br>
</p>
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
<input type="submit" value="<%=Html.Translate("Submit")%>">

<% } %>
</div>
</asp:Content>