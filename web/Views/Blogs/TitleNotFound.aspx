
<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Npgsql.Web.Blog.DataModel.BlogEntryCollection>" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="head" ID="head" runat="server">
	<title> </title>
</asp:Content>
	<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">
	</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

	Pas d'article trouvé @ "<%= Html.Encode(ViewData["PostTitle"]) %>"
	<% if (ViewData["UserName"]!=null) { %>

	 <br/>
 <%= Html.ActionLink("Poster?","Post/", new { user=ViewData["UserName"], title = ViewData["PostTitle"]}, new { @class="actionlink" }) %>
 <% } %>
	</asp:Content>

