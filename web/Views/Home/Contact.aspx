﻿<%@  Page Title="Contact" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div class="bigpanel">
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