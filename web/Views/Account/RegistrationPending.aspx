<%@ Page Title="Comptes utilisateur" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
Votre compte utilisateur 
<%= Html.Encode(YavscHelpers.SiteName) %>
a été créé, un e-mail de validation de votre compte a été envoyé a l'adresse fournie:<br/>
&lt;<%= Html.Encode(ViewData["email"]) %>&gt;.<br/>
Vous devriez le recevoir rapidement.<br/>
Pour valider votre compte, suivez le lien indiqué dans cet e-mail.
<div>
<a class="actionlink" href="<%=ViewData["ReturnUrl"]%>">Retour</a> 
</div>
</asp:Content>
