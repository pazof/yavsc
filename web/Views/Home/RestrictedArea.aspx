<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntryCollection>" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
	Ce contenu est d'accès restreint : &lt;<%= Html.Encode(ViewData["ControllerName"]) %>/<%= Html.Encode(ViewData["ActionName"]) %>&gt;

	Demandez à l'administrateur les autorisations suffisantes pour accèder à cet emplacement.

</asp:Content>