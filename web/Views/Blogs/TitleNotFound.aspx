<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntryCollection>" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
Pas d'article trouvé ici: &lt;<%= Html.Encode(ViewData["BlogUser"]) %>/<%= Html.Encode(ViewData["PostTitle"]) %>&gt;
<br/> <%= Html.TranslatedActionLink("Poster?","Post/", new { user = ViewData["BlogUser"], title = ViewData["PostTitle"]}, new { @class="actionlink" }) %>
</asp:Content>
