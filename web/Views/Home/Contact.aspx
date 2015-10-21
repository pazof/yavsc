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
<input type="submit" value="<%=Html.Translate("Submit")%>">
<p>
Directeur : Boudjouraf Brahim<br>
Adresse postale : 29 rue des près, 92 71200 LE CREUSOT<br>
Tél. : +33 6.58.91.36.78<br>
Tél. : +33 6.78.65.36.31<br>
Facebook : Brahms Totem Officiel (Totem Prod)<br>
Twitter : Totem Officiel<br>
Skyblog : Totem-Production.skyrock.com<br>
SIREN : 517 942 991<br>
SIRET : 517 942 991 000 12<br>
Activité Principalement Exercée (APE) : 9329Z Autres activités récréatives et de loisirs<br>
</p>
<% } %>

</asp:Content>