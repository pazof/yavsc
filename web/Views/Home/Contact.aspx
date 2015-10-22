<%@  Page Title="Contact" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div class="panel">
<p>
Directeur : <br>
Adresse postale : <br>
Tél. : +33 <br>
Tél. : +33 <br>
SIREN : <br>
SIRET : <br>
Activité Principalement Exercée (APE) : <br>
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