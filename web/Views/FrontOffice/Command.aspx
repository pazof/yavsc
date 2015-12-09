<%@ Page Title="Commande" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<Command>" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">

(pas implémenté)

 <%= Html.TranslatedActionLink("Votre panier","Basket","FrontOffice" ) %>

  <ul><li>
 <%= Html.TranslatedActionLink("Catalog","Catalog" ) %>
 </li><li>
 <%= Html.TranslatedActionLink("Estimates","Estimates" ) %>
 </li></ul>

</asp:Content>
