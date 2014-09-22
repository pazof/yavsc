<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntryCollection>" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="head" ID="head" runat="server">
	<title> </title>
</asp:Content>
	<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">
	</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

	Pas d'article trouvé ici: &lt;<%= Html.Encode(ViewData["BlogUser"]) %>/<%= Html.Encode(ViewData["PostTitle"]) %>&gt;
	 <br/>
 <%= Html.ActionLink("Poster?","Post/", new { user = ViewData["BlogUser"], title = ViewData["PostTitle"]}, new { @class="actionlink" }) %>
	</asp:Content>

