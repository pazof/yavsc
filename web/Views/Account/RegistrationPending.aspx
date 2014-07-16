<%@ Page Title="Comptes utilisateur - Index" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">
</asp:Content>
<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">	
<h2>Comptes utilisteur</h2>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<p>
Votre compte utilisateur 
<%= Html.Encode(YavscHelpers.SiteName) %>
a été créé, un e-mail de validation de votre compte a été envoyé a l'adresse fournie:<br/>
&lt;<%= Html.Encode(ViewData["email"]) %>&gt;.<br/>
Vous devriez le recevoir rapidement.<br/>
Pour valider votre compte, suivez le lien indiqué dans cet e-mail.
</p>
<a class="actionlink" href="<%=ViewData["ReturnUrl"]%>">Retour</a> 
</asp:Content>
