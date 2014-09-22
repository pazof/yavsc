<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntryCollection>" MasterPageFile="~/Models/App.master"%>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="head" ID="head" runat="server">
	<%= "<title>Les dernières parutions - " + Html.Encode(YavscHelpers.SiteName) + "</title>" %>
</asp:Content>
<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">	
<h2>Les dernières parutions</h2>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<% foreach (BlogEntry e in this.Model) { %>
<div class="blogpost">
<h3 class="blogtitle">
<%= Html.ActionLink(e.Title,"UserPost", new { user=e.UserName, title = e.Title }) %>
</h3>
<div class="metablog">(<%=Html.Encode(e.UserName)%> <%=e.Modified.ToString("yyyy/MM/dd") %>)</div>
<% if (Membership.GetUser()!=null)
	if (Membership.GetUser().UserName==e.UserName)
	 { %>
	 <%= Html.ActionLink("Editer","Edit", new { user = e.UserName, title = e.Title }, new { @class="actionlink" }) %>
	 <%= Html.ActionLink("Supprimer","Remove", new { user = e.UserName, title = e.Title }, new { @class="actionlink" } ) %>
	 <% } %>

</div>

<% } %>
	  <form runat="server" id="form1" method="GET">
	<% rp1.ResultCount = (int) ViewData["RecordCount"];
 rp1.CurrentPage = (int) ViewData["PageIndex"]; %>
	 <yavsc:ResultPages id="rp1" Action = "?pageIndex={0}" runat="server" ></yavsc:ResultPages> 
 		
	 </form>
</asp:Content>

