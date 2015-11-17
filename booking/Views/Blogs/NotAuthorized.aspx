<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntryCollection>" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
	Ce contenu est d'accès restreint : &lt;<%= Html.Encode(ViewData["BlogUser"]) %>/<%= Html.Encode(ViewData["PostTitle"]) %>&gt;
</asp:Content>